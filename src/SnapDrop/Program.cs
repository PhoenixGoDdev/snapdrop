using System;
using System.Threading;
using System.Windows;

namespace SnapDrop
{
    static class Program
    {
        const string MutexName = @"Local\SnapDrop.SingleInstance";
        const string ActivateName = @"Local\SnapDrop.Activate";

        [STAThread]
        static void Main(string[] args)
        {
            bool first;
            using (var mutex = new Mutex(true, MutexName, out first))
            {
                if (!first)
                {
                    // Already running: tell the running copy to open its Settings window (e.g. the Start-menu shortcut was clicked).
                    try { EventWaitHandle.OpenExisting(ActivateName).Set(); } catch { }
                    return;
                }

                var app = new SnapApp(args);
                var activate = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateName);
                var listener = new Thread(() =>
                {
                    while (activate.WaitOne())
                    {
                        var a = Application.Current;
                        if (a != null) a.Dispatcher.BeginInvoke(new Action(SnapApp.OpenSettings));
                    }
                }) { IsBackground = true, Name = "SnapDrop activation listener" };
                listener.Start();

                app.Run();
            }
        }
    }
}
