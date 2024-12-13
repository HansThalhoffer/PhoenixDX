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

namespace PhoenixWPF
{
    public class Spiel :IDisposable
    {
        public Spiel ()
        {
        }
        
        public void MapEventHandler(MapEventArgs e)
        {
           if (e.EventType == MapEventArgs.MapEventType.SelectGemark)
                SelectGemark(e);
            if (e.EventType == MapEventArgs.MapEventType.Log && e.LogEntry != null)
            {
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
                
                var bezeichner = Gemark.CreateBezeichner(e.GF, e.KF);
                var gem = SharedData.Map[bezeichner];
                var main = Application.Current.MainWindow;
                ListBox? lb = VisualTreeHelperExtensions.FindControlByName(main, "PropertyListBox") as ListBox;
                if (lb != null)
                {
                    lb.ItemsSource = gem.Properties;
                }
                
            }
            
        }

        public void Dispose()
        {
           
        }
    }
}
