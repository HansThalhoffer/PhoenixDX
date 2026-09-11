using PhoenixModel.ExternalTables;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Die Regeln für den Übergang in den nächsten Spielzug.
    ///
    /// Vorlage ist Zugverwaltung.Zugende der Altanwendung mit UnitHelper.MoveToNextTurn und
    /// CharHelper.MoveToNextTurn. Hier stehen nur die Regeln je Figur; das Kopieren der Datenbank
    /// und das Leeren der Rüstungstabellen gehört zur Zugabgabe.
    ///
    /// Wichtig ist die Reihenfolge: erst <see cref="SetzeAufAusgangsposition"/> für alle Figuren,
    /// dann <see cref="SchiebeInNächstenZug"/>. Sonst verlieren Figuren, die sich nicht bewegt
    /// haben, ihre Position, weil der Zugübergang gf_nach als neue Ausgangsposition übernimmt.
    /// </summary>
    public static class ZugendeRules {

        /// <summary>
        /// Charaktere, die keine Zauberer sind, regenerieren pauschal 5 Gutpunkte pro Monat
        /// </summary>
        public const int RegenerationCharakter = 5;

        /// <summary>
        /// Ab dieser Nummer sind Charaktere Spielerfiguren; darunter werden die Teleportpunkte
        /// zu Zugbeginn zurückgesetzt.
        /// </summary>
        public const int ErsteSpielerfigurNummer = 600;

        /// <summary>
        /// Die Zauberkraftpunkte, die ein Zauberer mit dem angegebenen Gutpunktwert pro Monat
        /// regeneriert (Regelwerk Tabelle 7, Tabelle Crossref_zauberer_teleport der crossref.mdb).
        /// </summary>
        public static int GetRegeneration(int gutpunkte) {
            if (SharedData.Crossref_zauberer_teleport == null)
                return 0;
            foreach (var eintrag in SharedData.Crossref_zauberer_teleport) {
                if (eintrag.GP >= gutpunkte)
                    return eintrag.Regeneration_GP;
            }
            return 0;
        }

        /// <summary>
        /// Eine Figur ohne Heerführer beziehungsweise ohne Gutpunkte existiert nicht mehr und
        /// verschwindet beim Zugübergang von der Karte.
        /// </summary>
        public static bool IstAufgelöst(Spielfigur? figur) {
            return figur switch {
                TruppenSpielfigur truppe => truppe.hf == 0,
                NamensSpielfigur namens => namens.GP_ges == 0,
                _ => false,
            };
        }

        /// <summary>
        /// Figuren, die sich in diesem Zug nicht bewegt haben, bleiben stehen: ihre Zielposition
        /// wird auf die Ausgangsposition gesetzt. Ohne das würden sie beim Zugübergang die
        /// Position 0/0 erben und von der Karte verschwinden.
        /// </summary>
        /// <returns>true, wenn die Figur angepasst wurde</returns>
        public static bool SetzeAufAusgangsposition(Spielfigur? figur) {
            if (figur == null || figur.gf_nach > 0)
                return false;
            figur.gf_nach = figur.gf_von;
            figur.kf_nach = figur.kf_von;
            return true;
        }

        /// <summary>
        /// Überträgt eine Figur in den nächsten Spielzug: die erreichte Position wird zur neuen
        /// Ausgangsposition, die Bewegungspunkte werden aufgefrischt, Befehle und Bewegungsspur
        /// gelöscht und die Werte des Zugbeginns gesichert.
        /// </summary>
        public static void SchiebeInNächstenZug(Spielfigur? figur) {
            if (figur == null)
                return;
            if (figur is TruppenSpielfigur truppe)
                SchiebeTruppeInNächstenZug(truppe);
            else if (figur is NamensSpielfigur namens)
                SchiebeNamensfigurInNächstenZug(namens);
        }

        private static void SchiebeTruppeInNächstenZug(TruppenSpielfigur truppe) {
            // die Werte des Zugbeginns sichern, damit die Verluste des Monats ablesbar bleiben
            truppe.staerke_alt = truppe.staerke;
            truppe.hf_alt = truppe.hf;
            truppe.LKP_alt = truppe.LKP;
            truppe.SKP_alt = truppe.SKP;
            truppe.pferde_alt = truppe.Pferde;
            truppe.GS_alt = truppe.GS;
            truppe.Kampfeinnahmen_alt = truppe.Kampfeinnahmen;

            ÜbernehmePosition(truppe);

            truppe.Befehl_bew = string.Empty;
            truppe.Befehl_ang = string.Empty;
            truppe.Befehl_erobert = string.Empty;
            truppe.Sonstiges = string.Empty;
            truppe.spaltetab = string.Empty;
            truppe.fusmit = string.Empty;
            truppe.isbanned = 0;
        }

        private static void SchiebeNamensfigurInNächstenZug(NamensSpielfigur namens) {
            namens.GP_ges_alt = namens.GP_ges;
            namens.GP_akt_alt = namens.GP_akt;

            // Gutpunkte regenerieren, aber nie über den Höchstwert hinaus
            if (namens.GP_akt < namens.GP_ges) {
                int regeneration = namens.BaseTyp == FigurType.Zauberer
                    ? GetRegeneration(namens.GP_ges)
                    : RegenerationCharakter;
                namens.GP_akt = Math.Min(namens.GP_akt + regeneration, namens.GP_ges);
            }

            ÜbernehmePosition(namens);

            // die Teleportpunkte der Nichtspielerfiguren werden jeden Monat zurückgesetzt
            if (namens.Nummer < ErsteSpielerfigurNummer) {
                namens.tp = 0;
                namens.tp_alt = 0;
            }
            namens.Teleport_gf_von = 0;
            namens.Teleport_kf_von = 0;
            namens.Teleport_gf_nach = 0;
            namens.Teleport_kf_nach = 0;

            namens.Befehl_magie = string.Empty;
            namens.Befehl_Teleport = string.Empty;
            namens.Befehl_bannt = string.Empty;
        }

        /// <summary>
        /// Die im abgelaufenen Zug erreichte Position wird zur Ausgangsposition des neuen Zuges.
        /// Aufgelöste Figuren verschwinden dabei von der Karte.
        /// </summary>
        private static void ÜbernehmePosition(Spielfigur figur) {
            if (IstAufgelöst(figur)) {
                figur.gf_von = 0;
                figur.kf_von = 0;
            }
            else {
                figur.gf_von = figur.gf_nach;
                figur.kf_von = figur.kf_nach;
            }
            figur.gf_nach = 0;
            figur.kf_nach = 0;

            figur.bp = figur.bp_max;
            figur.hoehenstufen = 0;
            new Bewegungsspur(figur).Clear();
        }
    }
}
