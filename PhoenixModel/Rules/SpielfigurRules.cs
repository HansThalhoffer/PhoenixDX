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
        /// TODO Berechnugn der Beweungspunkte 
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static int BerechneBewegungspunkte(Spielfigur figur) {
            //throw new NotImplementedException();
            return 0;
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
            int raumpunkte = 0;
            if (figur is TruppenSpielfigur truppe) {
                raumpunkte = kosten.Raumpunkte * truppe.staerke;
                if (truppe.Pferde > 0) {
                    var equipmentKosten = KostenView.GetKosten("P");
                    if (equipmentKosten != null)
                        raumpunkte += truppe.Pferde * equipmentKosten.Raumpunkte;
                }
                if (truppe.hf > 0) {
                    var equipmentKosten = KostenView.GetKosten("HF");
                    if (equipmentKosten != null)
                        raumpunkte += truppe.hf * equipmentKosten.Raumpunkte;
                }
                if (truppe.LKP > 0) {
                    var equipmentKosten = KostenView.GetKosten(truppe.BaseTyp == FigurType.Schiff ? "LKS" : "LKP");
                    if (equipmentKosten != null)
                        raumpunkte += truppe.LKP * equipmentKosten.Raumpunkte;
                }
                if (truppe.SKP > 0) {
                    var equipmentKosten = KostenView.GetKosten(truppe.BaseTyp == FigurType.Schiff ? "SKS" : "SKP");
                    if (equipmentKosten != null)
                        raumpunkte += truppe.SKP * equipmentKosten.Raumpunkte;
                }
               
            }
            return raumpunkte;
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
    }
}
