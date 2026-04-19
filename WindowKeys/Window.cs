namespace WindowKeys;

internal class Window
{
    public required nint Handle { get; init; }
    public required Native.RECT Rect { get; init; }
    public required nint InsertAfter { get; init; }
    public string ActivationString { get; set; } = string.Empty;
    public OverlayForm? OverlayForm { get; set; }
    public bool Dismissed { get; private set; }

    public void DismissOverlay()
    {
        OverlayForm?.Hide();
        Dismissed = true;
    }
}
