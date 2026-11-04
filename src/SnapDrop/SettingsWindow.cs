using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using WF = System.Windows.Forms;

namespace SnapDrop
{
    /// <summary>The Settings window (tray icon → Settings…). Changes apply when Save is pressed.</summary>
    class SettingsWindow
    {
        public const string RepoUrl = "https://github.com/PhoenixGoDdev/snapdrop";

        static SettingsWindow current;
        Window w;
        string folder;

        public static void ShowOrActivate()
        {
            if (current != null)
            {
                if (current.w.WindowState == WindowState.Minimized) current.w.WindowState = WindowState.Normal;
                current.w.Activate();
                return;
            }
            current = new SettingsWindow();
            current.Open();
        }

        T F<T>(string name) where T : class { return (T)w.FindName(name); }

        // ------------------------------------------------------------------ build

        void Open()
        {
            w = (Window)XamlReader.Parse(Xaml.SettingsWindow.Replace("{STYLES}", Xaml.Styles));
            w.Closed += (s, e) => current = null;
            if (Util.AppIcon != null)
            {
                w.Icon = Util.AppIcon;
                F<Image>("BrandLogo").Source = Util.AppIcon;
                F<Image>("AboutLogo").Source = Util.AppIcon;
            }
            F<TextBlock>("BrandVersion").Text = "Version " + SnapApp.Version;
            F<TextBlock>("AboutVersion").Text = "Version " + SnapApp.Version;

            // centre on the primary work area
            var wa = WF.Screen.PrimaryScreen.WorkingArea;
            double sc = Native.SystemScale(), aw = wa.Width / sc, ah = wa.Height / sc;
            w.Width = Math.Min(w.Width, aw * 0.95);
            w.Height = Math.Min(w.Height, ah * 0.92);
            w.WindowStartupLocation = WindowStartupLocation.Manual;
            w.Left = wa.Left / sc + (aw - w.Width) / 2;
            w.Top = wa.Top / sc + (ah - w.Height) / 2;

            WireNavigation();
            WireControls();
            LoadFromSettings();

            w.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape && !(Keyboard_IsHotkeyBoxFocused())) { w.Close(); e.Handled = true; }
            };
            w.Topmost = true; w.Show(); w.Activate(); w.Topmost = false;
        }

        bool Keyboard_IsHotkeyBoxFocused()
        {
            var f = System.Windows.Input.Keyboard.FocusedElement as TextBox;
            return f != null && f.IsReadOnly;
        }

        void WireNavigation()
        {
            var pages = new Dictionary<string, string>
            {
                { "NavGeneral", "PGeneral" }, { "NavShortcuts", "PShortcuts" }, { "NavThumb", "PThumb" }, { "NavAbout", "PAbout" }
            };
            foreach (var kv in pages)
            {
                var nav = F<RadioButton>(kv.Key);
                string target = kv.Value;
                nav.Checked += (s, e) =>
                {
                    foreach (var p in pages.Values) F<StackPanel>(p).Visibility = p == target ? Visibility.Visible : Visibility.Collapsed;
                };
            }
        }

        void WireControls()
        {
            F<Button>("BtnCancel").Click += (s, e) => w.Close();
            F<Button>("BtnSave").Click += (s, e) => Save();

            F<Button>("BtnBrowse").Click += (s, e) =>
            {
                var d = Util.PickFolder(folder);
                if (d != null) { folder = d; F<TextBox>("TxtFolder").Text = d; }
            };
            F<Button>("BtnOpenFolder").Click += (s, e) => Util.OpenFolder(folder);

            var prefix = F<TextBox>("TxtPrefix");
            prefix.TextChanged += (s, e) => UpdateExample();
            F<RadioButton>("FmtPng").Checked += (s, e) => UpdateExample();
            F<RadioButton>("FmtJpg").Checked += (s, e) => UpdateExample();

            foreach (var n in new[] { "HkRegion", "HkScreen", "HkWindow", "HkBar" })
                HotkeyBox.Attach(F<TextBox>(n), () => F<TextBlock>("Status").Text = "");
            F<Button>("ClrRegion").Click += (s, e) => F<TextBox>("HkRegion").Text = "";
            F<Button>("ClrScreen").Click += (s, e) => F<TextBox>("HkScreen").Text = "";
            F<Button>("ClrWindow").Click += (s, e) => F<TextBox>("HkWindow").Text = "";
            F<Button>("ClrBar").Click += (s, e) => F<TextBox>("HkBar").Text = "";
            F<Button>("BtnResetKeys").Click += (s, e) =>
            {
                var keep = Snapshot();
                Settings.ResetDefaults();
                F<TextBox>("HkRegion").Text = Settings.HotkeyRegion;
                F<TextBox>("HkScreen").Text = Settings.HotkeyScreen;
                F<TextBox>("HkWindow").Text = Settings.HotkeyWindow;
                F<TextBox>("HkBar").Text = Settings.HotkeyBar;
                F<CheckBox>("SwPrint").IsChecked = Settings.PrintScreenKey;
                Restore(keep);   // only the shortcut boxes change on screen; nothing is saved until Save
            };

            F<Button>("BtnGithub").Click += (s, e) => OpenUrl(RepoUrl);
            F<Button>("BtnIssues").Click += (s, e) => OpenUrl(RepoUrl + "/issues");
            F<Button>("BtnDataFolder").Click += (s, e) => Util.OpenFolder(Settings.DataFolder);
            F<Button>("BtnResetAll").Click += (s, e) =>
            {
                if (MessageBox.Show(w, "Reset every setting to its default?\nYour screenshots are not touched.", "SnapDrop", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;
                Settings.ResetDefaults();
                LoadFromSettings();
                F<TextBlock>("Status").Text = "Defaults restored. Press Save to apply.";
            };
        }

        static void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
        }

        // ------------------------------------------------------------------ settings <-> controls

        // ResetDefaults() mutates the static settings, so the shortcut-reset button snapshots and restores them.
        object[] Snapshot()
        {
            return new object[] { Settings.SaveFolder, Settings.FilePrefix, Settings.ImageFormat, Settings.CopyToClipboard, Settings.TimerSeconds,
                Settings.ShowThumbnail, Settings.ThumbCorner, Settings.ThumbSeconds, Settings.TerminalPaste,
                Settings.HotkeyRegion, Settings.HotkeyScreen, Settings.HotkeyWindow, Settings.HotkeyBar, Settings.PrintScreenKey, Settings.SavePolicy };
        }

        void Restore(object[] v)
        {
            Settings.SaveFolder = (string)v[0]; Settings.FilePrefix = (string)v[1]; Settings.ImageFormat = (string)v[2];
            Settings.CopyToClipboard = (bool)v[3]; Settings.TimerSeconds = (int)v[4]; Settings.ShowThumbnail = (bool)v[5];
            Settings.ThumbCorner = (string)v[6]; Settings.ThumbSeconds = (int)v[7]; Settings.TerminalPaste = (bool)v[8];
            Settings.HotkeyRegion = (string)v[9]; Settings.HotkeyScreen = (string)v[10]; Settings.HotkeyWindow = (string)v[11];
            Settings.HotkeyBar = (string)v[12]; Settings.PrintScreenKey = (bool)v[13]; Settings.SavePolicy = (string)v[14];
        }

        void LoadFromSettings()
        {
            folder = Settings.SaveFolder;
            F<CheckBox>("SwStartup").IsChecked = Settings.StartWithWindows;
            F<TextBox>("TxtFolder").Text = Settings.SaveFolder;
            F<TextBox>("TxtPrefix").Text = Settings.FilePrefix;
            F<RadioButton>(Settings.ImageFormat == "jpg" ? "FmtJpg" : "FmtPng").IsChecked = true;
            F<RadioButton>(Settings.SavePolicy == "always" ? "PolAlways" : "PolSmart").IsChecked = true;
            F<CheckBox>("SwClipboard").IsChecked = Settings.CopyToClipboard;
            F<RadioButton>(Settings.TimerSeconds == 3 ? "Tm3" : Settings.TimerSeconds == 5 ? "Tm5" : Settings.TimerSeconds == 10 ? "Tm10" : "Tm0").IsChecked = true;

            F<TextBox>("HkRegion").Text = Settings.HotkeyRegion;
            F<TextBox>("HkScreen").Text = Settings.HotkeyScreen;
            F<TextBox>("HkWindow").Text = Settings.HotkeyWindow;
            F<TextBox>("HkBar").Text = Settings.HotkeyBar;
            F<CheckBox>("SwPrint").IsChecked = Settings.PrintScreenKey;

            F<CheckBox>("SwThumb").IsChecked = Settings.ShowThumbnail;
            F<CheckBox>("SwTerminal").IsChecked = Settings.TerminalPaste;
            string corner = Settings.ThumbCorner;
            F<RadioButton>(corner == "TopLeft" ? "CornTL" : corner == "TopRight" ? "CornTR" : corner == "BottomLeft" ? "CornBL" : "CornBR").IsChecked = true;
            int d = Settings.ThumbSeconds;
            F<RadioButton>(d <= 5 ? "Dur5" : d <= 9 ? "Dur9" : d <= 15 ? "Dur15" : "Dur30").IsChecked = true;
            UpdateExample();
        }

        void UpdateExample()
        {
            string prefix = Settings.Sanitize(F<TextBox>("TxtPrefix").Text);
            string ext = F<RadioButton>("FmtJpg").IsChecked == true ? ".jpg" : ".png";
            F<TextBlock>("TxtExample").Text = "Example: " + prefix + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ext + "   (spaces become underscores)";
        }

        bool Checked(string name) { return F<CheckBox>(name).IsChecked == true; }
        bool Radio(string name) { return F<RadioButton>(name).IsChecked == true; }

        void Save()
        {
            var status = F<TextBlock>("Status");
            status.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF8E949C");

            // the same shortcut twice can never work
            var keys = new[] { "HkRegion", "HkScreen", "HkWindow", "HkBar" };
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var k in keys)
            {
                string t = F<TextBox>(k).Text.Trim();
                if (t.Length > 0 && !seen.Add(t))
                {
                    Fail("The shortcut " + t + " is used twice. Give each action its own keys.");
                    F<RadioButton>("NavShortcuts").IsChecked = true;
                    return;
                }
            }

            Settings.SaveFolder = folder;
            Settings.FilePrefix = Settings.Sanitize(F<TextBox>("TxtPrefix").Text);
            Settings.ImageFormat = Radio("FmtJpg") ? "jpg" : "png";
            Settings.SavePolicy = Radio("PolAlways") ? "always" : "smart";
            Settings.CopyToClipboard = Checked("SwClipboard");
            Settings.TimerSeconds = Radio("Tm3") ? 3 : Radio("Tm5") ? 5 : Radio("Tm10") ? 10 : 0;

            Settings.HotkeyRegion = F<TextBox>("HkRegion").Text.Trim();
            Settings.HotkeyScreen = F<TextBox>("HkScreen").Text.Trim();
            Settings.HotkeyWindow = F<TextBox>("HkWindow").Text.Trim();
            Settings.HotkeyBar = F<TextBox>("HkBar").Text.Trim();
            Settings.PrintScreenKey = Checked("SwPrint");

            Settings.ShowThumbnail = Checked("SwThumb");
            Settings.TerminalPaste = Checked("SwTerminal");
            Settings.ThumbCorner = Radio("CornTL") ? "TopLeft" : Radio("CornTR") ? "TopRight" : Radio("CornBL") ? "BottomLeft" : "BottomRight";
            Settings.ThumbSeconds = Radio("Dur5") ? 5 : Radio("Dur15") ? 15 : Radio("Dur30") ? 30 : 9;

            Settings.Save();
            try { System.IO.Directory.CreateDirectory(Settings.SaveFolder); } catch { }
            try { Settings.StartWithWindows = Checked("SwStartup"); } catch { }

            var app = Application.Current as SnapApp;
            var failed = app != null ? app.ApplyHotkeys() : new List<string>();
            if (failed.Count > 0)
            {
                Fail("Saved, but another app already uses: " + string.Join(", ", failed) + ". Pick different keys.");
                F<RadioButton>("NavShortcuts").IsChecked = true;
                return;
            }
            w.Close();
        }

        void Fail(string message)
        {
            var status = F<TextBlock>("Status");
            status.Text = message;
            status.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFFF6B6B");
        }
    }
}
