using System;

static class Keyboard
{
    // '*' için Shift+8 (scan code 0x09)
    public static void TapStar()
    {
        if (OperatingSystem.IsWindows())
        {
            WindowsKeyboard.TapWithShiftScan(0x09); // 8 tuşu
            return;
        }
        throw new PlatformNotSupportedException("Sadece Windows için eklendi.");
    }

    // '0' için üst sıra (scan code 0x0B)
    public static void TapDigit0()
    {
        if (OperatingSystem.IsWindows())
        {
            WindowsKeyboard.TapScanCode(0x0B);
            return;
        }
        throw new PlatformNotSupportedException("Sadece Windows için eklendi.");
    }

    // O harfi (scan code 0x18)
    public static void TapO()
    {
        if (OperatingSystem.IsWindows())
        {
            WindowsKeyboard.TapScanCode(0x18);
            return;
        }
        throw new PlatformNotSupportedException("Sadece Windows için eklendi.");
    }

    // P harfi (scan code 0x19)
    public static void TapP()
    {
        if (OperatingSystem.IsWindows())
        {
            WindowsKeyboard.TapScanCode(0x19);
            return;
        }
        throw new PlatformNotSupportedException("Sadece Windows için eklendi.");
    }
}
