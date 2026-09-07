using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LOVESIX.Core;

public static class MemoryHelper
{
	[Flags]
	public enum ProcessAccess : uint
	{
		QueryInformation = 0x400u,
		VmOperation = 8u,
		VmRead = 0x10u,
		VmWrite = 0x20u
	}

	public struct MemoryBasicInformation
	{
		public nint BaseAddress;

		public nint AllocationBase;

		public uint AllocationProtect;

		public nint RegionSize;

		public uint State;

		public uint Protect;

		public uint Type;
	}

	public struct MemoryRegion(nint address, long size)
	{
		public nint Address = address;

		public long Size = size;
	}

	private const uint MemCommit = 4096u;

	private const uint PageNoAccess = 1u;

	private const uint PageGuard = 256u;

	private const uint PageExecuteReadWrite = 64u;

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern nint OpenProcess(ProcessAccess access, bool inheritHandle, int processId);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool CloseHandle(nint handle);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool VirtualProtectEx(nint hProcess, nint lpAddress, nuint dwSize, uint flNewProtect, out uint lpflOldProtect);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int VirtualQueryEx(nint hProcess, nint lpAddress, out MemoryBasicInformation lpBuffer, int dwLength);

	public static nint OpenProcessSafe(int pid)
	{
		nint num = OpenProcess(ProcessAccess.QueryInformation | ProcessAccess.VmOperation | ProcessAccess.VmRead | ProcessAccess.VmWrite, inheritHandle: false, pid);
		if (num == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error(), $"ไม\u0e48สามารถเป\u0e34ดโปรเซส PID {pid}");
		}
		return num;
	}

	public static bool TryRead(nint handle, nint address, byte[] buffer)
	{
		if (handle == IntPtr.Zero || buffer == null || buffer.Length == 0)
		{
			return false;
		}
		int lpNumberOfBytesRead;
		return ReadProcessMemory(handle, address, buffer, buffer.Length, out lpNumberOfBytesRead);
	}

	public static byte[] Read(nint handle, nint address, int size)
	{
		byte[] array = new byte[size];
		TryRead(handle, address, array);
		return array;
	}

	public static bool Write(nint handle, nint address, byte[] bytes)
	{
		if (handle == IntPtr.Zero || bytes == null || bytes.Length == 0)
		{
			return false;
		}
		int lpNumberOfBytesWritten;
		if (VirtualProtectEx(handle, address, (nuint)bytes.Length, 64u, out var lpflOldProtect))
		{
			bool result = WriteProcessMemory(handle, address, bytes, bytes.Length, out lpNumberOfBytesWritten);
			VirtualProtectEx(handle, address, (nuint)bytes.Length, lpflOldProtect, out var _);
			return result;
		}
		return WriteProcessMemory(handle, address, bytes, bytes.Length, out lpNumberOfBytesWritten);
	}

	public static IEnumerable<MemoryRegion> EnumerateRegions(nint handle)
	{
		long address = 0L;
		MemoryBasicInformation lpBuffer = default(MemoryBasicInformation);
		int mbiSize = Marshal.SizeOf<MemoryBasicInformation>();
		while (VirtualQueryEx(handle, new IntPtr(address), out lpBuffer, mbiSize) != 0)
		{
			long regionSize = ((IntPtr)lpBuffer.RegionSize).ToInt64();
			if (regionSize <= 0)
			{
				break;
			}
			bool num = lpBuffer.State == 4096;
			bool flag = (lpBuffer.Protect & 1) == 0 && (lpBuffer.Protect & 0x100) == 0;
			if (num & flag)
			{
				yield return new MemoryRegion(lpBuffer.BaseAddress, regionSize);
			}
			long num2 = address + regionSize;
			if (num2 <= address)
			{
				break;
			}
			address = num2;
		}
	}
}
