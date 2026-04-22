namespace WindowKeys;

internal sealed class KeyboardHookScope : IDisposable
{
	// To avoid garbage collection
	private readonly Native.LowLevelKeyboardProc _callback;
	private nint _hookId;

	public KeyboardHookScope(Native.LowLevelKeyboardProc callback)
	{
		_callback = callback;
		_hookId = Native.SetKeyboardHook(_callback);
	}

	public nint HookId => _hookId;

	public void Dispose()
	{
		if (_hookId == nint.Zero) return;
		Native.RemoveKeyboardHook(_hookId);
		_hookId = nint.Zero;
	}
}