using PhoenixModel.Commands;
using PhoenixModel.dbErkenfara;
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
        /// kann hier der User eine Brücke bauen?
        /// </summary>
        /// 1.5.3	Bauwerk: Die Brücke	( BR )
        ///         Eine Brücke ermöglicht eine schnelle Überquerung eines Flusses.Sie wird über die Seiten zweier angrenzender Gemarken gebaut und darf nur errichtet werden, wenn beide Felder zu Beginn des Zuges zum eigenen Reichsgebiet gehören und dieselbe Höhenstufe besitzen.
        ///         Bei der Überquerung eines Flusses über eine Brücke wird gezogen, als ob an dieser Stelle kein Fluß vorhanden ist.Der Bau einer Brücke dauert einen Monat.
        ///         Baupunkte:	60		Preis:	3.000 GS
        /// <param name="kf">kleinfeld</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        public static bool CanConstructBridge(KleinFeld kf, Direction direction) {
            if (kf.IsWasser || ProgramView.BelongsToUser(kf) == false || ProgramView.CanConstruct() == false)
                return false;
            var pos = KartenKoordinaten.GetNachbar(kf, direction);

            // nachbar muss selbe Höhenstufe haben
            if (SharedData.Map != null && pos != null && SharedData.Map[pos.CreateBezeichner()].Terrain.Höhe == kf.Terrain.Höhe) {
                if (KleinfeldView.HasRiver(kf, direction) == true && KleinfeldView.HasBridge(kf, direction) == false) {
                    // nun noch Kosten berechnen und schauen, ob das Geld langt
                    int kosten = KostenView.GetGSKosten(ConstructionElementType.Bruecke);
                    if (SchatzkammerView.HasEnoughMoney(kf, kosten)) {
                        return true;
                    }
                    return true;
                }
            }
            return false;
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
        public static bool CanConstructRoad(KleinFeld kf, Direction direction) {
            if (kf.IsWasser || ProgramView.BelongsToUser(kf) == false || ProgramView.CanConstruct() == false)
                return false;
            if (KleinfeldView.HasRoad(kf, direction))
                return false;

            int kosten = KostenView.GetGSKosten(ConstructionElementType.Strasse);
            if (SchatzkammerView.HasEnoughMoney(kf, kosten)) {
                return true;
            }

            return true;
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
        public static bool CanConstructKai(KleinFeld kf, Direction direction) {
            if (kf.IsKüste || ProgramView.BelongsToUser(kf) == false || ProgramView.CanConstruct() == false)
                return false;

            // laut Regelwerk nur auf Höhe 1 möglich
            if (kf.Terrain.Höhe != 1)
                return false;
            if (KleinfeldView.HasKai(kf, direction))
                return false;

            var pos = KartenKoordinaten.GetNachbar(kf, direction);
            if (SharedData.Map != null && pos != null) {
                if (SharedData.Map[pos.CreateBezeichner()].IsWasser) {
                    int kosten = KostenView.GetGSKosten(ConstructionElementType.Kai);
                    if (SchatzkammerView.HasEnoughMoney(kf, kosten)) {
                        return true;
                    }
                }

            }
            return false;
        }

        /// <summary>
        /// kann hier der User eine Wallanlage bauen?
        /// </summary>
        /// <param name="kf">kleinfeld</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        public static bool CanConstructWall(KleinFeld kf, Direction direction) {
            if (kf.IsWasser || ProgramView.BelongsToUser(kf) == false || ProgramView.CanConstruct() == false)
                return false;
            if (KleinfeldView.HasWall(kf, direction) == false) {
                int kosten = KostenView.GetGSKosten(ConstructionElementType.Wall);
                if (SchatzkammerView.HasEnoughMoney(kf, kosten)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// kann hier der User eine Burg bauen?
        /// </summary>
        /// <param name="kf">kleinfeld</param>
        /// <param name="direction">Richtung</param>
        /// <returns></returns>
        public static bool CanConstructCastle(KleinFeld kf) {
            if (kf.IsWasser || ProgramView.BelongsToUser(kf) == false || ProgramView.CanConstruct() == false)
                return false;
            if (kf.Gebäude == null) {
                // nun noch Kosten berechnen und schauen, ob das Geld langt
                int kosten = KostenView.GetGSKosten(ConstructionElementType.Burg);
                if ( SchatzkammerView.HasEnoughMoney(kf, kosten) ) {
                    return true;
                }
            }
            return false;
        }
        


    }
}
