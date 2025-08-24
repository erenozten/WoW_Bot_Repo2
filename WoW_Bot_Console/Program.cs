using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;

record ClickPoints((int X, int Y) Star, (int X, int Y) Auctioning);

class Program
{
    static string ConfigPath => Path.Combine(AppContext.BaseDirectory, "clicks.json");

    static void Main(string[] args)
    {
        Console.WriteLine("8 sn hazırlık süresi... oyuna geçebilirsin.");

        // === TEST: Maus sağa sola hareket ===
        var mp = new MousePlayer();
        var cur = mp.GetCursor();

        for (int i = 0; i < 3; i++)
        {
            // 100 px sağa
            var right = (cur.X + 100, cur.Y);
            mp.MoveHumanLike(cur, right, 600);
            Thread.Sleep(500);

            // geri sola
            mp.MoveHumanLike(right, cur, 600);
            Thread.Sleep(500);
        }
        // === TEST BİTTİ ===


        Thread.Sleep(8000);   // <- burada bekliyor

        if (args.Length == 0)
        {
            Console.WriteLine("Kullanım:");
            Console.WriteLine("  dotnet run -- calibrate   # Yıldız ve Auctioning için koordinat yakala");
            Console.WriteLine("  dotnet run -- run         # Kaydedilenlere göre uygula (yıldız -> 0 -> Auctioning)");
            return;
        }

        if (args[0].Equals("calibrate", StringComparison.OrdinalIgnoreCase))
        {
            Calibrate();
            return;
        }

        if (args[0].Equals("run", StringComparison.OrdinalIgnoreCase))
        {
            if (!File.Exists(ConfigPath))
            {
                Console.WriteLine("clicks.json yok. Önce 'calibrate' çalıştır.");
                return;
            }
            var cfg = JsonSerializer.Deserialize<ClickPoints>(File.ReadAllText(ConfigPath))!;
            RunSequence(cfg);
            return;
        }

        Console.WriteLine("Bilinmeyen komut.");
    }

    static void Calibrate()
    {
        Console.WriteLine("Kalibrasyon başlıyor. Oyun penceresi açık kalsın.");
        var star = CapturePoint("YILDIZ (imleci yıldız üstüne getir)", seconds: 5);
        var auc = CapturePoint("AUCTIONING (imleci 'Auctioning' sekmesine getir)", seconds: 5);
        var cfg = new ClickPoints(star, auc);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Kaydedildi: {ConfigPath}");
    }

    static (int X, int Y) CapturePoint(string prompt, int seconds)
    {
        Console.Write($"{prompt} -> {seconds} sn içinde yakalanacak: ");
        for (int i = seconds; i >= 1; i--)
        {
            Console.Write($"{i} "); Thread.Sleep(1000);
        }
        Console.WriteLine();
        Win.GetCursorPos(out var p);
        Console.WriteLine($"Yakalandı: X={p.X}, Y={p.Y}");
        return (p.X, p.Y);
    }

    static void RunSequence(ClickPoints cfg)
    {
        var mp = new MousePlayer();

        Console.WriteLine("3 sn sonra başlıyor...");
        Thread.Sleep(3000);

        // 1) Yıldız
        var start = mp.GetCursor();
        var dur1 = MousePlayer.EstimateDurationMs(start, cfg.Star);
        mp.MoveHumanLike(start, cfg.Star, dur1);
        Thread.Sleep(Random.Shared.Next(40, 140));
        mp.ClickLeft();

        // 2) '0' tuşu
        Thread.Sleep(Random.Shared.Next(120, 280));
        KeySender.Tap('0'); // üst sıra 0

        // 3) Auctioning
        Thread.Sleep(Random.Shared.Next(180, 360));
        start = mp.GetCursor();
        var dur2 = MousePlayer.EstimateDurationMs(start, cfg.Auctioning);
        mp.MoveHumanLike(start, cfg.Auctioning, dur2);
        Thread.Sleep(Random.Shared.Next(40, 140));
        mp.ClickLeft();

        Console.WriteLine("Bitti.-----------------------------------");
    }

    // --- Mouse & Keyboard helpers ---

    static class KeySender
    {
        const int INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;

        public static void Tap(char c)
        {
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
                _ => throw new NotSupportedException($"Desteklenmeyen tuş: {c}")
            };

            Win.INPUT down = new() { type = INPUT_KEYBOARD, U = new Win.InputUnion { ki = new Win.KEYBDINPUT { wVk = vk } } };
            Win.INPUT up = new() { type = INPUT_KEYBOARD, U = new Win.InputUnion { ki = new Win.KEYBDINPUT { wVk = vk, dwFlags = KEYEVENTF_KEYUP } } };
            Win.SendInput(1, new[] { down }, Marshal.SizeOf<Win.INPUT>());
            Thread.Sleep(Random.Shared.Next(30, 90));
            Win.SendInput(1, new[] { up }, Marshal.SizeOf<Win.INPUT>());
        }
    }

    class MousePlayer
    {
        readonly Random _rng = new();

        public (int X, int Y) GetCursor() { Win.GetCursorPos(out var p); return (p.X, p.Y); }

        public static int EstimateDurationMs((int X, int Y) start, (int X, int Y) target)
        {
            double dx = target.X - start.X, dy = target.Y - start.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            int d = (int)(120 + 90 * Math.Log2(dist / 12.0 + 1));
            return Math.Max(120, d + Random.Shared.Next(-40, 60));
        }

        public void MoveHumanLike((int X, int Y) start, (int X, int Y) target, int approxMs)
        {
            var (x0, y0) = start; var (x2, y2) = target;
            int dx = x2 - x0, dy = y2 - y0;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double nx = len < 1 ? 0 : dx / len, ny = len < 1 ? 0 : dy / len;

            int ctrlDist = (int)Math.Clamp(len * 0.3 + _rng.Next(-20, 20), 20, 300);
            int cx = x0 + (int)(nx * ctrlDist) + _rng.Next(-15, 15);
            int cy = y0 + (int)(ny * ctrlDist) + _rng.Next(-15, 15);

            bool overshoot = _rng.NextDouble() < 0.2;
            if (overshoot) { x2 += _rng.Next(-8, 9); y2 += _rng.Next(-8, 9); }

            int steps = Math.Max(approxMs / 8, 30); // ~120Hz
            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;
                double s = t * t * t * (10 + t * (-15 + 6 * t)); // minimum-jerk
                double bx = (1 - s) * (1 - s) * x0 + 2 * (1 - s) * s * cx + s * s * x2;
                double by = (1 - s) * (1 - s) * y0 + 2 * (1 - s) * s * cy + s * s * y2;

                int jx = _rng.Next(-2, 3), jy = _rng.Next(-2, 3);
                Win.SetCursorPos((int)bx + jx, (int)by + jy);
                Thread.Sleep(8);
            }
            if (overshoot)
            {
                Thread.Sleep(_rng.Next(20, 60));
                Win.SetCursorPos(target.X, target.Y);
            }
        }

        public void ClickLeft()
        {
            var down = new Win.INPUT { type = Win.INPUT_MOUSE, U = new Win.InputUnion { mi = new Win.MOUSEINPUT { dwFlag = Win.MOUSEEVENTF_LEFTDOWN } } };
            var up = new Win.INPUT { type = Win.INPUT_MOUSE, U = new Win.InputUnion { mi = new Win.MOUSEINPUT { dwFlag = Win.MOUSEEVENTF_LEFTUP } } };
            Win.SendInput(1, new[] { down }, Marshal.SizeOf<Win.INPUT>());
            Thread.Sleep(_rng.Next(40, 120));
            Win.SendInput(1, new[] { up }, Marshal.SizeOf<Win.INPUT>());
        }
    }

    static class Win
    {
        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X; public int Y; }
        [StructLayout(LayoutKind.Sequential)] public struct INPUT { public int type; public InputUnion U; }
        [StructLayout(LayoutKind.Explicit)] public struct InputUnion { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; }
        [StructLayout(LayoutKind.Sequential)] public struct MOUSEINPUT { public int dx, dy; public uint mouseData, dwFlag, time; public IntPtr dwExtraInfo; }
        [StructLayout(LayoutKind.Sequential)] public struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

        [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")] public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }
}
