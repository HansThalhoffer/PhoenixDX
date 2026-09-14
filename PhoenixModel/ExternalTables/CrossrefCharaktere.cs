using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PhoenixModel.ExternalTables {
    
    public class CrossrefCharaktere : IEigenschaftler {

        // IEigenschaftler
        public string Bezeichner => Abkürzung;
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public Characterklasse Klasse { get; set; } = Characterklasse.none;
        public int MinGutPunkte { get; set; } = 0;
        public string Abkürzung { get; set; } = string.Empty;
        public string Bezeichnung {  get; set; } = string.Empty;   
        CrossrefCharaktere(Characterklasse klasse, int MinGP, string abkürzung, string bezeichnung) {
            this.Klasse = klasse;
            this.Abkürzung = abkürzung;
            this.MinGutPunkte = MinGP;
            this.Bezeichnung = bezeichnung;
        }

        /// <summary>
        /// Die Ämter der weltlichen Charaktere, in der Reihenfolge ihres Ranges.
        ///
        /// Die Gutpunkte stammen aus dem Beispiel zur Beförderung (Regelwerk 1.9.1): ein Burgherr
        /// wird "auf 24 / 24 GP gesetzt", der Stadthalter steht bei 24/36, der Herrscher bei
        /// 36/60. Für den Festungsherrn nennt das Regelwerk keinen Wert; er steht im Rang
        /// zwischen Stadthalter und Herrscher und ist hier entsprechend eingeordnet. Das ist eine
        /// Annahme und steht als solche in den offenen Regelfragen.
        ///
        /// Nicht über den Index ansprechen: die Aufzählung Characterklasse führt zusätzlich
        /// "none". <see cref="Get"/> sucht den Eintrag zur Klasse.
        /// </summary>
       public static readonly CrossrefCharaktere[] Kategorien =
       {
            // TODO: klärung mit wie vielen ein Heerführer gerüstet wird
            new CrossrefCharaktere(Characterklasse.HF, 0, "HF#", "Heerführer"),
            new CrossrefCharaktere(Characterklasse.BUH, 24, "BUH#", "Burgherr"),
            new CrossrefCharaktere(Characterklasse.STH, 36, "STH#", "Stadthalter"),
            new CrossrefCharaktere(Characterklasse.FSH, 48, "FSH#", "Festungsherr"),
            new CrossrefCharaktere(Characterklasse.HER, 60, "HER#", "Herrscher"),

        };

        /// <summary>
        /// Der Eintrag zu einem Amt, oder null wenn es keines gibt.
        /// </summary>
        public static CrossrefCharaktere? Get(Characterklasse klasse)
            => Kategorien.FirstOrDefault(eintrag => eintrag.Klasse == klasse);
    }
}
