using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SD = System.Drawing;
using Shapes = System.Windows.Shapes;
using WF = System.Windows.Forms;

namespace SnapDrop
{
    class PickResult
    {
        public Int32Rect Rect;          // physical screen pixels
        public BitmapSource Image;      // frozen crop of the pre-selection snapshot
    }

    /// <summary>Freezes every monitor, lets the user drag a region (or press Space to pick a window).</summary>
    static class Picker
    {
        public static bool WindowMode;
        public static List<Native.RECT> Windows = new List<Native.RECT>();
        static readonly List<OverlayWindow> overlays = new List<OverlayWindow>();
        static Action<PickResult> callback;

        public static void Start(bool windowMode, Action<PickResult> cb)
        {
            callback = cb;
            WindowMode = windowMode;
            Windows = EnumerateWindows();

            foreach (var s in WF.Screen.AllScreens)
            {
                var b = s.Bounds;
                overlays.Add(new OverlayWindow(s, Util.Grab(b.X, b.Y, b.Width, b.Height)));
            }
            foreach (var o in overlays) o.Show();

            Native.GetCursorPos(out var p);
            var target = overlays.FirstOrDefault(o => o.Bounds.Contains(p.X, p.Y)) ?? overlays[0];
            target.Activate();
            target.Focus();
        }

        public static void ToggleMode()
        {
            WindowMode = !WindowMode;
            foreach (var o in overlays) o.RefreshMode();
        }

        public static void Finish(PickResult r)
        {
            foreach (var o in overlays) o.Close();
            overlays.Clear();
            var cb = callback;
            callback = null;
            if (cb != null)
                Application.Current.Dispatcher.BeginInvoke(new Action(() => cb(r)), System.Windows.Threading.DispatcherPriority.Background);
        }

        static List<Native.RECT> EnumerateWindows()
        {
            var list = new List<Native.RECT>();
            uint me = (uint)Environment.ProcessId;
            Native.EnumWindows((h, l) =>
            {
                if (!Native.IsWindowVisible(h) || Native.IsIconic(h)) return true;
                Native.GetWindowThreadProcessId(h, out uint pid);
                if (pid == me) return true;
                Native.DwmGetWindowAttribute(h, 14, out int cloaked, 4);           // DWMWA_CLOAKED
                if (cloaked != 0) return true;
                int ex = Native.GetWindowLong(h, Native.GWL_EXSTYLE);
                if ((ex & Native.WS_EX_TOOLWINDOW) != 0) return true;
                if ((ex & Native.WS_EX_TRANSPARENT) != 0 && (ex & Native.WS_EX_LAYERED) != 0) return true;
                var sb = new StringBuilder(64);
                Native.GetClassName(h, sb, 64);
                string cls = sb.ToString();
                if (cls == "Progman" || cls == "WorkerW") return true;
                Native.RECT r;
                if (Native.DwmGetWindowAttribute(h, 9, out r, Marshal.SizeOf<Native.RECT>()) != 0)   // DWMWA_EXTENDED_FRAME_BOUNDS
                    Native.GetWindowRect(h, out r);
                if (r.Right - r.Left < 24 || r.Bottom - r.Top < 24) return true;
                list.Add(r);
                return true;
            }, IntPtr.Zero);
            return list;
        }
    }

    class OverlayWindow : Window
    {
        public SD.Rectangle Bounds;
        readonly BitmapSource shot;
        readonly Grid root = new Grid();
        readonly Canvas layer = new Canvas { IsHitTestVisible = false };
        readonly RectangleGeometry full = new RectangleGeometry(new Rect(0, 0, 1, 1));
        readonly RectangleGeometry hole = new RectangleGeometry(new Rect(0, 0, 0, 0));
        readonly Shapes.Path dim;
        readonly Shapes.Rectangle selBox, winBox;
        readonly Border tag, hint;
        readonly TextBlock tagText = new TextBlock { Foreground = Brushes.White, FontSize = 12, FontFamily = new FontFamily("Segoe UI") };
        readonly TextBlock hintText = new TextBlock { Foreground = Brushes.White, FontSize = 14, FontFamily = new FontFamily("Segoe UI") };
        Point start;
        bool dragging;

        double R { get { return shot.PixelWidth / Math.Max(1.0, root.ActualWidth); } }

        public OverlayWindow(WF.Screen s, BitmapSource shot)
        {
            Bounds = s.Bounds;
            this.shot = shot;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Topmost = true;
            Background = Brushes.Black;
            Cursor = Cursors.Cross;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = Bounds.X; Top = Bounds.Y; Width = Bounds.Width; Height = Bounds.Height;
            UseLayoutRounding = true;

            var geo = new GeometryGroup { FillRule = FillRule.EvenOdd };
            geo.Children.Add(full);
            geo.Children.Add(hole);
            dim = new Shapes.Path { Data = geo, Fill = new SolidColorBrush(Color.FromArgb(0x8C, 0, 0, 0)), Visibility = Visibility.Collapsed };
            selBox = new Shapes.Rectangle { Stroke = Brushes.White, StrokeThickness = 1, Visibility = Visibility.Collapsed };
            winBox = new Shapes.Rectangle
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                StrokeThickness = 3,
                Fill = new SolidColorBrush(Color.FromArgb(0x30, 0x3B, 0x82, 0xF6)),
                Visibility = Visibility.Collapsed
            };
            tag = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xD0, 0, 0, 0)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(7, 3, 7, 3),
                Child = tagText,
                Visibility = Visibility.Collapsed
            };
            hint = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xD0, 0x1E, 0x1F, 0x22)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(16, 8, 16, 8),
                Margin = new Thickness(0, 36, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false,
                Child = hintText
            };

            layer.Children.Add(dim);
            layer.Children.Add(winBox);
            layer.Children.Add(selBox);
            layer.Children.Add(tag);

            root.Children.Add(new Image { Source = shot, Stretch = Stretch.Fill });
            root.Children.Add(layer);
            root.Children.Add(hint);
            Content = root;

            root.SizeChanged += (a, b) => full.Rect = new Rect(0, 0, root.ActualWidth, root.ActualHeight);
            SourceInitialized += (a, b) => Place();
            ContentRendered += (a, b) => Place();
            PreviewKeyDown += OnKey;
            MouseLeftButtonDown += OnDown;
            MouseMove += OnMove;
            MouseLeftButtonUp += OnUp;
            MouseRightButtonDown += (a, b) => Picker.Finish(null);
            RefreshMode();
        }

        void Place()
        {
            var h = new WindowInteropHelper(this).Handle;
            Native.SetWindowPos(h, Native.HWND_TOPMOST, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, Native.SWP_NOACTIVATE);
        }

        public void RefreshMode()
        {
            dragging = false;
            hole.Rect = new Rect(0, 0, 0, 0);
            dim.Visibility = Visibility.Collapsed;
            selBox.Visibility = Visibility.Collapsed;
            winBox.Visibility = Visibility.Collapsed;
            tag.Visibility = Visibility.Collapsed;
            hint.Visibility = Visibility.Visible;
            hintText.Text = Picker.WindowMode
                ? "Click a window to capture it   ·   Space: select an area   ·   Esc: cancel"
                : "Drag to select an area   ·   Space: pick a window   ·   Esc: cancel";
            Cursor = Picker.WindowMode ? Cursors.Hand : Cursors.Cross;
            if (IsLoaded && Picker.WindowMode) UpdateWindowHighlight();
        }

        void OnKey(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { e.Handled = true; Picker.Finish(null); }
            else if (e.Key == Key.Space) { e.Handled = true; Picker.ToggleMode(); }
        }

        Point Clamp(Point p)
        {
            return new Point(Math.Max(0, Math.Min(root.ActualWidth, p.X)), Math.Max(0, Math.Min(root.ActualHeight, p.Y)));
        }

        void OnDown(object s, MouseButtonEventArgs e)
        {
            if (Picker.WindowMode)
            {
                var r = WindowUnderCursor();
                if (r.HasValue) Picker.Finish(MakeResult(r.Value.Left, r.Value.Top, r.Value.Right - r.Value.Left, r.Value.Bottom - r.Value.Top));
                return;
            }
            start = Clamp(e.GetPosition(root));
            dragging = true;
            CaptureMouse();
            hint.Visibility = Visibility.Collapsed;
            dim.Visibility = Visibility.Visible;
            UpdateSel(start, start);
        }

        void OnMove(object s, MouseEventArgs e)
        {
            if (Picker.WindowMode) { UpdateWindowHighlight(); return; }
            if (dragging) UpdateSel(start, Clamp(e.GetPosition(root)));
        }

        void OnUp(object s, MouseButtonEventArgs e)
        {
            if (!dragging) return;
            dragging = false;
            ReleaseMouseCapture();
            var end = Clamp(e.GetPosition(root));
            var pr = PixelRect(start, end);
            if (pr.Width < 4 || pr.Height < 4) { RefreshMode(); return; }
            Picker.Finish(MakeResult(Bounds.X + pr.X, Bounds.Y + pr.Y, pr.Width, pr.Height));
        }

        Int32Rect PixelRect(Point a, Point b)
        {
            var r = new Rect(a, b);
            double k = R;
            int x = (int)Math.Round(r.X * k), y = (int)Math.Round(r.Y * k);
            int w = (int)Math.Round(r.Width * k), h = (int)Math.Round(r.Height * k);
            x = Math.Max(0, Math.Min(shot.PixelWidth - 1, x));
            y = Math.Max(0, Math.Min(shot.PixelHeight - 1, y));
            w = Math.Max(0, Math.Min(shot.PixelWidth - x, w));
            h = Math.Max(0, Math.Min(shot.PixelHeight - y, h));
            return new Int32Rect(x, y, w, h);
        }

        void UpdateSel(Point a, Point b)
        {
            var r = new Rect(a, b);
            hole.Rect = r;
            selBox.Width = r.Width; selBox.Height = r.Height;
            Canvas.SetLeft(selBox, r.X); Canvas.SetTop(selBox, r.Y);
            selBox.Visibility = Visibility.Visible;
            var pr = PixelRect(a, b);
            tagText.Text = pr.Width + " × " + pr.Height;
            tag.Visibility = Visibility.Visible;
            tag.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double ty = r.Y - tag.DesiredSize.Height - 6;
            if (ty < 4) ty = r.Y + 6;
            Canvas.SetLeft(tag, Math.Max(4, r.X));
            Canvas.SetTop(tag, ty);
        }

        Native.RECT? WindowUnderCursor()
        {
            Native.GetCursorPos(out var p);
            foreach (var r in Picker.Windows)
                if (p.X >= r.Left && p.X < r.Right && p.Y >= r.Top && p.Y < r.Bottom) return r;
            return null;
        }

        void UpdateWindowHighlight()
        {
            var w = WindowUnderCursor();
            dim.Visibility = Visibility.Visible;
            if (!w.HasValue || !Bounds.IntersectsWith(new SD.Rectangle(w.Value.Left, w.Value.Top, w.Value.Right - w.Value.Left, w.Value.Bottom - w.Value.Top)))
            {
                hole.Rect = new Rect(0, 0, 0, 0);
                winBox.Visibility = Visibility.Collapsed;
                return;
            }
            double k = R;
            double x1 = Math.Max(0, (w.Value.Left - Bounds.X) / k), y1 = Math.Max(0, (w.Value.Top - Bounds.Y) / k);
            double x2 = Math.Min(root.ActualWidth, (w.Value.Right - Bounds.X) / k), y2 = Math.Min(root.ActualHeight, (w.Value.Bottom - Bounds.Y) / k);
            var r = new Rect(new Point(x1, y1), new Point(x2, y2));
            hole.Rect = r;
            winBox.Width = r.Width; winBox.Height = r.Height;
            Canvas.SetLeft(winBox, r.X); Canvas.SetTop(winBox, r.Y);
            winBox.Visibility = Visibility.Visible;
            hint.Visibility = Visibility.Collapsed;
        }

        PickResult MakeResult(int sx, int sy, int sw, int sh)
        {
            // intersect with this monitor, then crop from the frozen snapshot
            int x1 = Math.Max(sx, Bounds.X), y1 = Math.Max(sy, Bounds.Y);
            int x2 = Math.Min(sx + sw, Bounds.Right), y2 = Math.Min(sy + sh, Bounds.Bottom);
            if (x2 - x1 < 2 || y2 - y1 < 2) return null;
            var local = new Int32Rect(x1 - Bounds.X, y1 - Bounds.Y, x2 - x1, y2 - y1);
            var crop = new CroppedBitmap(shot, local);
            crop.Freeze();
            return new PickResult { Rect = new Int32Rect(x1, y1, x2 - x1, y2 - y1), Image = crop };
        }
    }
}
