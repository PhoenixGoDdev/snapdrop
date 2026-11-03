using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WF = System.Windows.Forms;

namespace SnapDrop
{
    /// <summary>The macOS-style floating preview that slides in at the bottom-right after a capture.</summary>
    class ThumbWindow
    {
        static readonly List<ThumbWindow> Stack = new List<ThumbWindow>();
        readonly string path;
        readonly BitmapSource bmp;
        Window w;
        Border card, hint;
        Button closeBtn;
        DispatcherTimer timer;
        bool pressed, dragging, gone;
        Point downPt;
        double top;

        ThumbWindow(string path, BitmapSource bmp) { this.path = path; this.bmp = bmp; }

        public static void Show(string path, BitmapSource bmp)
        {
            while (Stack.Count >= 5) Stack[0].Dismiss(false);
            var t = new ThumbWindow(path, bmp);
            Stack.Add(t);
            t.Open();
        }

        public static void HideAll() { foreach (var t in Stack) t.w.Visibility = Visibility.Hidden; }
        public static void ShowAll() { foreach (var t in Stack) t.w.Visibility = Visibility.Visible; }

        static void WorkArea(out double right, out double bottom)
        {
            var wa = WF.Screen.PrimaryScreen.WorkingArea;
            double sc = Native.SystemScale();
            right = wa.Right / sc;
            bottom = wa.Bottom / sc;
        }

        void Open()
        {
            w = (Window)XamlReader.Parse(Xaml.Thumb.Replace("{STYLES}", Xaml.Styles));
            card = (Border)w.FindName("Card");
            hint = (Border)w.FindName("Hint");
            closeBtn = (Button)w.FindName("CloseBtn");
            var pic = (Image)w.FindName("Pic");

            double k = Math.Min(Math.Min(220.0 / bmp.PixelWidth, 150.0 / bmp.PixelHeight), 2.0);
            pic.Width = Math.Max(48, bmp.PixelWidth * k);
            pic.Height = Math.Max(32, bmp.PixelHeight * k);
            pic.Source = bmp;
            w.Width = pic.Width + 42;
            w.Height = pic.Height + 42;

            w.SourceInitialized += (s, e) =>
                Native.AddExStyle(new WindowInteropHelper(w).Handle, Native.WS_EX_NOACTIVATE | Native.WS_EX_TOOLWINDOW);

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(9) };
            timer.Tick += (s, e) => Dismiss(true);

            w.MouseEnter += (s, e) => { timer.Stop(); Fade(hint, 1); Fade(closeBtn, 1); };
            w.MouseLeave += (s, e) =>
            {
                Fade(hint, 0); Fade(closeBtn, 0);
                if (!dragging && !gone) { timer.Interval = TimeSpan.FromSeconds(3); timer.Start(); }
            };
            card.MouseLeftButtonDown += (s, e) => { pressed = true; downPt = e.GetPosition(w); };
            card.MouseMove += (s, e) =>
            {
                if (!pressed || e.LeftButton != MouseButtonState.Pressed) return;
                var d = e.GetPosition(w) - downPt;
                if (Math.Abs(d.X) > 4 || Math.Abs(d.Y) > 4) { pressed = false; StartDrag(); }
            };
            card.MouseLeave += (s, e) =>
            {
                if (pressed && Mouse.LeftButton == MouseButtonState.Pressed) { pressed = false; StartDrag(); }
                pressed = false;
            };
            card.MouseLeftButtonUp += (s, e) =>
            {
                if (!pressed) return;
                pressed = false;
                OpenEditor();
            };
            closeBtn.Click += (s, e) => Dismiss(true);
            card.ContextMenu = BuildMenu();

            WorkArea(out double right, out double bottom);
            double finalLeft = right - w.Width + 6;
            w.WindowStartupLocation = WindowStartupLocation.Manual;
            w.Left = right + 30;
            LayoutAll(this, bottom);
            w.Top = top;
            w.Show();
            Util.Tween(w, Window.LeftProperty, finalLeft, 280);
            timer.Start();
        }

        static void LayoutAll(ThumbWindow except, double bottom)
        {
            double y = bottom + 6;
            for (int i = Stack.Count - 1; i >= 0; i--)
            {
                var t = Stack[i];
                t.top = y - t.w.Height;
                y = t.top + 20;
                if (t != except && t.w.IsVisible) Util.Tween(t.w, Window.TopProperty, t.top, 220);
            }
        }

        static void Fade(UIElement el, double to)
        {
            el.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(to, TimeSpan.FromMilliseconds(140)));
        }

        ContextMenu BuildMenu()
        {
            var m = new ContextMenu();
            Action<string, Action> add = (header, act) =>
            {
                var mi = new MenuItem { Header = header };
                mi.Click += (s, e) => act();
                m.Items.Add(mi);
            };
            add("Edit / crop…", OpenEditor);
            add("Copy", () => Util.CopyToClipboard(bmp, path));
            add("Copy file path", () => { try { Clipboard.SetText(path); } catch { } });
            add("Save as…", SaveAs);
            add("Show in folder", () => Util.ShowInFolder(path));
            m.Items.Add(new Separator());
            add("Delete", () => { try { File.Delete(path); } catch { } Dismiss(true); });
            add("Close", () => Dismiss(true));
            m.Opened += (s, e) => timer.Stop();
            return m;
        }

        void SaveAs()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = Path.GetFileName(path),
                Filter = "PNG image|*.png|JPEG image|*.jpg",
                InitialDirectory = Path.GetDirectoryName(path)
            };
            if (dlg.ShowDialog() != true) return;
            if (dlg.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)) Util.SaveJpg(bmp, dlg.FileName);
            else Util.SavePng(bmp, dlg.FileName);
        }

        void OpenEditor()
        {
            Editor.Open(path, bmp);
            Dismiss(true);
        }

        void StartDrag()
        {
            dragging = true;
            timer.Stop();
            w.Opacity = 0.35;
            DragDropEffects eff = DragDropEffects.None;
            try { eff = DragOut.Start(card, path, bmp); }
            finally
            {
                dragging = false;
                if (!gone) w.Opacity = 1;
            }
            // Windows Terminal (esp. when everything runs as admin, as with UAC off) refuses file drops:
            // behave like macOS and paste the screenshot's path into the terminal instead.
            if (eff == DragDropEffects.None && TerminalDrop.TryPaste(path, bmp)) { Dismiss(true); return; }
            if (eff != DragDropEffects.None) Dismiss(true);
            else if (!gone) { timer.Interval = TimeSpan.FromSeconds(4); timer.Start(); }
        }

        void Dismiss(bool animate)
        {
            if (gone) return;
            gone = true;
            if (timer != null) timer.Stop();
            Stack.Remove(this);
            WorkArea(out double right, out double bottom);
            LayoutAll(null, bottom);
            if (!animate || !w.IsVisible) { w.Close(); return; }
            Util.Tween(w, Window.LeftProperty, right + 30, 220, () => w.Close());
        }
    }

    /// <summary>Starts a real OLE drag with the file (and bitmap) so it can be dropped into any app.</summary>
    static class DragOut
    {
        public static DragDropEffects Start(UIElement src, string path, BitmapSource preview)
        {
            var data = new DataObject();
            data.SetFileDropList(new StringCollection { path });
            if (preview != null) data.SetImage(preview);
            // Terminals (Windows Terminal, conhost, Claude Code CLI) take text drops and paste them: give them the path.
            data.SetText(path.IndexOf(' ') >= 0 ? "\"" + path + "\"" : path);

            var ghost = new DragGhost(preview);
            QueryContinueDragEventHandler h = (s, e) => ghost.Follow();
            DragDrop.AddQueryContinueDragHandler(src, h);
            try
            {
                ghost.Begin();
                return DragDrop.DoDragDrop(src, data, DragDropEffects.Copy);   // Copy only: never lets a target "move" our file away
            }
            finally
            {
                DragDrop.RemoveQueryContinueDragHandler(src, h);
                ghost.Dispose();
            }
        }
    }

    /// <summary>A small click-through preview that follows the cursor during a drag.</summary>
    class DragGhost
    {
        readonly Window w;
        readonly IntPtr hwnd;

        public DragGhost(BitmapSource bmp)
        {
            double k = bmp == null ? 1 : Math.Min(Math.Min(150.0 / bmp.PixelWidth, 100.0 / bmp.PixelHeight), 1.0);
            w = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ShowInTaskbar = false,
                Topmost = true,
                ShowActivated = false,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
            };
            if (bmp != null)
            {
                w.Content = new Border
                {
                    Margin = new Thickness(8),
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(2),
                    Opacity = 0.9,
                    Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.6 },
                    Child = new Image { Source = bmp, Width = Math.Max(24, bmp.PixelWidth * k), Height = Math.Max(24, bmp.PixelHeight * k) }
                };
            }
            hwnd = new WindowInteropHelper(w).EnsureHandle();
            Native.AddExStyle(hwnd, Native.WS_EX_TRANSPARENT | Native.WS_EX_NOACTIVATE | Native.WS_EX_TOOLWINDOW);
        }

        public void Begin() { w.Show(); Follow(); }

        public void Follow()
        {
            Native.GetCursorPos(out var p);
            Native.SetWindowPos(hwnd, Native.HWND_TOPMOST, p.X + 14, p.Y + 14, 0, 0, Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
        }

        public void Dispose() { try { w.Close(); } catch { } }
    }
}

namespace SnapDrop
{
    /// <summary>When a drop lands on a terminal window that rejects it, paste the file path there (what dragging a file into a terminal does on macOS).</summary>
    static class TerminalDrop
    {
        static readonly string[] Terminals =
        {
            "WindowsTerminal", "WindowsTerminalPreview", "OpenConsole", "conhost",
            "wezterm-gui", "alacritty", "mintty", "ConEmu64", "ConEmu", "kitty"
        };

        public static bool TryPaste(string path, System.Windows.Media.Imaging.BitmapSource img)
        {
            Native.GetCursorPos(out var pt);
            IntPtr h = Native.WindowFromPoint(pt);
            if (h == IntPtr.Zero) return false;
            IntPtr root = Native.GetAncestor(h, 2);   // GA_ROOT
            if (root == IntPtr.Zero) return false;
            Native.GetWindowThreadProcessId(root, out uint pid);
            string name;
            try { name = System.Diagnostics.Process.GetProcessById((int)pid).ProcessName; } catch { return false; }
            if (Array.IndexOf(Terminals, name) < 0) return false;

            string text = path.IndexOf(' ') >= 0 ? "\"" + path + "\"" : path;
            try { Clipboard.SetText(text); } catch { return false; }

            Native.SetForegroundWindow(root);
            if (Native.GetForegroundWindow() != root)
            {
                // foreground-lock workaround: a tap on Alt lets us take focus
                Native.keybd_event(0x12, 0, 0, UIntPtr.Zero);
                Native.keybd_event(0x12, 0, 2, UIntPtr.Zero);
                Native.SetForegroundWindow(root);
            }
            Util.Later(() =>
            {
                Native.keybd_event(0x11, 0, 0, UIntPtr.Zero);   // Ctrl down
                Native.keybd_event(0x56, 0, 0, UIntPtr.Zero);   // V
                Native.keybd_event(0x56, 0, 2, UIntPtr.Zero);
                Native.keybd_event(0x11, 0, 2, UIntPtr.Zero);   // Ctrl up
                // put the image + file back on the clipboard once the terminal has read the text
                Util.Later(() => Util.CopyToClipboard(img, path), 700);
            }, 180);
            return true;
        }
    }
}
