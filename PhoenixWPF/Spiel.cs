using PhoenixDX.Structures;
using PhoenixModel.Helper;
using PhoenixModel.Karte;
using PhoenixWPF.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF
{
    public class Spiel
    {
        public void MapEventHandler(MapEventArgs e)
        {
           if (e.EventType == MapEventArgs.MapEventType.SelectGemark)
                SelectGemark(e);
        }

        public void SelectGemark(MapEventArgs e)
        {
            if (SharedData.Map != null && SharedData.Map.IsAddingCompleted)
            {
                Dictionary<string, Gemark>? map = SharedData.Map.FirstOrDefault();
                if (map != null)
                {
                    var bezeichner = Gemark.CreateBezeichner(e.GF, e.KF);
                    var gem = map[bezeichner];
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
}
