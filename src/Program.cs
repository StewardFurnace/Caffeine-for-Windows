using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace CaffeineForWindows
{
    sealed class Options
    {
        public bool On;
        public bool Off;
        public int Minutes;
    }

    static class Program
    {
        const string Usage =
            "Usage: Caffeine.exe [-on | -off] [-minutes N]\n\n" +
            "  -on            Keep the PC awake right after start\n" +
            "  -off           Start in the tray without keeping the PC awake\n" +
            "  -minutes N     Keep the PC awake for N minutes, then allow sleep again\n\n" +
            "Without options, Caffeine follows the \"Keep awake when Caffeine starts\" setting.";

        [STAThread]
        static void Main(string[] args)
        {
            Options options;
            if (!TryParse(args, out options))
            {
                MessageBox.Show(Usage, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool createdNew;
            using (var mutex = new Mutex(true, @"Local\CaffeineForWindows.SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        AppInfo.Name + " is already running.\n\n" +
                        "Look for the coffee cup icon in the notification area. " +
                        "If you don't see it, click the ^ arrow next to the clock.",
                        AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (var app = new TrayApp(options))
                    Application.Run(app);
            }
        }

        static bool TryParse(string[] args, out Options options)
        {
            options = new Options();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].TrimStart('-', '/').ToLowerInvariant();
                switch (arg)
                {
                    case "on":
                        options.On = true;
                        break;
                    case "off":
                        options.Off = true;
                        break;
                    case "minutes":
                    case "m":
                        int minutes;
                        if (i + 1 >= args.Length ||
                            !int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes) ||
                            minutes <= 0)
                            return false;
                        options.Minutes = minutes;
                        break;
                    default:
                        return false;
                }
            }
            return !(options.Off && (options.On || options.Minutes > 0));
        }
    }
}
