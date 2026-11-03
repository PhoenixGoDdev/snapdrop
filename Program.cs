using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SD = System.Drawing;
using WF = System.Windows.Forms;

namespace SnapDrop
{
    public enum Mode { Region, Window, Screen }

    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool first;
            using (var mutex = new Mutex(true, @"Local\SnapDrop.SingleInstance", out first))
            {
                if (!first) return;
                new SnapApp(args).Run();
            }
        }
    }

    class SnapApp : Application
    {
        const int HkScreen = 1, HkRegion = 2, HkBar = 3, HkPrint = 4, HkS = 5;
        readonly string[] args;
        WF.NotifyIcon tray;
        HwndSource hotkeys;
        static bool busy;

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
            Settings.Load();
            Util.CleanTemp();
            try { Directory.CreateDirectory(Settings.SaveFolder); } catch { }
            BuildTray();
            RegisterHotkeys();
            Util.Later(RunArgs, 400);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (hotkeys != null)
            {
                for (int i = 1; i <= 5; i++) Native.UnregisterHotKey(hotkeys.Handle, i);
                hotkeys.Dispose();
            }
            if (tray != null) { tray.Visible = false; tray.Dispose(); }
            base.OnExit(e);
        }

        static void Log(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Settings.LogPath));
                File.AppendAllText(Settings.LogPath, DateTime.Now + "\n" + ex + "\n\n");
            }
            catch { }
        }

        // Test / automation switches: --capture screen|region|window, --bar, --edit <file>, --thumb <file>
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

        void BuildTray()
        {
            var menu = new WF.ContextMenuStrip();
            Func<string, string, Action, WF.ToolStripMenuItem> item = (text, keys, act) =>
            {
                var mi = new WF.ToolStripMenuItem(text);
                if (keys != null) mi.ShortcutKeyDisplayString = keys;
                mi.Click += (s, e) => Util.Later(act, 150);
                menu.Items.Add(mi);
                return mi;
            };
            item("Capture selection", "Ctrl+Shift+S", () => Capture(Mode.Region, Settings.TimerSeconds));
            item("Capture window", null, () => Capture(Mode.Window, Settings.TimerSeconds));
            item("Capture full screen", "Ctrl+Shift+3", () => Capture(Mode.Screen, Settings.TimerSeconds));
            item("Screenshot options…", "Ctrl+Shift+5", CaptureBar.Show);
            menu.Items.Add(new WF.ToolStripSeparator());

            var clip = new WF.ToolStripMenuItem("Copy to clipboard") { CheckOnClick = true, Checked = Settings.CopyToClipboard };
            clip.CheckedChanged += (s, e) => { Settings.CopyToClipboard = clip.Checked; Settings.Save(); };
            var thumb = new WF.ToolStripMenuItem("Show floating thumbnail") { CheckOnClick = true, Checked = Settings.ShowThumbnail };
            thumb.CheckedChanged += (s, e) => { Settings.ShowThumbnail = thumb.Checked; Settings.Save(); };
            var startup = new WF.ToolStripMenuItem("Start with Windows") { CheckOnClick = true, Checked = Settings.StartWithWindows };
            startup.CheckedChanged += (s, e) => { try { Settings.StartWithWindows = startup.Checked; } catch { } };
            menu.Items.Add(clip);
            menu.Items.Add(thumb);
            menu.Items.Add(startup);
            menu.Items.Add(new WF.ToolStripSeparator());

            item("Open screenshots folder", null, () => Util.OpenFolder(Settings.SaveFolder));
            item("Change save folder…", null, () =>
            {
                var d = Util.PickFolder(Settings.SaveFolder);
                if (d != null) { Settings.SaveFolder = d; Settings.Save(); }
            });
            menu.Items.Add(new WF.ToolStripSeparator());
            var quit = new WF.ToolStripMenuItem("Quit SnapDrop");
            quit.Click += (s, e) => Shutdown();
            menu.Items.Add(quit);

            tray = new WF.NotifyIcon { Icon = MakeIcon(), Text = "SnapDrop — Ctrl+Shift+S to capture", ContextMenuStrip = menu, Visible = true };
            tray.MouseClick += (s, e) =>
            {
                if (e.Button == WF.MouseButtons.Left) Util.Later(() => Capture(Mode.Region, Settings.TimerSeconds), 120);
            };
        }

        static SD.Icon MakeIcon()
        {
            using (var bmp = new SD.Bitmap(32, 32))
            using (var g = SD.Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SD.Drawing2D.SmoothingMode.AntiAlias;
                using (var gp = new SD.Drawing2D.GraphicsPath())
                {
                    int x = 1, y = 1, w = 30, h = 30, r = 9;
                    gp.AddArc(x, y, r * 2, r * 2, 180, 90);
                    gp.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
                    gp.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
                    gp.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
                    gp.CloseFigure();
                    using (var br = new SD.SolidBrush(SD.Color.FromArgb(59, 130, 246))) g.FillPath(br, gp);
                }
                using (var pen = new SD.Pen(SD.Color.White, 2.6f) { StartCap = SD.Drawing2D.LineCap.Round, EndCap = SD.Drawing2D.LineCap.Round })
                {
                    int a = 7, b = 25, l = 6;
                    g.DrawLines(pen, new[] { new SD.Point(a, a + l), new SD.Point(a, a), new SD.Point(a + l, a) });
                    g.DrawLines(pen, new[] { new SD.Point(b, a + l), new SD.Point(b, a), new SD.Point(b - l, a) });
                    g.DrawLines(pen, new[] { new SD.Point(a, b - l), new SD.Point(a, b), new SD.Point(a + l, b) });
                    g.DrawLines(pen, new[] { new SD.Point(b, b - l), new SD.Point(b, b), new SD.Point(b - l, b) });
                }
                g.FillEllipse(SD.Brushes.White, 12.5f, 12.5f, 7, 7);
                return SD.Icon.FromHandle(bmp.GetHicon());
            }
        }

        // ------------------------------------------------------------ hotkeys

        void RegisterHotkeys()
        {
            var p = new HwndSourceParameters("SnapDropHotkeys") { ParentWindow = Native.HWND_MESSAGE, Width = 0, Height = 0 };
            hotkeys = new HwndSource(p);
            hotkeys.AddHook(Hook);
            uint cs = Native.MOD_CONTROL | Native.MOD_SHIFT | Native.MOD_NOREPEAT;
            var failed = new List<string>();
            if (!Native.RegisterHotKey(hotkeys.Handle, HkS, cs, 0x53)) failed.Add("Ctrl+Shift+S");
            if (!Native.RegisterHotKey(hotkeys.Handle, HkScreen, cs, 0x33)) failed.Add("Ctrl+Shift+3");
            if (!Native.RegisterHotKey(hotkeys.Handle, HkRegion, cs, 0x34)) failed.Add("Ctrl+Shift+4");
            if (!Native.RegisterHotKey(hotkeys.Handle, HkBar, cs, 0x35)) failed.Add("Ctrl+Shift+5");
            Native.RegisterHotKey(hotkeys.Handle, HkPrint, Native.MOD_NOREPEAT, 0x2C);   // PrintScreen -> selection (best effort)
            if (failed.Count > 0)
                tray.ShowBalloonTip(5000, "SnapDrop", string.Join(", ", failed) + " is already used by another app. Use the tray icon instead.", WF.ToolTipIcon.Warning);
            else if (args.Length == 0)
                tray.ShowBalloonTip(3500, "SnapDrop is running", "Ctrl+Shift+4  selection\nCtrl+Shift+3  full screen\nCtrl+Shift+5  options", WF.ToolTipIcon.Info);
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
                        case HkScreen: Capture(Mode.Screen, Settings.TimerSeconds); break;
                        case HkRegion:
                        case HkS:
                        case HkPrint: Capture(Mode.Region, Settings.TimerSeconds); break;
                        case HkBar: CaptureBar.Show(); break;
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

        public static string SaveImage(BitmapSource img)
        {
            string dir = Settings.SaveFolder;
            try { Directory.CreateDirectory(dir); }
            catch { dir = Path.Combine(Path.GetTempPath(), "SnapDrop"); Directory.CreateDirectory(dir); }
            string name = "Screenshot " + DateTime.Now.ToString("yyyy-MM-dd 'at' HH.mm.ss");
            string path = Path.Combine(dir, name + ".png");
            for (int n = 2; File.Exists(path); n++) path = Path.Combine(dir, name + " (" + n + ").png");
            Util.SavePng(img, path);
            return path;
        }
    }

    /// <summary>Big centered "3…2…1" shown before a timed capture. It is closed before the shot is taken.</summary>
    static class Countdown
    {
        public static void Run(int seconds, Action done)
        {
            var tb = new TextBlock { Text = seconds.ToString(), Foreground = Brushes.White, FontSize = 54, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center };
            var w = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ShowInTaskbar = false,
                Topmost = true,
                ShowActivated = false,
                ResizeMode = ResizeMode.NoResize,
                Width = 130,
                Height = 110,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Content = new Border { CornerRadius = new CornerRadius(26), Background = new SolidColorBrush(Color.FromArgb(0xE0, 0x12, 0x12, 0x14)), Child = tb, Padding = new Thickness(0, 4, 0, 0) }
            };
            var wa = WF.Screen.PrimaryScreen.WorkingArea;
            double sc = Native.SystemScale();
            w.Left = (wa.Left + wa.Width / 2.0) / sc - w.Width / 2;
            w.Top = wa.Top / sc + 40;
            w.Show();

            int n = seconds;
            var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            t.Tick += (s, e) =>
            {
                n--;
                if (n > 0) { tb.Text = n.ToString(); return; }
                t.Stop();
                w.Close();
                Util.Later(done, 200);
            };
            t.Start();
        }
    }

    /// <summary>The Ctrl+Shift+5 panel: entire screen / window / selection, timer, options.</summary>
    static class CaptureBar
    {
        static Window open;

        public static void Show()
        {
            if (open != null) { open.Activate(); return; }
            var w = (Window)XamlReader.Parse(Xaml.Bar.Replace("{STYLES}", Xaml.Styles));
            open = w;
            w.Closed += (s, e) => open = null;
            w.WindowStartupLocation = WindowStartupLocation.Manual;
            w.Left = -10000;
            w.Top = -10000;

            Action<Mode> go = m =>
            {
                int d = Settings.TimerSeconds;
                w.Close();
                Util.Later(() => SnapApp.Capture(m, d), 120);
            };
            ((Button)w.FindName("BScreen")).Click += (s, e) => go(Mode.Screen);
            ((Button)w.FindName("BWindow")).Click += (s, e) => go(Mode.Window);
            ((Button)w.FindName("BRegion")).Click += (s, e) => go(Mode.Region);
            ((Button)w.FindName("BClose")).Click += (s, e) => w.Close();

            foreach (var n in new[] { "T0", "T3", "T5", "T10" })
            {
                var rb = (RadioButton)w.FindName(n);
                int secs = int.Parse((string)rb.Tag);
                rb.IsChecked = secs == Settings.TimerSeconds;
                rb.Checked += (s, e) => { Settings.TimerSeconds = secs; Settings.Save(); };
            }

            var opts = (Button)w.FindName("BOpts");
            opts.Click += (s, e) => BuildOptions(opts).IsOpen = true;

            w.PreviewKeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Escape) w.Close(); };
            w.Loaded += (s, e) =>
            {
                var wa = WF.Screen.PrimaryScreen.WorkingArea;
                double sc = Native.SystemScale();
                w.Left = (wa.Left + wa.Width / 2.0) / sc - w.ActualWidth / 2;
                w.Top = wa.Bottom / sc - w.ActualHeight - 36;
                w.Activate();
            };
            w.Show();
        }

        static ContextMenu BuildOptions(Button target)
        {
            var m = new ContextMenu { PlacementTarget = target, Placement = System.Windows.Controls.Primitives.PlacementMode.Top };
            var clip = new MenuItem { Header = "Copy to clipboard", IsCheckable = true, IsChecked = Settings.CopyToClipboard };
            clip.Click += (s, e) => { Settings.CopyToClipboard = clip.IsChecked; Settings.Save(); };
            var thumb = new MenuItem { Header = "Show floating thumbnail", IsCheckable = true, IsChecked = Settings.ShowThumbnail };
            thumb.Click += (s, e) => { Settings.ShowThumbnail = thumb.IsChecked; Settings.Save(); };
            var folder = new MenuItem { Header = "Save to: " + Settings.SaveFolder + "  (change…)" };
            folder.Click += (s, e) =>
            {
                var d = Util.PickFolder(Settings.SaveFolder);
                if (d != null) { Settings.SaveFolder = d; Settings.Save(); }
            };
            var openF = new MenuItem { Header = "Open screenshots folder" };
            openF.Click += (s, e) => Util.OpenFolder(Settings.SaveFolder);
            m.Items.Add(clip);
            m.Items.Add(thumb);
            m.Items.Add(new Separator());
            m.Items.Add(folder);
            m.Items.Add(openF);
            return m;
        }
    }
}
