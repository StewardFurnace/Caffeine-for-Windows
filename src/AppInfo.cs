using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Caffeine for Windows")]
[assembly: AssemblyDescription("Keeps your PC awake: no sleep, no screen off, no auto-lock.")]
[assembly: AssemblyProduct("Caffeine for Windows")]
[assembly: AssemblyCompany("Caffeine for Windows contributors")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Caffeine for Windows contributors. MIT License.")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: ComVisible(false)]

namespace CaffeineForWindows
{
    static class AppInfo
    {
        public const string Name = "Caffeine for Windows";
        public const string Version = "1.0.0";

        // Project page opened from the tray menu. Change it if the project lives elsewhere.
        public const string Website = "https://sourceforge.net/projects/caffeine-for-windows/";
    }
}
