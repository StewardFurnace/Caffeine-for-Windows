# Caffeine for Windows – Keep Your PC Awake

**Caffeine for Windows** is a free, portable app that keeps your PC awake on Windows 10 and Windows 11: no sleep, no screen off, no auto-lock and no "Away" status in Teams or Slack. One click on the coffee cup in the tray turns it on, another click turns it off.

## Screenshots

![Caffeine for Windows keeps the PC awake on Windows 11: orange coffee cup in the tray, upload keeps running](assets/screenshots/caffeine-keep-pc-awake-windows-11.png)

![Without Caffeine the Windows screen goes to sleep and the upload pauses](assets/screenshots/windows-screen-sleep-without-caffeine.png)

## Features

- **Keep PC awake with one click.** Click the tray icon to prevent sleep and screen off
- **Timer.** Stay awake for 15 or 30 minutes, 1, 2, 4 or 8 hours, or until you turn it off
- **Prevent auto-lock and Teams/Slack "Away" status.** After 59 seconds without input it presses F15, a key no app uses
- **Keep the screen on or only the PC.** Choose whether the display stays on
- **Works on Windows 11 and Windows 10,** including Windows 11 24H2 and later
- **Portable.** A single .exe, no installation, no admin rights
- **Doesn't change your power plan.** Uses the documented `SetThreadExecutionState` API; nothing is left behind after exit
- **Start with Windows** and start already active
- **Command line:** `-on`, `-off`, `-minutes 90`
- **Free and open source.** No ads, no telemetry, no network access; MIT license

## How to Install

1. Download `CaffeineForWindows-1.0.0-portable.zip` from the [latest release](https://github.com/StewardFurnace/Caffeine-for-Windows/releases/latest).
2. Extract the ZIP to any folder.
3. Run `Caffeine.exe`. A coffee cup appears in the notification area next to the clock (click the **^** arrow if you don't see it). Click the cup: **orange** means your PC stays awake, **grey** means normal sleep settings.

Right-click the cup for the timer and options. To stop, choose **Exit**; your normal sleep settings return immediately.

If Windows SmartScreen shows "Windows protected your PC", click **More info**, then **Run anyway**. This appears for new apps that aren't code-signed yet.

## FAQ

### Is it safe?
Yes. Caffeine for Windows is open source, so you can read every line of the code in [`src/`](src). It has no installer, no network access and needs no admin rights. It only saves its options in the registry under your own user account (`HKEY_CURRENT_USER\Software\CaffeineForWindows`).

### How do I keep my PC from going to sleep without changing power settings?
Run Caffeine and click the coffee cup. Your power plan stays untouched; Windows simply stays awake while the cup is orange.

### How do I keep my screen on in Windows 11?
Right-click the cup, make sure **Keep display on** is ticked, then click the cup to turn Caffeine on.

### How do I stop Teams or Slack from showing me as Away?
Right-click the cup and turn on **Prevent auto-lock and "Away" status**. Teams and Slack will keep showing you as available.

### How do I keep my PC awake for a few hours only?
Right-click the cup → **Keep awake for** → choose 1, 2, 4 or 8 hours. When time is up, Caffeine turns itself off and Windows can sleep again.

### Does it work on Windows 11 24H2?
Yes. It uses the documented Windows API rather than fake mouse moves, so it keeps working after Windows updates.

### Will it bypass my company's lock screen policy?
It stops the inactivity lock and screensaver on most PCs. Follow your organization's security rules; if your IT department requires the lock, don't use this option.

### Is this the Zhorn Software Caffeine?
No. Caffeine for Windows is an independent open source project and is not affiliated with Zhorn Software.

## Download

**[⬇ Download Caffeine for Windows (latest release)](https://github.com/StewardFurnace/Caffeine-for-Windows/releases/latest)**

Free for personal and commercial use under the [MIT License](LICENSE). Website: [stewardfurnace.github.io/Caffeine-for-Windows](https://stewardfurnace.github.io/Caffeine-for-Windows/)
