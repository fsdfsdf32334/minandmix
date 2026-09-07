using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LOVESIX.Core;

public class Cheat
{
	public bool IsEnabled;

	public List<PatchedAddress> Patches = new List<PatchedAddress>();

	public nint Handle;

	public Timer LockTimer;

	public string LastStatus = "";

	public List<nint> CachedMatches;

	public string Name { get; set; }

	public string Group { get; set; }

	public byte[] Pattern { get; set; }

	public byte[] Mask { get; set; }

	public byte[] Patch { get; set; }

	public string SearchHex { get; set; } = "";
	public string PatchHex { get; set; } = "";
	public string DefaultPatchHex { get; set; } = "";

	public bool PatchAll { get; set; }

	public int LockIntervalMs { get; set; }

	public void ClearCache()
	{
		CachedMatches = null;
	}

	public void WarmCache(nint handle)
	{
		if (CachedMatches == null || CachedMatches.Count <= 0)
		{
			List<nint> list = MemoryScanner.Scan(handle, Pattern, Mask, (!PatchAll) ? 1 : int.MaxValue);
			if (list.Count > 0)
			{
				CachedMatches = list;
			}
		}
	}

	public bool Enable(nint handle)
	{
		Handle = handle;
		if (IsEnabled)
		{
			return true;
		}
		List<nint> list;
		if (CachedMatches != null && CachedMatches.Count > 0)
		{
			list = new List<nint>(CachedMatches);
		}
		else
		{
			list = MemoryScanner.Scan(handle, Pattern, Mask, (!PatchAll) ? 1 : int.MaxValue);
			if (list.Count == 0)
			{
				LastStatus = "[" + Name + "] ไม\u0e48พบ AOB Pattern";
				return false;
			}
			CachedMatches = new List<nint>(list);
		}
		foreach (nint item in list)
		{
			byte[] array = new byte[Patch.Length];
			if (!MemoryHelper.TryRead(handle, item, array))
			{
				continue;
			}
			if (!MemoryHelper.Write(handle, item, Patch))
			{
				foreach (PatchedAddress patch in Patches)
				{
					MemoryHelper.Write(handle, patch.Address, patch.Original);
				}
				Patches.Clear();
				CachedMatches = null;
				LastStatus = $"[{Name}] เข\u0e35ยนหน\u0e48วยความจำล\u0e49มเหลว @ 0x{((IntPtr)item).ToInt64():X}";
				return false;
			}
			Patches.Add(new PatchedAddress
			{
				Address = item,
				Original = array
			});
		}
		if (Patches.Count == 0)
		{
			CachedMatches = null;
			LastStatus = "[" + Name + "] ไม\u0e48พบ AOB Pattern";
			return false;
		}
		IsEnabled = true;
		LastStatus = $"[{Name}] ON — แก\u0e49ไข {Patches.Count} ตำแหน\u0e48ง";
		return true;
	}

	public bool Disable()
	{
		if (LockTimer != null)
		{
			try
			{
				LockTimer.Stop();
			}
			catch
			{
			}
			try
			{
				LockTimer.Dispose();
			}
			catch
			{
			}
			LockTimer = null;
		}
		int count = Patches.Count;
		int num = 0;
		foreach (PatchedAddress patch in Patches)
		{
			if (Handle != IntPtr.Zero && MemoryHelper.Write(Handle, patch.Address, patch.Original))
			{
				num++;
			}
		}
		Patches.Clear();
		IsEnabled = false;
		LastStatus = $"[{Name}] OFF — ค\u0e37นค\u0e48าเด\u0e34ม {num}/{count}";
		return true;
	}
}
