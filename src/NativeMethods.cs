using System;
using System.Runtime.InteropServices;

namespace CaffeineForWindows
{
    static class NativeMethods
    {
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;

        const byte VK_F15 = 0x7E;
        const uint KEYEVENTF_KEYUP = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SetThreadExecutionState(uint esFlags);

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        /// <summary>Milliseconds since the last keyboard or mouse input in this session.</summary>
        public static uint GetIdleMilliseconds()
        {
            var info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
            if (!GetLastInputInfo(ref info))
                return 0;
            return unchecked((uint)Environment.TickCount - info.dwTime);
        }

        /// <summary>
        /// Presses and releases F15, a key almost no keyboard has and no app reacts to.
        /// Windows counts it as user activity, which stops the screensaver, the
        /// inactivity lock and "Away" status in chat apps.
        /// </summary>
        public static void PressF15()
        {
            keybd_event(VK_F15, 0, 0, UIntPtr.Zero);
            keybd_event(VK_F15, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
