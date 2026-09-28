using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CaffeineForWindows
{
    /// <summary>
    /// The whole app: a tray icon that keeps the PC awake while it is "on".
    /// Left click toggles, right click opens the menu.
    /// </summary>
    sealed class TrayApp : ApplicationContext
    {
        const uint PreventLockIdleMs = 59000;

        static readonly int[] DurationMinutes = { 0, 15, 30, 60, 120, 240, 480 };

        readonly Settings settings;
        readonly Icon iconOn;
        readonly Icon iconOff;
        readonly NotifyIcon tray;
        readonly ContextMenuStrip menu;
        readonly Timer timer;

        ToolStripMenuItem activeItem;
        ToolStripMenuItem durationItem;
        ToolStripMenuItem displayItem;
        ToolStripMenuItem preventLockItem;
        ToolStripMenuItem autostartItem;
        ToolStripMenuItem activateOnStartupItem;

        bool active;
        int activeMinutes;      // 0 = until turned off
        DateTime activeUntil;   // UTC, only meaningful when activeMinutes > 0

        public TrayApp(Options options)
        {
            settings = Settings.Load();

            int size = SystemInformation.SmallIconSize.Width;
            iconOn = IconFactory.CreateIcon(size, true);
            iconOff = IconFactory.CreateIcon(size, false);

            menu = BuildMenu();
            tray = new NotifyIcon();
            tray.ContextMenuStrip = menu;
            tray.Icon = iconOff;
            tray.MouseClick += OnTrayClick;
            tray.Visible = true;

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += OnTick;
            timer.Start();

            bool startActive = !options.Off && (options.On || options.Minutes > 0 || settings.ActivateOnStartup);
            if (startActive)
                Activate(options.Minutes);
            else
                Deactivate();

            if (!settings.FirstRunDone)
            {
                tray.ShowBalloonTip(8000, AppInfo.Name,
                    "Caffeine lives here in the notification area.\n" +
                    "Click the cup to turn it on or off. Right-click for options.",
                    ToolTipIcon.Info);
                settings.FirstRunDone = true;
                settings.Save();
            }
        }

        ContextMenuStrip BuildMenu()
        {
            var strip = new ContextMenuStrip();

            activeItem = new ToolStripMenuItem("Keep PC awake", null, delegate { Toggle(); });
            activeItem.Font = new Font(activeItem.Font, FontStyle.Bold);

            durationItem = new ToolStripMenuItem("Keep awake for");
            foreach (int minutes in DurationMinutes)
            {
                int m = minutes;
                var item = new ToolStripMenuItem(DurationLabel(m), null, delegate { Activate(m); });
                item.Tag = m;
                durationItem.DropDownItems.Add(item);
            }

            displayItem = new ToolStripMenuItem("Keep display on", null, delegate
            {
                settings.KeepDisplayOn = !settings.KeepDisplayOn;
                settings.Save();
                if (active)
                    ApplyExecutionState();
                UpdateUi();
            });

            preventLockItem = new ToolStripMenuItem("Prevent auto-lock and \"Away\" status", null, delegate
            {
                settings.PreventLock = !settings.PreventLock;
                settings.Save();
                UpdateUi();
            });
            preventLockItem.ToolTipText =
                "After 59 seconds without input, Caffeine presses F15 (a key no app uses),\n" +
                "so the screensaver, the lock screen and chat apps see you as active.";

            autostartItem = new ToolStripMenuItem("Start with Windows", null, delegate
            {
                try
                {
                    Autostart.Set(!Autostart.IsEnabled());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not change the startup setting:\n" + ex.Message,
                        AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                UpdateUi();
            });

            activateOnStartupItem = new ToolStripMenuItem("Keep awake when Caffeine starts", null, delegate
            {
                settings.ActivateOnStartup = !settings.ActivateOnStartup;
                settings.Save();
                UpdateUi();
            });

            strip.Items.Add(activeItem);
            strip.Items.Add(durationItem);
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(displayItem);
            strip.Items.Add(preventLockItem);
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(autostartItem);
            strip.Items.Add(activateOnStartupItem);
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(new ToolStripMenuItem("Project website", null, delegate { OpenWebsite(); }));
            strip.Items.Add(new ToolStripMenuItem("About " + AppInfo.Name, null, delegate { ShowAbout(); }));
            strip.Items.Add(new ToolStripMenuItem("Exit", null, delegate { ExitThread(); }));

            strip.Opening += delegate { UpdateUi(); };
            return strip;
        }

        void OnTrayClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Toggle();
        }

        void Toggle()
        {
            if (active)
                Deactivate();
            else
                Activate(0);
        }

        void Activate(int minutes)
        {
            active = true;
            activeMinutes = minutes;
            if (minutes > 0)
                activeUntil = DateTime.UtcNow.AddMinutes(minutes);
            ApplyExecutionState();
            UpdateUi();
        }

        void Deactivate()
        {
            active = false;
            activeMinutes = 0;
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
            UpdateUi();
        }

        void ApplyExecutionState()
        {
            uint flags = NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED;
            if (settings.KeepDisplayOn)
                flags |= NativeMethods.ES_DISPLAY_REQUIRED;
            NativeMethods.SetThreadExecutionState(flags);
        }

        void OnTick(object sender, EventArgs e)
        {
            if (!active)
                return;

            if (activeMinutes > 0 && DateTime.UtcNow >= activeUntil)
            {
                Deactivate();
                tray.ShowBalloonTip(5000, AppInfo.Name,
                    "Time is up. Your PC can sleep again.", ToolTipIcon.Info);
                return;
            }

            if (settings.PreventLock && NativeMethods.GetIdleMilliseconds() >= PreventLockIdleMs)
                NativeMethods.PressF15();

            if (activeMinutes > 0)
                UpdateTooltip();
        }

        void UpdateUi()
        {
            tray.Icon = active ? iconOn : iconOff;
            activeItem.Checked = active;
            displayItem.Checked = settings.KeepDisplayOn;
            preventLockItem.Checked = settings.PreventLock;
            autostartItem.Checked = Autostart.IsEnabled();
            activateOnStartupItem.Checked = settings.ActivateOnStartup;
            foreach (ToolStripMenuItem item in durationItem.DropDownItems)
                item.Checked = active && (int)item.Tag == activeMinutes;
            UpdateTooltip();
        }

        void UpdateTooltip()
        {
            string text;
            if (!active)
                text = "Caffeine: OFF - click to keep PC awake";
            else if (activeMinutes > 0)
                text = "Caffeine: ON - " + FormatRemaining(activeUntil - DateTime.UtcNow) + " left";
            else
                text = "Caffeine: ON - PC stays awake";

            // NotifyIcon.Text throws above 63 characters.
            tray.Text = text.Length > 63 ? text.Substring(0, 63) : text;
        }

        static string FormatRemaining(TimeSpan left)
        {
            if (left < TimeSpan.Zero)
                left = TimeSpan.Zero;
            return string.Format("{0}:{1:00}:{2:00}", (int)left.TotalHours, left.Minutes, left.Seconds);
        }

        static string DurationLabel(int minutes)
        {
            if (minutes == 0)
                return "Until I turn it off";
            if (minutes < 60)
                return minutes + " minutes";
            return minutes == 60 ? "1 hour" : (minutes / 60) + " hours";
        }

        static void OpenWebsite()
        {
            try
            {
                Process.Start(AppInfo.Website);
            }
            catch (Exception)
            {
                MessageBox.Show(AppInfo.Website, AppInfo.Name);
            }
        }

        static void ShowAbout()
        {
            MessageBox.Show(
                AppInfo.Name + " " + AppInfo.Version + "\n\n" +
                "Keeps your PC awake: no sleep, no screen off, no auto-lock.\n\n" +
                "Free and open source under the MIT License.\n" +
                AppInfo.Website + "\n\n" +
                "Not affiliated with Zhorn Software's Caffeine.",
                "About " + AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void ExitThreadCore()
        {
            timer.Stop();
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
            tray.Visible = false;
            base.ExitThreadCore();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
                tray.Dispose();
                menu.Dispose();
                iconOn.Dispose();
                iconOff.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
