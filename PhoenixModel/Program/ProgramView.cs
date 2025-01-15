using PhoenixModel.dbPZE;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ViewModel;
using static PhoenixModel.EventsAndArgs.ViewEventArgs;

namespace PhoenixModel.Program {
    public static class ProgramView
    {
        public static Nation? SelectedNation { get; set; }

        public delegate void ViewEventHandler(object? sender, ViewEventArgs e);
        public static event ViewEventHandler? OnViewEvent;

        private static void _OnViewEvent(ViewEventArgs args)
        {
            if (OnViewEvent == null)
                return;
            OnViewEvent(null, args);
        }

        public static void LogError(int gf, int kf, string titel, string msg)
        {
            _OnViewEvent(new ViewEventArgs(gf,kf, new LogEntry(LogEntry.LogType.Error,titel, msg)));
        }

        /*YAGNI
        
        public static void UpdateData(Gebäude gebäude)
        {
            _OnViewEvent(new ViewEventArgs(gebäude));
        }

        public static void UpdateData(Spielfigur figur)
        {
            _OnViewEvent(new ViewEventArgs(figur));
        }*/

        /// <summary>
        /// wird gefeuert, wenn Daten im größeren Umfange geändert wurden und sich somit eine komplette Initialiserung der abhängigen Klassen lohnt
        /// </summary>
        public static void Update(ViewEventType what)
        {
            _OnViewEvent(new ViewEventArgs(what));
        }

        /// <summary>
        /// wird gefeuert, wenn alle Daten geladen wurden. 
        /// </summary>
        public static void DataLoadingCompleted()
        {
            _OnViewEvent(new ViewEventArgs(ViewEventType.EverythingLoaded));
        }

        /// <summary>
        /// überprüft den Besitz der KleinFeld/des Kleinfelds
        /// </summary>
        /// <param name="position">eine Gemarkpostion mit validem Bezeichner</param>
        /// <returns>wahr, wenn die übergebene KleinFeld/Kleinfeld zu der Nation/Nation des authentifizeriten Nutzers gehört</returns>
        public static bool BelongsToUser(Spielfigur figur)
        {
            if (SharedData.Map == null)
            {
                ProgramView.LogError("Die Kartendaten sind nicht geladen"," Ohne Kartendaten kann ein Besitz einer Spielfigur nicht ermittelt werden");
                return false;
            }            
            if (ProgramView.SelectedNation == null)
            {
                ProgramView.LogError("Keine Nation ausgewählt","Ohne eine ausgewählte Nation kann kein Besitz einer Spielfigur ermittelt werden");
                return false;
            }
            if (figur.Nation == null)
            {
                ProgramView.LogError($"Spielfigur {figur.Bezeichner} ohne Nation", "Der Spielfigur {figur.Bezeichner} ist keine Nation zugeordnet, daher kann der Besitz nicht ermittelt werden");
                return false;
            }

            // das Kleinfeld des Rüstotes gehört evtl. zum Nation des aktuellen Nutzers und kann daher ausgeählt werden
            return ProgramView.SelectedNation == figur.Nation;
        }

        /// <summary>
        /// überprüft den Besitz der KleinFeld/des Kleinfelds
        /// </summary>
        /// <param name="position">eine Gemarkpostion mit validem Bezeichner</param>
        /// <returns>wahr, wenn die übergebene KleinFeld/Kleinfeld zu der Nation/Nation des authentifizeriten Nutzers gehört</returns>
        public static bool BelongsToUser( KleinfeldPosition position)
        {
            if (SharedData.Map == null)
            {
                ProgramView.LogError("Die Kartendaten sind nicht geladen", "Die Kartendaten sind nicht geladen, daher kann ein Besitz eines Kleinfeldes nicht ermittelt werden");
                return false;
            }
            if (SharedData.Map.ContainsKey(position.CreateBezeichner()) == false)
            {
                ProgramView.LogError($"Unbekannte Position  Position {position.CreateBezeichner()}", $"Die Position {position.CreateBezeichner()} befindet sich auf einem nicht existenten Kleinfeld");
                return false;
            }
            if (ProgramView.SelectedNation == null)
            {
                ProgramView.LogError("Keine Nation ausgewählt", "Ohne eine ausgewählte Nation kann kein Besitz eines Kleinfeldes ermittelt werden");
                return false;
            }
            // das Kleinfeld des Rüstotes gehört evtl. zum Nation des aktuellen Nutzers und kann daher ausgeählt werden
            return ProgramView.SelectedNation == SharedData.Map[position.CreateBezeichner()].Nation;
        }

        public static void LogError(string titel, string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Error, titel, msg)));
        }
        public static void LogWarning(string titel, string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Warning, titel, msg)));
        }
        public static void LogInfo(string titel, string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Info, titel, msg)));
        }
        public static void LogError(KleinfeldPosition pos, string titel, string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Error, $"[{pos.CreateBezeichner()}] {titel}", msg)));
        }
        public static void LogWarning(KleinfeldPosition pos, string titel, string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Warning, $"[{pos.CreateBezeichner()}] {titel}", msg)));
        }
        public static void LogInfo(KleinfeldPosition pos, string titel, string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Info, $"[{pos.CreateBezeichner()}] {titel}", msg)));
        }
    }
}
