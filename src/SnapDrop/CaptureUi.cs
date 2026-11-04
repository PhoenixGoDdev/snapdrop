using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using WF = System.Windows.Forms;

namespace SnapDrop
{
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

    /// <summary>The "options bar": entire screen / window / selection, timer, quick options.</summary>
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
            var openF = new MenuItem { Header = "Open screenshots folder" };
            openF.Click += (s, e) => Util.OpenFolder(Settings.SaveFolder);
            var more = new MenuItem { Header = "All settings…" };
            more.Click += (s, e) => SnapApp.OpenSettings();
            m.Items.Add(clip);
            m.Items.Add(thumb);
            m.Items.Add(new Separator());
            m.Items.Add(openF);
            m.Items.Add(more);
            return m;
        }
    }
}
