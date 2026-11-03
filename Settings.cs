using System;
using System.IO;
using Microsoft.Win32;

namespace SnapDrop
{
    static class Settings
    {
        static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SnapDrop");
        static readonly string File_ = Path.Combine(Dir, "settings.ini");
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static string SaveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "SnapDrop");
        public static bool CopyToClipboard = true;
        public static bool ShowThumbnail = true;
        public static int TimerSeconds = 0;

        public static string LogPath { get { return Path.Combine(Dir, "error.log"); } }

        public static void Load()
        {
            try
            {
                foreach (var line in File.ReadAllLines(File_))
                {
                    int i = line.IndexOf('=');
                    if (i < 0) continue;
                    string k = line.Substring(0, i).Trim(), v = line.Substring(i + 1).Trim();
                    switch (k)
                    {
                        case "SaveFolder": if (v.Length > 0) SaveFolder = v; break;
                        case "CopyToClipboard": CopyToClipboard = v == "1"; break;
                        case "ShowThumbnail": ShowThumbnail = v == "1"; break;
                        case "TimerSeconds": int.TryParse(v, out TimerSeconds); break;
                    }
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllLines(File_, new[]
                {
                    "SaveFolder=" + SaveFolder,
                    "CopyToClipboard=" + (CopyToClipboard ? "1" : "0"),
                    "ShowThumbnail=" + (ShowThumbnail ? "1" : "0"),
                    "TimerSeconds=" + TimerSeconds,
                });
            }
            catch { }
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
                    if (value) k.SetValue("SnapDrop", "\"" + Environment.ProcessPath + "\"");
                    else k.DeleteValue("SnapDrop", false);
                }
            }
        }
    }
}
