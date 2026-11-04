using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SD = System.Drawing;
using WF = System.Windows.Forms;

namespace SnapDrop
{
    public enum Mode { Region, Window, Screen }

    /// <summary>The tray application: owns the tray icon, the global hotkeys and the capture flow.</summary>
    class SnapApp : Application
    {
        const int IdRegion = 1, IdScreen = 2, IdWindow = 3, IdBar = 4, IdPrint = 5;

        readonly string[] args;
        WF.NotifyIcon tray;
        WF.ToolStripMenuItem miRegion, miWindow, miScreen, miBar;
        HwndSource hotkeys;
        static bool busy;

        public static string Version
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return v == null ? "1.0.0" : v.ToString(3);
            }
        }

        public SnapApp(string[] a)
        {
            args = a;
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += (s, ex) =>
            {
                Log(ex.Exception);
                busy = false;
                ThumbWindow.ShowAll();
                ex.Handled = true;
            };
            bool firstRun = !File.Exists(Settings.FilePath);
            Settings.Load();
            if (firstRun) Settings.Save();
            ScreenshotStore.Sweep();
            Util.CleanTemp();
            try { Directory.CreateDirectory(Settings.SaveFolder); } catch { }

            BuildTray();
            var failed = ApplyHotkeys();
            bool background = Array.IndexOf(args, "--background") >= 0;
            if (failed.Count > 0)
                tray.ShowBalloonTip(6000, "SnapDrop", "These shortcuts are already used by another app:\n" + string.Join("\n", failed) + "\nChange them in Settings.", WF.ToolTipIcon.Warning);
            else if (firstRun && !background)
                tray.ShowBalloonTip(5000, "SnapDrop is running", "Press " + Describe(Settings.HotkeyRegion, "Ctrl+Shift+S") + " to capture an area.\nRight-click the tray icon for more.", WF.ToolTipIcon.Info);

            Util.Later(RunArgs, 400);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ThumbWindow.FinalizeAll();
            UnregisterAll();
            if (hotkeys != null) hotkeys.Dispose();
            if (tray != null) { tray.Visible = false; tray.Dispose(); }
            base.OnExit(e);
        }

        public static void Log(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(Settings.DataFolder);
                File.AppendAllText(Settings.LogPath, DateTime.Now + "\n" + ex + "\n\n");
            }
            catch { }
        }

        public static void OpenSettings() { SettingsWindow.ShowOrActivate(); }

        // Developer / test switches: --capture screen|region|window, --bar, --settings, --edit <file>, --thumb <file>
        void RunArgs()
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--capture" when i + 1 < args.Length:
                        var m = args[++i];
                        Capture(m == "screen" ? Mode.Screen : m == "window" ? Mode.Window : Mode.Region, 0);
                        break;
                    case "--bar": CaptureBar.Show(); break;
                    case "--settings": OpenSettings(); break;
                    case "--edit" when i + 1 < args.Length: Editor.Open(args[++i]); break;
                    case "--thumb" when i + 1 < args.Length:
                        var p = args[++i];
                        var bi = new BitmapImage();
                        bi.BeginInit(); bi.CacheOption = BitmapCacheOption.OnLoad; bi.UriSource = new Uri(p); bi.EndInit(); bi.Freeze();
                        ThumbWindow.Show(p, bi);
                        break;
                }
            }
        }

        // ------------------------------------------------------------ tray

        static string Describe(string hotkey, string fallback = "") { return string.IsNullOrEmpty(hotkey) ? fallback : hotkey; }

        void BuildTray()
        {
            var menu = new WF.ContextMenuStrip();
            Func<string, Action, WF.ToolStripMenuItem> item = (text, act) =>
            {
                var mi = new WF.ToolStripMenuItem(text);
                mi.Click += (s, e) => Util.Later(act, 150);   // let the menu disappear before capturing
                menu.Items.Add(mi);
                return mi;
            };
            miRegion = item("Capture area", () => Capture(Mode.Region, Settings.TimerSeconds));
            miWindow = item("Capture window", () => Capture(Mode.Window, Settings.TimerSeconds));
            miScreen = item("Capture full screen", () => Capture(Mode.Screen, Settings.TimerSeconds));
            miBar = item("Screenshot options…", CaptureBar.Show);
            menu.Items.Add(new WF.ToolStripSeparator());
            item("Settings…", OpenSettings);
            item("Open screenshots folder", () => Util.OpenFolder(Settings.SaveFolder));
            menu.Items.Add(new WF.ToolStripSeparator());
            var quit = new WF.ToolStripMenuItem("Quit SnapDrop");
            quit.Click += (s, e) => Shutdown();
            menu.Items.Add(quit);

            tray = new WF.NotifyIcon { Icon = LoadIcon(), ContextMenuStrip = menu, Visible = true };
            tray.MouseClick += (s, e) =>
            {
                if (e.Button == WF.MouseButtons.Left) Util.Later(() => Capture(Mode.Region, Settings.TimerSeconds), 120);
            };
            RefreshTrayText();
        }

        void RefreshTrayText()
        {
            miRegion.ShortcutKeyDisplayString = Settings.HotkeyRegion;
            miWindow.ShortcutKeyDisplayString = Settings.HotkeyWindow;
            miScreen.ShortcutKeyDisplayString = Settings.HotkeyScreen;
            miBar.ShortcutKeyDisplayString = Settings.HotkeyBar;
            string hint = string.IsNullOrEmpty(Settings.HotkeyRegion) ? "click to capture" : Settings.HotkeyRegion + " to capture";
            tray.Text = "SnapDrop - " + hint;   // NotifyIcon text is limited to 63 characters
        }

        /// <summary>The icon embedded in the exe (assets/icon.ico), or a drawn fallback.</summary>
        public static SD.Icon LoadIcon()
        {
            try
            {
                var ic = SD.Icon.ExtractAssociatedIcon(Environment.ProcessPath);
                if (ic != null) return ic;
            }
            catch { }
            using (var bmp = new SD.Bitmap(32, 32))
            using (var g = SD.Graphics.FromImage(bmp))
            {
                g.Clear(SD.Color.FromArgb(59, 130, 246));
                g.FillEllipse(SD.Brushes.White, 11, 11, 10, 10);
                return SD.Icon.FromHandle(bmp.GetHicon());
            }
        }

        // ------------------------------------------------------------ hotkeys

        void UnregisterAll()
        {
            if (hotkeys == null) return;
            for (int id = IdRegion; id <= IdPrint; id++) Native.UnregisterHotKey(hotkeys.Handle, id);
        }

        /// <summary>(Re)registers every shortcut from Settings. Returns the ones another app already owns.</summary>
        public List<string> ApplyHotkeys()
        {
            if (hotkeys == null)
            {
                var p = new HwndSourceParameters("SnapDropHotkeys") { ParentWindow = Native.HWND_MESSAGE, Width = 0, Height = 0 };
                hotkeys = new HwndSource(p);
                hotkeys.AddHook(Hook);
            }
            UnregisterAll();

            var failed = new List<string>();
            Action<int, string, string> reg = (id, text, label) =>
            {
                Hotkey.Combo c;
                if (!Hotkey.TryParse(text, out c)) return;
                if (!Native.RegisterHotKey(hotkeys.Handle, id, c.Mods | Native.MOD_NOREPEAT, c.Vk)) failed.Add(label + " (" + text + ")");
            };
            reg(IdRegion, Settings.HotkeyRegion, "Capture area");
            reg(IdScreen, Settings.HotkeyScreen, "Capture full screen");
            reg(IdWindow, Settings.HotkeyWindow, "Capture window");
            reg(IdBar, Settings.HotkeyBar, "Options bar");
            if (Settings.PrintScreenKey) Native.RegisterHotKey(hotkeys.Handle, IdPrint, Native.MOD_NOREPEAT, 0x2C);   // best effort

            if (tray != null) RefreshTrayText();
            return failed;
        }

        IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Native.WM_HOTKEY)
            {
                handled = true;
                int id = (int)wParam;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    switch (id)
                    {
                        case IdScreen: Capture(Mode.Screen, Settings.TimerSeconds); break;
                        case IdWindow: Capture(Mode.Window, Settings.TimerSeconds); break;
                        case IdRegion:
                        case IdPrint: Capture(Mode.Region, Settings.TimerSeconds); break;
                        case IdBar: CaptureBar.Show(); break;
                    }
                }));
            }
            return IntPtr.Zero;
        }

        // ------------------------------------------------------------ capture flow

        public static void Capture(Mode mode, int delay)
        {
            if (busy) return;
            busy = true;
            try
            {
                ThumbWindow.HideAll();           // keep older thumbnails out of the new shot
                Util.FlushUI();
                Thread.Sleep(110);

                if (mode == Mode.Screen)
                {
                    var b = WF.Screen.FromPoint(WF.Cursor.Position).Bounds;
                    if (delay > 0) Countdown.Run(delay, () => Finish(Util.Grab(b.X, b.Y, b.Width, b.Height)));
                    else Finish(Util.Grab(b.X, b.Y, b.Width, b.Height));
                    return;
                }

                Picker.Start(mode == Mode.Window, res =>
                {
                    if (res == null) { Cancel(); return; }
                    if (delay > 0)
                        Countdown.Run(delay, () => Finish(Util.Grab(res.Rect.X, res.Rect.Y, res.Rect.Width, res.Rect.Height)));
                    else Finish(res.Image);
                });
            }
            catch (Exception ex)
            {
                Log(ex);
                Cancel();
            }
        }

        static void Cancel()
        {
            busy = false;
            ThumbWindow.ShowAll();
        }

        static void Finish(BitmapSource img)
        {
            try
            {
                string path = SaveImage(img);
                if (Settings.CopyToClipboard) Util.CopyToClipboard(img, path);
                ThumbWindow.ShowAll();
                if (Settings.ShowThumbnail) ThumbWindow.Show(path, img);
            }
            catch (Exception ex) { Log(ex); }
            finally { busy = false; }
        }

        /// <summary>Writes the capture as "Prefix_2026-11-04_02.56.28.png" (no spaces, so pasted paths just work). See ScreenshotStore for where.</summary>
        public static string SaveImage(BitmapSource img)
        {
            string name = Settings.FilePrefix + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
            string path = ScreenshotStore.NewPath(name, Settings.Extension);   // staging folder in "smart" mode
            Util.SaveByExtension(img, path);
            return path;
        }
    }
}
