using PhoenixWPF.Helper;
using System.Runtime.CompilerServices;
using Xunit;

// Die Tests arbeiten alle auf denselben statischen Daten in SharedData. Liefen sie parallel,
// würden sie sich gegenseitig die geladenen Tabellen unter den Füßen wegziehen.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Tests {

    /// <summary>
    /// Grundeinstellungen, die für jeden Testlauf gelten, unabhängig davon welcher Test startet.
    /// </summary>
    internal static class TestKonfiguration {

        /// <summary>
        /// Wird vor dem ersten Test der Assembly ausgeführt.
        ///
        /// Wichtig ist das Abschalten der Benutzerdialoge: sonst öffnet ein Testlauf, dem eine
        /// Datenbank oder ein Passwort fehlt, einen modalen Datei- oder Passwortdialog auf dem
        /// Bildschirm und blockiert, bis jemand ihn wegklickt.
        /// </summary>
        [ModuleInitializer]
        internal static void Initialisieren() {
            StorageSystem.BenutzerdialogeErlaubt = false;
        }
    }
}
