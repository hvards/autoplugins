using System.Diagnostics;
using System.Runtime.InteropServices;

using AutoContracts;

namespace Auto.Plugins;

internal partial class FocusWindowCommand : ICommand
{
	public string Name => "FocusWindow";
	public string Description => "Focus window by handle or process name.";
	public Guid Id { get; } = Guid.Parse("5E668C73-2D6F-4432-BF11-20B15E4758A3");
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
		var input = (string)(args[0] ?? string.Empty);
		var handle = ResolveHandle(input);
		if (handle == nint.Zero || !IsWindow(handle))
			return false;

		// Simulate Alt keypress to gain foreground rights
		Span<Input> inputs =
		[
			new() { type = INPUT_KEYBOARD, u = new InputUnion { ki = new KeyboardInput { wVk = VK_MENU } } },
			new() { type = INPUT_KEYBOARD, u = new InputUnion {
				ki = new KeyboardInput { wVk = VK_MENU, dwFlags = KEYEVENTF_KEYUP } } }
		];
		SendInput(2, inputs, Marshal.SizeOf<Input>());

		var currentThread = GetCurrentThreadId();
		var foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), nint.Zero);
		AttachThreadInput(currentThread, foregroundThread, true);
		try
		{
			if (IsIconic(handle))
				ShowWindow(handle, SW_RESTORE);
			SetForegroundWindow(handle);
			BringWindowToTop(handle);
		}
		finally
		{
			AttachThreadInput(currentThread, foregroundThread, false);
		}
		return true;
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

	private const byte VK_MENU = 0x12;
	private const uint KEYEVENTF_KEYUP = 0x0002;
	private const int INPUT_KEYBOARD = 1;
	private const int SW_RESTORE = 9;

	[StructLayout(LayoutKind.Sequential)]
	private struct KeyboardInput
	{
		public ushort wVk;
		public ushort wScan;
		public uint dwFlags;
		public uint time;
		public nint dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MouseInput
	{
		public int dx;
		public int dy;
		public uint mouseData;
		public uint dwFlags;
		public uint time;
		public nint dwExtraInfo;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct InputUnion
	{
		[FieldOffset(0)] public MouseInput mi;
		[FieldOffset(0)] public KeyboardInput ki;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct Input
	{
		public int type;
		public InputUnion u;
	}

	[LibraryImport("user32.dll")]
	private static partial nint GetForegroundWindow();

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetForegroundWindow(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool BringWindowToTop(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool AttachThreadInput(uint idAttach, uint idAttachTo,
		[MarshalAs(UnmanagedType.Bool)] bool fAttach);

	[LibraryImport("user32.dll")]
	private static partial uint GetWindowThreadProcessId(nint hWnd, nint lpdwProcessId);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsWindow(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsIconic(nint hWnd);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ShowWindow(nint hWnd, int nCmdShow);

	[LibraryImport("kernel32.dll")]
	private static partial uint GetCurrentThreadId();

	[LibraryImport("user32.dll")]
	private static partial uint SendInput(uint nInputs, Span<Input> pInputs, int cbSize);
}