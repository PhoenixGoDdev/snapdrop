using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace SnapDrop
{
    /// <summary>User preferences, stored as simple key=value lines in %APPDATA%\SnapDrop\settings.ini.</summary>
    static class Settings
    {
        static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SnapDrop");
        static readonly string IniPath = Path.Combine(Dir, "settings.ini");
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static string DataFolder { get { return Dir; } }
        public static string FilePath { get { return IniPath; } }
        public static string LogPath { get { return Path.Combine(Dir, "error.log"); } }

        // ---- general
        public static string SaveFolder;
        public static string FilePrefix;
        public static string ImageFormat;        // "png" | "jpg"
        public static string SavePolicy;         // "smart" = keep a copy unless dragged out | "always"
        public static bool CopyToClipboard;
        public static int TimerSeconds;          // default capture delay: 0, 3, 5, 10

        // ---- thumbnail
        public static bool ShowThumbnail;
        public static string ThumbCorner;        // BottomRight | BottomLeft | TopRight | TopLeft
        public static int ThumbSeconds;          // how long it stays on screen
        public static bool TerminalPaste;        // dropping on a terminal pastes the file path

        // ---- shortcuts ("" = disabled)
        public static string HotkeyRegion;
        public static string HotkeyScreen;
        public static string HotkeyWindow;
        public static string HotkeyBar;
        public static bool PrintScreenKey;

        static Settings() { ResetDefaults(); }

        public static void ResetDefaults()
        {
            SaveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "SnapDrop");
            FilePrefix = "Screenshot";
            ImageFormat = "png";
            SavePolicy = "smart";
            CopyToClipboard = true;
            TimerSeconds = 0;
            ShowThumbnail = true;
            ThumbCorner = "BottomRight";
            ThumbSeconds = 9;
            TerminalPaste = true;
            HotkeyRegion = "Ctrl+Shift+S";
            HotkeyScreen = "Ctrl+Shift+3";
            HotkeyWindow = "";
            HotkeyBar = "Ctrl+Shift+5";
            PrintScreenKey = true;
        }

        public static void Load()
        {
            try
            {
                foreach (var line in File.ReadAllLines(IniPath))
                {
                    int i = line.IndexOf('=');
                    if (i < 0) continue;
                    string k = line.Substring(0, i).Trim(), v = line.Substring(i + 1).Trim();
                    switch (k)
                    {
                        case "SaveFolder": if (v.Length > 0) SaveFolder = v; break;
                        case "FilePrefix": FilePrefix = Sanitize(v); break;
                        case "ImageFormat": ImageFormat = v == "jpg" ? "jpg" : "png"; break;
                        case "SavePolicy": SavePolicy = v == "always" ? "always" : "smart"; break;
                        case "CopyToClipboard": CopyToClipboard = v == "1"; break;
                        case "TimerSeconds": int.TryParse(v, out TimerSeconds); break;
                        case "ShowThumbnail": ShowThumbnail = v == "1"; break;
                        case "ThumbCorner": if (Array.IndexOf(Corners, v) >= 0) ThumbCorner = v; break;
                        case "ThumbSeconds": if (int.TryParse(v, out int s) && s >= 2 && s <= 120) ThumbSeconds = s; break;
                        case "TerminalPaste": TerminalPaste = v == "1"; break;
                        case "HotkeyRegion": HotkeyRegion = v; break;
                        case "HotkeyScreen": HotkeyScreen = v; break;
                        case "HotkeyWindow": HotkeyWindow = v; break;
                        case "HotkeyBar": HotkeyBar = v; break;
                        case "PrintScreenKey": PrintScreenKey = v == "1"; break;
                    }
                }
            }
            catch { /* first run: defaults */ }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllLines(IniPath, new[]
                {
                    "SaveFolder=" + SaveFolder,
                    "FilePrefix=" + FilePrefix,
                    "ImageFormat=" + ImageFormat,
                    "SavePolicy=" + SavePolicy,
                    "CopyToClipboard=" + B(CopyToClipboard),
                    "TimerSeconds=" + TimerSeconds,
                    "ShowThumbnail=" + B(ShowThumbnail),
                    "ThumbCorner=" + ThumbCorner,
                    "ThumbSeconds=" + ThumbSeconds,
                    "TerminalPaste=" + B(TerminalPaste),
                    "HotkeyRegion=" + HotkeyRegion,
                    "HotkeyScreen=" + HotkeyScreen,
                    "HotkeyWindow=" + HotkeyWindow,
                    "HotkeyBar=" + HotkeyBar,
                    "PrintScreenKey=" + B(PrintScreenKey),
                });
            }
            catch { }
        }

        public static readonly string[] Corners = { "BottomRight", "BottomLeft", "TopRight", "TopLeft" };
        public static bool CornerIsRight { get { return ThumbCorner.EndsWith("Right"); } }
        public static bool CornerIsBottom { get { return ThumbCorner.StartsWith("Bottom"); } }
        public static string Extension { get { return ImageFormat == "jpg" ? ".jpg" : ".png"; } }

        static string B(bool v) { return v ? "1" : "0"; }

        /// <summary>Keeps the file-name prefix safe and free of spaces (spaces break pasted paths in terminals).</summary>
        public static string Sanitize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "Screenshot";
            var bad = Path.GetInvalidFileNameChars();
            var chars = new List<char>();
            foreach (char c in s.Trim()) chars.Add(Array.IndexOf(bad, c) >= 0 || c == ' ' ? '_' : c);
            return new string(chars.ToArray());
        }

        public static bool StartWithWindows
        {
            get
            {
                using (var k = Registry.CurrentUser.OpenSubKey(RunKey))
                    return k != null && k.GetValue("SnapDrop") != null;
            }
            set
            {
                using (var k = Registry.CurrentUser.OpenSubKey(RunKey, true))
                {
                    if (k == null) return;
                    if (value) k.SetValue("SnapDrop", "\"" + Environment.ProcessPath + "\" --background");
                    else k.DeleteValue("SnapDrop", false);
                }
            }
        }
    }
}
