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
            // Zuerst das Netz, dann alles andere: was beim Start schiefgeht, soll auch berichtet
            // werden und nicht nur das Fenster verschwinden lassen.
            Program.Absturzbericht.Richte_ein(this);
            Program.Kommandozeile.Lies(e.Args);
            base.OnStartup(e);
        }
    }

}
