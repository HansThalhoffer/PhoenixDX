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
        /// lädt aus der Feindaufklaerung.dat die Daten und legt diese an
        /// </summary>
        /// <param name="inBackground"></param>
        public static void LoadFeinderkennung(string databaseLocation) {
            if (!File.Exists(databaseLocation)) {
                ProgramView.LogError("Zugdaten: Es wurde keine Feindaufklärung gefunden", $"Die Datei {databaseLocation} wurde nicht gefunden. Bitte überprüfe die Daten für die Anwendung.");
                return;
            }
            SharedData.Feinde = [];
            List<Feinde> list = [];
            foreach (var line in File.ReadAllLines(databaseLocation)) {
                list.Add(new PhoenixModel.ExternalTables.Feinde(line));
            }
            foreach (var feind in  list.Where(item => item.Nation != ProgramView.SelectedNation && item.gf > 0 && item.kf > 0 && item.kf <=48))
                SharedData.Feinde.Add(feind);
        }
    }
}

