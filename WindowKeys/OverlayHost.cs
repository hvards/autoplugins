namespace WindowKeys;

internal sealed class OverlayHost
{
	private static readonly char[] SelectionKeys = ['A', 'R', 'S', 'T', 'N', 'E', 'I', 'O'];

	private readonly OverlaySettings _settings;
	private readonly List<OverlayForm> _overlayForms = [];
	private readonly Form _dialog;

	private List<Window> _windows = [];
	private nint _hookId;
	private string _input = "";
	private bool _active;
	private string? _selectedHandle;

	public OverlayHost(OverlaySettings? settings = null)
	{
		_settings = settings ?? OverlaySettings.Default;
		_dialog = new Form
		{
			FormBorderStyle = FormBorderStyle.None,
			ShowInTaskbar = false,
			Size = new Size(0, 0),
			Opacity = 0
		};
		_ = _dialog.Handle;
	}

	public string? RunPicker()
	{
		_windows = Native.GetWindowsInZOrder();
		if (_windows.Count == 0) return null;

		_input = "";
		_active = true;
		_selectedHandle = null;

		ShowOverlays();
		using var hook = new KeyboardHookScope(KeyboardHookCallback);
		_hookId = hook.HookId;

		_dialog.ShowDialog();

		return _selectedHandle;
	}

	private nint KeyboardHookCallback(int nCode, nint wParam, ref Native.KeyboardInput lParam)
	{
		if (nCode < 0 || !_active)
			return Native.CallNextHook(_hookId, nCode, wParam, ref lParam);

		if (wParam is not (Native.WM_KEYDOWN or Native.WM_SYSKEYDOWN))
			return Native.CallNextHook(_hookId, nCode, wParam, ref lParam);

		var vkCode = lParam.wVk;

		if (vkCode == (ushort)Keys.Escape)
		{
			Finish(null);
			return 1;
		}

		var key = ((Keys)vkCode).ToString();
		if (key.Length != 1 || !SelectionKeys.Contains(key[0]))
			return 1;

		_input += key;

		var match = false;
		foreach (var window in _windows.Where(x => !x.Dismissed))
		{
			if (window.ActivationString.StartsWith(_input))
			{
				if (window.ActivationString == _input)
				{
					Finish(window.Handle.ToString());
					return 1;
				}
				window.OverlayForm?.UpdateActivationString(window.ActivationString[_input.Length..]);
				match = true;
				continue;
			}
			window.DismissOverlay();
		}

		if (!match) Finish(null);
		return 1;
	}

	private void Finish(string? handle)
	{
		if (!_active) return;
		_active = false;
		_selectedHandle = handle;

		foreach (var win in _windows.Where(x => !x.Dismissed))
			win.DismissOverlay();

		_dialog.Close();
	}

	private void ShowOverlays()
	{
		var combinations = CombinationGenerator.Generate(_windows.Count, SelectionKeys);

		while (_overlayForms.Count < _windows.Count)
			_overlayForms.Add(new OverlayForm(_settings));

		for (var i = _windows.Count; i < _overlayForms.Count; i++)
			_overlayForms[i].Hide();

		var cIndex = 0;
		var formIndex = 0;
		foreach (var win in SortedByPosition(_windows))
		{
			win.ActivationString = combinations[cIndex++];
			var occludingRects = Geometry.GetOccludingRects(
				win.Rect,
				_windows.TakeWhile(w => w != win).Select(w => w.Rect));
			var form = _overlayForms[formIndex++];
			if (form.Configure(win.Rect, win.ActivationString, occludingRects))
			{
				form.Show();
				Native.ZOrderInsertWindowAfter(form.Handle, win.InsertAfter);
			}
			else
			{
				form.Hide();
			}
			win.OverlayForm = form;
		}
	}

	private static IEnumerable<Window> SortedByPosition(IEnumerable<Window> windows) =>
		windows.OrderBy(x => x.Rect.Left)
			.ThenBy(x => x.Rect.Bottom)
			.ThenBy(x => x.Rect.Right)
			.ThenBy(x => x.Rect.Top);
}