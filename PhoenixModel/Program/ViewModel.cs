using Microsoft.VisualBasic.FileIO;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Program
{
    public static class ViewModel
    {

        public delegate void ViewEventHandler(object? sender, ViewEventArgs e);
        public static event ViewEventHandler? OnViewEvent;

        private static void _OnViewEvent(ViewEventArgs args)
        {
            if (OnViewEvent == null)
                return;
            OnViewEvent(null, args);
        }

        public static void LogError(int gf, int kf, string msg)
        {
            _OnViewEvent(new ViewEventArgs(gf,kf, new LogEntry(LogEntry.LogType.Error,msg)));
        }
        public static void LogError(string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Error, msg)));
        }
    }
}
