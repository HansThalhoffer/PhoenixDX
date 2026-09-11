using PhoenixModel.Database;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;
using System.Data.Common;
using System.IO;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database {

    /// <summary>
    /// Das Ergebnis einer Zugabgabe
    /// </summary>
    public class ZugabgabeErgebnis {
        public bool Erfolgreich { get; init; }
        public string Meldung { get; init; } = string.Empty;
        public string Details { get; init; } = string.Empty;
        /// <summary>Die Datenbank des Folgezuges, sofern sie angelegt wurde</summary>
        public string? NeueDatenbank { get; init; }
        /// <summary>Das Archiv, das an die Spielleitung geht</summary>
        public string? ÜbergabeArchiv { get; init; }
        public int ÜbernommeneFiguren { get; init; }
        public int AufgelösteFiguren { get; init; }
        public int NeuerReichsschatz { get; init; }

        public static ZugabgabeErgebnis Fehler(string meldung, string details = "")
            => new() { Erfolgreich = false, Meldung = meldung, Details = details };
    }

    /// <summary>
    /// Die Zugabgabe schliesst den Zug ab und legt die Datenbank des Folgezuges an.
    ///
    /// Vorlage ist Zugverwaltung.CopyReichDBFromOldZug der Altanwendung. Der Ablauf ist bewusst so
    /// gewählt, dass die Datenbank des abgegebenen Zuges als Archiv erhalten bleibt: sie behält die
    /// Rüstungen, die Bewegungsspuren und die Endpositionen. Erst die Kopie wird auf den Folgezug
    /// umgestellt.
    ///
    /// Die Reihenfolge ist:
    /// 1. den laufenden Monat abrechnen und die Figuren stehen lassen, die sich nicht bewegt haben
    /// 2. die Quelle als abgeschlossen markieren
    /// 3. die Datei kopieren
    /// 4. in der Kopie die Rüstungstabellen leeren, die Figuren in den Folgezug schieben,
    ///    den neuen Reichsschatz eintragen und den Monat weiterstellen
    /// </summary>
    public static class Zugabgabe {

        /// <summary>
        /// Die Tabellen, die im Folgezug leer anfangen. Was gerüstet wurde, ist bezahlt und steht
        /// ab dem Folgezug als Figur auf der Karte.
        /// </summary>
        private static readonly string[] ZuLeerendeTabellen = [
            Ruestung.TableName, RuestungBauwerke.TableName, RuestungRuestorte.TableName
        ];

        /// <summary>
        /// Der Pfad, an dem die Datenbank des Zielzuges liegen wird. Die Zugverzeichnisse liegen
        /// nebeneinander und heissen nach ihrer Zugnummer.
        /// </summary>
        public static string? BestimmeZielpfad(string quelle, int zielZug) {
            string? verzeichnis = Path.GetDirectoryName(quelle);
            string? wurzel = verzeichnis == null ? null : Path.GetDirectoryName(verzeichnis);
            if (wurzel == null)
                return null;
            return Path.Combine(wurzel, zielZug.ToString(), Path.GetFileName(quelle));
        }

        /// <summary>
        /// Gibt es die Datenbank des Folgezuges schon? Dann wurde der Zug bereits einmal abgegeben
        /// und ein erneutes Abgeben würde die dort begonnene Arbeit verwerfen.
        /// </summary>
        public static bool ZielExistiert(string quelle, int zielZug) {
            string? ziel = BestimmeZielpfad(quelle, zielZug);
            return ziel != null && File.Exists(ziel);
        }

        /// <summary>
        /// Führt die Zugabgabe durch.
        /// </summary>
        /// <param name="quelle">die Zugdatenbank des laufenden Zuges</param>
        /// <param name="passwort">das Passwort der Zugdatenbank</param>
        /// <param name="aktuellerZug">der laufende Zugmonat</param>
        /// <param name="zielÜberschreiben">
        /// eine bereits vorhandene Datenbank des Folgezuges überschreiben. Das verwirft alles, was
        /// im Folgezug schon gemacht wurde, und muss vom Benutzer ausdrücklich bestätigt werden.
        /// </param>
        public static ZugabgabeErgebnis Durchführen(string quelle, EncryptedString passwort, int aktuellerZug, bool zielÜberschreiben = false) {
            if (File.Exists(quelle) == false)
                return ZugabgabeErgebnis.Fehler("Die Zugdatenbank existiert nicht",
                    $"Unter {quelle} liegt keine Datei. Ohne sie lässt sich der Zug nicht abgeben.");

            var bericht = ZugabgabeView.ErstelleBericht();
            if (bericht.KannAbgegebenWerden == false)
                return ZugabgabeErgebnis.Fehler("Der Zug kann nicht abgegeben werden", string.Join(" ", bericht.Hindernisse));

            int folgeZug = aktuellerZug + 1;
            string? ziel = BestimmeZielpfad(quelle, folgeZug);
            if (ziel == null)
                return ZugabgabeErgebnis.Fehler("Der Ablageort für den Folgezug lässt sich nicht bestimmen",
                    $"Aus {quelle} ergibt sich kein Verzeichnis, neben dem das Verzeichnis {folgeZug} liegen könnte.");

            if (File.Exists(ziel) && zielÜberschreiben == false)
                return ZugabgabeErgebnis.Fehler($"Die Zugdaten für Zug {folgeZug} gibt es bereits",
                    $"{ziel} existiert schon. Ein erneutes Abgeben verwirft alles, was in Zug {folgeZug} bereits gemacht wurde.");

            string klartext = new PasswordHolder(passwort).DecryptedPassword ?? string.Empty;

            // 1. den laufenden Monat abschliessen: wer sich nicht bewegt hat, bleibt stehen
            var armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
            foreach (var figur in armee)
                ZugendeRules.SetzeAufAusgangsposition(figur);

            var abgerechnet = SchatzkammerRules.Zwischenbilanz(aktuellerZug);
            if (abgerechnet == null)
                return ZugabgabeErgebnis.Fehler($"Die Schatzkammer hat keinen Stand für Zug {aktuellerZug}",
                    "Ohne den Stand des laufenden Monats lässt sich der Reichsschatz des Folgezuges nicht berechnen.");

            var folge = SchatzkammerRules.BereiteFolgemonatVor(aktuellerZug);
            if (folge == null)
                return ZugabgabeErgebnis.Fehler("Der Reichsschatz des Folgezuges lässt sich nicht berechnen", string.Empty);

            // 2. die Abrechnung des laufenden Monats in der Quelle festhalten
            try {
                using var quellDb = Öffne(quelle, klartext);
                using var befehl = quellDb.OpenDBCommand();
                abgerechnet.Save(befehl);
                SetzeStehengebliebeneFiguren(befehl);
            }
            catch (Exception ex) {
                return ZugabgabeErgebnis.Fehler("Der laufende Zug liess sich nicht abschliessen", ex.Message);
            }

            // 3. das Paket für die Spielleitung. Es enthält den abgegebenen Zug, nicht den
            // Folgezug, und muss stehen, bevor irgendetwas weitergestellt wird - ohne Übergabe
            // ist der Zug nicht abgegeben.
            var paket = SpielleitungsUebergabe.ErstelleZugpaket(quelle, aktuellerZug);
            if (paket.Erfolgreich == false)
                return ZugabgabeErgebnis.Fehler(paket.Meldung,
                    $"{paket.Details} Ohne Übergabe an die Spielleitung wurde der Zug nicht abgegeben; es hat sich nichts verändert.");

            // 4. kopieren - ab hier wird nur noch in der Kopie gearbeitet
            try {
                string? zielVerzeichnis = Path.GetDirectoryName(ziel);
                if (zielVerzeichnis != null)
                    Directory.CreateDirectory(zielVerzeichnis);
                File.Copy(quelle, ziel, zielÜberschreiben);
            }
            catch (Exception ex) {
                return ZugabgabeErgebnis.Fehler($"Die Zugdatenbank liess sich nicht nach {ziel} kopieren", ex.Message);
            }

            // 5. die Kopie auf den Folgezug umstellen
            int aufgelöst = 0;
            int übernommen = 0;
            try {
                using var zielDb = Öffne(ziel, klartext);
                using var befehl = zielDb.OpenDBCommand();

                foreach (string tabelle in ZuLeerendeTabellen) {
                    befehl.CommandText = $"DELETE FROM {tabelle}";
                    befehl.ExecuteNonQuery();
                }

                foreach (var figur in armee) {
                    if (ZugendeRules.IstAufgelöst(figur)) {
                        LöscheFigur(befehl, figur);
                        aufgelöst++;
                        continue;
                    }
                    ZugendeRules.SchiebeInNächstenZug(figur);
                    if (figur is IDatabaseTable tabelle)
                        tabelle.Save(befehl);
                    übernommen++;
                }

                folge.Save(befehl);

                // die settings-Tabelle führt genau eine Zeile. Sie wird hier direkt gesetzt und
                // nicht über das geladene Objekt, denn das gehört noch zum laufenden Zug.
                befehl.CommandText = $"UPDATE {ZugdatenSettings.TableName} SET Monat = {folgeZug}, Phase = {(int)Zugphase.Rüstphase}";
                befehl.ExecuteNonQuery();
            }
            catch (Exception ex) {
                return ZugabgabeErgebnis.Fehler($"Die Zugdaten für Zug {folgeZug} liessen sich nicht vorbereiten",
                    $"{ex.Message} Die Datei {ziel} ist unvollständig und sollte gelöscht werden, bevor der Zug erneut abgegeben wird.");
            }

            // 6. erst jetzt gilt der Zug als abgegeben. Bricht vorher etwas ab, bleibt er offen
            // und kann nach dem Beheben der Ursache erneut abgegeben werden.
            try {
                using var quellDb = Öffne(quelle, klartext);
                using var befehl = quellDb.OpenDBCommand();
                befehl.CommandText = $"UPDATE {ZugdatenSettings.TableName} SET Phase = {(int)Zugphase.Abgeschlossen}";
                befehl.ExecuteNonQuery();
            }
            catch (Exception ex) {
                return ZugabgabeErgebnis.Fehler($"Zug {aktuellerZug} liess sich nicht als abgegeben vermerken",
                    $"{ex.Message} Die Zugdaten für Zug {folgeZug} liegen aber bereits unter {ziel}.");
            }

            var geladeneSettings = ZugView.Settings;
            if (geladeneSettings != null)
                geladeneSettings.Phase = (int)Zugphase.Abgeschlossen;

            SpielWPF.Log(new PhoenixModel.Program.LogEntry($"Zug {aktuellerZug} wurde abgegeben",
                $"Die Zugdaten für Zug {folgeZug} liegen unter {ziel}. Übernommen wurden {übernommen} Figuren, "
                + $"{aufgelöst} sind aufgelöst worden. Der Reichsschatz beträgt {folge.Reichschatz} GS. "
                + $"Für die Spielleitung liegt {paket.Archiv} bereit."));

            return new ZugabgabeErgebnis {
                Erfolgreich = true,
                Meldung = $"Zug {aktuellerZug} ist abgegeben",
                Details = $"Die Zugdaten für Zug {folgeZug} liegen unter {ziel}.",
                NeueDatenbank = ziel,
                ÜbergabeArchiv = paket.Archiv,
                ÜbernommeneFiguren = übernommen,
                AufgelösteFiguren = aufgelöst,
                NeuerReichsschatz = folge.Reichschatz,
            };
        }

        private static AccessDatabase Öffne(string datei, string passwort) {
            var db = new AccessDatabase(datei, passwort);
            if (db.Open() == false) {
                db.Dispose();
                throw new IOException($"Die Datenbank {datei} liess sich nicht öffnen");
            }
            return db;
        }

        /// <summary>
        /// Hält in der Datenbank fest, dass nicht bewegte Figuren stehen geblieben sind.
        ///
        /// Das entspricht MoveUnmovedUnits der Altanwendung. In der Datenbank steht bei einer Figur,
        /// die sich nie bewegt hat, in gf_nach eine 0 - beim Zugübergang würde sie damit auf das
        /// Feld 0/0 wandern.
        /// </summary>
        private static void SetzeStehengebliebeneFiguren(DbCommand befehl) {
            string[] tabellen = [
                Krieger.TableName, Reiter.TableName, Schiffe.TableName,
                Kreaturen.TableName, Character.TableName, Zauberer.TableName
            ];
            foreach (string tabelle in tabellen) {
                befehl.CommandText = $"UPDATE {tabelle} SET gf_nach = gf_von, kf_nach = kf_von WHERE gf_nach <= 0";
                befehl.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Entfernt eine aufgelöste Figur aus dem Folgezug. Die Figurtabellen haben kein Delete,
        /// weil im laufenden Spiel nichts gelöscht wird - beim Zugübergang schon.
        /// </summary>
        private static void LöscheFigur(DbCommand befehl, Spielfigur figur) {
            if (figur is not IDatabaseTable tabelle)
                return;
            befehl.CommandText = $"DELETE FROM {tabelle.TableName} WHERE Nummer = {figur.Nummer}";
            befehl.ExecuteNonQuery();
        }
    }
}
