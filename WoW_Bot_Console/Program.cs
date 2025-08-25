using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("4 sn hazırlık süresi... oyuna geçebilirsin.");
        await Task.Delay(TimeSpan.FromSeconds(3));

        Console.WriteLine("3 sn hazırlık süresi... Notepad'e geç!");
        await Task.Delay(3000);

        Keyboard.TapO();
        await Task.Delay(120);
        Keyboard.TapP();
        await Task.Delay(120);
        Keyboard.TapO();
        await Task.Delay(120);
        Keyboard.TapP();

        Console.WriteLine("Bitti. Notepad'e O P O P yazması lazım.");

        Console.WriteLine("Bitti. Notepad’e O P O P yazması lazım.");

        // --- Klavye testi: O ve P ---

        // --- Klavye testi: '*' sonra '0 0 0' ---
        await Task.Delay(50);
        Keyboard.TapO();
        await Task.Delay(120);
        Keyboard.TapP();
        await Task.Delay(120);
        Keyboard.TapO();
        await Task.Delay(120);
        Keyboard.TapP();

        Console.WriteLine("Klavyeden: O ve P basıldı.");

        await Task.Delay(300);
        Keyboard.TapStar();          // artık parametresiz
        await Task.Delay(120);
        Keyboard.TapDigit0();
        await Task.Delay(120);
        Keyboard.TapDigit0();
        await Task.Delay(120);
        Keyboard.TapDigit0();

        Console.WriteLine("Klavyeden: '*' ve ardından '0 0 0' basıldı.");

        try
        {
            var (x, y) = Mouse.GetCursor();
            Console.WriteLine($"Başlangıç konumu: {x},{y}");

            // Test: sağa sola küçük hareket
            Mouse.MoveTo(x + 100, y);
            await Task.Delay(400);
            Mouse.MoveTo(x - 100, y);
            await Task.Delay(400);
            Mouse.MoveTo(x, y);

            Console.WriteLine("------------Test bitti.");
        }
        catch (PlatformNotSupportedException pnse)
        {
            Console.WriteLine(pnse.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        Console.ReadKey();
    }
}

static class Mouse
{
    public static (int x, int y) GetCursor()
    {
        if (OperatingSystem.IsWindows()) return WindowsMouse.GetCursor();
        if (OperatingSystem.IsMacOS()) return MacMouse.GetCursor();
        throw new PlatformNotSupportedException("Bu platformda (ne Windows ne macOS) mouse P/Invoke uygulanmadı.");
    }

    public static void MoveTo(int x, int y)
    {
        if (OperatingSystem.IsWindows()) { WindowsMouse.MoveTo(x, y); return; }
        if (OperatingSystem.IsMacOS()) { MacMouse.MoveTo(x, y); return; }
        throw new PlatformNotSupportedException("Bu platformda (ne Windows ne macOS) mouse P/Invoke uygulanmadı.");
    }
}

#region macOS (CoreGraphics / Quartz)
static class MacMouse
{
    // CoreGraphics framework yolu
    private const string CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    // CGPoint
    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double x;
        public double y;
        public CGPoint(double x, double y) { this.x = x; this.y = y; }
    }

    private enum CGMouseButton : uint { Left = 0, Right = 1, Center = 2 }

    private enum CGEventType : uint
    {
        MouseMoved = 5,
        LeftMouseDown = 1,
        LeftMouseUp = 2,
        RightMouseDown = 3,
        RightMouseUp = 4,
        OtherMouseDown = 25,
        OtherMouseUp = 26
    }

    private enum CGEventTapLocation : uint
    {
        HID = 0,
        Session = 1,
        AnnotatedSession = 2
    }

    [DllImport(CoreGraphics)]
    private static extern IntPtr CGEventCreate(IntPtr source /* pass IntPtr.Zero */);

    [DllImport(CoreGraphics)]
    private static extern CGPoint CGEventGetLocation(IntPtr @event);

    [DllImport(CoreGraphics)]
    private static extern IntPtr CGEventCreateMouseEvent(
        IntPtr source, CGEventType mouseType, CGPoint mouseCursorPosition, CGMouseButton mouseButton);

    [DllImport(CoreGraphics)]
    private static extern void CGEventPost(CGEventTapLocation tap, IntPtr @event);

    [DllImport(CoreFoundation)]
    private static extern void CFRelease(IntPtr cf);

    public static (int x, int y) GetCursor()
    {
        IntPtr evt = CGEventCreate(IntPtr.Zero);
        if (evt == IntPtr.Zero) throw new InvalidOperationException("CGEventCreate failed.");
        try
        {
            var loc = CGEventGetLocation(evt);
            // macOS'ta koordinatlar sol-alt (origin) kabul edilebilir; pratikte bu şekilde iş görür.
            return ((int)loc.x, (int)loc.y);
        }
        finally { CFRelease(evt); }
    }

    public static void MoveTo(int x, int y)
    {
        var pos = new CGPoint(x, y);
        IntPtr evt = CGEventCreateMouseEvent(IntPtr.Zero, CGEventType.MouseMoved, pos, CGMouseButton.Left);
        if (evt == IntPtr.Zero) throw new InvalidOperationException("CGEventCreateMouseEvent failed.");
        try
        {
            CGEventPost(CGEventTapLocation.HID, evt);
        }
        finally { CFRelease(evt); }
    }
}

#endregion


