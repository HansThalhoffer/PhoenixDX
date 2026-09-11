using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {

    /// <summary>
    /// Die Sicht auf die Bewegungen der Spielfiguren des aktuellen Zuges.
    ///
    /// Die Bewegung einer Figur steht nicht als Befehl in der Datenbank, sondern ergibt sich aus
    /// dem Startfeld (gf_von/kf_von) und den Wegpunkten x1/y1 bis x19/y19. Beim Laden der Zugdaten
    /// werden daraus wieder <see cref="MoveCommand"/> Objekte erzeugt, damit die Bewegungen in der
    /// Befehlsliste erscheinen und zurückgenommen werden können.
    /// </summary>
    public static class BewegungView {

        /// <summary>
        /// Die Kleinfelder, die zuletzt für die Anzeige einer Bewegung markiert wurden.
        /// Sie werden gemerkt, damit beim Wechsel der Auswahl genau diese wieder entmarkiert
        /// werden können und die eigenen Markierungen des Benutzers erhalten bleiben.
        /// </summary>
        private static readonly List<KleinFeld> _markierteFelder = [];

        /// <summary>
        /// Zeigt in der Karte, welchen Weg die Figur in diesem Zug bereits zurückgelegt hat
        /// und welche Kleinfelder sie mit den verbleibenden Bewegungspunkten noch erreichen kann.
        /// </summary>
        public static void ZeigeBewegung(Spielfigur? figur) {
            VersteckeBewegung();
            if (figur == null || SharedData.Map == null || SharedData.Map.IsAddingCompleted == false)
                return;

            foreach (var feld in BewegungsRules.GetErreichbareFelder(figur))
                Markiere(feld, MarkerType.Bewegung);

            // der bereits zurückgelegte Weg liegt optisch über der Reichweite
            var start = KleinfeldView.GetKleinfeld(new KleinfeldPosition(figur.gf_von, figur.kf_von));
            if (start != null && figur.schritt > 0)
                Markiere(start, MarkerType.Weg);
            foreach (var punkt in new Bewegungsspur(figur).ToList()) {
                var feld = KleinfeldView.GetKleinfeld(punkt);
                if (feld != null)
                    Markiere(feld, MarkerType.Weg);
            }
        }

        /// <summary>
        /// Bewegt die Figur auf dem günstigsten Weg zu dem angegebenen Kleinfeld.
        /// Der Weg wird gesucht, als Befehl formuliert und ausgeführt, damit er in der Befehlsliste
        /// erscheint und zurückgenommen werden kann.
        ///
        /// Teleportfelder kann diese Funktion nicht ansteuern, weil dafür das Auftauchfeld beim
        /// Benutzer erfragt werden muss - dafür gibt es in der Oberfläche die Bewegungssteuerung.
        /// </summary>
        public static Commands.Parser.CommandResult BewegeZu(Spielfigur? figur, KleinfeldPosition? ziel) {
            if (figur == null)
                return new CommandResultError("Es ist keine Spielfigur ausgewählt", "Für eine Bewegung muss zuerst eine Spielfigur ausgewählt werden", null);

            if (TeleportRules.IstTeleportfeld(KleinfeldView.GetKleinfeld(ziel)))
                return new CommandResultError("Für ein Teleportfeld muss das Auftauchfeld gewählt werden",
                    $"{ziel?.CreateBezeichner()} ist ein Teleportfeld. Die Bewegung dorthin muss über die Oberfläche erfolgen, damit der Auftauchpunkt ausgewählt werden kann.", null);

            var weg = BewegungsRules.FindeWeg(figur, ziel, out string fehler);
            if (weg == null || weg.Wegpunkte.Count == 0)
                return new CommandResultError("Dorthin führt kein Weg", fehler, null);

            string befehl = Commands.MoveCommandParser.GenerateCommand(figur, weg.Wegpunkte);
            if (Commands.Parser.CommandParser.ParseCommand(befehl, out var command) == false || command == null)
                return new CommandResultError("Der Bewegungsbefehl konnte nicht erzeugt werden", befehl, null);

            // die Anzeige in der Karte aktualisiert der Aufrufer über ZeigeBewegung
            return command.ExecuteCommand();
        }

        /// <summary>
        /// Entfernt die zuletzt für eine Bewegung gesetzten Markierungen wieder
        /// </summary>
        public static void VersteckeBewegung() {
            if (_markierteFelder.Count == 0)
                return;
            KleinfeldView.UnMark(_markierteFelder);
            _markierteFelder.Clear();
        }

        /// <summary>
        /// Markiert ein Kleinfeld für die Bewegungsanzeige. Markierungen, die der Benutzer selbst
        /// gesetzt hat, werden nicht überschrieben.
        /// </summary>
        private static void Markiere(KleinFeld feld, MarkerType typ) {
            bool schonVonUns = _markierteFelder.Contains(feld);
            if (feld.Mark != MarkerType.None && schonVonUns == false)
                return;
            KleinfeldView.Mark(feld, typ, true);
            if (schonVonUns == false)
                _markierteFelder.Add(feld);
        }

        /// <summary>
        /// Erzeugt aus der in der Zugdatenbank gespeicherten Bewegungsspur einer Figur wieder
        /// ein ausgeführtes <see cref="MoveCommand"/>.
        /// </summary>
        /// <returns>null, wenn sich die Figur in diesem Zug noch nicht bewegt hat</returns>
        public static MoveCommand? RekonstruiereBewegung(Spielfigur figur) {
            var spur = new Bewegungsspur(figur);
            var wegpunkte = spur.ToList();
            if (wegpunkte.Count == 0)
                return null;

            var start = new KleinfeldPosition(figur.gf_von, figur.kf_von);
            var ziel = wegpunkte[wegpunkte.Count - 1];
            var via = wegpunkte.Count > 1 ? wegpunkte.Take(wegpunkte.Count - 1).ToList() : null;

            string befehl = $"Bewege {SimpleParser.UnitTypeToString(figur.Typ)} {figur.Nummer} von {start.CreateBezeichner()} nach {ziel.CreateBezeichner()}";
            if (via != null)
                befehl += " via " + string.Join(", ", via.Select(p => p.CreateBezeichner()));

            var command = new MoveCommand(befehl) {
                Figur = figur.Typ,
                UnitId = figur.Nummer,
                FromLocation = start,
                ToLocation = ziel,
                ViaLocations = via,
            };
            command.SetzeZustandVorBewegung(BewegungsZustand.ZugStart(figur));
            return command;
        }

        /// <summary>
        /// Stellt die Bewegungen aller Figuren des eigenen Reiches als Befehle wieder her.
        /// Wird aufgerufen, nachdem die Zugdaten geladen wurden.
        /// </summary>
        public static void RekonstruiereAlleBewegungen() {
            var armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
            foreach (var figur in armee) {
                var command = RekonstruiereBewegung(figur);
                if (command != null)
                    SharedData.CommandQueue.Enqueue(command);
            }
        }
    }
}
