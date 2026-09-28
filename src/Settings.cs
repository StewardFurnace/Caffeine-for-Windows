using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CaffeineForWindows
{
    /// <summary>User preferences, stored under HKCU\Software\CaffeineForWindows.</summary>
    sealed class Settings
    {
        const string KeyPath = @"Software\CaffeineForWindows";

        public bool KeepDisplayOn = true;
        public bool PreventLock;
        public bool ActivateOnStartup = true;
        public bool FirstRunDone;

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(KeyPath))
                {
                    if (key == null)
                        return s;
                    s.KeepDisplayOn = ReadBool(key, "KeepDisplayOn", s.KeepDisplayOn);
                    s.PreventLock = ReadBool(key, "PreventLock", s.PreventLock);
                    s.ActivateOnStartup = ReadBool(key, "ActivateOnStartup", s.ActivateOnStartup);
                    s.FirstRunDone = ReadBool(key, "FirstRunDone", s.FirstRunDone);
                }
            }
            catch (Exception)
            {
                // Unreadable settings are not fatal: fall back to defaults.
            }
            return s;
        }

        public void Save()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(KeyPath))
                {
                    key.SetValue("KeepDisplayOn", KeepDisplayOn ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("PreventLock", PreventLock ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("ActivateOnStartup", ActivateOnStartup ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("FirstRunDone", FirstRunDone ? 1 : 0, RegistryValueKind.DWord);
                }
            }
            catch (Exception)
            {
                // Settings simply won't persist; the app keeps working.
            }
        }

        static bool ReadBool(RegistryKey key, string name, bool fallback)
        {
            object value = key.GetValue(name);
            return value is int ? (int)value != 0 : fallback;
        }
    }

    /// <summary>"Start with Windows" via the per-user Run key (no admin rights needed).</summary>
    static class Autostart
    {
        const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string ValueName = "Caffeine for Windows";

        static string Command
        {
            get { return "\"" + Application.ExecutablePath + "\""; }
        }

        public static bool IsEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath))
                {
                    var value = key == null ? null : key.GetValue(ValueName) as string;
                    return string.Equals(value, Command, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void Set(bool enabled)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
            {
                if (enabled)
                    key.SetValue(ValueName, Command, RegistryValueKind.String);
                else
                    key.DeleteValue(ValueName, false);
            }
        }
    }
}
