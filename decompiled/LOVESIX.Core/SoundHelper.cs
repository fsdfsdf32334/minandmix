using System;
using System.Threading;

namespace LOVESIX.Core;

public static class SoundHelper
{
	public static void PlayToggle(bool on)
	{
		if (!AppSettings.Current.NotifySound) return;

		ThreadPool.QueueUserWorkItem(_ =>
		{
			try
			{
				if (on)
				{
					// High futuristic chirp
					Console.Beep(1200, 60);
					Console.Beep(1600, 80);
				}
				else
				{
					// Low down tone
					Console.Beep(900, 60);
					Console.Beep(650, 80);
				}
			}
			catch
			{
				// Fallback to system sound if hardware beep not supported
				try
				{
					System.Media.SystemSounds.Asterisk.Play();
				}
				catch { }
			}
		});
	}
}
