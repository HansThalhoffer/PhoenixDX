using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {

    /// <summary>
    /// Die Phasen, in die der Spielzug eines Reiches aufgeteilt ist.
    /// Die Werte entsprechen der Spalte Phase in der Tabelle settings der Zugdatenbank.
    /// </summary>
    public enum Zugphase {
        /// <summary>Es wird gerüstet und gebaut</summary>
        Rüstphase = 0,
        /// <summary>Es wird bewegt und gekämpft</summary>
        Bewegungsphase = 1,
        /// <summary>Der Zug ist abgegeben, es geht nichts mehr</summary>
        Abgeschlossen = 2,
    }

    /// <summary>
    /// Die Sicht auf den laufenden Spielzug: Monat, Phase und Zugreihenfolge.
    ///
    /// Der Monat steht an zwei Stellen - im Namen des Zugdatenverzeichnisses und in der Tabelle
    /// settings der Zugdatenbank. Die settings-Tabelle wurde laut Kommentar der Altanwendung genau
    /// dafür angelegt, abzusichern, dass nicht die falsche Datei im falschen Verzeichnis liegt.
    /// Geprüft wurde das bisher nie; <see cref="PrüfeMonatskonsistenz"/> holt das nach.
    /// </summary>
    public static class ZugView {

        /// <summary>
        /// Die Einstellungen der geladenen Zugdatenbank
        /// </summary>
        public static ZugdatenSettings? Settings {
            get {
                if (SharedData.ZugdatenSettings == null || SharedData.ZugdatenSettings.Count == 0)
                    return null;
                return SharedData.ZugdatenSettings.Last();
            }
        }

        /// <summary>
        /// Der Monat, den die geladene Zugdatenbank selbst angibt
        /// </summary>
        public static Zugmonat? MonatLautDatenbank => Settings == null ? null : new Zugmonat(Settings.Monat);

        /// <summary>
        /// Der Zug, mit dem die Anwendung arbeitet. Er kommt aus dem Verzeichnisnamen der Zugdaten.
        /// </summary>
        public static Zugmonat AktuellerZug => new(ProgramView.SelectedMonth);

        /// <summary>
        /// Die Phase, in der diese Sitzung steckt, solange die Zugdaten keine brauchbare führen.
        /// </summary>
        private static Zugphase? _phaseDieserSitzung = null;

        /// <summary>
        /// Gehört die settings-Zeile zu dem Zug, der gespielt wird?
        ///
        /// Nur dann sagt ihre Phase etwas über diesen Zug aus. Im Datenbestand ist das nicht so:
        /// die Tabelle läuft dem Zugverzeichnis um sieben Monate voraus - in Verzeichnis 168 steht
        /// Monat 175, in 169 steht 176, in 170 steht 177 -, während die Schatzkammer mit dem
        /// Verzeichnis übereinstimmt. Die Zeile gehört also zu einem anderen Monat.
        ///
        /// Das bleibt so, bis der Spieler die Phase wechselt: <see cref="SetzePhase"/> stellt den
        /// Monat dabei richtig, und von da an gehört die Zeile zu diesem Zug.
        /// </summary>
        public static bool PhaseStehtInDenZugdaten
            => Settings != null && Settings.Monat == ProgramView.SelectedMonth;

        /// <summary>
        /// Die Phase, in der sich der Zug befindet.
        ///
        /// Der Zug beginnt mit dem Rüsten (Regelwerk 3.3); die Bewegungsphase folgt, wenn der
        /// Spieler die Rüstphase beendet. Steht in den Zugdaten eine Phase, die zu diesem Zug
        /// gehört, gilt sie. Sonst führt die Anwendung die Phase selbst und fängt beim Rüsten an.
        ///
        /// Das ist kein Schönheitsfehler, sondern hat die Anwendung lahmgelegt: in jedem Zug des
        /// Bestandes steht Phase = 1, also Bewegungsphase, in einer Zeile, die zu einem anderen
        /// Monat gehört. Damit war die Rüstphase nie erreichbar und es liess sich nichts bauen,
        /// nichts rüsten - ohne dass irgendwo stand, warum.
        ///
        /// Ein abgeschlossener Zug bleibt dagegen abgeschlossen, auch wenn die Zeile sonst nicht
        /// passt: diese Angabe setzt niemand versehentlich, und zurück geht es ohnehin nicht.
        ///
        /// Sobald der Spieler die Phase selbst wechselt, stellt <see cref="SetzePhase"/> den Monat
        /// der Zeile richtig. Von da an gehört sie zu diesem Zug, und die Phase übersteht das
        /// Schliessen der Anwendung.
        /// </summary>
        public static Zugphase Phase {
            get {
                var settings = Settings;
                if (settings == null)
                    return Zugphase.Rüstphase;

                // alles jenseits der bekannten Werte gilt als abgeschlossener Zug
                if (settings.Phase >= (int)Zugphase.Abgeschlossen)
                    return Zugphase.Abgeschlossen;

                if (PhaseStehtInDenZugdaten == false)
                    return _phaseDieserSitzung ?? Zugphase.Rüstphase;

                return (Zugphase)settings.Phase;
            }
        }

        /// <summary>
        /// In der Rüstphase wird gerüstet und gebaut (Regelwerk 3.3)
        /// </summary>
        public static bool KannRüsten => Phase == Zugphase.Rüstphase;

        /// <summary>
        /// Bewegt wird erst, wenn die Rüstphase abgeschlossen ist
        /// </summary>
        public static bool KannBewegen => Phase == Zugphase.Bewegungsphase;

        /// <summary>
        /// Eine lesbare Beschreibung der aktuellen Phase für die Oberfläche
        /// </summary>
        public static string PhasenBeschreibung => Phase switch {
            Zugphase.Rüstphase => "Rüstphase",
            Zugphase.Bewegungsphase => "Bewegungsphase",
            _ => "Zug abgeschlossen",
        };

        /// <summary>
        /// Der Name der Anwendung, wie er im Fenstertitel steht
        /// </summary>
        public const string Anwendungsname = "Phoenix";

        /// <summary>
        /// Was der Fenstertitel zeigt: für welches Reich gespielt wird, welcher Zug läuft, in
        /// welchem Monat er liegt, was für ein Monat das ist und in welcher Phase der Zug steckt.
        ///
        /// Etwa: <c>Phoenix - Theostelos - Zug 612, Larn (Sommer, Einnahmemonat) - Bewegungsphase</c>
        ///
        /// Zwei Dinge stecken in "was für ein Monat": der Rüst- oder Einnahmemonat aus dem
        /// Jahreslauf (Regelwerk 3.2 und 3.3, siehe <see cref="Zugmonat"/>) und die Phase innerhalb
        /// des eigenen Zuges. Das eine sagt, was dieser Monat im Spieljahr bedeutet, das andere,
        /// was der Spieler gerade tun darf.
        ///
        /// Was noch nicht geladen ist, fehlt auch im Titel - dann steht dort, dass nichts geladen
        /// ist. So sieht man auf einen Blick, ob die Anwendung überhaupt Daten hat.
        /// </summary>
        public static string Titelzeile {
            get {
                List<string> teile = [Anwendungsname];

                string? reich = ProgramView.SelectedNation?.Reich;
                if (string.IsNullOrWhiteSpace(reich) == false)
                    teile.Add(reich);

                var zug = AktuellerZug;
                if (zug.IstGültig) {
                    teile.Add($"Zug {zug.Zug}, {zug.Beschreibung}");
                    teile.Add(PhasenBeschreibung);
                }

                if (teile.Count == 1)
                    teile.Add("keine Zugdaten geladen");
                return string.Join(" - ", teile);
            }
        }

        /// <summary>
        /// Schaltet auf die nächste Phase weiter.
        ///
        /// Aus der Rüstphase geht es in die Bewegungsphase - danach kann nicht mehr gerüstet werden.
        /// Der Schritt in den abgeschlossenen Zug gehört zur Zugabgabe und passiert nicht hier.
        /// Zurück geht es nicht; das entspricht der Altanwendung, in der der Phasenknopf ebenfalls
        /// nur in eine Richtung schaltet.
        /// </summary>
        public static Result NächstePhase() {
            return Phase switch {
                Zugphase.Rüstphase => SetzePhase(Zugphase.Bewegungsphase),
                Zugphase.Bewegungsphase => Result.Fail("Die Bewegungsphase ist die letzte Phase des Zuges",
                    "Der Zug wird über die Zugabgabe abgeschlossen, nicht über den Phasenwechsel."),
                _ => Result.Fail("Der Zug ist bereits abgeschlossen",
                    "Nach der Zugabgabe kann in diesem Zug nichts mehr geändert werden."),
            };
        }

        /// <summary>
        /// Setzt die Phase und schreibt sie in die Zugdatenbank.
        /// </summary>
        /// <param name="phase">die neue Phase</param>
        /// <param name="erzwingen">
        /// erlaubt auch einen Rückschritt - nur für die Spielleitung gedacht, die eine versehentlich
        /// beendete Rüstphase wieder öffnen muss
        /// </param>
        public static Result SetzePhase(Zugphase phase, bool erzwingen = false) {
            var settings = Settings;
            if (settings == null)
                return Result.Fail("Die Zugdaten sind nicht geladen",
                    "Ohne die Tabelle settings der Zugdatenbank lässt sich die Phase nicht ändern.");

            var alt = Phase;
            if (alt == phase)
                return Result.Fail($"Der Zug ist bereits in der {PhasenBeschreibung}", "Es gibt nichts zu ändern.");
            if (phase < alt && erzwingen == false)
                return Result.Fail("Eine Phase lässt sich nicht zurücknehmen",
                    $"Der Zug ist in der {PhasenBeschreibung}. Nur die Spielleitung kann eine bereits abgeschlossene Phase wieder öffnen.");

            settings.Phase = (int)phase;

            // Die Zeile bekommt den Monat, zu dem sie gehört.
            //
            // Ohne das hätte die Phase keinen Ort, an dem sie überlebt: die Zeile nennt einen
            // anderen Monat, ihr Wert wird beim Lesen nicht beachtet, und beim nächsten Start
            // stünde der Zug wieder in der Rüstphase - obwohl sie beendet wurde und es laut
            // Regelwerk kein Zurück gibt. Genau das ist aufgefallen: Zug 168, 169 und 170 zeigten
            // immer nur die Rüstphase, und damit war keine Figur zu bewegen.
            //
            // Je Zugdatenbank gibt es genau eine settings-Zeile, und die Datenbank gehört zu genau
            // einem Zug. Der Monat, den sie nennt, ist also schlicht falsch, wenn er von dem des
            // Verzeichnisses abweicht - die Schatzkammer derselben Datei hält es mit dem
            // Verzeichnis. Richtiggestellt wird er nur hier, wo der Benutzer ohnehin eine
            // Entscheidung über den Zug trifft, und er wird dabei genannt.
            if (PhaseStehtInDenZugdaten == false) {
                int falscherMonat = settings.Monat;
                settings.Monat = ProgramView.SelectedMonth;
                ProgramView.LogInfo($"Der Monat der settings-Tabelle wurde auf {settings.Monat} richtiggestellt",
                    $"Dort stand {falscherMonat}, während Zugverzeichnis und Schatzkammer {settings.Monat} nennen. "
                    + "Ohne die Richtigstellung wäre die Phase beim nächsten Start wieder verloren.");
            }

            // Auch für diese Sitzung merken - falls das Schreiben scheitert, gilt wenigstens hier
            // die Phase, die der Benutzer gesetzt hat.
            _phaseDieserSitzung = phase;
            SharedData.StoreQueue.Enqueue(settings);
            ProgramView.LogInfo($"Der Zug ist jetzt in der {PhasenBeschreibung}",
                $"Die Phase wurde von {alt} auf {phase} umgestellt.");
            ProgramView.Update(EventsAndArgs.ViewEventArgs.ViewEventType.UpdateEverything);
            return Result.Success($"Der Zug ist jetzt in der {PhasenBeschreibung}", $"Die Phase wurde von {alt} auf {phase} umgestellt.");
        }

        /// <summary>
        /// Der jüngste Monat, für den die Schatzkammer der geladenen Zugdatenbank einen Stand führt.
        ///
        /// Die Schatzkammer ist eine fortlaufende Historie mit einer Zeile je Monat und wird beim
        /// Zugende fortgeschrieben. Ihr letzter Eintrag ist damit ein unabhängiger Beleg dafür,
        /// welcher Monat in dieser Datenbank zuletzt gespielt wurde.
        /// </summary>
        public static int LetzterSchatzkammerMonat {
            get {
                if (SharedData.Schatzkammer == null || SharedData.Schatzkammer.Count == 0)
                    return 0;
                return SharedData.Schatzkammer.Max(k => k.monat);
            }
        }

        /// <summary>
        /// Bestimmt den Zug, mit dem die Anwendung rechnet, und meldet Widersprüche.
        ///
        /// Der Monat steht an mehreren Stellen: im Namen des Zugdatenverzeichnisses, in der Tabelle
        /// settings und - als Historie - in der Schatzkammer. Massgeblich ist der Verzeichnisname,
        /// weil er sich mit dem letzten Eintrag der Schatzkammer deckt; die settings-Tabelle läuft
        /// in gewachsenen Datenbeständen davon weg.
        ///
        /// Vom Monat hängen Rüst- und Einnahmemonat, die Zugreihenfolge, der Auftauchpunkt, der
        /// Stand der Schatzkammer und der Zugmonat neuer Rüstungsaufträge ab.
        /// </summary>
        /// <param name="monatAusVerzeichnis">der Zug, aus dessen Verzeichnis geladen wurde, oder 0</param>
        /// <returns>true, wenn alle Quellen übereinstimmen</returns>
        public static bool BestimmeAktuellenZug(int monatAusVerzeichnis = 0) {
            // Ein anderer Zug wird geladen - die Phase der letzten Sitzung gilt nicht mehr
            _phaseDieserSitzung = null;
            // und die Befehle des vorigen Monats gehören nicht in die Liste des laufenden
            int fremde = SharedData.Commands.EntferneFremdeZüge();
            if (fremde > 0)
                ProgramView.LogInfo($"{fremde} Befehle aus einem anderen Zug wurden aus der Liste genommen",
                    "Unter 'Aktueller Zug' steht, was in diesem Monat gemacht wurde. Ältere Befehle sind "
                    + "längst ausgewertet und lassen sich nicht mehr zurücknehmen.");
            var ausDatenbank = MonatLautDatenbank;
            int ausSchatzkammer = LetzterSchatzkammerMonat;

            if (monatAusVerzeichnis > 0)
                ProgramView.SelectedMonth = monatAusVerzeichnis;
            else if (ausSchatzkammer > 0)
                ProgramView.SelectedMonth = ausSchatzkammer;
            else if (ausDatenbank != null && ausDatenbank.IstGültig)
                ProgramView.SelectedMonth = ausDatenbank.Zug;

            int gewählt = ProgramView.SelectedMonth;
            bool stimmig = true;

            if (ausSchatzkammer > 0 && ausSchatzkammer != gewählt) {
                stimmig = false;
                ProgramView.LogWarning("Die Schatzkammer passt nicht zum Zugmonat",
                    $"Es wird mit Zug {gewählt} gerechnet, der letzte Eintrag der Schatzkammer stammt aber aus Monat {ausSchatzkammer}. "
                    + "Damit stimmt der Reichsschatz nicht, den die Anwendung anzeigt.");
            }

            if (ausDatenbank != null && ausDatenbank.IstGültig && ausDatenbank.Zug != gewählt) {
                stimmig = false;
                ProgramView.LogWarning("Der Monat in der settings-Tabelle passt nicht zum Zugmonat",
                    $"Es wird mit Zug {gewählt} gerechnet, die Tabelle settings der Zugdatenbank gibt aber {ausDatenbank} an"
                    + (ausSchatzkammer > 0 ? $", während die Schatzkammer bis Monat {ausSchatzkammer} reicht" : string.Empty)
                    + ". Die settings-Tabelle sollte richtiggestellt werden. Solange sie nicht "
                    + "stimmt, führt die Anwendung die Zugphase selbst und beginnt mit der Rüstphase; "
                    + "die Phase aus der Tabelle bleibt unbeachtet.");
            }

            return stimmig;
        }

        #region Zugreihenfolge

        /// <summary>
        /// Die Zugreihenfolge eines Monats, aufsteigend sortiert.
        /// Die Tabelle steht in der Kartendatenbank und führt den Monat als Text.
        /// </summary>
        public static List<Zugreihenfolge> GetZugreihenfolge(int zug) {
            if (SharedData.Zugreihenfolge == null)
                return [];
            return SharedData.Zugreihenfolge
                .Where(z => SimpleParser.ParseInt(z.Monat ?? string.Empty) == zug)
                .OrderBy(z => z.Reihenfolge)
                .ToList();
        }

        /// <summary>
        /// Die Zugreihenfolge des aktuellen Monats
        /// </summary>
        public static List<Zugreihenfolge> GetZugreihenfolge() => GetZugreihenfolge(AktuellerZug.Zug);

        /// <summary>
        /// Die Position der Nation in der Zugreihenfolge des Monats, oder 0 wenn sie nicht geführt wird
        /// </summary>
        public static int GetPositionInZugreihenfolge(Nation? nation, int zug) {
            if (nation == null)
                return 0;
            foreach (var eintrag in GetZugreihenfolge(zug)) {
                if (string.Equals(eintrag.Reich, nation.Name, StringComparison.OrdinalIgnoreCase))
                    return eintrag.Reihenfolge;
            }
            return 0;
        }

        /// <summary>
        /// Der von der Spielleitung für diesen Monat ausgewürfelte Auftauchpunkt für die Reise
        /// von der Pirateninsel nach Erkenfara (Regelwerk 6.6.3).
        ///
        /// Er steht in der Zugreihenfolge und gilt für den ganzen Monat, also für alle Reiche gleich.
        /// </summary>
        /// <returns>die Position des Teleportpunktes oder null, wenn für den Monat keiner eingetragen ist</returns>
        public static KleinfeldPosition? GetAuftauchpunkt(int zug) {
            foreach (var eintrag in GetZugreihenfolge(zug)) {
                var position = SimpleParser.ParseLocation(eintrag.Auftauchpunkt_A ?? string.Empty);
                if (position != null && position.gf > 0)
                    return position;
            }
            return null;
        }

        /// <summary>
        /// Der Auftauchpunkt des aktuellen Monats
        /// </summary>
        public static KleinfeldPosition? GetAuftauchpunkt() => GetAuftauchpunkt(AktuellerZug.Zug);

        #endregion
    }
}
