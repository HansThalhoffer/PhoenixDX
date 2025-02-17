using PhoenixModel.Commands;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Rules {
    public static class ConstructRules {

        /// <summary>
        /// Überprüft, ob ein Bauwerk nicht auf Wasser errichtet werden kann.
        /// </summary>
        /// <param name="kf">Das zu überprüfende Kleinfeld.</param>
        /// <returns>Ein Fehler-Resultat, falls das Bauwerk nicht erlaubt ist, sonst null.</returns>
        public static Result? IsNotAllowedOnWater(KleinFeld? kf) {
            if (kf == null)
                return Result.Fail($"Wo ist das Kleinfeld?", $"Bitte ein Kleinfeld übergeben");

            if (kf.IsWasser)
                return Result.Fail("Das Bauwerk kann nicht auf dem Wasser errichtet werden", "Bitte wähle ein weniger nasses Kleinfeld aus");
            return Result.Success();
        }

        /// <summary>
        /// Überprüft, ob das Bauwerk auf eigenem Gebiet errichtet werden darf.
        /// </summary>
        /// <param name="kf">Das zu überprüfende Kleinfeld.</param>
        /// <returns>Ein Fehler-Resultat, falls das Bauwerk nicht erlaubt ist, sonst null.</returns>
        public static Result? IsAllowedForOwner(KleinFeld? kf) {
            if (kf == null)
                return Result.Fail($"Wo ist das Kleinfeld?", $"Bitte ein Kleinfeld übergeben");

            if (ProgramView.BelongsToUser(kf) == false)
                return Result.Fail("Das Bauwerk kann nur auf dem eigenen Gebiet errichtet werden", "Bitte wähle ein Kleinfeld aus, das dir gehört");
            return Result.Success();
        }

        /// <summary>
        /// Überprüft, ob sich das Spiel in der Rüstphase befindet.
        /// </summary>
        /// <returns>Ein Fehler-Resultat, falls das Bauwerk nicht erlaubt ist, sonst null.</returns>
        public static Result? IsRüstPhase() {
            if (ProgramView.CanConstruct() == false)
                return Result.Fail("Das Bauwerk kann nur in der Rüstphase errichtet werden", "Bitte wähle einen passenden Zeitpunkt dafür");
            return Result.Success();
        }

        /// <summary>
        /// Überprüft, ob genügend Geld für den Bau vorhanden ist.
        /// </summary>
        /// <param name="kf">Das Kleinfeld, auf dem das Bauwerk errichtet werden soll.</param>
        /// <param name="kosten">Die Kosten für das Bauwerk.</param>
        /// <returns>Ein Fehler-Resultat, falls nicht genügend Geld vorhanden ist, sonst null.</returns>
        public static Result? IsEnoughMoney(KleinFeld? kf, int kosten) {
            if (kf == null)
                return Result.Fail($"Wo ist das Kleinfeld?", $"Bitte ein Kleinfeld übergeben");

            if (SchatzkammerView.HasEnoughMoney(kf, kosten) == false)
                return Result.Fail($"Das Bauwerk kostet mehr als in der Schatzkammer ist", $"Bitte gehe betteln! {kosten} kostet das Bauwerk. In der Schatzkammer sind nur noch {SchatzkammerView.MoneyToSpendThisTurn()}");
            return Result.Success();
        }

        /// <summary>
        /// kann hier der User eine Brücke bauen?
        /// </summary>
        /// 1.5.3	Bauwerk: Die Brücke	( BR )
        ///         Eine Brücke ermöglicht eine schnelle Überquerung eines Flusses.Sie wird über die Seiten zweier angrenzender Gemarken gebaut und darf nur errichtet werden, wenn beide Felder zu Beginn des Zuges zum eigenen Reichsgebiet gehören und dieselbe Höhenstufe besitzen.
        ///         Bei der Überquerung eines Flusses über eine Brücke wird gezogen, als ob an dieser Stelle kein Fluß vorhanden ist.Der Bau einer Brücke dauert einen Monat.
        ///         Baupunkte:	60		Preis:	3.000 GS
        /// <param name="kf">kleinfeld</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        public static Result CanConstructBridge(KleinFeld kf, Direction direction) {
            if (IsNotAllowedOnWater(kf) is Result wasserTest && wasserTest.HasErrors)
                return wasserTest;

            if (IsAllowedForOwner(kf) is Result ownerTest && ownerTest.HasErrors)
                return ownerTest;

            if (IsRüstPhase() is Result phaseTest && phaseTest.HasErrors)
                return phaseTest;


            var pos = KartenKoordinaten.GetNachbar(kf, direction);
            // nachbar muss selbe Höhenstufe haben
            if (SharedData.Map != null && pos != null && SharedData.Map[pos.CreateBezeichner()].Terrain.Höhe == kf.Terrain.Höhe) {

                if (KleinfeldView.HasRiver(kf, direction) == false)
                    return Result.Fail($"Ohne Fluss keine Brücke", $"In dieser Richtung gibt es keinen Fluss {direction}");

                if (KleinfeldView.HasBridge(kf, direction) == true)
                    return Result.Fail($"Hier steht schon eine Brücke", $"In dieser Richtung {direction} braucht es keine mehr");

                // nun noch Kosten berechnen und schauen, ob das Geld langt
                if (IsEnoughMoney(kf, KostenView.GetGSKosten(ConstructionElementType.Bruecke)) is Result moneyTest && moneyTest.HasErrors)
                    return moneyTest;

                //var cmd = new ConstructCommand($"Errichte Brücke im {direction} von {kf.CreateBezeichner()}");
                return Result.Success();
            }
            else
                return Result.Fail($"Das geht zu steil bergauf für eine Brücke", $"Die Brücke muss auf benachbarten Feldern mit gleicher Höhenstüfe errichtet werden");
        }

        /// <summary>
        /// kann hier der User eine Strasse bauen?
        /// 1.5.1	Bauwerk: Die Straße	( STR )
        ///         Eine Straße ermöglicht generell ein schnelleres Vorankommen.Strassen gehen von einem Feldmittelpunkt zu einem benachbarten Feldmittelpunkt.Eine Truppe bewegt sich quasi von Feldmitte zu Feldmitte auf Strasse.Straßen können nicht gebaut werden, wenn die Höhenstufendifferenz zum Nachbarfeld, in das die Straße führen soll, größer als 1 ist.
        ///         Auf Straßen können Heere 2 Höhenstufen hintereinander in einem Monat überwinden, sofern eine Straße über mehrere Gemarken diese in ihrem Verlauf überbrückt und die Bewegungspunkte des Heeres ausreichen.Darüber hinaus ermöglichen Straßen in gewissen Geländeformationen die Bewegung von Katapulten(siehe Tabelle 5 ).
        ///         Alle in einem Monat  in einer Gemark errichteten Straßen sind getrennte Bauwerke in Bezug auf.Der Bau einer Straße dauert einen Monat. 
        ///         Bauwerke haben keine automatischen Strassen mehr.
        ///         Jede Einheit verfügt über 2 Höhenstufenpunkte, das Überwinden einer Höhenstufe kostet ohne Hilfsmittel 2 Punkte.
        ///         Eine Strasse oder eine Kaianlage vermindern die Kosten auf 1.
        ///         Beispiel 1:
        ///         Ein Heer zieht über Strasse auf ein Hochlandfeld (Kosten 2 Punkte - 1 wegen Strasse = 1 Punkt). Das Heer hat nun 1 HP(Höhenstufenpunkt) über.
        /// 
        ///         Es könnte damit nun zB.eine weitere Höhenstufe auf einer Strasse überqueren oder aber auch über eine Kaianlage einschiffen.
        ///         Es kann nicht ohne eine Strasse eine weitere HS überwinden.
        /// 
        ///         Beispiel 2:
        ///         Ein Heer Schifft über eine Kaianlage ein und hat damit einen Höhenstufenpunkt über, somit kann es in der gleichen Runde auch wieder über eine Kaianlage ausschiffen.
        /// </summary>
        /// <param name="kf">kleinfeld</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        public static Result CanConstructRoad(KleinFeld? kf, Direction direction) {
            if (kf == null)
                return Result.Fail($"Wo ist das Kleinfeld?", $"Bitte ein Kleinfeld übergeben");

            if (IsNotAllowedOnWater(kf) is Result wasserTest && wasserTest.HasErrors)
                return wasserTest;

            if (IsAllowedForOwner(kf) is Result ownerTest && ownerTest.HasErrors)
                return ownerTest;

            if (IsRüstPhase() is Result phaseTest && phaseTest.HasErrors)
                return phaseTest;

            if (KleinfeldView.HasRoad(kf, direction))
                return Result.Fail($"Hier ist schon eine Strasse", $"In dieser Richtung {direction} braucht es keine mehr");

            if (IsEnoughMoney(kf, KostenView.GetGSKosten(ConstructionElementType.Strasse)) is Result moneyTest && moneyTest.HasErrors)
                return moneyTest;

            //  var cmd = new ConstructCommand($"Errichte Straße im {direction} von {kf.CreateBezeichner()}");
            return Result.Success();
        }


        /// <summary>
        /// kann hier der User eine Kaianlage bauen?
        /// 1.5.2	Bauwerk: Die Kaianlage	( KA )
        ///         Eine Kaianlage ermöglicht eine schnellere Überquerung einer Küstenlinie.Denn sie ermöglicht einem Heer die Überquerung einer weiteren Höhenstufe und wirkt dadurch wie eine Straße.An dieser Gemarkseite wird das Ein- und Ausschiffen von Truppen beschleunigt.Sie wird immer in einer Gemark der Höhenstufe 1 an einer zum Wasser liegenden Gemarkseite errichtet.Siehe auch Strasse.
        ///         Der Bau einer Kaianlage dauert einen Monat.
        ///         Baupunkte:	60		Preis:	3.000 GS
        /// </summary>
        /// <param name="kf">kleinfeld</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        public static Result CanConstructKai(KleinFeld? kf, Direction direction) {
            if (kf == null)
                return Result.Fail($"Wo ist das Kleinfeld?", $"Bitte ein Kleinfeld übergeben");

            if (IsNotAllowedOnWater(kf) is Result wasserTest && wasserTest.HasErrors)
                return wasserTest;

            if (IsAllowedForOwner(kf) is Result ownerTest && ownerTest.HasErrors)
                return ownerTest;

            if (IsRüstPhase() is Result phaseTest && phaseTest.HasErrors)
                return phaseTest;

            // laut Regelwerk nur auf Höhe 1 möglich
            if (kf.Terrain.Höhe != 1)
                return Result.Fail($"Hier ist es zu hoch", $"Kai Anlagen dürfen nur in der Höhenstufe 1 gebaut werden");

            if (KleinfeldView.HasKai(kf, direction))
                return Result.Fail($"Hier ist schon ein Kai", $"In dieser Richtung {direction} braucht es keinen mehr");

            var pos = KartenKoordinaten.GetNachbar(kf, direction);
            if (SharedData.Map != null && pos != null) {
                if (SharedData.Map[pos.CreateBezeichner()].IsWasser) {
                    if (IsEnoughMoney(kf, KostenView.GetGSKosten(ConstructionElementType.Kai)) is Result moneyTest && moneyTest.HasErrors)
                        return moneyTest;
                    else
                        return Result.Success();
                }
                else
                    return Result.Fail($"Hier ist keine Küste", $"Kai-Anlagen müssen ins Wasser zeigen");

            }
            return Result.Fail("Geht nicht", "Weiß nicht, warum?");
        }

        /// <summary>
        /// kann hier der User eine Wallanlage bauen?
        /// </summary>
        /// <param name="kf">kleinfeld</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        public static Result CanConstructWall(KleinFeld? kf, Direction direction) {
            if (kf == null)
                return Result.Fail($"Wo ist das Kleinfeld?", $"Bitte ein Kleinfeld übergeben");

            if (IsNotAllowedOnWater(kf) is Result wasserTest && wasserTest.HasErrors)
                return wasserTest;

            if (IsAllowedForOwner(kf) is Result ownerTest && ownerTest.HasErrors)
                return ownerTest;

            if (IsRüstPhase() is Result phaseTest && phaseTest.HasErrors)
                return phaseTest;

            if (KleinfeldView.HasWall(kf, direction))
                return Result.Fail($"Hier ist schon ein Wall", $"In dieser Richtung {direction} braucht es keinen mehr");
            if (IsEnoughMoney(kf, KostenView.GetGSKosten(ConstructionElementType.Wall)) is Result moneyTest && moneyTest.HasErrors)
                return moneyTest;
            return Result.Success();
        }

        /// <summary>
        /// kann hier der User eine Burg bauen?
        /// </summary>
        /// <param name="kf">kleinfeld</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        public static Result CanConstructCastle(KleinFeld? kf) {
            if (kf == null)
                return Result.Fail($"Wo ist das Kleinfeld?", $"Bitte ein Kleinfeld übergeben");

            if (IsNotAllowedOnWater(kf) is Result wasserTest && wasserTest.HasErrors)
                return wasserTest;

            if (IsAllowedForOwner(kf) is Result ownerTest && ownerTest.HasErrors)
                return ownerTest;

            if (IsRüstPhase() is Result phaseTest && phaseTest.HasErrors)
                return phaseTest;

            if (kf != null && kf.Gebäude != null)
                return Result.Fail($"Hier ist schon ein Gebäude", $"Da braucht es keines mehr");

            if (IsEnoughMoney(kf, KostenView.GetGSKosten(ConstructionElementType.Burg)) is Result moneyTest && moneyTest.HasErrors)
                return moneyTest;

            return Result.Success();
        }



    }
}
