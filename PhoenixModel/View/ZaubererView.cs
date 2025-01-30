using PhoenixModel.dbCrossRef;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {

    /// <summary>
    /// Diese Klasse implementiert die aktuellen Regeln für Zauberer. 
    /// Sie sollte bevorzugt vor den Werten aus der Zugdaten-Datenbank verwendet werden.
    /// </summary>
    public static class ZaubererView {

        /// <summary>
        /// Ermittelt die Kategorie eines Zauberers basierend auf seinen Gesamt-GP.
        /// </summary>
        /// <param name="wiz">Der Zauberer, dessen Kategorie ermittelt werden soll.</param>
        /// <returns>Die entsprechende Crossref_zauberer_teleport-Kategorie oder null, falls nicht gefunden.</returns>
        public static Crossref_zauberer_teleport? GetZaubererKategorie(Zauberer wiz) {
            if (SharedData.Crossref_zauberer_teleport != null) {
                foreach (var f in SharedData.Crossref_zauberer_teleport) {
                    if (wiz.GP_ges <= f.GP) {
                        return f;
                    }
                }
            }
            // Fehlerprotokollierung, falls die Kategorie nicht festgestellt werden kann
            ProgramView.LogError($"Die Kategorie des Zauberers kann {wiz} nicht festgestellt werden",
                $"Die Tabelle {Crossref_zauberer_teleport.TableName} muss geladen werden. Das ist nicht geschehen.");
            return null;
        }

        /// <summary>
        /// Ermittelt die Klasse eines Zauberers basierend auf seiner Kategorie.
        /// </summary>
        /// <param name="wiz">Der Zauberer, dessen Klasse ermittelt werden soll.</param>
        /// <returns>Die entsprechende Zaubererklasse oder Zaubererklasse.none, falls nicht gefunden.</returns>
        public static Zaubererklasse GetKlasse(Zauberer wiz) {
            if (GetZaubererKategorie(wiz) is var kategorie and not null) {
                Zaubererklasse zaubererklasse;
                if (Enum.TryParse(kategorie.ZX, true, out zaubererklasse)) // Case-insensitive comparison
                    return zaubererklasse;
            }
            return Zaubererklasse.none;
        }

        /// <summary>
        /// Gibt die maximalen Teleportpunkte eines Zauberers basierend auf seiner Kategorie zurück.
        /// </summary>
        /// <param name="wiz">Der Zauberer, dessen Teleportpunkte ermittelt werden sollen.</param>
        /// <returns>Die maximale Anzahl an Teleportpunkten oder 0, falls keine Kategorie gefunden wird.</returns>
        public static int GetMaxTeleportPunkte(Zauberer wiz) {
            if (GetZaubererKategorie(wiz) is var kategorie and not null) {
                return kategorie.Teleport;
            }
            return 0;
        }

        /// <summary>
        /// Gibt die Regenerationspunkte eines Zauberers basierend auf seiner Kategorie zurück.
        /// </summary>
        /// <param name="wiz">Der Zauberer, dessen Regeneration ermittelt werden soll.</param>
        /// <returns>Die Regenerationspunkte oder 0, falls keine Kategorie gefunden wird.</returns>
        public static int GetRegeneration(Zauberer wiz) {
            if (GetZaubererKategorie(wiz) is var kategorie and not null) {
                return kategorie.Regeneration_GP;
            }
            return 0;
        }


        /// <summary>
        /// Ermittelt ob der Zauberer eine Barriere errichten kann
        /// </summary>
        public static bool CanCastBarriere( Zauberer figur, KleinfeldPosition? kf = null, Direction? direction = null) {
            return true;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer auf ein benachbartes Kleinfeld bannen kann
        /// </summary>
        public static bool CanCastBannen( Zauberer figur, KleinfeldPosition? kf = null) {
            return true;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer teleportieren kann und damit einige Leute mitnehmen
        /// </summary>
        public static bool CanCastTeleport( Zauberer figur, KleinfeldPosition? kf = null, List<Spielfigur>? teleportPayLoad = null) {
            return true;
        }

        /// <summary>
        /// Ermittelt ob der Zaubeer einen benachbarten Zauberer zum Duell auffordern kann
        /// </summary>
        public static bool CanCastDuell( Zauberer figur, KleinfeldPosition? kf = null) {
            return true;
        }
    }
}