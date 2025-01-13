using PhoenixDX.Structures;
using PhoenixModel.Helper;
using PhoenixModel.dbErkenfara;
using PhoenixWPF.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PhoenixWPF.Pages;
using PhoenixDX;
using PhoenixWPF.Host;
using static PhoenixModel.Program.LogEntry;
using PhoenixModel.Program;
using PhoenixModel.dbZugdaten;
using PhoenixModel.View;

namespace PhoenixWPF.Program
{
    public class SpielWPF : IDisposable
    {
        public SpielWPF()
        {
        }

        public void ViewEventHandler(ViewEventArgs e)
        {
            switch (e.EventType)
            {
          
                case ViewEventArgs.ViewEventType.Log:
                    {
                        if (e.LogEntry != null)
                        {
                            if (e.GF > 0 && e.KF > 0)
                            {
                                e.LogEntry.Titel = $"[{e.GF}/{e.KF}] {e.LogEntry.Titel}";
                            }
                            Log(e.LogEntry);
                        }
                        break;
                    }
            }
        }

        public void MapEventHandler(MapEventArgs e)
        {
            switch (e.EventType)
            {
            case MapEventArgs.MapEventType.SelectGemark:
            {
                SelectGemark(e);
                break;
            }
            case MapEventArgs.MapEventType.Log:
            {
                if (e.LogEntry != null)
                {
                    if (e.GF > 0 && e.KF > 0)
                    {
                        e.LogEntry.Titel = $"[{e.GF}/{e.KF}] {e.LogEntry.Titel}";
                    }
                    Log(e.LogEntry);
                }
                break;
            }
            case MapEventArgs.MapEventType.Zoom:
            {
                if (Main.Instance.Options != null && e.floatValue != null)
                    Main.Instance.Options.ChangeZoomLevel(e.floatValue.Value);
                break;
            }

            
            }

        }

        public static void Log(LogEntry logentry)
        {
            LogPage.AddToLog(logentry);
        }

        public static void LogInfo(string titel, string message)
        {
            Log(new LogEntry(LogType.Info, titel, message));
        }

        public static void LogWarning(string titel, string message)
        {
            Log(new LogEntry(LogType.Warning, titel, message));
        }

        public static void LogError(string titel, string message)
        {
            Log(new LogEntry(LogType.Error, titel, message));
        }

        public void SelectGemark(MapEventArgs e)
        {
            if (SharedData.Map != null && SharedData.Map.IsAddingCompleted)
            {

                var bezeichner = KleinfeldPosition.CreateBezeichner(e.GF, e.KF);
                var gem = SharedData.Map[bezeichner];
                Main.Instance.SelectionHistory.Current = gem;

                /// TEST
                KleinfeldView.UnMark();
                var nachbarn = KleinfeldView.GetNachbarn(gem,1);
                if (nachbarn != null) {
                    foreach (var g in nachbarn) {
                        KleinfeldView.Mark(g, MarkerType.Fatality,true);
                    }
                }
            }
        }

        public void SelectGemark(KleinfeldPosition pos)
        {
            if (SharedData.Map != null && SharedData.Map.IsAddingCompleted)
            {
                Main.Instance.Map?.Goto(pos);
                if (pos is ISelectable select)
                    Main.Instance.SelectionHistory.Current = select;
            }
        }


        public void Goto(KleinfeldPosition pos)
        {
            SelectGemark(pos);
        }



        public void Dispose()
        {  }
    }
}
