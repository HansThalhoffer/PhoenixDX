using System.Configuration;
using System.Data;
using System.Windows;

namespace PhoenixWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Die Schalter des Programmstarts muessen gelesen sein, bevor das Hauptfenster geladen
        /// wird - Main.StartInstance fragt sie beim Laden der Datenbanken ab.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            Program.Kommandozeile.Lies(e.Args);
            base.OnStartup(e);
        }
    }

}
