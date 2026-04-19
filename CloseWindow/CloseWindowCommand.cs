using System.Diagnostics;
using System.Runtime.InteropServices;

using AutoContracts;

namespace Auto.Plugins;

internal partial class CloseWindowCommand : ICommand
{
	public string Name => "CloseWindow";
	public string Description => "Close window by handle or process name.";
	public Guid Id { get; } = Guid.Parse("C6D6565F-D3E3-4F27-A087-9EB774297F68");
	public Type ReturnType { get; } = typeof(bool);
	public bool RequiresSta => false;

	public void Init() { }

	public List<PluginArgument> ExpectedArguments { get; } =
	[
		new()
		{
			Name = "Target",
			Type = typeof(string)
		},
		new()
		{
			Name = "Force",
			Type = typeof(bool)
		}
	];

	public object? Execute(object?[] args)
	{
		var input = (string)(args[0] ?? string.Empty);
		var force = (bool)(args[1] ?? false);

		var handle = ResolveHandle(input);
		if (handle == nint.Zero || !IsWindow(handle))
			return false;

		if (!force)
			return PostMessage(handle, WM_CLOSE, nint.Zero, nint.Zero);

		GetWindowThreadProcessId(handle, out var pid);
		if (pid == 0)
			return false;

		try
		{
			using var process = Process.GetProcessById((int)pid);
			process.Kill();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
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

	private const uint WM_CLOSE = 0x0010;

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsWindow(nint hWnd);

	[LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

	[LibraryImport("user32.dll")]
	private static partial uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);
}
