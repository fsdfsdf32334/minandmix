using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LOVESIX.Core;

public class GlobalKeyboardHook : IDisposable
{
	private delegate nint HookProc(int nCode, nint wParam, nint lParam);

	private struct KbdLlHookStruct
	{
		public int vkCode;

		public int scanCode;

		public int flags;

		public int time;

		public nint dwExtraInfo;
	}

	private const int WH_KEYBOARD_LL = 13;

	private const int WM_KEYDOWN = 256;

	private const int WM_KEYUP = 257;

	private const int WM_SYSKEYDOWN = 260;

	private const int WM_SYSKEYUP = 261;

	private readonly HookProc _proc;

	private nint _hookId = IntPtr.Zero;

	public event Action<Keys> KeyDown;

	public event Action<Keys> KeyUp;

	public GlobalKeyboardHook()
	{
		_proc = HookCallback;
	}

	public void Install()
	{
		if (_hookId != IntPtr.Zero)
		{
			return;
		}
		using Process process = Process.GetCurrentProcess();
		using ProcessModule processModule = process.MainModule;
		_hookId = SetWindowsHookEx(13, _proc, GetModuleHandle(processModule.ModuleName), 0u);
	}

	public void Uninstall()
	{
		if (_hookId != IntPtr.Zero)
		{
			UnhookWindowsHookEx(_hookId);
			_hookId = IntPtr.Zero;
		}
	}

	private nint HookCallback(int nCode, nint wParam, nint lParam)
	{
		if (nCode >= 0)
		{
			Keys vkCode = (Keys)Marshal.PtrToStructure<KbdLlHookStruct>(lParam).vkCode;
			switch ((int)wParam)
			{
			case 256:
			case 260:
				KeyDown?.Invoke(vkCode);
				break;
			case 257:
			case 261:
				KeyUp?.Invoke(vkCode);
				break;
			}
		}
		return CallNextHookEx(_hookId, nCode, wParam, lParam);
	}

	public void Dispose()
	{
		Uninstall();
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(nint hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern nint GetModuleHandle(string lpModuleName);
}
