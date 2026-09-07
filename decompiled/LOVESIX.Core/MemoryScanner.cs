using System;
using System.Collections.Generic;

namespace LOVESIX.Core;

public static class MemoryScanner
{
	public static List<nint> Scan(nint handle, byte[] pattern, byte[] mask, int maxMatches)
	{
		List<nint> list = new List<nint>();
		int num = pattern.Length;
		if (num == 0)
		{
			return list;
		}
		int num2 = -1;
		byte b = 0;
		for (int i = 0; i < num; i++)
		{
			if (mask[i] == byte.MaxValue)
			{
				if (num2 == -1)
				{
					num2 = i;
					b = pattern[i];
				}
				else if (pattern[i] != 0)
				{
					num2 = i;
					b = pattern[i];
					break;
				}
			}
		}
		if (num2 == -1)
		{
			return list;
		}
		int num3 = num - 1;
		int num4 = 1048576;
		byte[] destinationArray = new byte[num3];
		bool flag = false;
		foreach (MemoryHelper.MemoryRegion item in MemoryHelper.EnumerateRegions(handle))
		{
			long num5 = ((IntPtr)item.Address).ToInt64();
			long num6 = num5 + item.Size;
			long num7 = num5;
			flag = false;
			while (num7 < num6)
			{
				long num8 = Math.Min(num4, num6 - num7);
				long num9 = num7;
				if (flag && num7 - num3 >= num5)
				{
					num9 = num7 - num3;
				}
				byte[] array = new byte[num7 - num9 + num8];
				if (!MemoryHelper.TryRead(handle, new IntPtr(num9), array))
				{
					num7 += num8;
					continue;
				}
				for (int j = 0; j <= array.Length - num; j++)
				{
					if (array[j + num2] != b)
					{
						continue;
					}
					bool flag2 = true;
					for (int k = 0; k < num; k++)
					{
						if (mask[k] == byte.MaxValue && array[j + k] != pattern[k])
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						list.Add(new IntPtr(num9 + j));
						if (list.Count >= maxMatches)
						{
							return list;
						}
					}
				}
				if (array.Length >= num3)
				{
					Array.Copy(array, array.Length - num3, destinationArray, 0, num3);
					flag = true;
				}
				num7 += num8;
			}
		}
		return list;
	}
}
