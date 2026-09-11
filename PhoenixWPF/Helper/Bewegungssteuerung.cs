using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Program;

namespace PhoenixWPF.Helper {

    /// <summary>
    /// Bindeglied zwischen der Oberfläche und den Bewegungsregeln.
    ///
    /// Hier liegt alles, was für eine Bewegung eine Rückfrage beim Benutzer braucht - im Modell
    /// selbst darf es keine Dialoge geben. Konkret ist das die Auswahl des Feldes, auf dem eine
    /// Figur nach einem Teleport wieder erscheint.
    /// </summary>
    public static class Bewegungssteuerung {

        /// <summary>
        /// Bewegt die Figur ein Kleinfeld weit in die angegebene Richtung
        /// </summary>
        public static CommandResult BewegeInRichtung(Spielfigur? figur, Direction richtung) {
            if (figur == null)
                return new CommandResultError("Es ist keine Spielfigur ausgewählt", "Für eine Bewegung muss zuerst eine Spielfigur ausgewählt werden", null);

            var schritt = BewegungsRules.PrüfeSchritt(figur, richtung);
            if (schritt.HasErrors)
                return new CommandResultError(schritt, null);
            if (schritt.Ziel == null)
                return new CommandResultError("Kein Zielfeld", $"Im {(DirectionNames)richtung} von {figur.CreateBezeichner()} liegt kein Kleinfeld", null);

            return Ausführen(figur, [new KleinfeldPosition(schritt.Ziel.gf, schritt.Ziel.kf)], schritt.Ziel);
        }

        /// <summary>
        /// Bewegt die Figur auf dem günstigsten Weg zu dem angegebenen Kleinfeld
        /// </summary>
        public static CommandResult BewegeZuZielfeld(Spielfigur? figur, KleinFeld? ziel) {
            if (figur == null)
                return new CommandResultError("Es ist keine Spielfigur ausgewählt", "Für eine Bewegung muss zuerst eine Spielfigur ausgewählt werden", null);

            var weg = BewegungsRules.FindeWeg(figur, ziel, out string fehler);
            if (weg == null || weg.Wegpunkte.Count == 0)
                return new CommandResultError("Dorthin führt kein Weg", fehler, null);

            return Ausführen(figur, weg.Wegpunkte, ziel);
        }

        /// <summary>
        /// Erzeugt den Bewegungsbefehl und führt ihn aus. Endet der Weg auf einem Teleportfeld,
        /// wird vorher das Auftauchfeld erfragt und an den Weg angehängt.
        /// </summary>
        private static CommandResult Ausführen(Spielfigur figur, List<KleinfeldPosition> wegpunkte, KleinFeld? letztesFeld) {
            if (TeleportRules.IstTeleportfeld(letztesFeld)) {
                var dialog = new TeleportDialog(figur, letztesFeld!);
                dialog.ShowDialog();
                if (dialog.Auftauchfeld == null)
                    return new CommandResultError("Teleport abgebrochen",
                        $"Ohne ein Auftauchfeld kann {figur.Bezeichner} das Teleportfeld {letztesFeld!.CreateBezeichner()} nicht betreten", null);
                wegpunkte = [.. wegpunkte, dialog.Auftauchfeld];
            }

            string befehl = MoveCommandParser.GenerateCommand(figur, wegpunkte);
            if (CommandParser.ParseCommand(befehl, out var command) == false || command == null)
                return new CommandResultError("Der Bewegungsbefehl konnte nicht erzeugt werden", befehl, null);

            return command.ExecuteCommand();
        }
    }
}
