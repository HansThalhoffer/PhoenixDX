using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.EventsAndArgs.MapEventArgs;

namespace PhoenixModel.EventsAndArgs {
    public class ViewEventArgs {
        public enum ViewEventType {
            None, Log, UpdateGebäude, UpdateSpielfiguren, UpdateDiplomatie, UpdateEverything, EverythingLoaded
        }

        public int GF = 0, KF = 0;
        public LogEntry? LogEntry { get; set; } = null;
        public object? Data { get; set; } = null;
        public ViewEventType EventType { get; set; } = ViewEventType.None;

        public ViewEventArgs(ViewEventType eventType) {
            EventType = eventType;
        }

        public ViewEventArgs(int gf, int kf, LogEntry? value) {
            GF = gf;
            KF = kf;
            EventType = ViewEventType.Log;
            LogEntry = value;
        }

        public ViewEventArgs(Gebäude value) {
            GF = value.gf;
            KF = value.kf;
            EventType = ViewEventType.UpdateGebäude;
            Data = value;
        }

        public ViewEventArgs(Spielfigur value) {
            GF = value.gf;
            KF = value.kf;
            EventType = ViewEventType.UpdateSpielfiguren;
            Data = value;
        }
    }
}
