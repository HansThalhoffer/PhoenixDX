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
            None, SelectGemark, Log, Zoom, UpdateAll
        }

        public int GF = 0, KF = 0;
        public MapEventType EventType { get; set; }
        public LogEntry? LogEntry { get; set; } = null;
        public float? floatValue = null;


        public MapEventArgs(MapEventType mapevent)
        {
            EventType = mapevent;
        }


        public MapEventArgs(int gf, int kf, MapEventType mapevent, float? value)
        {
            GF = gf;
            KF = kf;
            EventType = mapevent;
            floatValue = value;
        }

        public MapEventArgs(KleinfeldPosition gem, MapEventType mapevent)
        {
            GF = gem.gf;
            KF = gem.kf;
            EventType = mapevent;
        }

        public MapEventArgs(int gf, int kf, MapEventType mapevent)
        {
            GF = gf;
            KF = kf;
            EventType = mapevent;
        }

        public MapEventArgs(LogEntry logEntry)
        {
            GF = 0;
            KF = 0;
            EventType = MapEventType.Log;
            LogEntry = logEntry;
        }

        public MapEventArgs(int gf, int kf, LogEntry logEntry)
        {
            GF = gf;
            KF = kf;
            EventType = MapEventType.Log;
            LogEntry = logEntry;
        }
    }
}
