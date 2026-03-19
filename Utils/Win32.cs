using System.Runtime.InteropServices;

namespace CrossHairPlus.Utils;

public static class Win32
{
    public const int WM_NCHITTEST = 0x0084;
    public const int HTTRANSPARENT = -1;

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_NOREPEAT = 0x4000;

    public const uint VK_H = 0x48;

    public const int HOTKEY_TOGGLE = 1;
    public const int HOTKEY_RESET = 2;
}