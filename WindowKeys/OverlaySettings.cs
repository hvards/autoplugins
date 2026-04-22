namespace WindowKeys;

internal class OverlaySettings
{
	public string FontFamily { get; init; } = "Consolas";
	public int FontSize { get; init; } = 48;
	public string BackgroundColor { get; init; } = "#0E0E0E";
	public string BorderColor { get; init; } = "#87CEEB";
	public int BorderWidth { get; init; } = 5;
	public int CornerRadius { get; init; } = 14;

	public static OverlaySettings Default { get; } = new();
}