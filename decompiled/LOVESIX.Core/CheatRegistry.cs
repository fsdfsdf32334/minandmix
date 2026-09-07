using System;
using System.Collections.Generic;

namespace LOVESIX.Core;

public static class CheatRegistry
{
	public static byte[] H(string hex)
	{
		return Convert.FromHexString(hex.Replace(" ", "").Replace("??", "00"));
	}

	public static byte[] MaskFromHex(string hex)
	{
		string[] array = hex.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		byte[] array2 = new byte[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = (byte)((!array[i].Contains("?")) ? byte.MaxValue : 0);
		}
		return array2;
	}

	public static Cheat Make(string name, string aob, string patch, string group = null, bool patchAll = false, int lockMs = 0)
	{
		string activePatch = patch;
		if (AppSettings.Current.CheatOverrides != null && AppSettings.Current.CheatOverrides.TryGetValue(name, out string? customPatch) && !string.IsNullOrWhiteSpace(customPatch))
		{
			activePatch = customPatch;
		}

		return new Cheat
		{
			Name = name,
			Group = group,
			Pattern = H(aob),
			Mask = MaskFromHex(aob),
			Patch = H(activePatch),
			SearchHex = aob,
			PatchHex = activePatch,
			DefaultPatchHex = patch,
			PatchAll = patchAll,
			LockIntervalMs = lockMs
		};
	}

	public static List<Cheat> Build()
	{
		List<Cheat> list = new List<Cheat>();
		string aob = "1F 85 EB 3D 5C 6E";
		(string, string)[] array = new(string, string)[14]
		{
			("0.3", "9A 99 99 3E"),
			("0.45", "6A 66 E6 3E"),
			("0.5", "00 00 00 3F"),
			("0.9", "14 AE 67 3F"),
			("1.05", "66 66 86 3F"),
			("1.2", "9A 99 99 3F"),
			("1.3", "66 66 A6 3F"),
			("1.4", "33 33 B3 3F"),
			("1.5", "00 00 C0 3F"),
			("1.6", "CD CC CC 3F"),
			("1.7", "9A 99 D9 3F"),
			("1.8", "66 66 E6 3F"),
			("1.9", "33 33 F3 3F"),
			("2.0", "00 00 00 40")
		};
		for (int i = 0; i < array.Length; i++)
		{
			var (text, text2) = array[i];
			list.Add(Make("หม\u0e31ดจม " + text, aob, text2 + " 5C 6E", "หม\u0e31ดจม"));
		}
		string aob2 = "00 00 48 41 00 00 00 00 A8 50 46";
		(string, string)[] array2 = new(string, string)[16]
		{
			("50", "00 00 48 42"),
			("100", "00 00 C8 42"),
			("200", "00 00 48 43"),
			("300", "00 00 96 43"),
			("400", "00 00 C8 43"),
			("500", "00 00 FA 43"),
			("600", "00 00 16 44"),
			("700", "00 00 2F 44"),
			("800", "00 00 48 44"),
			("900", "00 00 61 44"),
			("1000", "00 00 7A 44"),
			("2000", "00 00 FA 44"),
			("3000", "00 80 3B 45"),
			("4000", "00 00 7A 45"),
			("5000", "00 40 9C 45"),
			("10000", "00 40 1C 46")
		};
		array = array2;
		for (int i = 0; i < array.Length; i++)
		{
			var (text3, text4) = array[i];
			list.Add(Make("หล\u0e31ง " + text3, aob2, text4 + " 00 00 00 00 A8 50 46", "ต\u0e31วไหล:หล\u0e31ง"));
		}
		string aob3 = "00 00 20 41 00 00 00 00 27 23";
		array = array2;
		for (int i = 0; i < array.Length; i++)
		{
			var (text5, text6) = array[i];
			list.Add(Make("เซ " + text5, aob3, text6 + " 00 00 00 00 27 23", "ต\u0e31วไหล:เซ"));
		}
		list.Add(Make("0.3", "1F 85 EB 3D 5C 6E", "9A 99 99 3E 5C 6E"));
		list.Add(Make("Sink จม", "CD CC 9C 40 00 00 20 41 00 00 48 41 00 00 00", "CD CC 8C 40 00 00 20 41 00 00 48 41 00 00 C8"));
		list.Add(Make("ค\u0e48าย\u0e31ต 0.95", "DC DD 9D 3F", "33 33 73 3F"));
		list.Add(Make("หม\u0e31ดไว", "CF F7 13 3F 00 00 80 3F 00", "7B 14 0E 3F 00 00 80 3F 00"));
		list.Add(Make("หม\u0e31ดไว v2", "75 77 17 40 01 00 00 00 00 00", "F0 8B 15 40 01 00 00 00 00 00"));
		list.Add(Make("ส\u0e31\u0e48งจม", "5D 41 7E CC 4A 24 5A FD 8D 9D", "AD 9C B4 95 4A 24 5A FD 8D 9D"));
		list.Add(Make("ส\u0e31\u0e48งพ\u0e38\u0e48ง", "9B A4 0A 6C 00 00 00 00 C4 4C", "B5 22 91 0A 00 00 00 00 C4 4C"));
		list.Add(Make("ห\u0e31นไวไม\u0e48ต\u0e48อยหล\u0e31ง", "0E 00 00 00 09 39 F6 8C 00 00", "06 80 82 4E 09 39 F6 8C 00 00"));
		list.Add(Make("ขาน\u0e34\u0e48งด\u0e39ดพ\u0e37\u0e49น", "CD CC AC 40 00 00 20 41 00 00", "D7 A3 A8 40 00 00 20 41 00 00"));
		list.Add(Make("ต\u0e31วไหล", "00 00 20 41 00 00 00 00 27 23", "00 00 A0 41 00 00 00 00 A8 50 46"));
		list.Add(Make("เกราะแตกต\u0e31วไหล", "CD CC AC 40 00 00 20 41 00 00", "34 33 AB 40 00 00 20 41 9A 99"));
		list.Add(Make("ไม\u0e49+3", "06 03 35 25 00 00 00 00 ?? ??", "25 6A C4 18", null, patchAll: true));
		list.Add(Make("God 152", "?? 40 00 00 00 00 00 6E 61 6C", "98 40 00 00 00 00 00 6E 61 6C", null, patchAll: true));
		list.Add(Make("ว\u0e34\u0e48งไว 1.2", "00 00 80 3F 00 00 C8 42 ?? ??", "9A 99 99 3F 00 00 C8 42 00 00", null, patchAll: true));
		list.Add(Make("น\u0e49ำว\u0e34\u0e48งไม\u0e48ลด", "00 00 C8 42 00 00 C8 42 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ??", "00 00 C8 42 00 00 C8 42 00 00 00 00", null, patchAll: true, 100));
		list.Add(Make("qป\u0e34ดต\u0e31วช\u0e38บ 1", "2E 00 00 00 4C B0 21 2E 00 00", "00 00 00 00 4C B0 21 2E 00 00"));
		list.Add(Make("qป\u0e34ดต\u0e31วช\u0e38บ 2", "1F 85 EB 3D 79", "CD CC CC 3E 79 37 00 00 00 6E"));
		list.Add(Make("ม\u0e35ดทร\u0e34คใหม\u0e48", "EF 33 D2 50 E6 89 7D 2C C7 4E", "43 8A DE A3 E6 89 7D 2C C7 4E"));
		list.Add(Make("ม\u0e35ดร\u0e35ต\u0e31วไว 1", "66 66 26 3F 00 00 80 3F 20", "B8 1E 05 3F 00 00 80 3F 20 AE"));
		list.Add(Make("ม\u0e35ดร\u0e35ต\u0e31วไว 2", "0C 02 2B 3F 00 00 80 3F C0", "33 33 B3 3E 00 00 80 3F C0"));

		// Load custom user AOBs from settings
		try
		{
			if (AppSettings.Current.CustomAobs != null)
			{
				foreach (var custom in AppSettings.Current.CustomAobs)
				{
					if (!string.IsNullOrWhiteSpace(custom.Name) && !string.IsNullOrWhiteSpace(custom.SearchAob) && !string.IsNullOrWhiteSpace(custom.PatchAob))
					{
						list.Add(Make(custom.Name, custom.SearchAob, custom.PatchAob, null, custom.PatchAll, custom.LockMs));
					}
				}
			}
		}
		catch { }

		return list;
	}
}
