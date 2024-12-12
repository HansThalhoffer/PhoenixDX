using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Helper
{
    public class MapEventArgs
    {
        public enum MapEventType
        {
            None, SelectGemark
        }

        public int GF = 0, KF = 0;
        public MapEventType EventType { get; set; }
        public LogEntry? LogEntry { get; set; } = null;

        public MapEventArgs(int gf, int kf, MapEventType mapevent)
        {
            GF = gf;
            KF = kf;
            EventType = mapevent;
        }

        public MapEventArgs(int gf, int kf, MapEventType mapevent, LogEntry logEntry)
        {
            GF = gf;
            KF = kf;
            EventType = mapevent;
        }
    }
}
