using System.Runtime.InteropServices;
using System.Text;

namespace WindowKeys;

internal static partial class Native
{
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
        public readonly int Size => (Right - Left) * (Bottom - Top);
    }

    private const uint GW_HWNDNEXT = 2;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;

    private static readonly string[] ExcludedTitles =
        ["Program Manager", "Windows Input Experience", "Default IME", "MSCTFIME UI"];

    public static List<Window> GetWindowsInZOrder()
    {
        var windows = new List<Window>();
        var nextHWnd = GetTopWindow(nint.Zero);
        var hWnd = nint.Zero;

        while (nextHWnd != nint.Zero)
        {
            var prevHWnd = hWnd;
            hWnd = nextHWnd;
            nextHWnd = GetWindow(hWnd, GW_HWNDNEXT);

            var title = GetWindowTitle(hWnd);
            if (string.IsNullOrEmpty(title) || ExcludedTitles.Contains(title)) continue;
            if (!GetVisibleRect(hWnd, out var rect) || rect.Size < 2500) continue;
            if (windows.Any(x => Geometry.IsRectInside(x.Rect, rect))) continue;

            windows.Add(new Window { Rect = rect, Handle = hWnd, InsertAfter = prevHWnd });
        }

        return windows;
    }

    public static void ZOrderInsertWindowAfter(nint windowHandle, nint hWndInsertAfter) =>
        SetWindowPos(windowHandle, hWndInsertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

    private static bool GetVisibleRect(nint hWnd, out RECT rect)
    {
        rect = default;
        if (!IsWindowVisible(hWnd)) return false;
        if (!GetWindowRect(hWnd, out rect)) return false;
        DwmGetWindowAttribute(hWnd, 14, out var cloaked, Marshal.SizeOf<int>());
        return cloaked <= 0;
    }

    private static string GetWindowTitle(nint hWnd)
    {
        var length = GetWindowTextLength(hWnd);
        var sb = new StringBuilder(length + 1);
        GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    [LibraryImport("user32.dll")]
    private static partial nint GetTopWindow(nint hWnd);

    [LibraryImport("user32.dll")]
    private static partial nint GetWindow(nint hWnd, uint uCmd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(nint hWnd, out RECT lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowTextLengthW")]
    private static partial int GetWindowTextLength(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy,
        uint uFlags);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmGetWindowAttribute(nint hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

    public delegate nint LowLevelKeyboardProc(int nCode, nint wParam, ref KeyboardInput lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }

    private const int WH_KEYBOARD_LL = 13;
    public const nint WM_KEYDOWN = 0x0100;
    public const nint WM_SYSKEYDOWN = 0x0104;

    public static nint SetKeyboardHook(LowLevelKeyboardProc proc)
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        using var module = process.MainModule;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(module!.ModuleName), 0);
    }

    public static bool RemoveKeyboardHook(nint hookId) => UnhookWindowsHookEx(hookId);

    public static nint CallNextHook(nint hookId, int nCode, nint wParam, ref KeyboardInput lParam)
        => CallNextHookEx(hookId, nCode, wParam, ref lParam);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW")]
    private static partial nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnhookWindowsHookEx(nint hhk);

    [LibraryImport("user32.dll")]
    private static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, ref KeyboardInput lParam);

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint GetModuleHandle(string lpModuleName);
}
