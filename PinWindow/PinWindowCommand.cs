using System.Diagnostics;
using System.Runtime.InteropServices;

using AutoContracts;

namespace Auto.Plugins;

internal partial class PinWindowCommand : ICommand
{
	public string Name => "PinWindow";
	public string Description => "Pin window always-on-top, by handle or process name.";
	public Guid Id { get; } = Guid.Parse("B7F4A8C2-1E5D-4F39-A6B2-3C8D9E1F7A04");
	public Type ReturnType { get; } = typeof(bool);
	public bool RequiresSta => false;

	public void Init() { }

	public List<PluginArgument> ExpectedArguments { get; } =
	[
		new()
		{
			Name = "Target",
			Type = typeof(string)
		}
	];

	public object? Execute(object?[] args)
	{
		if (args.Length == 0 || args[0] is not string input || string.IsNullOrWhiteSpace(input))
			return false;

		var handle = ResolveHandle(input);
		if (handle == nint.Zero || !IsWindow(handle))
			return false;

		var pinned = (GetWindowLongPtr(handle, GWL_EXSTYLE).ToInt64() & WS_EX_TOPMOST) == 0;
		var posOk = SetWindowPos(handle, pinned ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0,
			SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

		var color = pinned ? GREEN_COLORREF : DWMWA_COLOR_DEFAULT;
		DwmSetWindowAttribute(handle, DWMWA_BORDER_COLOR, ref color, sizeof(uint));

		return posOk;
	}

	private static nint ResolveHandle(string input)
	{
		if (nint.TryParse(input, out var handle))
			return handle;

		var name = input.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
			? input[..^4]
			: input;

		foreach (var process in Process.GetProcessesByName(name))
		{
			using var _ = process;
			if (process.MainWindowHandle != nint.Zero)
				return process.MainWindowHandle;
		}
		return nint.Zero;
	}

	private const nint HWND_TOPMOST = -1;
	private const nint HWND_NOTOPMOST = -2;
	private const int GWL_EXSTYLE = -20;
	private const long WS_EX_TOPMOST = 0x00000008;
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_NOMOVE = 0x0002;
	private const uint SWP_NOACTIVATE = 0x0010;
	private const uint DWMWA_BORDER_COLOR = 34;
	private const uint DWMWA_COLOR_DEFAULT = 0xFFFFFFFF;
	private const uint GREEN_COLORREF = 0x0000FF00;

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsWindow(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetWindowPos(nint hWnd, nint hWndInsertAfter,
		int X, int Y, int cx, int cy, uint uFlags);

	[LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
	private static partial nint GetWindowLongPtr(nint hWnd, int nIndex);

	[LibraryImport("dwmapi.dll")]
	private static partial int DwmSetWindowAttribute(nint hwnd, uint attr,
		ref uint pvAttribute, uint cbAttribute);
}
