using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {
    
    /// <summary>
    /// Die Klasse setzt die aktuellen Regeln um, sie sollte bevorzugt vor den Werten in der Zugdaten Datenbank genutzt werden
    /// </summary>
    public static class ZaubererView {

        public static Crossref_zauberer_teleport? GetZaubererKategorie(Zauberer wiz) {
            if (SharedData.Crossref_zauberer_teleport != null) {
                foreach (var f in SharedData.Crossref_zauberer_teleport) {
                    if (wiz.GP_ges <= f.GP) {
                        return f;
                    }
                }
            }
            ProgramView.LogError($"Die Kategorie des Zauberers kann {wiz} nicht festgestellt werden", $"Die Tabelle {Crossref_zauberer_teleport.TableName} muss geladen werden. Das ist nicht geschehen.");
            return null;
        }

        public static Zaubererklasse GetKlasse(Zauberer wiz) {
            if(GetZaubererKategorie(wiz) is var kategorie and not null) {
                Zaubererklasse zaubererklasse;
                if (Enum.TryParse(kategorie.ZX, true, out zaubererklasse)) // Case-insensitive comparison
                    return zaubererklasse;
            }
            return Zaubererklasse.none;
        }

        public static int GetMaxTeleportPunkte(Zauberer wiz) {
            if (GetZaubererKategorie(wiz) is var kategorie and not null) {
                return kategorie.Teleport;
            }
            return 0;
        }
        
        public static int GetRegeneration(Zauberer wiz) {
            if (GetZaubererKategorie(wiz) is var kategorie and not null) {
                return kategorie.Regeneration_GP;
            }
            return 0;
        }
    }
}
