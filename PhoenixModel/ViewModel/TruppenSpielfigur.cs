using PhoenixModel.ExternalTables;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Die Spielfiguren unterscheiden sich in zwei Typen: Truppen- und NamensSpielfigur.
    /// Diese wurden in der Altanwendung noch in weitere Tabellen aufgeteilt, obwohl viele Daten identisch sind.
    /// Hier werden diese wieder weitgehend zusammengefasst.
    /// </summary>
    public abstract class TruppenSpielfigur : Spielfigur {
        /// <summary>
        /// Gibt die aktuelle Stärke der Truppen-Spielfigur zurück.
        /// </summary>
        public override string Stärke { get { return staerke.ToString("n0"); } }

        /// <summary>
        /// Stärke vor Beginn des aktuellen Zuges.
        /// </summary>
        public int staerke_alt { get; set; } = 0;

        /// <summary>
        /// Aktuelle Stärke der Spielfigur.
        /// </summary>
        public int staerke { get; set; } = 0;

        /// <summary>
        /// Hitpoints vor Beginn des aktuellen Zuges.
        /// </summary>
        public int hf_alt { get; set; } = 0;

        /// <summary>
        /// Aktuelle Hitpoints der Spielfigur.
        /// </summary>
        public int hf { get; set; } = 0;

        /// <summary>
        /// Leichte Katapulte vor Beginn des aktuellen Zuges.
        /// </summary>
        public int lkp_alt { get; set; }
        public int LKP_alt { get => lkp_alt; set { lkp_alt = value; } }

        /// <summary>
        /// Aktuelle Anzahl der leichten Katapulte.
        /// </summary>
        public int LKP { get; set; }

        /// <summary>
        /// Schwere Katapulte vor Beginn des aktuellen Zuges.
        /// </summary>
        public int skp_alt { get; set; }
        public int SKP_alt { get => skp_alt; set { skp_alt = value; } }

        /// <summary>
        /// Aktuelle Anzahl der schweren Katapulte.
        /// </summary>
        public int SKP { get; set; }

        /// <summary>
        /// Anzahl der Pferde vor Beginn des aktuellen Zuges.
        /// </summary>
        public int pferde_alt { get; set; }

        /// <summary>
        /// Aktuelle Anzahl der Pferde.
        /// </summary>
        public int Pferde { get; set; }

        /// <summary>
        /// Gibt an, ob die Einheit eine Garde ist.
        /// </summary>
        public bool Garde { get; set; }

        /// <summary>
        /// Gibt an, ob die Einheit gebannt ist.
        /// </summary>
        public int isbanned { get; set; }

        /// <summary>
        /// Befindet sich ein Heer auf einer Flotte oder Schifft sich dort ein oder aus, dann steht hier die TargetID der Flotte abgelegt
        /// gleichzeitig wird auf der Flotte die Nummern aller Einheiten hinterlegt, die sich auf den Schiffen befinden
        /// Einschiffen und Ausschiffen wird in Befehl_bew hinterlegt
        /// </summary>
        public string auf_Flotte { get; set; } = string.Empty;

        /// <summary>
        /// Weitere Informationen zur Einheit.
        /// </summary>
        public string Sonstiges { get; set; } = string.Empty;
        /// <summary>
        /// Die Truppen spalten sich auf in 2 Heere
        /// es ist noch unklar, wie mehre Audteilungen hier abgelegt werden
        /// </summary>
        public string spaltetab { get; set; } = string.Empty;
        /// <summary>
        /// zwei Heere fusionieren miteinander
        /// es ist noch unklar, wie mehre Fuionsierungen hier abgelegt werden
        /// </summary>
        public string fusmit { get; set; } = string.Empty;
        /// <summary>
        /// auch hier wieder unterschiedliche Schreibweisen
        /// </summary>
        public string chars { get; set; } = string.Empty;
        public string Chars { get => chars; set { chars = value; } }

        /// <summary>
        /// Befehl zur Bewegung.
        /// hier steht zB
        /// #AGV:102 - wenn sich das Reiterheeer 205 durch Absitzen von den Pferden in ein Kriegerheer verwandelt => Reiterheer aufgelöst, nun steht auf dem Kleinfeld Krieger 102 mit Pferden
        ///            Bei den Kriegern 102 steht dann im gleichen Feld #AGZ:205
        /// #AGZ:205 - wenn sich das Kriegerheer 102 durch Aufsitzen auf ausreichend Pferde in ein Reiterheer verwandelt => Kriegerheer evtl. aufgelöst bzw abgespalten zu einem Reiterheer 205
        ///            Bei den Reitern steht dann #AGZ: 102
        ///            Die Befehle lassen sich kombinieren und verketten. So können die Pferde den Besitzer wechseln, indem erst aufgesessen, dann wieder abgesessen wird
        /// #SCE:    - wenn Krieger oder Reiter einschiffen, dann steht dort die TargetID der Flotte. Gleichzeitig wird auch bei beiden Einheiten <see cref="auf_Flotte"/> aktualisiert
        ///            bei den Schiffen steht dann #SCA:[Truppe]
        /// #SCA:    - wenn Krieger oder Reiter ausschiffen, dann steht dort die TargetID der Flotte. Gleichzeitig wird auch bei beiden Einheiten <see cref="auf_Flotte"/> aktualisiert
        ///            bei den Schiffen steht dann #SCA:[Truppe]
        /// "#SCEA   - heißt (vermutlich) auf die Flotte und gleich wieder runter im selben Zug
        /// </summary>
        public string Befehl_bew { get; set; } = string.Empty;

        /// <summary>
        /// Befehl zum Angriff.
        /// </summary>
        public string Befehl_ang { get; set; } = string.Empty;

        /// <summary>
        /// Befehl zur Eroberung.
        /// </summary>
        public string Befehl_erobert { get; set; } = string.Empty;

        /// <summary>
        /// Gibt eine Liste mit Eigenschaften der Spielfigur zurück.
        /// </summary>
        public override List<Eigenschaft> Eigenschaften {
            get {
                List<Eigenschaft> list = [];

                list.Add(new Eigenschaft("ID", Nummer.ToString(), false, this));
                string typ = BaseTyp.ToString();
                string str = Stärke;
                string katapult = string.Empty;
                int lineCount = 1;
                if (LKP > 0) {
                    typ += BaseTyp == FigurType.Schiff ? "\r+ Leichte Kriegsschiffe" : "\r+ Leichte Katapulte";
                    katapult += "\r+ " + LKP + " LK";
                    lineCount++;
                }
                if (SKP > 0) {
                    typ += BaseTyp == FigurType.Schiff ? "\r+ Schwere Kriegsschiffe" : "\r+ Schwere Katapulte";
                    katapult += "\r+ " + SKP + " SK";
                    lineCount++;
                }
                if (Pferde > 0) {
                    typ += "\r+ Pferde";
                    katapult += "\r+ " + Pferde;
                    lineCount++;
                }
                list.Add(new Eigenschaft("Typ", typ, false, this));
                list.Add(new Eigenschaft("Stärke", str, false, this));
                list.Add(new Eigenschaft("Katapulte", katapult, false, this));
                list.Add(new Eigenschaft("Koordinaten", CreateBezeichner(), false, this));
                list.Add(new Eigenschaft(NamensSpielfigur.HeaderBeschriftung, Titel, true, this));
                list.Add(new Eigenschaft(NamensSpielfigur.HeaderCharakterName, CharakterName, true, this));
                list.Add(new Eigenschaft(NamensSpielfigur.HeaderSpielerName, SpielerName, true, this));
                return list;
            }
        }
    }
}
