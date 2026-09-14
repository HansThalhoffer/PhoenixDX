using PhoenixModel.dbPZE;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ExternalTables {
    public class Feinde : Feindaufklaerung {
        public FigurType Typ { get; set; } = FigurType.None;
        public Nation? Nation { get; set; } = null;

        public Feinde(string zeile) {
            var parts = zeile.Split(';');
            try {
                Nummer = int.Parse(parts[0]);
                Reich = parts[1];
                Art = parts[2];
                gf = int.Parse(parts[3]);
                kf = int.Parse(parts[4]);
                if (parts.Length > 5) {
                    Notiz= parts[5];
                }
            }
            catch (Exception ex) {
                ProgramView.LogError($"Fehler beim Laden der Feinderkennung {zeile}", ex.Message);
            }
            if (Art.StartsWith("Krieger"))
                Typ = FigurType.Krieger;
            else if (Art.StartsWith("Reiter"))
                Typ = FigurType.Reiter;
            else if (Art.StartsWith("Kreatur"))
                Typ = FigurType.Kreatur;
            else if (Art.StartsWith("Schiff"))
                Typ = FigurType.Schiff;
            else if (Art.StartsWith("Zauberer")) {
                Typ = FigurType.Zauberer;
                if (Notiz.StartsWith("C"))
                    Typ = FigurType.CharakterZauberer;
            }
            else if (Art.StartsWith("Character"))
                Typ = FigurType.Charakter;
            else
                ProgramView.LogError($"Der Figur konnte kein Typ zugordnet werden", $"Die Zeile war nicht verständlich:{zeile}");
            this.Nation = NationenView.GetNationFromString(Reich);
        }

        /// <summary>
        /// hole alle Fremd eines Kleinfeldes
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Gegner GetFeinde(KleinfeldPosition pos) {
            Gegner gegner = new Gegner();
            if (SharedData.Feinde != null && SharedData.Feinde.Count > 0) {
                gegner.AddRange(SharedData.Feinde.Where(item => item.gf == pos.gf && item.kf == pos.kf));
            }
            return gegner;
        }

        /// <summary>
        /// Die Zeilennummer des Pseudo-Eintrags, mit dem die Altanwendung den Zeitpunkt der letzten
        /// Aktualisierung in der Datei ablegt. Er ist keine Einheit und gehört nicht auf die Karte.
        /// </summary>
        public const int Zeitstempeleintrag = 99999;

        /// <summary>
        /// Lädt die Feindaufklärung aus der Datei.
        ///
        /// Die Datei führt alle Einheiten, die das eigene Reich aufgeklärt hat - und je nach
        /// Herkunft auch die eigenen. Eigene gehören nicht hinein: die stehen vollständig in den
        /// Zugdaten, wären hier nur eine zweite, ältere Wahrheit und erschienen auf der Karte als
        /// Fremde. Die Altanwendung filtert sie ebenso heraus.
        ///
        /// Was geladen wurde, wird gemeldet. Es gibt mehrere Dateien dieses Namens im Datenbestand,
        /// und sie unterscheiden sich; ohne Meldung fällt eine veraltete nicht auf.
        /// </summary>
        public static void LoadFeinderkennung(string databaseLocation) {
            if (!File.Exists(databaseLocation)) {
                ProgramView.LogError("Zugdaten: Es wurde keine Feindaufklärung gefunden", $"Die Datei {databaseLocation} wurde nicht gefunden. Bitte überprüfe die Daten für die Anwendung.");
                return;
            }

            List<Feinde> gelesen = [];
            int leerzeilen = 0;
            foreach (var zeile in File.ReadAllLines(databaseLocation)) {
                if (string.IsNullOrWhiteSpace(zeile) || zeile.TrimStart().StartsWith('#')) {
                    leerzeilen++;
                    continue;
                }
                gelesen.Add(new Feinde(zeile));
            }

            var eigenes = ProgramView.SelectedNation;
            int eigene = 0, ohnePosition = 0, zeitstempel = 0;
            List<Feinde> brauchbar = [];
            foreach (var feind in gelesen) {
                if (feind.Nummer == Zeitstempeleintrag || string.Equals(feind.Reich, "Update", StringComparison.OrdinalIgnoreCase)) {
                    zeitstempel++;
                    continue;
                }
                if (eigenes != null && feind.Nation == eigenes) {
                    eigene++;
                    continue;
                }
                // 0/0 heisst: die Einheit ist bekannt, aber nicht geortet. Zeichnen laesst sie
                // sich nicht, verschweigen sollte man sie auch nicht.
                if (feind.gf <= 0 || feind.kf <= 0 || feind.kf > 48) {
                    ohnePosition++;
                    continue;
                }
                brauchbar.Add(feind);
            }
            // SharedData.Feinde ist eine BlockingCollection - erst leeren, dann fuellen
            SharedData.Feinde = [];
            foreach (var feind in brauchbar)
                SharedData.Feinde.Add(feind);

            var reiche = brauchbar.Select(feind => feind.Reich).Distinct().OrderBy(reich => reich);
            ProgramView.LogInfo($"{brauchbar.Count} fremde Einheiten aus der Feindaufklärung",
                $"Gelesen aus {databaseLocation}.\r\r"
                + $"{gelesen.Count} Einträge, davon {eigene} eigene übersprungen"
                + (ohnePosition > 0 ? $", {ohnePosition} bekannt aber nicht geortet" : string.Empty)
                + (zeitstempel > 0 ? $", {zeitstempel} Zeitstempeleintrag" : string.Empty)
                + (leerzeilen > 0 ? $", {leerzeilen} Leerzeilen" : string.Empty)
                + $".\r\rReiche: {string.Join(", ", reiche)}.");
        }
    }
}

