using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SD = System.Drawing;
using SDI = System.Drawing.Imaging;

namespace SnapDrop
{
    static class Util
    {
        /// <summary>Grabs a rectangle of the screen in physical pixels.</summary>
        public static BitmapSource Grab(int x, int y, int w, int h)
        {
            using (var bmp = new SD.Bitmap(w, h, SDI.PixelFormat.Format32bppRgb))
            {
                using (var g = SD.Graphics.FromImage(bmp))
                {
                    IntPtr dst = g.GetHdc();
                    IntPtr scr = Native.GetDC(IntPtr.Zero);
                    try { Native.BitBlt(dst, 0, 0, w, h, scr, x, y, 0x40CC0020); }   // SRCCOPY | CAPTUREBLT (includes layered windows)
                    finally { Native.ReleaseDC(IntPtr.Zero, scr); g.ReleaseHdc(dst); }
                }
                var data = bmp.LockBits(new SD.Rectangle(0, 0, w, h), SDI.ImageLockMode.ReadOnly, SDI.PixelFormat.Format32bppRgb);
                try
                {
                    var src = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgr32, null, data.Scan0, data.Stride * h, data.Stride);
                    src.Freeze();
                    return src;
                }
                finally { bmp.UnlockBits(data); }
            }
        }

        public static void SavePng(BitmapSource s, string path)
        {
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(s));
            using (var fs = new FileStream(path, FileMode.Create)) enc.Save(fs);
        }

        public static void SaveJpg(BitmapSource s, string path)
        {
            var conv = new FormatConvertedBitmap(s, PixelFormats.Bgr32, null, 0);
            var enc = new JpegBitmapEncoder { QualityLevel = 92 };
            enc.Frames.Add(BitmapFrame.Create(conv));
            using (var fs = new FileStream(path, FileMode.Create)) enc.Save(fs);
        }

        /// <summary>Puts the image (and, optionally, the file itself) on the clipboard so it pastes into chat apps, docs and Explorer.</summary>
        public static void CopyToClipboard(BitmapSource img, string filePath)
        {
            for (int i = 0; i < 6; i++)
            {
                try
                {
                    var d = new DataObject();
                    d.SetImage(img);
                    if (filePath != null) d.SetFileDropList(new StringCollection { filePath });
                    Clipboard.SetDataObject(d, true);
                    return;
                }
                catch { Thread.Sleep(60); }
            }
        }

        public static void Tween(DependencyObject o, DependencyProperty p, double to, int ms, Action done = null)
        {
            var target = (IAnimatable)o;
            double from = (double)o.GetValue(p);
            var a = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(ms)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            a.Completed += (s, e) =>
            {
                target.BeginAnimation(p, null);
                o.SetValue(p, to);
                if (done != null) done();
            };
            target.BeginAnimation(p, a);
        }

        public static void FlushUI()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        }

        public static void Later(Action a, int ms)
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
            t.Tick += (s, e) => { t.Stop(); a(); };
            t.Start();
        }

        public static string DragTempDir
        {
            get { return Path.Combine(Path.GetTempPath(), "SnapDrop", "drag"); }
        }

        public static void CleanTemp()
        {
            try
            {
                if (!Directory.Exists(DragTempDir)) return;
                foreach (var d in Directory.GetDirectories(DragTempDir))
                    if (Directory.GetCreationTime(d) < DateTime.Now.AddDays(-1)) Directory.Delete(d, true);
            }
            catch { }
        }

        public static void ShowInFolder(string path)
        {
            try { Process.Start("explorer.exe", "/select,\"" + path + "\""); } catch { }
        }

        public static void OpenFolder(string dir)
        {
            try { Directory.CreateDirectory(dir); Process.Start("explorer.exe", "\"" + dir + "\""); } catch { }
        }

        public static string PickFolder(string current)
        {
            using (var d = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = current,
                Description = "Where should screenshots be saved?",
                UseDescriptionForTitle = true
            })
                return d.ShowDialog() == System.Windows.Forms.DialogResult.OK ? d.SelectedPath : null;
        }
    }
}
