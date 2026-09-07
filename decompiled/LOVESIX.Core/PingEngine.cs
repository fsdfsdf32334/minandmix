using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace LOVESIX.Core;

public static class NativeDivert
{
	public const int WINDIVERT_LAYER_NETWORK = 0;

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct WINDIVERT_ADDRESS
	{
		public long Timestamp;
		public uint LayerFlags;
		public uint Reserved2;
		public fixed byte Reserved3[64];

		public bool Outbound => ((LayerFlags >> 17) & 1) == 1;
		public bool Loopback => ((LayerFlags >> 18) & 1) == 1;
		public bool IPv6 => ((LayerFlags >> 20) & 1) == 1;
	}

	[DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	public static extern IntPtr WinDivertOpen([MarshalAs(UnmanagedType.LPStr)] string filter, int layer, short priority, ulong flags);

	[DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	public static unsafe extern bool WinDivertRecv(IntPtr handle, byte* pPacket, uint packetLen, uint* pReadLen, WINDIVERT_ADDRESS* pAddr);

	[DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	public static unsafe extern bool WinDivertSend(IntPtr handle, byte* pPacket, uint packetLen, uint* pSendLen, WINDIVERT_ADDRESS* pAddr);

	[DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	public static extern bool WinDivertClose(IntPtr handle);

	[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
	public static extern uint TimeBeginPeriod(uint uMilliseconds);

	[DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
	public static extern uint TimeEndPeriod(uint uMilliseconds);
}

public static class IpHelper
{
	private const int AF_INET = 2;
	private const int TCP_TABLE_OWNER_PID_ALL = 5;
	private const int UDP_TABLE_OWNER_PID = 1;

	[DllImport("iphlpapi.dll", SetLastError = true)]
	private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder, int ulAf, int tableClass, uint reserved);

	[DllImport("iphlpapi.dll", SetLastError = true)]
	private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize, bool bOrder, int ulAf, int tableClass, uint reserved);

	[StructLayout(LayoutKind.Sequential)]
	private struct MIB_TCPROW_OWNER_PID
	{
		public uint dwState;
		public uint dwLocalAddr;
		public uint dwLocalPort;
		public uint dwRemoteAddr;
		public uint dwRemotePort;
		public uint dwOwningPid;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MIB_UDPROW_OWNER_PID
	{
		public uint dwLocalAddr;
		public uint dwLocalPort;
		public uint dwOwningPid;
	}

	public static ushort ConvertPort(uint port)
	{
		return (ushort)(((port & 0xFF) << 8) | ((port >> 8) & 0xFF));
	}

	public static HashSet<int> GetPortsForPids(HashSet<int> targetPids)
	{
		HashSet<int> ports = new HashSet<int>();
		if (targetPids == null || targetPids.Count == 0) return ports;

		int size = 0;
		GetExtendedTcpTable(IntPtr.Zero, ref size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
		if (size > 0)
		{
			IntPtr buf = Marshal.AllocHGlobal(size);
			try
			{
				if (GetExtendedTcpTable(buf, ref size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0) == 0)
				{
					int numEntries = Marshal.ReadInt32(buf);
					IntPtr rowPtr = (IntPtr)((long)buf + 4);
					int rowSize = Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID));
					for (int i = 0; i < numEntries; i++)
					{
						MIB_TCPROW_OWNER_PID row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
						if (targetPids.Contains((int)row.dwOwningPid))
						{
							ports.Add(ConvertPort(row.dwLocalPort));
						}
						rowPtr = (IntPtr)((long)rowPtr + rowSize);
					}
				}
			}
			finally { Marshal.FreeHGlobal(buf); }
		}

		size = 0;
		GetExtendedUdpTable(IntPtr.Zero, ref size, true, AF_INET, UDP_TABLE_OWNER_PID, 0);
		if (size > 0)
		{
			IntPtr buf = Marshal.AllocHGlobal(size);
			try
			{
				if (GetExtendedUdpTable(buf, ref size, true, AF_INET, UDP_TABLE_OWNER_PID, 0) == 0)
				{
					int numEntries = Marshal.ReadInt32(buf);
					IntPtr rowPtr = (IntPtr)((long)buf + 4);
					int rowSize = Marshal.SizeOf(typeof(MIB_UDPROW_OWNER_PID));
					for (int i = 0; i < numEntries; i++)
					{
						MIB_UDPROW_OWNER_PID row = Marshal.PtrToStructure<MIB_UDPROW_OWNER_PID>(rowPtr);
						if (targetPids.Contains((int)row.dwOwningPid))
						{
							ports.Add(ConvertPort(row.dwLocalPort));
						}
						rowPtr = (IntPtr)((long)rowPtr + rowSize);
					}
				}
			}
			finally { Marshal.FreeHGlobal(buf); }
		}

		return ports;
	}
}

public struct DelayedPacket
{
	public byte[] Data;
	public uint Length;
	public NativeDivert.WINDIVERT_ADDRESS Address;
	public long ReleaseTimestamp;
}

public class EngineStats
{
	public long InterceptedPackets;
	public long DelayedPackets;
	public long PassedThroughPackets;
	public long DroppedPackets;
	public int CurrentSimulatedPing;
	public int ActivePortCount;
	public string PortsSummary = "";
}

public class PingEngine
{
	private volatile bool _isRunning = false;
	private IntPtr _divertHandle = IntPtr.Zero;
	private Thread? _recvThread;
	private Thread? _sendThread;
	private Thread? _portMonitorThread;

	private readonly ConcurrentQueue<DelayedPacket> _queue = new ConcurrentQueue<DelayedPacket>();
	private readonly AutoResetEvent _sendWaitHandle = new AutoResetEvent(false);

	public int MinPingMs = 19;
	public int MaxPingMs = 37;
	public int FixedPingMs = 50;
	public bool IsFixedMode = false;
	public int JitterMs = 0;
	public int PacketLossPercent = 0;
	public int Direction = 0;

	public bool IsAllProcesses = true;
	public int TargetPid = 0;
	public string TargetProcessName = "";
	public bool MatchSameProcessNames = true;
	public HashSet<int> CustomManualPorts = new HashSet<int>();

	private HashSet<int> _activePids = new HashSet<int>();
	private HashSet<int> _activePorts = new HashSet<int>();
	private readonly object _portLock = new object();

	private long _intercepted = 0;
	private long _delayed = 0;
	private long _passed = 0;
	private long _dropped = 0;
	private int _lastPing = 0;

	public bool IsRunning => _isRunning;

	public EngineStats GetStats()
	{
		int portCount;
		string portsStr = "";
		lock (_portLock)
		{
			portCount = _activePorts.Count;
			if (portCount > 0)
			{
				List<int> plist = new List<int>(_activePorts);
				if (plist.Count > 5)
				{
					portsStr = string.Join(", ", plist.GetRange(0, 5)) + $" ... (+{plist.Count - 5})";
				}
				else
				{
					portsStr = string.Join(", ", plist);
				}
			}
		}

		return new EngineStats
		{
			InterceptedPackets = Interlocked.Read(ref _intercepted),
			DelayedPackets = Interlocked.Read(ref _delayed),
			PassedThroughPackets = Interlocked.Read(ref _passed),
			DroppedPackets = Interlocked.Read(ref _dropped),
			CurrentSimulatedPing = _lastPing,
			ActivePortCount = portCount,
			PortsSummary = portsStr
		};
	}

	public void Start()
	{
		if (_isRunning) return;

		string filter = "(ip or ipv6) and (tcp or udp) and !loopback";
		_divertHandle = NativeDivert.WinDivertOpen(filter, NativeDivert.WINDIVERT_LAYER_NETWORK, 0, 0);

		if (_divertHandle == IntPtr.Zero || _divertHandle == (IntPtr)(-1))
		{
			int err = Marshal.GetLastWin32Error();
			string errMsg = "ไม่สามารถเชื่อมต่อไดรเวอร์เครือข่ายได้ (Error Code: " + err + ")\n";
			if (err == 5) errMsg += "กรุณาคลิกขวาที่โปรแกรมแล้วเลือก 'Run as administrator'";
			else if (err == 2) errMsg += "ไม่พบไฟล์ WinDivert.dll หรือ WinDivert64.sys ในโฟลเดอร์";
			else errMsg += "กรุณาปิดโปรแกรมจำลองปิงอื่นก่อนเริ่มใช้งาน";
			throw new Exception(errMsg);
		}

		_isRunning = true;
		NativeDivert.TimeBeginPeriod(1);

		RefreshPorts();

		_recvThread = new Thread(RecvWorker) { IsBackground = true, Priority = ThreadPriority.AboveNormal };
		_sendThread = new Thread(SendWorker) { IsBackground = true, Priority = ThreadPriority.Highest };
		_portMonitorThread = new Thread(PortMonitorWorker) { IsBackground = true, Priority = ThreadPriority.BelowNormal };

		_recvThread.Start();
		_sendThread.Start();
		_portMonitorThread.Start();
	}

	public void Stop()
	{
		if (!_isRunning) return;
		_isRunning = false;

		if (_divertHandle != IntPtr.Zero && _divertHandle != (IntPtr)(-1))
		{
			NativeDivert.WinDivertClose(_divertHandle);
			_divertHandle = IntPtr.Zero;
		}

		_sendWaitHandle.Set();

		if (_recvThread != null && _recvThread.IsAlive) _recvThread.Join(400);
		if (_sendThread != null && _sendThread.IsAlive) _sendThread.Join(400);
		if (_portMonitorThread != null && _portMonitorThread.IsAlive) _portMonitorThread.Join(400);

		while (_queue.TryDequeue(out _)) { }

		NativeDivert.TimeEndPeriod(1);
	}

	private void PortMonitorWorker()
	{
		while (_isRunning)
		{
			try { RefreshPorts(); } catch { }
			Thread.Sleep(400);
		}
	}

	public void RefreshPorts()
	{
		if (IsAllProcesses)
		{
			lock (_portLock) { _activePorts.Clear(); }
			return;
		}

		HashSet<int> targetPids = new HashSet<int>();
		if (TargetPid > 0)
		{
			targetPids.Add(TargetPid);
			if (MatchSameProcessNames && !string.IsNullOrEmpty(TargetProcessName))
			{
				try
				{
					string pureName = Path.GetFileNameWithoutExtension(TargetProcessName);
					Process[] procs = Process.GetProcessesByName(pureName);
					for (int i = 0; i < procs.Length; i++)
					{
						targetPids.Add(procs[i].Id);
					}
				}
				catch { }
			}
		}

		HashSet<int> foundPorts = IpHelper.GetPortsForPids(targetPids);

		if (CustomManualPorts != null)
		{
			foreach (int cp in CustomManualPorts)
			{
				foundPorts.Add(cp);
			}
		}

		lock (_portLock)
		{
			_activePids = targetPids;
			_activePorts = foundPorts;
		}
	}

	private unsafe void RecvWorker()
	{
		const int BUFFER_SIZE = 65535;
		byte[] buffer = new byte[BUFFER_SIZE];
		NativeDivert.WINDIVERT_ADDRESS addr = new NativeDivert.WINDIVERT_ADDRESS();
		uint readLen = 0;
		uint sendLen = 0;
		Random rand = new Random();

		fixed (byte* pBuffer = buffer)
		{
			while (_isRunning && _divertHandle != IntPtr.Zero)
			{
				try
				{
					if (!NativeDivert.WinDivertRecv(_divertHandle, pBuffer, (uint)BUFFER_SIZE, &readLen, &addr))
					{
						if (!_isRunning) break;
						continue;
					}

					Interlocked.Increment(ref _intercepted);

					if (readLen == 0 || readLen > BUFFER_SIZE) continue;

					bool outbound = addr.Outbound;
					if ((Direction == 1 && outbound) || (Direction == 2 && !outbound))
					{
						NativeDivert.WinDivertSend(_divertHandle, pBuffer, readLen, &sendLen, &addr);
						Interlocked.Increment(ref _passed);
						continue;
					}

					bool matchProcess = false;
					if (IsAllProcesses)
					{
						matchProcess = true;
					}
					else
					{
						ushort localPort = 0;
						if (!addr.IPv6)
						{
							byte ihl = (byte)((buffer[0] & 0x0F) * 4);
							if (readLen >= (uint)(ihl + 4))
							{
								byte protocol = buffer[9];
								if (protocol == 6 || protocol == 17)
								{
									ushort srcPort = (ushort)((buffer[ihl] << 8) | buffer[ihl + 1]);
									ushort dstPort = (ushort)((buffer[ihl + 2] << 8) | buffer[ihl + 3]);
									localPort = outbound ? srcPort : dstPort;
								}
							}
						}
						else
						{
							if (readLen >= 44)
							{
								byte nextHeader = buffer[6];
								if (nextHeader == 6 || nextHeader == 17)
								{
									ushort srcPort = (ushort)((buffer[40] << 8) | buffer[40 + 1]);
									ushort dstPort = (ushort)((buffer[40 + 2] << 8) | buffer[40 + 3]);
									localPort = outbound ? srcPort : dstPort;
								}
							}
						}

						if (localPort > 0)
						{
							lock (_portLock)
							{
								matchProcess = _activePorts.Contains(localPort);
							}
						}
					}

					if (!matchProcess)
					{
						NativeDivert.WinDivertSend(_divertHandle, pBuffer, readLen, &sendLen, &addr);
						Interlocked.Increment(ref _passed);
						continue;
					}

					if (PacketLossPercent > 0 && rand.Next(100) < PacketLossPercent)
					{
						Interlocked.Increment(ref _dropped);
						continue;
					}

					int delayMs = 0;
					if (!IsFixedMode)
					{
						int min = Math.Min(MinPingMs, MaxPingMs);
						int max = Math.Max(MinPingMs, MaxPingMs);
						delayMs = rand.Next(min, max + 1);
					}
					else
					{
						delayMs = FixedPingMs;
					}

					if (JitterMs > 0)
					{
						delayMs = Math.Max(0, delayMs + rand.Next(-JitterMs, JitterMs + 1));
					}

					_lastPing = delayMs;

					if (delayMs <= 0)
					{
						NativeDivert.WinDivertSend(_divertHandle, pBuffer, readLen, &sendLen, &addr);
						Interlocked.Increment(ref _passed);
						continue;
					}

					byte[] packetCopy = new byte[readLen];
					Marshal.Copy((IntPtr)pBuffer, packetCopy, 0, (int)readLen);

					long releaseTick = Stopwatch.GetTimestamp() + (long)(delayMs * (Stopwatch.Frequency / 1000.0));

					DelayedPacket delayedPkt = new DelayedPacket
					{
						Data = packetCopy,
						Length = readLen,
						Address = addr,
						ReleaseTimestamp = releaseTick
					};

					_queue.Enqueue(delayedPkt);
					Interlocked.Increment(ref _delayed);
					_sendWaitHandle.Set();
				}
				catch { }
			}
		}
	}

	private unsafe void SendWorker()
	{
		uint sendLen = 0;
		while (_isRunning)
		{
			try
			{
				if (_queue.TryPeek(out DelayedPacket packet))
				{
					long now = Stopwatch.GetTimestamp();
					if (now >= packet.ReleaseTimestamp)
					{
						if (_queue.TryDequeue(out packet))
						{
							if (_isRunning && _divertHandle != IntPtr.Zero)
							{
								fixed (byte* pData = packet.Data)
								{
									NativeDivert.WINDIVERT_ADDRESS addr = packet.Address;
									NativeDivert.WinDivertSend(_divertHandle, pData, packet.Length, &sendLen, &addr);
								}
							}
						}
						continue;
					}
					else
					{
						double remainingMs = ((packet.ReleaseTimestamp - now) * 1000.0) / Stopwatch.Frequency;
						if (remainingMs > 2.0) Thread.Sleep(1);
						else Thread.SpinWait(30);
					}
				}
				else
				{
					_sendWaitHandle.WaitOne(2);
				}
			}
			catch { }
		}
	}
}
