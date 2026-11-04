using System;
using System.IO;

namespace SnapDrop
{
    /// <summary>
    /// Decides where screenshots live.
    ///
    /// "Smart" mode (default): a new capture is written to a temporary staging folder. If you drag it out
    /// (into a folder, the desktop, a chat, a terminal...) that is where it lives and nothing is added to your
    /// screenshots folder. If you don't, it is copied into the screenshots folder when the thumbnail goes away.
    /// "Always" mode: every capture is saved straight to the screenshots folder.
    /// </summary>
    static class ScreenshotStore
    {
        static readonly TimeSpan KeepStaged = TimeSpan.FromDays(3);

        public static string StagingDir { get { return Path.Combine(Path.GetTempPath(), "SnapDrop", "pending"); } }
        public static bool Smart { get { return Settings.SavePolicy != "always"; } }

        public static bool InStaging(string path)
        {
            try { return string.Equals(Path.GetDirectoryName(path), StagingDir, StringComparison.OrdinalIgnoreCase); }
            catch { return false; }
        }

        /// <summary>The path a brand-new capture should be written to.</summary>
        public static string NewPath(string name, string ext)
        {
            string dir = Smart ? StagingDir : Settings.SaveFolder;
            try { Directory.CreateDirectory(dir); }
            catch { dir = StagingDir; Directory.CreateDirectory(dir); }

            string path = Path.Combine(dir, name + ext);
            for (int n = 2; File.Exists(path) || File.Exists(FinalPath(path)); n++)
                path = Path.Combine(dir, name + "_" + n + ext);
            return path;
        }

        static string FinalPath(string staged) { return Path.Combine(Settings.SaveFolder, Path.GetFileName(staged)); }

        // ---- "used" marker: the screenshot was dragged/saved somewhere else, so don't keep a second copy.
        public static void MarkUsed(string path)
        {
            if (!InStaging(path)) return;
            try { File.WriteAllText(path + ".used", ""); } catch { }
        }

        public static bool IsUsed(string path) { return File.Exists(path + ".used"); }

        /// <summary>Copies a staged, un-dragged screenshot into the screenshots folder. Returns the path to use afterwards.</summary>
        public static string Finalize(string path)
        {
            if (!Smart || !InStaging(path) || !File.Exists(path) || IsUsed(path)) return path;
            try
            {
                Directory.CreateDirectory(Settings.SaveFolder);
                File.Copy(path, FinalPath(path), true);
            }
            catch (Exception ex) { SnapApp.Log(ex); }
            return path;   // the staged file stays valid (clipboard / pending drops may still point at it)
        }

        /// <summary>After the editor changed a staged file, keep an already-saved copy in sync.</summary>
        public static void OnEdited(string path)
        {
            if (!InStaging(path)) return;
            string dest = FinalPath(path);
            try { if (File.Exists(dest)) File.Copy(path, dest, true); } catch { }
        }

        /// <summary>"Delete" in the thumbnail menu: remove every copy.</summary>
        public static void Discard(string path)
        {
            try { File.Delete(path); } catch { }
            try { File.Delete(path + ".used"); } catch { }
            if (InStaging(path)) { try { File.Delete(FinalPath(path)); } catch { } }
        }

        /// <summary>
        /// Runs at startup: rescues staged screenshots that were never dragged out (e.g. SnapDrop was closed or crashed
        /// while the thumbnail was still showing) and removes staged files older than a few days.
        /// </summary>
        public static void Sweep()
        {
            try
            {
                if (!Directory.Exists(StagingDir)) return;
                foreach (var f in Directory.GetFiles(StagingDir))
                {
                    if (f.EndsWith(".used")) continue;
                    var age = DateTime.Now - File.GetLastWriteTime(f);
                    if (age > KeepStaged)
                    {
                        File.Delete(f);
                        try { File.Delete(f + ".used"); } catch { }
                    }
                    else if (age > TimeSpan.FromSeconds(20) && !IsUsed(f) && !File.Exists(FinalPath(f))) Finalize(f);
                }
            }
            catch { }
        }
    }
}
