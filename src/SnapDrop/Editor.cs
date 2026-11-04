using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using WF = System.Windows.Forms;

namespace SnapDrop
{
    /// <summary>Crop + annotate window. Everything is drawn in image-pixel coordinates inside a Viewbox.</summary>
    class Editor
    {
        readonly string path;
        Window w;
        Image baseImg;
        Canvas ann, over;
        Grid wrap, stage;
        Border cropBar, dragChip;
        TextBlock status;
        Button undoBtn, redoBtn;
        BitmapSource src;

        string tool = "crop";
        Color color = Color.FromRgb(0xFF, 0x3B, 0x30);
        double thick = 6;

        readonly List<(Action undo, Action redo)> hist = new List<(Action, Action)>();
        int hp;
        bool dirty, allowClose, handoff;   // handoff: the file is now owned by a thumbnail / drag, so don't save it again

        // drawing state
        bool drawing;
        Point p0;
        Polyline curLine;
        Shape cur;
        TextBox activeText;

        // crop state
        Rect? crop;
        int dragMode, edges;      // dragMode: 0 none, 1 new, 2 move, 3 resize
        Rect cropStart;
        Point dragStart;

        Editor(string path) { this.path = path; }

        public static void Open(string path, BitmapSource img = null)
        {
            try { new Editor(path).Create(img ?? Load(path)); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "SnapDrop"); }
        }

        static BitmapSource Load(string p)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(p);
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        T F<T>(string name) where T : class { return (T)w.FindName(name); }

        void Create(BitmapSource img)
        {
            w = (Window)XamlReader.Parse(Xaml.Editor.Replace("{STYLES}", Xaml.Styles));
            baseImg = F<Image>("BaseImg"); ann = F<Canvas>("Ann"); over = F<Canvas>("Over");
            wrap = F<Grid>("Wrap"); stage = F<Grid>("Stage"); cropBar = F<Border>("CropBar"); dragChip = F<Border>("DragChip");
            status = F<TextBlock>("Status"); undoBtn = F<Button>("UndoBtn"); redoBtn = F<Button>("RedoBtn");
            w.Title = "SnapDrop – " + System.IO.Path.GetFileName(path);
            if (Util.AppIcon != null) w.Icon = Util.AppIcon;

            BuildPalette();
            foreach (var n in new[] { "TCrop", "TPen", "THi", "TArrow", "TBox", "TText", "TBlur" })
            {
                var rb = F<RadioButton>(n);
                rb.Checked += (s, e) => SetTool((string)rb.Tag);
            }

            over.MouseLeftButtonDown += Down;
            over.MouseMove += Move;
            over.MouseLeftButtonUp += Up;
            stage.SizeChanged += (s, e) => { if (crop.HasValue) DrawCrop(); };

            undoBtn.Click += (s, e) => Undo();
            redoBtn.Click += (s, e) => Redo();
            F<Button>("CopyBtn").Click += (s, e) => CopyImage();
            F<Button>("SaveAsBtn").Click += (s, e) => SaveAs();
            F<Button>("DoneBtn").Click += (s, e) => SaveAndClose();
            F<Button>("CropApply").Click += (s, e) => ApplyCrop();
            F<Button>("CropCancel").Click += (s, e) => ClearCrop();

            // drag the edited image straight out of the editor
            bool chipDown = false;
            dragChip.MouseLeftButtonDown += (s, e) => chipDown = true;
            dragChip.MouseLeftButtonUp += (s, e) => chipDown = false;
            dragChip.MouseMove += (s, e) =>
            {
                if (!chipDown || e.LeftButton != MouseButtonState.Pressed) return;
                chipDown = false;
                DragOutEdited();
            };

            w.PreviewKeyDown += OnKey;
            w.Closed += (s, e) => { if (!handoff) ScreenshotStore.Finalize(path); };
            w.Closing += (s, e) =>
            {
                if (allowClose || !dirty) return;
                var r = MessageBox.Show(w, "Save your changes to this screenshot?", "SnapDrop", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (r == MessageBoxResult.Cancel) e.Cancel = true;
                else if (r == MessageBoxResult.Yes) { e.Cancel = true; SaveAndClose(); }
            };

            SetImage(img);
            UpdateUndoButtons();
            var wa = WF.Screen.PrimaryScreen.WorkingArea;
            double sc = Native.SystemScale(), aw = wa.Width / sc, ah = wa.Height / sc;
            w.MinWidth = Math.Min(1140, aw * 0.95);
            w.Width = Math.Min(1200, aw * 0.95);
            w.Height = Math.Min(800, ah * 0.92);
            w.WindowStartupLocation = WindowStartupLocation.Manual;
            w.Left = wa.Left / sc + (aw - w.Width) / 2;
            w.Top = wa.Top / sc + (ah - w.Height) / 2;
            w.Topmost = true;       // pop to the front even though the thumbnail never took focus
            w.Show();
            w.Activate();
            w.Topmost = false;
            F<RadioButton>("TCrop").IsChecked = true;
            SetTool("crop");
        }

        void BuildPalette()
        {
            var colors = F<StackPanel>("Colors");
            bool first = true;
            foreach (var hex in new[] { "FF3B30", "FF9F0A", "FFD60A", "30D158", "0A84FF", "FFFFFF", "1C1C1E" })
            {
                var c = (Color)ColorConverter.ConvertFromString("#" + hex);
                var rb = new RadioButton { GroupName = "c", Style = (Style)w.FindResource("Swatch"), Background = new SolidColorBrush(c), IsChecked = first, ToolTip = "#" + hex };
                rb.Checked += (s, e) => color = c;
                colors.Children.Add(rb);
                first = false;
            }
            var sizes = F<StackPanel>("Sizes");
            var defs = new[] { ("S", 3.0), ("M", 6.0), ("L", 11.0) };
            foreach (var d in defs)
            {
                var rb = new RadioButton { GroupName = "s", Style = (Style)w.FindResource("ToolBtn"), Content = d.Item1, IsChecked = d.Item2 == 6.0, ToolTip = "Stroke / text size" };
                var val = d.Item2;
                rb.Checked += (s, e) => thick = val;
                sizes.Children.Add(rb);
            }
        }

        void SetImage(BitmapSource s)
        {
            src = s;
            baseImg.Source = s;
            wrap.Width = s.PixelWidth;
            wrap.Height = s.PixelHeight;
            ClearCrop();
        }

        // ---------------------------------------------------------------- tools

        void SetTool(string t)
        {
            CommitText();
            ClearCrop();
            tool = t;
            over.Cursor = t == "text" ? Cursors.IBeam : Cursors.Cross;
            switch (t)
            {
                case "crop": status.Text = "Drag on the image to choose the crop area · drag the handles to adjust · Enter applies, Esc cancels"; break;
                case "text": status.Text = "Click where the text should go, type, then press Enter (Shift+Enter for a new line)"; break;
                case "blur": status.Text = "Drag over anything sensitive to pixelate it"; break;
                default: status.Text = "Draw on the image · Ctrl+Z undoes · colour and size are in the toolbar"; break;
            }
        }

        static Point Clip(Point p, double w, double h)
        {
            return new Point(Math.Max(0, Math.Min(w, p.X)), Math.Max(0, Math.Min(h, p.Y)));
        }

        Point Pos(MouseEventArgs e) { return Clip(e.GetPosition(over), wrap.Width, wrap.Height); }

        Brush Solid(Color c) { var b = new SolidColorBrush(c); b.Freeze(); return b; }

        void Down(object s, MouseButtonEventArgs e)
        {
            CommitText();
            var p = Pos(e);
            p0 = p;
            if (tool == "text") { StartText(p); return; }
            over.CaptureMouse();
            switch (tool)
            {
                case "crop": CropDown(p); break;
                case "pen":
                case "hi":
                    bool hi = tool == "hi";
                    curLine = new Polyline
                    {
                        Stroke = Solid(hi ? Color.FromArgb(0x70, color.R, color.G, color.B) : color),
                        StrokeThickness = hi ? Math.Max(16, thick * 4) : thick,
                        StrokeLineJoin = PenLineJoin.Round,
                        StrokeStartLineCap = hi ? PenLineCap.Flat : PenLineCap.Round,
                        StrokeEndLineCap = hi ? PenLineCap.Flat : PenLineCap.Round,
                        Points = new PointCollection { p, p }
                    };
                    ann.Children.Add(curLine);
                    drawing = true;
                    break;
                case "arrow":
                    cur = new Path { Stroke = Solid(color), Fill = Solid(color), StrokeThickness = thick, StrokeStartLineCap = PenLineCap.Round, StrokeLineJoin = PenLineJoin.Round };
                    ann.Children.Add(cur);
                    drawing = true;
                    break;
                case "box":
                    cur = new Rectangle { Stroke = Solid(color), StrokeThickness = thick, StrokeLineJoin = PenLineJoin.Round, RadiusX = 2, RadiusY = 2 };
                    ann.Children.Add(cur);
                    drawing = true;
                    break;
                case "blur":
                    cur = new Rectangle { Stroke = Brushes.White, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 3 }, Fill = new SolidColorBrush(Color.FromArgb(0x40, 0x3B, 0x82, 0xF6)) };
                    ann.Children.Add(cur);
                    drawing = true;
                    break;
            }
        }

        void Move(object s, MouseEventArgs e)
        {
            var p = Pos(e);
            if (tool == "crop") { CropMove(p, e.LeftButton == MouseButtonState.Pressed && over.IsMouseCaptured); return; }
            if (!drawing) return;
            if (curLine != null) curLine.Points.Add(p);
            else if (cur is Path) ((Path)cur).Data = ArrowGeo(p0, p, thick);
            else if (cur is Rectangle) SetRect((Rectangle)cur, new Rect(p0, p));
        }

        void Up(object s, MouseButtonEventArgs e)
        {
            if (over.IsMouseCaptured) over.ReleaseMouseCapture();
            if (tool == "crop") { CropUp(); return; }
            if (!drawing) return;
            drawing = false;
            var p = Pos(e);

            UIElement el = null;
            if (curLine != null) el = curLine;
            else if (cur != null)
            {
                var r = new Rect(p0, p);
                bool tiny = tool == "arrow" ? (p - p0).Length < 6 : (r.Width < 4 || r.Height < 4);
                if (tiny) ann.Children.Remove(cur);
                else if (tool == "blur")
                {
                    ann.Children.Remove(cur);
                    el = MakePixelate(r);
                    ann.Children.Add(el);
                }
                else el = cur;
            }
            curLine = null;
            cur = null;
            if (el == null) return;
            var added = el;
            Push(() => ann.Children.Remove(added), () => ann.Children.Add(added));
        }

        static void SetRect(Rectangle rc, Rect r)
        {
            rc.Width = r.Width; rc.Height = r.Height;
            Canvas.SetLeft(rc, r.X); Canvas.SetTop(rc, r.Y);
        }

        static Geometry ArrowGeo(Point a, Point b, double t)
        {
            var g = new StreamGeometry();
            using (var c = g.Open())
            {
                var v = a - b;
                double len = v.Length;
                if (len < 1) { c.BeginFigure(a, false, false); c.LineTo(b, true, true); }
                else
                {
                    v /= len;
                    double hl = Math.Min(len * 0.9, Math.Max(18, t * 4.8));
                    double ang = Math.PI / 7.5, ca = Math.Cos(ang), sa = Math.Sin(ang);
                    Func<double, Vector> rot = sgn => new Vector(v.X * ca - sgn * v.Y * sa, sgn * v.X * sa + v.Y * ca);
                    var p1 = b + rot(1) * hl;
                    var p2 = b + rot(-1) * hl;
                    var baseMid = b + v * (hl * 0.72);
                    c.BeginFigure(a, false, false);
                    c.LineTo(baseMid, true, true);
                    c.BeginFigure(p1, true, true);
                    c.LineTo(b, true, true);
                    c.LineTo(p2, true, true);
                }
            }
            g.Freeze();
            return g;
        }

        UIElement MakePixelate(Rect r)
        {
            var flat = Flatten();
            int x = (int)Math.Max(0, Math.Floor(r.X)), y = (int)Math.Max(0, Math.Floor(r.Y));
            int cw = (int)Math.Min(flat.PixelWidth - x, Math.Ceiling(r.Width)), ch = (int)Math.Min(flat.PixelHeight - y, Math.Ceiling(r.Height));
            cw = Math.Max(1, cw); ch = Math.Max(1, ch);
            var crop = new CroppedBitmap(flat, new Int32Rect(x, y, cw, ch));
            int blk = Math.Max(8, flat.PixelWidth / 160);
            var small = new TransformedBitmap(crop, new ScaleTransform(1.0 / blk, 1.0 / blk));
            var brush = new ImageBrush(small) { Stretch = Stretch.Fill };
            RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.NearestNeighbor);
            var rect = new Rectangle { Width = cw, Height = ch, Fill = brush };
            RenderOptions.SetBitmapScalingMode(rect, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(rect, x); Canvas.SetTop(rect, y);
            return rect;
        }

        // ---------------------------------------------------------------- text

        void StartText(Point p)
        {
            var tb = new TextBox
            {
                FontSize = 12 + thick * 3.3,
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.SemiBold,
                Foreground = Solid(color),
                CaretBrush = Solid(color),
                Background = new SolidColorBrush(Color.FromArgb(0x30, 0, 0, 0)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(3),
                MinWidth = 60,
                AcceptsReturn = true
            };
            Canvas.SetLeft(tb, p.X);
            Canvas.SetTop(tb, p.Y);
            ann.Children.Add(tb);
            activeText = tb;
            tb.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0) { e.Handled = true; CommitText(); }
                else if (e.Key == Key.Escape) { e.Handled = true; activeText = null; ann.Children.Remove(tb); }
            };
            tb.LostKeyboardFocus += (s, e) => { if (activeText == tb) CommitText(); };
            tb.Dispatcher.BeginInvoke(new Action(() => { tb.Focus(); Keyboard.Focus(tb); }), DispatcherPriority.Input);
        }

        void CommitText()
        {
            var tb = activeText;
            if (tb == null) return;
            activeText = null;
            ann.Children.Remove(tb);
            if (string.IsNullOrWhiteSpace(tb.Text)) return;
            var t = new TextBlock
            {
                Text = tb.Text,
                FontSize = tb.FontSize,
                FontFamily = tb.FontFamily,
                FontWeight = tb.FontWeight,
                Foreground = tb.Foreground
            };
            Canvas.SetLeft(t, Canvas.GetLeft(tb) + 4);
            Canvas.SetTop(t, Canvas.GetTop(tb) + 4);
            ann.Children.Add(t);
            Push(() => ann.Children.Remove(t), () => ann.Children.Add(t));
        }

        // ---------------------------------------------------------------- crop

        double ViewScale()
        {
            var t = wrap.TransformToVisual(stage);
            return Math.Max(1e-6, t.Transform(new Point(1, 0)).X - t.Transform(new Point(0, 0)).X);
        }

        int EdgesAt(Point p, Rect r, double m)
        {
            int e = 0;
            bool inX = p.X >= r.Left - m && p.X <= r.Right + m, inY = p.Y >= r.Top - m && p.Y <= r.Bottom + m;
            if (!inX || !inY) return -1;
            if (Math.Abs(p.X - r.Left) <= m) e |= 1;
            if (Math.Abs(p.X - r.Right) <= m) e |= 4;
            if (Math.Abs(p.Y - r.Top) <= m) e |= 2;
            if (Math.Abs(p.Y - r.Bottom) <= m) e |= 8;
            return e;
        }

        void CropDown(Point p)
        {
            dragStart = p;
            if (crop.HasValue)
            {
                var r = crop.Value;
                int e = EdgesAt(p, r, 12 / ViewScale());
                if (e > 0) { dragMode = 3; edges = e; cropStart = r; return; }
                if (r.Contains(p)) { dragMode = 2; cropStart = r; return; }
            }
            dragMode = 1;
            crop = new Rect(p, new Size(0, 0));
        }

        void CropMove(Point p, bool down)
        {
            if (!down || dragMode == 0)
            {
                if (crop.HasValue)
                {
                    int e = EdgesAt(p, crop.Value, 12 / ViewScale());
                    over.Cursor = e <= 0 ? (crop.Value.Contains(p) ? Cursors.SizeAll : Cursors.Cross)
                        : (e == 1 || e == 4) ? Cursors.SizeWE
                        : (e == 2 || e == 8) ? Cursors.SizeNS
                        : (e == 3 || e == 12) ? Cursors.SizeNWSE : Cursors.SizeNESW;
                }
                return;
            }
            if (dragMode == 1) crop = new Rect(dragStart, p);
            else if (dragMode == 2)
            {
                var d = p - dragStart;
                var r = cropStart;
                r.X = Math.Max(0, Math.Min(wrap.Width - r.Width, r.X + d.X));
                r.Y = Math.Max(0, Math.Min(wrap.Height - r.Height, r.Y + d.Y));
                crop = r;
            }
            else
            {
                double l = cropStart.Left, t = cropStart.Top, rt = cropStart.Right, b = cropStart.Bottom;
                if ((edges & 1) != 0) l = p.X;
                if ((edges & 4) != 0) rt = p.X;
                if ((edges & 2) != 0) t = p.Y;
                if ((edges & 8) != 0) b = p.Y;
                crop = new Rect(new Point(l, t), new Point(rt, b));
            }
            DrawCrop();
        }

        void CropUp()
        {
            dragMode = 0;
            if (crop.HasValue && (crop.Value.Width < 6 || crop.Value.Height < 6)) crop = null;
            DrawCrop();
        }

        void ClearCrop()
        {
            crop = null;
            dragMode = 0;
            if (over != null) over.Children.Clear();
            if (cropBar != null) cropBar.Visibility = Visibility.Collapsed;
        }

        void DrawCrop()
        {
            over.Children.Clear();
            if (!crop.HasValue || crop.Value.Width < 1 || crop.Value.Height < 1)
            {
                cropBar.Visibility = Visibility.Collapsed;
                return;
            }
            var r = crop.Value;
            double s = ViewScale();
            var geo = new GeometryGroup { FillRule = FillRule.EvenOdd };
            geo.Children.Add(new RectangleGeometry(new Rect(0, 0, wrap.Width, wrap.Height)));
            geo.Children.Add(new RectangleGeometry(r));
            over.Children.Add(new Path { Data = geo, Fill = new SolidColorBrush(Color.FromArgb(0xA0, 0, 0, 0)), IsHitTestVisible = false });
            var outline = new Rectangle { Stroke = Brushes.White, StrokeThickness = 1.5 / s, Width = r.Width, Height = r.Height, IsHitTestVisible = false };
            Canvas.SetLeft(outline, r.X); Canvas.SetTop(outline, r.Y);
            over.Children.Add(outline);

            double hs = 10 / s;
            var accent = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6));
            double[] xs = { r.Left, r.Left + r.Width / 2, r.Right }, ys = { r.Top, r.Top + r.Height / 2, r.Bottom };
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    if (i == 1 && j == 1) continue;
                    var h = new Rectangle { Width = hs, Height = hs, Fill = Brushes.White, Stroke = accent, StrokeThickness = 1.5 / s, IsHitTestVisible = false };
                    Canvas.SetLeft(h, xs[i] - hs / 2); Canvas.SetTop(h, ys[j] - hs / 2);
                    over.Children.Add(h);
                }

            status.Text = "Crop: " + (int)Math.Round(r.Width) + " × " + (int)Math.Round(r.Height) + " px   ·   Enter applies, Esc cancels";
            cropBar.Visibility = Visibility.Visible;
        }

        void ApplyCrop()
        {
            if (!crop.HasValue) return;
            CommitText();
            var r = crop.Value;
            var flat = Flatten();
            int x = Math.Max(0, (int)Math.Round(r.X)), y = Math.Max(0, (int)Math.Round(r.Y));
            int cw = Math.Min(flat.PixelWidth - x, (int)Math.Round(r.Width)), ch = Math.Min(flat.PixelHeight - y, (int)Math.Round(r.Height));
            if (cw < 2 || ch < 2) return;
            var wb = new WriteableBitmap(new CroppedBitmap(flat, new Int32Rect(x, y, cw, ch)));
            wb.Freeze();

            var oldSrc = src;
            var oldKids = ann.Children.Cast<UIElement>().ToList();
            Action doIt = () => { SetImage(wb); ann.Children.Clear(); };
            Action undo = () => { SetImage(oldSrc); ann.Children.Clear(); foreach (var k in oldKids) ann.Children.Add(k); };
            doIt();
            Push(undo, doIt);
            status.Text = "Cropped to " + cw + " × " + ch + " px";
        }

        // ---------------------------------------------------------------- history

        void Push(Action undo, Action redo)
        {
            if (hp < hist.Count) hist.RemoveRange(hp, hist.Count - hp);
            hist.Add((undo, redo));
            hp++;
            dirty = true;
            UpdateUndoButtons();
        }

        void Undo() { CommitText(); if (hp == 0) return; hist[--hp].undo(); UpdateUndoButtons(); }
        void Redo() { CommitText(); if (hp >= hist.Count) return; hist[hp++].redo(); UpdateUndoButtons(); }

        void UpdateUndoButtons()
        {
            undoBtn.IsEnabled = hp > 0;
            redoBtn.IsEnabled = hp < hist.Count;
        }

        // ---------------------------------------------------------------- output

        BitmapSource Flatten()
        {
            CommitText();
            int pw = src.PixelWidth, ph = src.PixelHeight;
            var holder = F<Grid>("Holder");
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
                dc.DrawRectangle(new VisualBrush(holder), null, new Rect(0, 0, pw, ph));
            var rtb = new RenderTargetBitmap(pw, ph, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            rtb.Freeze();
            return rtb;
        }

        void CopyImage()
        {
            Util.CopyToClipboard(Flatten(), null);
            status.Text = "Copied to the clipboard";
        }

        void SaveAs()
        {
            var dlg = new SaveFileDialog
            {
                FileName = System.IO.Path.GetFileName(path),
                Filter = "PNG image|*.png|JPEG image|*.jpg",
                InitialDirectory = System.IO.Path.GetDirectoryName(path)
            };
            if (dlg.ShowDialog(w) != true) return;
            var flat = Flatten();
            if (dlg.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)) Util.SaveJpg(flat, dlg.FileName);
            else Util.SavePng(flat, dlg.FileName);
            status.Text = "Saved " + dlg.FileName;
        }

        void SaveAndClose()
        {
            if (!dirty) { allowClose = true; w.Close(); return; }
            try
            {
                var flat = Flatten();
                Util.SaveByExtension(flat, path);
                ScreenshotStore.OnEdited(path);
                if (Settings.CopyToClipboard) Util.CopyToClipboard(flat, path);
                handoff = Settings.ShowThumbnail;   // the new thumbnail will finalize it
                allowClose = true;
                w.Close();
                if (Settings.ShowThumbnail) ThumbWindow.Show(path, flat);
            }
            catch (Exception ex) { MessageBox.Show(w, ex.Message, "Could not save"); }
        }

        void DragOutEdited()
        {
            try
            {
                var flat = Flatten();
                string dir = System.IO.Path.Combine(Util.DragTempDir, Guid.NewGuid().ToString("N").Substring(0, 8));
                System.IO.Directory.CreateDirectory(dir);
                string tmp = System.IO.Path.Combine(dir, System.IO.Path.GetFileName(path));
                Util.SaveByExtension(flat, tmp);
                if (DragOut.Start(dragChip, tmp, flat) != DragDropEffects.None)
                {
                    handoff = true;
                    ScreenshotStore.MarkUsed(path);   // dragged somewhere: no extra copy in the screenshots folder
                }
            }
            catch (Exception ex) { status.Text = "Drag failed: " + ex.Message; }
        }

        void OnKey(object s, KeyEventArgs e)
        {
            if (activeText != null) return;
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            if (ctrl && e.Key == Key.Z) { if (shift) Redo(); else Undo(); e.Handled = true; }
            else if (ctrl && e.Key == Key.Y) { Redo(); e.Handled = true; }
            else if (ctrl && e.Key == Key.C) { CopyImage(); e.Handled = true; }
            else if (ctrl && e.Key == Key.S) { SaveAndClose(); e.Handled = true; }
            else if (e.Key == Key.Enter && crop.HasValue) { ApplyCrop(); e.Handled = true; }
            else if (e.Key == Key.Escape && crop.HasValue) { ClearCrop(); e.Handled = true; }
        }
    }
}
