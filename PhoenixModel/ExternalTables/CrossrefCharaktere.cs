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
            this.Abkürzung = abkürzung;
            this.MinGutPunkte = MinGP;
            this.Bezeichnung = bezeichnung; 
        }

       public static readonly CrossrefCharaktere[] Kategorien =
       {
            // TODO: klärung mit wie vielen ein Heerführer gerüstet wird
            new CrossrefCharaktere(Characterklasse.HF, 0, "HF#", "Heerführer"),
            new CrossrefCharaktere(Characterklasse.BUH, 24, "HF#", "Heerführer"),
            new CrossrefCharaktere(Characterklasse.FSH, 36, "HF#", "Heerführer"),
            new CrossrefCharaktere(Characterklasse.HER, 60, "HF#", "Heerführer"),

        };
    }
}
