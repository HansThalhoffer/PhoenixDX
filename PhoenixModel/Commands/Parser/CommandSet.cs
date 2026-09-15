using PhoenixModel.Program;
using PhoenixModel.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PhoenixModel.Commands.Parser {
    /// <summary>
    /// Spezialisierung des BlockingSet
    /// </summary>
    public class CommandSet : ObservableCollection<BaseCommand>, INotifyCollectionChanged, IEnumerable<BaseCommand> {

        public IEnumerable<BaseCommand> GetCommands(ISelectable selectable) {
            return this.Where(item => item.HasEffectOn(selectable));
        }
        /// <summary>
        /// Nimmt einen Befehl in die Liste des laufenden Zuges auf.
        ///
        /// Was zu einem anderen Monat gehört, bleibt draussen. Die Liste heisst "Aktueller Zug"
        /// und soll auch nur den zeigen: beim Laden eines Zuges werden dessen Bauaufträge und
        /// Bewegungen wieder zu Befehlen, und ohne diese Grenze sammelte sich beim Blättern durch
        /// alte Züge alles an, was je gemacht wurde.
        /// </summary>
        /// <remarks>
        /// Hier stand einmal ein Umweg über einen SynchronizationContext, der die Änderung auf den
        /// Thread der Oberfläche schieben sollte. Das Feld wurde nie gesetzt - der Compiler hat es
        /// als CS0649 gemeldet -, der Umweg lief also immer ins Leere und sah nur so aus, als wäre
        /// die Sache erledigt.
        ///
        /// Die Sammlung weiss nichts von Oberflächen und soll es auch nicht. Wer sie an ein
        /// DataGrid bindet, muss die Änderung selbst auf seinen Thread holen; CommandPage und
        /// CommanndHistoryPage tun das, seit eine von der Karte aus ausgelöste Bewegung in keiner
        /// Liste ankam.
        /// </remarks>
        public new void Add(BaseCommand command) {
            if (command.GehörtZumAktuellenZug == false)
                return;
            base.Add(command);
        }

        /// <summary>
        /// Nimmt einen Befehl zurück.
        ///
        /// Zurücknehmen lässt sich nur, was im laufenden Zug gemacht wurde
        /// (<see cref="BaseCommand.GehörtZumAktuellenZug"/>). Ein Befehl aus einem vergangenen
        /// Monat ist ausgewertet; ihn zurückzunehmen würde die eigene Karte gegen die Auswertung
        /// der Spielleitung verschieben.
        /// </summary>
        /// <returns>true, wenn der Befehl zurückgenommen wurde</returns>
        /// <summary>
        /// Wirft alles aus der Liste, was nicht zum laufenden Zug gehört.
        ///
        /// Gebraucht beim Zugwechsel: was vorher drinstand, gehört zum vorigen Monat.
        /// </summary>
        /// <returns>wieviele Befehle gegangen sind</returns>
        public int EntferneFremdeZüge() {
            var fremde = this.Where(befehl => befehl.GehörtZumAktuellenZug == false).ToList();
            foreach (var befehl in fremde)
                Remove(befehl);
            return fremde.Count;
        }

        public bool Undo(BaseCommand command) {
            if (command.GehörtZumAktuellenZug == false) {
                ProgramView.LogWarning($"Der Befehl stammt aus Zug {command.Zug} und lässt sich nicht zurücknehmen",
                    $"Gespielt wird Zug {ProgramView.SelectedMonth}. Zurücknehmen lässt sich nur, was im "
                    + "laufenden Zug gemacht wurde - alles Ältere hat die Spielleitung längst ausgewertet.");
                return false;
            }
            if (command.CanUndo == true) {
                var result = command.UndoCommand();
                if (result.HasErrors == false) {
                    Remove(command);
                    return true;
                }
                ProgramView.LogError(result.Title, result.Message);
            }
            return false;
        }
    }
}
