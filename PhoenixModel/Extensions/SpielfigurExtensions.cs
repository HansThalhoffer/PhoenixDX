using PhoenixModel.Commands;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Runtime.CompilerServices;

namespace PhoenixModel.Extensions {
    public static class SpielfigurExtensions {

        /// <summary>
        /// Setzt eine Figur auf ein Kleinfeld - keine Bewegung
        /// </summary>
        /// <param name="spielfigur">Die Spielfigur, die gesetzt wird.</param>
        /// <param name="kleinfeld">Das Kleinfeld, auf das die Figur gesetzt werden soll.</param>
        public static void PutOnKleinfeld(this Spielfigur spielfigur, KleinFeld kleinfeld) {
            if (spielfigur.gf_von > 0 || !string.IsNullOrEmpty(spielfigur.ph_xy) || spielfigur.kf_von > 0) {
                ProgramView.LogError(
                    $"Fehler bei dem Setzen der Figur {spielfigur.Bezeichner} auf {kleinfeld.Bezeichner}",
                    "Eine bereits auf dem Spielfeld befindliche Figur darf nicht erneut gesetzt werden, sondern muss bewegt werden."
                );
                return;
            }

            spielfigur.ph_xy = kleinfeld.ph_xy;
            spielfigur.gf_von = kleinfeld.gf;
            spielfigur.kf_von = kleinfeld.kf;
        }

        /// <summary>
        /// Setzt eine Figur auf ein Kleinfeld - keine Bewegung
        /// </summary>
        /// <param name="spielfigur">Die Spielfigur, die gesetzt wird.</param>
        /// <param name="kleinfeld">Das Kleinfeld, auf das die Figur gesetzt werden soll.</param>
        public static void MoveToKleinfeld(this Spielfigur spielfigur, KleinFeld kleinfeld) {
            if (spielfigur.gf > 0 || !string.IsNullOrEmpty(spielfigur.ph_xy) || spielfigur.kf > 0) {
                ProgramView.LogError(
                    $"Fehler bei dem Setzen der Figur {spielfigur.Bezeichner} auf {kleinfeld.Bezeichner}",
                    "Eine bereits auf dem Spielfeld befindliche Figur darf nicht erneut gesetzt werden, sondern muss bewegt werden."
                );
                return;
            }

            spielfigur.ph_xy = kleinfeld.ph_xy;
            spielfigur.gf_nach = kleinfeld.gf;
            spielfigur.kf_nach = kleinfeld.kf;
        }


        /// <summary>
        /// Ermittelt ob die Figur auf einem Schiff ist
        /// </summary>
        public static bool IsOnShip(this Spielfigur spielfigur) {
            return spielfigur.BaseTyp != ExternalTables.FigurType.Schiff && spielfigur is TruppenSpielfigur figur && string.IsNullOrEmpty(figur.auf_Flotte) == false;
        }


        /// <summary>
        /// Ermittelt ob die Figur auf einem Schiff ist und Land in der Nähe
        /// </summary>
        public static bool CanDisEmbark(this Spielfigur figur) {
            if (figur is NamensSpielfigur)
                return false;
            if (figur.IsOnShip() == false)
                return false;
            var nachbarn = KleinfeldView.GetNachbarn(figur);
            if (nachbarn != null) {
                foreach (var nachbar in nachbarn) {
                    if (nachbar.IsWasser == false) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Ermittelt ob die Figur an der Küste ist und ein Schiff in der Nähe
        /// </summary>
        public static bool CanEmbark(this Spielfigur figur) {
            if (figur is NamensSpielfigur)
                return false;
            if (figur.IsOnShip() == true)
                return false;
            var nachbarn = KleinfeldView.GetNachbarn(figur);
            if (nachbarn != null) {
                foreach (var nachbar in nachbarn) {
                    if (nachbar.IsWasser) {
                        var armee = nachbar.Truppen;
                        if (armee != null && armee.Count > 0) {
                            foreach (var arme in armee) {
                                if (arme.BaseTyp == PhoenixModel.ExternalTables.FigurType.Schiff) {
                                    return true;
                                }
                            }
                        }
                    }
                }

            }
            return false;
        }


        /// <summary>
        /// Ermittelt ob die Figur schießen kann
        /// </summary>
        public static bool CanShoot(this Spielfigur figur) {
            return figur is TruppenSpielfigur truppen && (truppen.LKP > 0 || truppen.SKP > 0);
        }

        /// <summary>
        /// Ermittelt ob die Figur aufsitzen kann
        /// </summary>
        public static bool CanSattleUp(this Spielfigur figur) {
            return figur is Krieger truppen && (truppen.Pferde > 0);
        }

        /// <summary>
        /// Ermittelt ob die Figur aufsitzen kann
        /// </summary>
        public static bool CanSattleDown(this Spielfigur figur) {
            return figur is Reiter;
        }

        /// <summary>
        /// Ermittelt den aktuellen Wert von gf (aus gf_nach oder gf_von)
        /// </summary>
        public static int GetGf(this Spielfigur spielfigur) {
            return spielfigur.gf_nach > 0 ? spielfigur.gf_nach : spielfigur.gf_von;
        }

        /// <summary>
        /// Ermittelt den aktuellen Wert von kf (aus kf_nach oder kf_von).
        /// </summary>
        public static int GetKf(this Spielfigur spielfigur) {
            return spielfigur.kf_nach > 0 ? spielfigur.kf_nach : spielfigur.kf_von;
        }


        /// <summary>
        /// Ermittelt ob der Zauberer eine Barriere errichten kann
        /// </summary>
        public static bool CanCastBarriere(this Spielfigur figur, KleinfeldPosition? kf = null, Direction? direction = null) {
            return SpielfigurRules.CanCastBarriere(figur,kf,direction);
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer auf ein benachbartes Kleinfeld bannen kann
        /// </summary>
        public static bool CanCastBannen(this Spielfigur figur, KleinfeldPosition? kf = null) {
            return SpielfigurRules.CanCastBannen(figur, kf);
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer teleportieren kann und damit einige Leute mitnehmen
        /// </summary>
        public static bool CanCastTeleport(this Spielfigur figur, KleinfeldPosition? kf = null, List<Spielfigur>? teleportPayLoad = null) {
            return SpielfigurRules.CanCastTeleport(figur, kf, teleportPayLoad);
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer einen benachbarten Zauberer zum Duell auffordern kann
        /// </summary>
        public static bool CanCastDuell(this Spielfigur figur, KleinfeldPosition? kf = null) {
            return SpielfigurRules.CanCastDuell(figur, kf);
        }


        /// <summary>
        /// Ermittelt ob die Truppe sich aufteilen kann
        /// </summary>
        public static bool CanSplit(this Spielfigur figur) {
            return SpielfigurRules.CanSplit(figur);
        }

        /// <summary>
        /// Ermittelt ob die Spielfigur Fusionieren kann
        /// </summary>
        public static bool CanFustion(this Spielfigur figur) {
            return SpielfigurRules.CanFusion(figur);
        }
    

        /// <summary>
        /// Weist die Spielfigur der ausgewählten Nation zu.
        /// </summary>
        /// <param name="spielfigur">Die Spielfigur, die zugewiesen wird.</param>
        /// <exception cref="InvalidOperationException">Wird ausgelöst, wenn keine Nation ausgewählt ist.</exception>
        public static void AssignToSelectedReich(this Spielfigur spielfigur) {
            if (ProgramView.SelectedNation == null)
                throw new InvalidOperationException("Zuerst muss eine Nation ausgewählt sein.");

            spielfigur.Nation = ProgramView.SelectedNation;
        }

        public static IEnumerable<IPhoenixCommand>? GetCommands(this Spielfigur spielfigur) {
            return SharedData.Commands.GetCommands(spielfigur);
        }
    }
}
