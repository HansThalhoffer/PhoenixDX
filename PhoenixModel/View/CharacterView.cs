using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {
    public class CharacterView {
        /// <summary>
        /// Die Charakterkategorie ist nicht wie die Zauberer zu verstehen, da Charaktere durch Beförderung ihre zusätzlichen Gutpunkte erzielen
        /// d.h. sie werden in ein Amt gesetzt und regenerieren pro Runde 5 Gutpunkte
        /// Für die Generierung von Testdaten ist die Funktion aber sehr hilfreich
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static CrossrefCharaktere? GetAssumedCharacterKategorie(Character figur) {
            string beschriftung = figur.Beschriftung.ToUpper();
            if (beschriftung.StartsWith("HF"))
                return CrossrefCharaktere.Kategorien[(int)Characterklasse.HF];
            else if (beschriftung.StartsWith("BUH"))
                return CrossrefCharaktere.Kategorien[(int)Characterklasse.BUH];
            else if (beschriftung.StartsWith("STH"))
                return CrossrefCharaktere.Kategorien[(int)Characterklasse.STH];
            else if (beschriftung.StartsWith("FSH"))
                return CrossrefCharaktere.Kategorien[(int)Characterklasse.FSH];
            else if (beschriftung.StartsWith("HER"))
                return CrossrefCharaktere.Kategorien[(int)Characterklasse.HER];

            foreach (var f in CrossrefCharaktere.Kategorien.Reverse()) {
                if (f.MinGutPunkte <= figur.GP_ges) {
                    return f;
                }
            }

            ProgramView.LogError($"Die Kategorie des Charakters kann {figur} nicht festgestellt werden", $"Unklar warum.");
            return null;
        }

        /// <summary>
        /// holt die anhand der Gutpunkte geschätzte Charakterklasse
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static Characterklasse GetAssumedKlasse(Character figur) {
            if (GetAssumedCharacterKategorie(figur) is var kategorie and not null) {
                return kategorie.Klasse;
            }
            return Characterklasse.none;
        }
    }
}
