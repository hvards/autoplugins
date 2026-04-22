using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace WindowKeys;

internal class OverlayForm : Form
{
	private readonly OverlaySettings _settings;
	private string _activationString = string.Empty;
	private Native.RECT _rect;
	private IReadOnlyList<Native.RECT> _occludingRects = [];
	private readonly Font _font;
	private Size _textSize;

	public OverlayForm(OverlaySettings settings)
	{
		_settings = settings;
		_font = new Font(settings.FontFamily, settings.FontSize);
		FormBorderStyle = FormBorderStyle.None;
		StartPosition = FormStartPosition.Manual;
		ShowInTaskbar = false;
		TopMost = true;
		BackColor = ColorTranslator.FromHtml(settings.BackgroundColor);
	}

	public bool Configure(Native.RECT rect, string activationString, IReadOnlyList<Native.RECT> occludingRects)
	{
		_rect = rect;
		_activationString = activationString;
		_occludingRects = occludingRects;
		return LayoutOverlay();
	}

	public void UpdateActivationString(string activationString)
	{
		_activationString = activationString;
		using var g = CreateGraphics();
		_textSize = TextRenderer.MeasureText(g, _activationString, _font, Size.Empty, TextFormatFlags.NoPadding);
		Invalidate();
	}

	protected override CreateParams CreateParams
	{
		get
		{
			var cp = base.CreateParams;
			cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
			return cp;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_font.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		DrawBorder(e.Graphics);
		DrawActivationString(e.Graphics);
	}

	private bool LayoutOverlay()
	{
		using var g = CreateGraphics();
		_textSize = TextRenderer.MeasureText(g, _activationString, _font, Size.Empty, TextFormatFlags.NoPadding);
		var position = Geometry.GetActivationStringPosition(_rect, _occludingRects, _textSize);
		if (position == null) return false;

		var w = _textSize.Width + 24 + _settings.BorderWidth + 1;
		var h = _textSize.Height + _settings.BorderWidth + 1;
		Size = new Size(w, h);
		Location = new Point(position.Value.X - w / 2, position.Value.Y - h / 2);

		var inset = _settings.BorderWidth / 2f;
		var radius = new SizeF(_settings.CornerRadius, _settings.CornerRadius);
		var rect = new RectangleF(inset, inset,
			ClientSize.Width - _settings.BorderWidth - 1,
			ClientSize.Height - _settings.BorderWidth - 1);

		using var path = new GraphicsPath();
		path.AddRoundedRectangle(rect, radius);
		var oldRegion = Region;
		Region = new Region(path);
		oldRegion?.Dispose();
		return true;
	}

	private void DrawBorder(Graphics graphics)
	{
		using var pen = new Pen(ColorTranslator.FromHtml(_settings.BorderColor), _settings.BorderWidth);

		float bw = pen.Width;
		float inset = bw / 2f;

		var rect = new RectangleF(
			inset,
			inset,
			ClientSize.Width - bw - 1f,
			ClientSize.Height - bw - 1f);

		var radius = new SizeF(_settings.CornerRadius, _settings.CornerRadius);
		graphics.DrawRoundedRectangle(pen, rect, radius);
	}

	private void DrawActivationString(Graphics graphics)
	{
		var position = new Point((Width - _textSize.Width) / 2, (Height - _textSize.Height) / 2);
		TextRenderer.DrawText(graphics, _activationString, _font, position, Color.White, TextFormatFlags.NoPadding);
	}
}