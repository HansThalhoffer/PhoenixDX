using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
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
            // Nicht über den Index: die Aufzählung Characterklasse hat sechs Werte, die Tabelle
            // hatte vier Einträge in anderer Reihenfolge. Ein Stadthalter kam so als Festungsherr
            // heraus, und ein Herrscher liess die Anwendung mit einer IndexOutOfRangeException
            // stehen.
            string beschriftung = figur.Beschriftung.ToUpper();
            // Ein Zivilist hat kein Amt - die Aufzählung führt ihn deshalb nicht, und die
            // Kategorientabelle hat keinen Eintrag für ihn. Ohne diese Zeile fiele er in die
            // Schätzung unten und käme als Heerführer heraus, weil der bei 0 Gutpunkten anfängt.
            if (beschriftung.StartsWith("CIV") || beschriftung.StartsWith("ZIV"))
                return null;
            if (beschriftung.StartsWith("BUH"))
                return CrossrefCharaktere.Get(Characterklasse.BUH);
            else if (beschriftung.StartsWith("STH"))
                return CrossrefCharaktere.Get(Characterklasse.STH);
            else if (beschriftung.StartsWith("FSH"))
                return CrossrefCharaktere.Get(Characterklasse.FSH);
            else if (beschriftung.StartsWith("HER"))
                return CrossrefCharaktere.Get(Characterklasse.HER);
            else if (beschriftung.StartsWith("HF"))
                return CrossrefCharaktere.Get(Characterklasse.HF);

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
