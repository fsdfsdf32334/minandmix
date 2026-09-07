using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace LOVESIX.Core;

public class PingProfile
{
	public string Name { get; set; } = "";
	public int PingMin { get; set; } = 19;
	public int PingMax { get; set; } = 37;
	public int JitterMs { get; set; } = 5;
	public int PacketLossPercent { get; set; } = 0;
}

public class CustomAobItem
{
	public string Name { get; set; } = "";
	public string SearchAob { get; set; } = "";
	public string PatchAob { get; set; } = "";
	public bool PatchAll { get; set; } = false;
	public int LockMs { get; set; } = 0;
}

public class CheatPreset
{
	public string Name { get; set; } = "";
	public List<string> EnabledCheats { get; set; } = new List<string>();
	public Dictionary<string, string> GroupSelections { get; set; } = new Dictionary<string, string>();
	public bool PingEnabled { get; set; }
	public int PingMin { get; set; } = 19;
	public int PingMax { get; set; } = 37;
}

public class UsageLogEntry
{
	public DateTime Timestamp { get; set; } = DateTime.Now;
	public string Type { get; set; } = "";
	public string Message { get; set; } = "";
}

public class AppSettings
{
	public bool AutoAttach { get; set; } = true;

	public bool AlwaysOnTop { get; set; }

	public bool NotifySound { get; set; } = true;

	public bool CloseToTray { get; set; } = true;

	public bool HoldPush { get; set; }

	public Keys HoldPushKey { get; set; }

	public bool HoldSink { get; set; }

	public Keys HoldSinkKey { get; set; }

	// Hide Window / Boss Key
	public Keys HideHotkey { get; set; } = Keys.F11;

	// Emergency Panic Key (restore all memory immediately)
	public Keys PanicHotkey { get; set; } = Keys.F10;

	// Ping Settings
	public int PingMin { get; set; } = 19;
	public int PingMax { get; set; } = 37;
	public bool PingFixedMode { get; set; } = false;
	public int PingFixedVal { get; set; } = 50;
	public int PingDirection { get; set; } = 0;
	public bool PingMatchSameName { get; set; } = true;
	public Keys PingHotkey { get; set; } = Keys.F6;

	// Extended Ping Settings
	public int JitterMs { get; set; } = 0;
	public int PacketLossPercent { get; set; } = 0;
	public bool OverlayEnabled { get; set; } = false;
	public int ActiveProfileIndex { get; set; } = -1;

	public List<CustomAobItem> CustomAobs { get; set; } = new List<CustomAobItem>();

	// Saved cheat presets (named cheat activation sets)
	public List<CheatPreset> Presets { get; set; } = new List<CheatPreset>();

	// Automation rules
	public bool AutoStartPingOnAttach { get; set; }
	public int AutoDisableCheatsAfterMinutes { get; set; }
	public int AutoPanicAfterMinutes { get; set; }

	// Lightweight usage/report history
	public List<UsageLogEntry> UsageHistory { get; set; } = new List<UsageLogEntry>();

	// Custom Patch Overrides for each cheat (Name -> Custom Patch Hex)
	public Dictionary<string, string> CheatOverrides { get; set; } = new Dictionary<string, string>();

	public List<PingProfile> Profiles { get; set; } = new List<PingProfile>
	{
		new PingProfile { Name = "วอร์เซิฟ", PingMin = 19, PingMax = 37, JitterMs = 5, PacketLossPercent = 0 },
		new PingProfile { Name = "ไหลหนี", PingMin = 80, PingMax = 120, JitterMs = 10, PacketLossPercent = 0 },
		new PingProfile { Name = "ปกติ", PingMin = 0, PingMax = 0, JitterMs = 0, PacketLossPercent = 0 }
	};

	private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MIXANDMIN", "settings.json");

	public static AppSettings Current { get; private set; } = Load();

	private static AppSettings Load()
	{
		try
		{
			if (File.Exists(FilePath))
			{
				return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings();
			}
		}
		catch
		{
		}
		return new AppSettings();
	}

	public static void Save()
	{
		try
		{
			string? directoryName = Path.GetDirectoryName(FilePath);
			if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.WriteAllText(FilePath, JsonSerializer.Serialize(Current, new JsonSerializerOptions
			{
				WriteIndented = true
			}));
		}
		catch
		{
		}
	}

	public static bool ExportToFile(string path)
	{
		try
		{
			File.WriteAllText(path, JsonSerializer.Serialize(Current, new JsonSerializerOptions
			{
				WriteIndented = true
			}));
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool ImportFromFile(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path));
				if (loaded != null)
				{
					Current = loaded;
					Save();
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}
}
