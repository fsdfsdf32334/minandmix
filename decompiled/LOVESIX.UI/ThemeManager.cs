using System;
using System.Collections.Generic;
using System.Drawing;

namespace LOVESIX.UI;

/// <summary>
/// Advanced Theme Manager supporting Dark, Light, and Auto themes
/// </summary>
public static class ThemeManager
{
    public enum ThemeMode { Dark, Light, Auto }
    
    private static ThemeMode _currentMode = ThemeMode.Dark;
    private static Dictionary<ThemeMode, ThemePalette> _themes = new();
    
    public static event Action<ThemeMode> ThemeChanged;
    
    static ThemeManager()
    {
        InitializeThemes();
    }
    
    private static void InitializeThemes()
    {
        // Dark Theme (Default - Optimized for Gaming)
        _themes[ThemeMode.Dark] = new ThemePalette
        {
            Name = "Dark (Gaming)",
            Back = Color.FromArgb(13, 18, 27),
            Panel = Color.FromArgb(20, 28, 42),
            Panel2 = Color.FromArgb(15, 22, 35),
            Field = Color.FromArgb(25, 35, 50),
            Text = Color.FromArgb(230, 235, 245),
            Muted = Color.FromArgb(120, 135, 160),
            Accent = Color.FromArgb(100, 200, 255),
            Accent2 = Color.FromArgb(80, 160, 255),
            On = Color.FromArgb(76, 200, 120),
            Danger = Color.FromArgb(255, 100, 100),
            Warning = Color.FromArgb(255, 170, 50),
            Border = Color.FromArgb(50, 70, 100),
            CardHeader = Color.FromArgb(18, 25, 40),
            NavActive = Color.FromArgb(25, 35, 55),
            On2 = Color.FromArgb(150, 220, 180)
        };
        
        // Light Theme
        _themes[ThemeMode.Light] = new ThemePalette
        {
            Name = "Light Professional",
            Back = Color.FromArgb(245, 247, 250),
            Panel = Color.FromArgb(255, 255, 255),
            Panel2 = Color.FromArgb(240, 243, 248),
            Field = Color.FromArgb(235, 240, 248),
            Text = Color.FromArgb(20, 30, 50),
            Muted = Color.FromArgb(100, 120, 150),
            Accent = Color.FromArgb(10, 120, 200),
            Accent2 = Color.FromArgb(30, 100, 180),
            On = Color.FromArgb(40, 160, 80),
            Danger = Color.FromArgb(200, 40, 40),
            Warning = Color.FromArgb(220, 120, 0),
            Border = Color.FromArgb(200, 210, 225),
            CardHeader = Color.FromArgb(250, 252, 255),
            NavActive = Color.FromArgb(235, 242, 255),
            On2 = Color.FromArgb(80, 180, 130)
        };
    }
    
    public static ThemeMode CurrentMode
    {
        get => _currentMode;
        set
        {
            if (_currentMode != value)
            {
                _currentMode = value;
                ThemeChanged?.Invoke(value);
            }
        }
    }
    
    public static ThemePalette GetCurrentPalette()
    {
        if (_currentMode == ThemeMode.Auto)
        {
            // Auto-detect based on system time (day/night)
            int hour = DateTime.Now.Hour;
            return (hour >= 6 && hour < 18) ? _themes[ThemeMode.Light] : _themes[ThemeMode.Dark];
        }
        return _themes[_currentMode];
    }
    
    public static ThemePalette GetTheme(ThemeMode mode)
    {
        return _themes.TryGetValue(mode, out var theme) ? theme : _themes[ThemeMode.Dark];
    }
}

/// <summary>
/// Theme Palette Configuration
/// </summary>
public class ThemePalette
{
    public string Name { get; set; }
    public Color Back { get; set; }
    public Color Panel { get; set; }
    public Color Panel2 { get; set; }
    public Color Field { get; set; }
    public Color Text { get; set; }
    public Color Muted { get; set; }
    public Color Accent { get; set; }
    public Color Accent2 { get; set; }
    public Color On { get; set; }
    public Color On2 { get; set; }
    public Color Danger { get; set; }
    public Color Warning { get; set; }
    public Color Border { get; set; }
    public Color CardHeader { get; set; }
    public Color NavActive { get; set; }
}
