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

namespace PhoenixWPF.Program
{
    public class SpielWPF : IDisposable
    {
        public SpielWPF()
        {
        }

        public void MapEventHandler(MapEventArgs e)
        {
            if (e.EventType == MapEventArgs.MapEventType.SelectGemark)
                SelectGemark(e);
            if (e.EventType == MapEventArgs.MapEventType.Log && e.LogEntry != null)
            {
                if (e.GF > 0 && e.KF > 0)
                {
                    e.LogEntry.Message = $"[{e.GF}/{e.KF}] {e.LogEntry.Message}";
                }
                Log(e.LogEntry);
            }

        }

        public static void Log(LogEntry logentry)
        {
            LogPage.AddToLog(logentry);
        }

        public void SelectGemark(MapEventArgs e)
        {
            if (SharedData.Map != null && SharedData.Map.IsAddingCompleted)
            {

                var bezeichner = GemarkPosition.CreateBezeichner(e.GF, e.KF);
                var gem = SharedData.Map[bezeichner];
                Main.Instance.PropertyDisplay?.Display(gem.Eigenschaften);
            }
        }

        public void Dispose()
        {  }
    }
}
