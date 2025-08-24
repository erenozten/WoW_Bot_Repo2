using System;
using System.Runtime.InteropServices;

static class WindowsMouse
{
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    private const uint MOUSEEVENTF_LEFTUP   = 0x04;

    public static (int x, int y) GetCursor()
    {
        if (!GetCursorPos(out var p)) throw new InvalidOperationException("GetCursorPos failed.");
        return (p.X, p.Y);
    }

    public static void MoveTo(int x, int y)
    {
        if (!SetCursorPos(x, y)) throw new InvalidOperationException("SetCursorPos failed.");
    }

    public static void LeftClick()
    {
        GetCursorPos(out var p);
        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, p.X, p.Y, 0, IntPtr.Zero);
    }
}
