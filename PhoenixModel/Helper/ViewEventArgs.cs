using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Helper.MapEventArgs;

namespace PhoenixModel.Helper
{
    public class ViewEventArgs
    {
        public enum ViewEventType
        {
            None, Log
        }

        public int GF = 0, KF = 0;
        public LogEntry? LogEntry { get; set; } = null;
        public ViewEventType EventType { get; set; } = ViewEventType.None;

        public ViewEventArgs(int gf, int kf, LogEntry? value)
        {
            GF = gf;
            KF = kf;
            EventType = ViewEventType.Log;
            LogEntry = value;
        }

    }
}
