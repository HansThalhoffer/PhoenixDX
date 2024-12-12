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

namespace PhoenixWPF
{
    public class Spiel
    {
        public void MapEventHandler(MapEventArgs e)
        {
           if (e.EventType == MapEventArgs.MapEventType.SelectGemark)
                SelectGemark(e);
        }

        public enum LogType 
        {
            Info,
            Warning,
            Error
        }

        public static void Log(LogType logType, string message)
        {
            LogPage.AddToLog(logType, message);
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

    }
}
