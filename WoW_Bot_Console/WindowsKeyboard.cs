using System.Runtime.InteropServices;

static class WindowsKeyboard
{
    private const int INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const ushort VK_SHIFT = 0x10;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT { public int type; public InputUnion U; }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll")] private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    private static void KeyDown(ushort vk)
    {
        var input = new INPUT { type = INPUT_KEYBOARD, U = new InputUnion { ki = new KEYBDINPUT { wVk = vk } } };
        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    private static void KeyUp(ushort vk)
    {
        var input = new INPUT { type = INPUT_KEYBOARD, U = new InputUnion { ki = new KEYBDINPUT { wVk = vk, dwFlags = KEYEVENTF_KEYUP } } };
        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    public static void TapVk(byte vk)
    {
        KeyDown(vk);
        Task.Delay(Random.Shared.Next(30, 90)).Wait();
        KeyUp(vk);
    }

    public static void TapWithShift(byte vkBase)
    {
        KeyDown(VK_SHIFT);
        Task.Delay(Random.Shared.Next(20, 40)).Wait();
        TapVk(vkBase);
        Task.Delay(Random.Shared.Next(20, 40)).Wait();
        KeyUp(VK_SHIFT);
    }
}


