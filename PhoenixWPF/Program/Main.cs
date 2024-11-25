using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PhoenixModel.Program;
using PhoenixWPF.Database;
using PhoenixWPF.Helper;

namespace PhoenixWPF.Program
{
    public class Main :IDisposable
    {
       

        private static Main _instance = new Main();
        public AppSettings? Settings { get; private set; }
        
        static public Main Instance { get { return _instance; } }

        public void InitInstance() 
        {
            Settings = new AppSettings("Settings.jpk");
            Settings.InitializeSettings();
        }

        public void Dispose()
        {
            if (Settings != null) 
                Settings.Dispose(); 
        }
    }
}
