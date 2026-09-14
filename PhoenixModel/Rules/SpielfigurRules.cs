using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Rules {
    public static class SpielfigurRules {


        /// <summary>
        /// Die maximalen Bewegungspunkte einer Spielfigur.
        /// Die Berechnung steht in <see cref="BewegungsRules.BerechneBewegungspunkte(Spielfigur?)"/>,
        /// zusammen mit den übrigen Bewegungsregeln.
        /// </summary>
        public static int BerechneBewegungspunkte(Spielfigur figur) {
            return BewegungsRules.BerechneBewegungspunkte(figur);
        }

        /// <summary>
        /// Berechnung der Raumpunkte aus den Daten einer Spielfigur
        /// Krieger 1
        /// Pferd 1
        /// Reiter 2
        /// Schiff 100
        /// LKP/LKS 1000
        /// SKP/SKS 2000
        /// Char/Wiz Gutpunkte * 50
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static int BerechneRaumpunkte(Spielfigur figur) {
            // die formeln für die RP von Charaktern und Zauberern ist fix
            if (figur is NamensSpielfigur namens) {
                if (figur is Character hero && hero.IsSpielerFigur == false)
                    return 600;
                return namens.GP_akt * 50;
            }
            // in der Kostentabelle stehen die Raumpunktwerte zu den einzelnen Figurtypen 
            var kosten = KostenView.GetKosten(figur);
            if (kosten == null) {
                ProgramView.LogError($"Für die Figur {figur} findet sich kein Eintrag in der Kostentabelle", "Für die Berechnung der Raumpunkte muss die Figur in der Kostentabelle existieren");
                return 0;
            }
            if (figur is TruppenSpielfigur truppe)
                return BerechneRaumpunkte(truppe, truppe.staerke, truppe.Pferde, truppe.hf, truppe.LKP, truppe.SKP);
            return 0;
        }

        /// <summary>
        /// Berechnet die Raumpunkte einer gedachten Zusammensetzung.
        ///
        /// Gebraucht wird das beim Teilen eines Heeres: dort muss feststehen, ob beide Hälften die
        /// Mindestgrösse erreichen, bevor überhaupt etwas verändert wird.
        /// </summary>
        /// <param name="vorbild">die Truppe, deren Gattung die Kosten bestimmt</param>
        public static int BerechneRaumpunkte(TruppenSpielfigur vorbild, int stärke, int pferde, int heerführer, int lkp, int skp) {
            var kosten = KostenView.GetKosten(vorbild);
            if (kosten == null) {
                ProgramView.LogError($"Für die Figur {vorbild} findet sich kein Eintrag in der Kostentabelle", "Für die Berechnung der Raumpunkte muss die Figur in der Kostentabelle existieren");
                return 0;
            }

            int raumpunkte = kosten.Raumpunkte * stärke;
            raumpunkte += Raumpunkte("P", pferde);
            raumpunkte += Raumpunkte("HF", heerführer);
            raumpunkte += Raumpunkte(vorbild.BaseTyp == FigurType.Schiff ? "LKS" : "LKP", lkp);
            raumpunkte += Raumpunkte(vorbild.BaseTyp == FigurType.Schiff ? "SKS" : "SKP", skp);
            return raumpunkte;
        }

        /// <summary>
        /// Die Raumpunkte einer Anzahl eines Rüstgutes laut Kostentabelle
        /// </summary>
        private static int Raumpunkte(string rüstgut, int anzahl) {
            if (anzahl <= 0)
                return 0;
            var kosten = KostenView.GetKosten(rüstgut);
            return kosten == null ? 0 : anzahl * kosten.Raumpunkte;
        }

        /// <summary>
        /// Ermittelt ob die Truppe sich aufteilen kann
        /// </summary>
        public static bool CanSplit(Spielfigur figur) {
            return figur is TruppenSpielfigur truppe && CanSplit(truppe);
        }

        /// <summary>
        /// Ermittelt ob die Spielfigur Fusionieren kann
        /// </summary>
        public static bool CanFusion(Spielfigur figur) {
            return figur is TruppenSpielfigur truppe && CanFusion(truppe);
        }

        /// <summary>
        /// Ermittelt ob die Truppe sich aufteilen kann
        /// </summary>
        public static bool CanSplit(TruppenSpielfigur truppe) {
            return truppe.Heerführer > 1 && truppe.staerke > 1;
        }

        /// <summary>
        /// Ermittelt ob die Figur mit einer auf dem gleichen Kleinfeld vorhandenen Truppe fusionieren kann
        /// </summary>
        public static bool CanFusion(TruppenSpielfigur truppe) {
            var kf = KleinfeldView.GetKleinfeld(truppe);
            if (kf == null)
                return false;
            // eine weitere Spielfigur gleichen Typs auf dem Kleinfeld?
            return kf.Truppen.Where(t => t.BaseTyp == truppe.BaseTyp && t != truppe).Any();
        }


        /// <summary>
        /// Ermittelt ob der Zauberer eine Barriere errichten kann
        /// </summary>
        public static bool CanCastBarriere(Spielfigur figur, KleinfeldPosition? kf = null, Direction? direction = null) {
            if (figur is Zauberer wiz)
                return CanCastBarriere(wiz, kf, direction);
            return false;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer auf ein benachbartes Kleinfeld bannen kann
        /// </summary>
        public static bool CanCastBannen(Spielfigur figur, KleinfeldPosition? kf = null) {
            if (figur is Zauberer wiz)
                return CanCastBannen(wiz, kf);
            return false;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer teleportieren kann und damit einige Leute mitnehmen
        /// </summary>
        public static bool CanCastTeleport(Spielfigur figur, KleinfeldPosition? kf = null, List<Spielfigur>? teleportPayLoad = null) {
            if (figur is Zauberer wiz)
                return CanCastTeleport(wiz, kf, teleportPayLoad);
            return false;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer einen benachbarten Zauberer zum Duell auffordern kann
        /// </summary>
        public static bool CanCastDuell(Spielfigur figur, KleinfeldPosition? kf = null) {
            if (figur is Zauberer wiz)
                return CanCastDuell(wiz, kf);
            return false;
        }

        /// <summary>
        /// Ermittelt ob der Zauberer eine Barriere errichten kann
        /// </summary>
        public static bool CanCastBarriere(Zauberer figur, KleinfeldPosition? kf = null, Direction? direction = null) {
            return true;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer auf ein benachbartes Kleinfeld bannen kann
        /// </summary>
        public static bool CanCastBannen(Zauberer figur, KleinfeldPosition? kf = null) {
            return true;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer teleportieren kann und damit einige Leute mitnehmen
        /// </summary>
        public static bool CanCastTeleport(Zauberer figur, KleinfeldPosition? kf = null, List<Spielfigur>? teleportPayLoad = null) {
            return true;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer einen benachbarten Zauberer zum Duell auffordern kann
        /// </summary>
        public static bool CanCastDuell(Zauberer figur, KleinfeldPosition? kf = null) {
            return true;
        }

        /// <summary>
        /// Legt eine Rechenkopie eines Heeres an.
        ///
        /// Gebraucht wird sie in der Kampfauswertung: der Beschuss nimmt Truppen weg, bevor der
        /// Nahkampf rechnet, und beides soll nachvollziehbar bleiben, ohne die Figuren des
        /// Spielers zu verändern. Die Kopie ist deshalb ausdrücklich kein Spielstein: sie gehört
        /// keiner Sammlung an, steht in keiner Datenbank und wird nie gespeichert. Kopiert wird
        /// nur, was für den Kampf zählt.
        /// </summary>
        /// <returns>die Kopie, oder null bei einer Gattung ohne Entsprechung</returns>
        public static TruppenSpielfigur? KopiereFürBerechnung(TruppenSpielfigur? vorbild) {
            TruppenSpielfigur? kopie = vorbild switch {
                Krieger => new Krieger(),
                Reiter => new Reiter(),
                Schiffe => new Schiffe(),
                Kreaturen => new Kreaturen(),
                _ => null,
            };
            if (kopie == null || vorbild == null)
                return null;

            kopie.Nummer = vorbild.Nummer;
            kopie.Nation = vorbild.Nation;
            kopie.staerke = vorbild.staerke;
            kopie.hf = vorbild.hf;
            kopie.LKP = vorbild.LKP;
            kopie.SKP = vorbild.SKP;
            kopie.Pferde = vorbild.Pferde;
            kopie.Garde = vorbild.Garde;
            kopie.isbanned = vorbild.isbanned;
            kopie.GS = vorbild.GS;
            kopie.Kampfeinnahmen = vorbild.Kampfeinnahmen;
            kopie.auf_Flotte = vorbild.auf_Flotte;
            kopie.gf_von = vorbild.gf_von;
            kopie.kf_von = vorbild.kf_von;
            kopie.gf_nach = vorbild.gf_nach;
            kopie.kf_nach = vorbild.kf_nach;
            return kopie;
        }

    }
}
