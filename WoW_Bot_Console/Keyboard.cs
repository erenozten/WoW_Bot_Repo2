static class Keyboard
{
    // '*' için iki seçenek: Numpad '*' (VK_MULTIPLY) veya Shift+8
    public static void TapStar(bool preferNumpad = false)
    {
        if (OperatingSystem.IsWindows())
        {
            if (preferNumpad) WindowsKeyboard.TapVk(0x6A);           // VK_MULTIPLY
            else WindowsKeyboard.TapWithShift(0x38);                 // '8' + Shift  (VK_8)
            return;
        }
        throw new PlatformNotSupportedException("Klavye P/Invoke sadece Windows için eklendi.");
    }

    // '0' (üst sıra)
    public static void TapDigit0()
    {
        if (OperatingSystem.IsWindows()) { WindowsKeyboard.TapVk(0x30); return; } // VK_0
        throw new PlatformNotSupportedException("Klavye P/Invoke sadece Windows için eklendi.");
    }

    // İstersen tek tek harf/rakam göndermek için:
    public static void TapChar(char c)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Klavye P/Invoke sadece Windows için eklendi.");

        byte vk = c switch
        {
            '0' => 0x30,
            '1' => 0x31,
            '2' => 0x32,
            '3' => 0x33,
            '4' => 0x34,
            '5' => 0x35,
            '6' => 0x36,
            '7' => 0x37,
            '8' => 0x38,
            '9' => 0x39,
            _ => throw new NotSupportedException($"Desteklenmeyen karakter: {c}")
        };
        WindowsKeyboard.TapVk(vk);
    }
}


