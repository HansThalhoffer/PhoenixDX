using PhoenixModel.dbErkenfara;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Pages;
using System.Windows.Input;
using static PhoenixModel.Program.LogEntry;

namespace PhoenixWPF.Program {
    public class SpielWPF : IDisposable {
        public SpielWPF() {
        }

        public void ViewEventHandler(ViewEventArgs e) {
            switch (e.EventType) {

                case ViewEventArgs.ViewEventType.Log: {
                        if (e.LogEntry != null) {
                            if (e.GF > 0 && e.KF > 0) {
                                e.LogEntry.Titel = $"[{e.GF}/{e.KF}] {e.LogEntry.Titel}";
                            }
                            Log(e.LogEntry);
                        }
                        break;
                    }
            }
        }

        public void MapEventHandler(MapEventArgs e) {
            switch (e.EventType) {
                case MapEventArgs.MapEventType.Loaded: {
                        // hier wird der Zoom aus den User Settings übertragen.
                        Main.Map?.SetZoom(Main.Instance.Settings.UserSettings.Zoom);
                        Main.Map?.SetCameraPosition(Main.Instance.Settings.UserSettings.CameraPosition);
                        break;
                    }
                case MapEventArgs.MapEventType.SelectGemark: {
                        SelectGemark(e);
                        break;
                    }
                case MapEventArgs.MapEventType.Log: {
                        if (e.LogEntry != null) {
                            if (e.GF > 0 && e.KF > 0) {
                                e.LogEntry.Titel = $"[{e.GF}/{e.KF}] {e.LogEntry.Titel}";
                            }
                            Log(e.LogEntry);
                        }
                        break;
                    }
                case MapEventArgs.MapEventType.Zoom: {
                        if (Main.Instance.Options != null && e.floatValue != null)
                            Main.Instance.Options.ChangeZoomLevel(e.floatValue.Value);
                        break;
                    }


            }

        }

        public static void Log(LogEntry logentry) {
            LogPage.AddToLog(logentry);
        }

        public static void LogInfo(string titel, string message) {
            Log(new LogEntry(LogType.Info, titel, message));
        }

        public static void LogWarning(string titel, string message) {
            Log(new LogEntry(LogType.Warning, titel, message));
        }

        public static void LogError(string titel, string message) {
            Log(new LogEntry(LogType.Error, titel, message));
        }

        public void SelectGemark(MapEventArgs e) {
            if (SharedData.Map != null && SharedData.Map.IsAddingCompleted) {

                var bezeichner = KleinfeldPosition.CreateBezeichner(e.GF, e.KF);
                if (SharedData.Map.TryGetValue(bezeichner, out var gem) == false || gem == null)
                    return;

                // Umschalt + Klick bewegt die ausgewählte Spielfigur auf dem günstigsten Weg dorthin.
                // Die Auswahl bleibt dabei auf der Figur, damit man mehrere Züge hintereinander machen kann.
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) {
                    if (Main.Instance.SelectionHistory.Current is Spielfigur figur) {
                        var ergebnis = Helper.Bewegungssteuerung.BewegeZuZielfeld(figur, gem);
                        if (ergebnis.HasErrors)
                            LogError(ergebnis.Title, ergebnis.Message);
                        else
                            Main.Instance.SelectionHistory.Refresh();
                    }
                    else {
                        LogInfo("Es ist keine Spielfigur ausgewählt",
                            "Für eine Bewegung mit Umschalt+Klick muss zuerst die Figur ausgewählt werden, die sich bewegen soll");
                    }
                    return;
                }

                // Strg + Klick setzt oder entfernt eine eigene Markierung auf einem Feld des eigenen Reiches
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && gem.Nation == ProgramView.SelectedNation) {
                    MarkerType mark = (gem.Mark == MarkerType.None) ? MarkerType.User : MarkerType.None;
                    KleinfeldView.Mark(gem, mark, true);
                }

                Main.Instance.SelectionHistory.Current = gem;

                // Test Pfad sichtbar machen
                /*IEnumerable<KleinFeld>? list = KleinfeldView.GetPath(gem, "SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO SO");
                if (list != null) {
                    KleinfeldView.UnMark();
                    foreach (var f in list) {
                        KleinfeldView.Mark(f, MarkerType.Fatality, true);
                    }
                }*/
                /// TEST - nachbarn markieren
                /*KleinfeldView.UnMark();
                var nachbarn = KleinfeldView.GetNachbarn(gem, 2);
                if (nachbarn != null) {
                    foreach (var g in nachbarn) {
                        KleinfeldView.Mark(g, MarkerType.Fatality, true);
                    }
                }*/
            }
        }

        public void SelectGemark(KleinfeldPosition pos) {
            if (SharedData.Map != null && SharedData.Map.IsAddingCompleted) {
                Main.Map?.Goto(pos);
                if (pos is ISelectable select)
                    Main.Instance.SelectionHistory.Current = select;
            }
            
        }


        public void Goto(KleinfeldPosition pos) {
            SelectGemark(pos);
        }

        public void Dispose() { }
    }
}
