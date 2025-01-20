using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Extensions;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database.Generatoren {
    internal static class TestDataGenerator {

        /// <summary>
        /// Die Testdaten werden in den Zug 999 geschrieben, der dafür manuell angelegt werden muss
        /// es kann eine beliebige Zugdatei des ausgewählten Reiches ab 2020 verwendet werden und in das Verzeichnis Zugdaten/999 kopiert werden
        /// die darin enthaltenen Daten werden überschrieben
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static void GeneriereTestdatenFürZug999() {
            if (ProgramView.SelectedNation == null)
                throw new Exception("Reich muss ausgewählt sein");

            string path = Main.Instance.Settings.UserSettings.DatabaseLocationZugdaten;
            path = StorageSystem.ExtractBasePath(path, "Zugdaten");
            path = Path.Combine(path, "999");
            path = Path.Combine(path, $"{ProgramView.SelectedNation.Name}.mdb");
            if (File.Exists(path) == false) {
                SpielWPF.LogError("Der Zug 999 mus angelegt werden", $"Bitte kopiere eine beliebige Zudaten datenbank in {path}");
                return;
            }
            PasswordHolder password = new((EncryptedString)Main.Instance.Settings.UserSettings.PasswordReich);
            string connectionString = $"Provider = Microsoft.ACE.OLEDB.16.0; Data Source = {path}; Persist Security Info = False;";
            connectionString += $"Jet OLEDB:Database Password={password.DecryptedPassword};";

            if (SharedData.Map == null)
                throw new Exception("Kartendaten fehlen");

            KleinFeld[] eigeneGebiet = SharedData.Map.Values.Where(kf => kf.Nation == ProgramView.SelectedNation).ToArray();
            using (OleDbConnection connection = new OleDbConnection(connectionString)) {
                connection.Open();
                OleDbCommand command = new OleDbCommand("DELETE FROM Krieger", connection);
                var result = command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM Schiffe";
                result = command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM Reiter";
                result = command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM Kreaturen";
                result = command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM Zauberer";
                result = command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM chars";
                result = command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM Schenkungen";
                result = command.ExecuteNonQuery();
                var schiffe = GenerateSchiffe(20);
                var krieger = GenerateKrieger(eigeneGebiet, 20);
                var reiter = GenerateReiter(eigeneGebiet, 20);
                var zauberer = GenerateZauberer(eigeneGebiet, 20);
                var charakter = GenerateCharacter(eigeneGebiet, 12);
                PutOnSchiffe(schiffe, krieger, 5);
                PutOnSchiffe(schiffe, reiter, 5);
                Save(schiffe, command);
                Save(krieger, command);
                Save(reiter, command);
                Save(charakter, command);
                Save(zauberer, command);
                var schenkungen = GenerateSchenkungen(8);
                Save(schenkungen, command);

                // todo schenkungen
            }
        }

        private static void Save(IEnumerable<IDatabaseTable> figuren, DbCommand command) {
            foreach (var figur in figuren) {
                figur.Insert(command);
            }
        }


        /// <summary>
        /// um Figuren auf Schiffe zu bringen, oder Einschiffen oder Ausschiffen Befehl_bew "#SCEA:[Schiffnummer]
        /// muss der Befehl auch auf den Schiffen hinterlegt werden in der Tabelle Befehl_bew "#SCA:[Truppe]" oder "#SCE:[Truppe]"
        /// zusätzlich muss in dem Feld auf_Flotte bei den Truppen das Schiff und bei den Schiffen die Einheiten hinterlegt werden
        /// </summary>
        /// <param name="schiffe"></param>
        /// <param name="figuren"></param>
        /// <param name="count"></param>
        private static void PutOnSchiffe(IEnumerable<Schiffe> schiffe, IEnumerable<TruppenSpielfigur> figuren, int count) {
            Random random = new Random();
            int anzahlFiguren = figuren.Count();
            int anzahlSchiffe = schiffe.Count();
            for (int i = 0; i < count; i++) {
                Schiffe schiff = schiffe.ElementAt(random.Next(0, anzahlSchiffe - 1));
                TruppenSpielfigur truppenSpielfigur = figuren.ElementAt(random.Next(0, anzahlFiguren - 1));
                schiff.auf_Flotte = $"#{truppenSpielfigur.Nummer}";
                truppenSpielfigur.auf_Flotte = $"#{schiff.Nummer}";
            }
        }

        /// <summary>
        /// gibt eine Zufallszahl zurück, wenn eine gewisse Wahrscheinlichkeit eintrifft, ansonsten 0
        /// </summary>
        /// <param name="random"></param>
        /// <param name="wahrscheinlichkeit"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        private static int Zufall(Random random, int wahrscheinlichkeit, int minValue, int maxValue) {
            return random?.Next(1, 100) < wahrscheinlichkeit ? random.Next(minValue, maxValue) : 0;
        }

        private static void Fill(ref Spielfigur figur, KleinFeld kf) {
            figur.PutOnKleinfeld(kf);  
        }

        /// <summary>
        /// füllt die allgemeinen Werte einer Truppenspielfigur (Schiffe, Reiter, Kreaturen, Krieger)
        /// </summary>
        /// <param name="figur"></param>
        /// <param name="kf"></param>
        private static void Fill(ref TruppenSpielfigur figur, KleinFeld kf) {
            Random random = new();
            int lKP = Zufall(random, 20, 1, 20);
            int sKP = Zufall(random, 10, 1, 5); ;
            if ((figur is Krieger || figur is Reiter) && (lKP > 0 || sKP > 0)) {
                if (ProgramView.SelectedNation != null) {
                    var gebäude = BauwerkeView.GetGebäude(ProgramView.SelectedNation);
                    if (gebäude != null) {
                        var bw = gebäude[random.Next(0, gebäude.Count - 1)];
                        if (SharedData.Map != null)
                            kf = SharedData.Map[bw.CreateBezeichner()];
                    }
                }
            }
            Spielfigur spielfigur = figur as Spielfigur;
            Fill(ref spielfigur, kf);
            figur.staerke = (random.Next(500, 6000) / 1000) * (figur is Schiffe? 200:(figur is Reiter ? 500 : 1000));
            figur.hf = random.Next(1, 20);
            figur.LKP = lKP;
            figur.lkp_alt = lKP;
            figur.SKP = sKP;
            figur.skp_alt = sKP;
            figur.GS = Zufall(random, 10, 6000, 15000);
            figur.Kampfeinnahmen = Zufall(random, 10, 6000, 15000);
        }

        /// <summary>
        /// einfache Namen für Spieler
        /// </summary>
        private static readonly string[] GermanNames =
        {
            // Male Names
            "Alexander Schmidt", "Andreas Müller", "Anton Wagner", "Benedikt Fischer", "Christian Weber",
            "Christoph Meyer", "Daniel Hoffmann", "Elias Schäfer", "Erik Becker", "Fabian Schröder",
            "Felix Schneider", "Florian Neumann", "Franz Braun", "Friedrich Zimmermann", "Georg Krüger",
            "Gustav Hartmann", "Heinrich Lange", "Jakob Schulz", "Jonas Lehmann", "Julian Köhler",
            "Karl Maier", "Klaus Klein", "Leon Wolf", "Ludwig Schmitt", "Matthias Bauer",

            // Female Names
            "Amalia Schröder", "Anneliese Schulz", "Beatrix Hoffmann", "Charlotte Weber", "Claudia Fischer",
            "Dagmar Wagner", "Daniela Meier", "Elfriede Becker", "Emilia Lehmann", "Erika Braun",
            "Esther Klein", "Franziska Zimmermann", "Freya Maier", "Gertrud Wolf", "Gisela Krüger",
            "Greta Müller", "Helga Neumann", "Ingrid Schröder", "Johanna Köhler", "Katharina Lange",
            "Leonie Schmitt", "Lieselotte Schäfer", "Margarete Hartmann", "Marianne Bauer", "Sabine Meyer"
        };

        /// <summary>
        /// Namen für Held*innen
        /// </summary>
        private static readonly string[] HeroNames =
        {
            // Male Names
            "Aldric Eisenherz", "Borin Sturmfels", "Cedric Nachtschatten", "Draven Dunkelwald", "Eldrin Sternenlicht",
            "Faelar Mondschatten", "Garrik Donnerherz", "Haldor Schattenfluch", "Ivar Blutwolf", "Jorik Morgentau",
            "Kaelen Flinkklinge", "Lucian Feuerglut", "Magnus Frosthain", "Nyx Dämmerborn", "Orin Windreiter",
            "Pelor Sonnenspeer", "Quintus Schattenstreif", "Ragnar Wolfszahn", "Sorin Sturmrufer", "Talon Schwarzspeer",
            "Ulric Eisenfaust", "Veyron Nachtstürmer", "Wystan Silberglanz", "Xandor Sternenschmied", "Zephyrus Windwandler",

            // Female Names
            "Aelara Mondveil", "Brynna Sturmlied", "Celestia Morgenstrahl", "Delara Nachtgale", "Elaris Sternenschleier",
            "Faenya Sonnenflüstern", "Gwyndolin Frostblüte", "Hestia Glutfeuer", "Isolde Schattentänzerin", "Jelena Lichtklinge",
            "Kaida Sturmreiter", "Lyara Nebelkind", "Melisara Dämmerblüte", "Nymeria Windschatten", "Orlena Donnerstimme",
            "Phaedra Flammenherz", "Quenara Frostflüstern", "Rhiannon Schattenflamme", "Selene Sternenspeer", "Talia Sonnenweber",
            "Ulyssia Dunkelmond", "Vespera Nachtdorn", "Wynora Silberbach", "Xanthe Feuerschlag", "Zyra Sturmjäger"
        };
        /// <summary>
        /// Namen für Zauberer*innen
        /// </summary>
        private static readonly string[] WizardNames =
        {
            // Male Wizards
            "Aldric Zauberherz", "Borin Runenstein", "Cedric Dämmersturm", "Draven Schattenfels", "Eldrin Sternenweise",
            "Faelar Mondrunen", "Garrik Feuerzunge", "Haldor Schattenschwur", "Ivar Blutfeder", "Jorik Nebelweber",
            "Kaelen Zauberkind", "Lucian Flammenlied", "Magnus Frostfluch", "Nyx Nachtruf", "Orin Windseher",
            "Pelor Sonnenwirker", "Quintus Schattenleser", "Ragnar Wolfsweise", "Sorin Sturmblick", "Talon Schicksalsfluch",
            "Ulric Runenfaust", "Veyron Dämmerglut", "Wystan Silberspruch", "Xandor Sternenseher", "Zephyrus Windzeichen",

            // Female Wizards
            "Aelara Mondweberin", "Brynna Runenzunge", "Celestia Morgenhauch", "Delara Nachtschrift", "Elaris Sternenlicht",
            "Faenya Sonnenwirkerin", "Gwyndolin Frosthauch", "Hestia Glutlied", "Isolde Schattenruferin", "Jelena Zauberklinge",
            "Kaida Sturmkristall", "Lyara Nebelfluch", "Melisara Dämmerrose", "Nymeria Windläuferin", "Orlena Donnerwort",
            "Phaedra Flammenschwur", "Quenara Frosthüterin", "Rhiannon Schattensang", "Selene Sternenmantel", "Talia Sonnenkuss",
            "Ulyssia Dunkelmantel", "Vespera Nachtzauber", "Wynora Silberbann", "Xanthe Feuermythos", "Zyra Sturmglanz"
        };

        /// <summary>
        /// Füllt die Daten von Zauberern und Charaktern
        /// </summary>
        /// <param name="figur"></param>
        /// <param name="kf"></param>
        private static void Fill(ref NamensSpielfigur figur, KleinFeld kf) {
            Random random = new();
            figur.GP_ges = Zufall(random, 10, 32, 70);
            if (figur.GP_ges ==0)
                figur.GP_ges = Zufall(random, 20, 16, 32);
            if (figur.GP_ges == 0)
                figur.GP_ges = Zufall(random, 40, 8, 16);
            if (figur.GP_ges == 0)
                figur.GP_ges = random.Next(4, 8);
            figur.GP_ges_alt = figur.GP_ges;

            if (figur is Zauberer wiz) {
                if (random.Next(47) % 3 == 0) 
                    figur.Beschriftung = $"C{wiz.Klasse.ToString()}";
                else
                    figur.Beschriftung = wiz.Klasse.ToString();
                figur.tp_alt = wiz.MaxTeleportPunkte;
                figur.tp = random.Next(wiz.MaxTeleportPunkte - 4, wiz.MaxTeleportPunkte);
                figur.CharakterName = WizardNames[random.Next(WizardNames.Length)];
                figur.GP_akt = random.Next(figur.GP_ges - 4, figur.GP_ges);
                // TODO 
            }
            else if (figur is Character hero) { 
                figur.CharakterName = HeroNames[random.Next(HeroNames.Length)];
                var kategorie = CharacterView.GetAssumedCharacterKategorie(hero);
                if (kategorie != null) {
                    figur.Beschriftung = kategorie.Abkürzung.Replace("#", random.Next(1000).ToString()); ;
                }
                else 
                    figur.Beschriftung = "???";
                figur.GP_akt = figur.GP_ges;
                figur.CharakterName = HeroNames[random.Next(HeroNames.Length)];
                figur.SpielerName = GermanNames[random.Next(GermanNames.Length)];
            }
            if (figur.Beschriftung.StartsWith("C"))
                figur.SpielerName = GermanNames[random.Next(GermanNames.Length)];
            Spielfigur spielfigur = figur as Spielfigur;
            Fill(ref spielfigur, kf);
        }

        /// <summary>
        /// Berechnet die aktuellen Baupunkte und Bewegungspunkte von Truppen
        /// </summary>
        /// <param name="figur"></param>
        /// <param name="kf"></param>
        private static void Calculate(ref TruppenSpielfigur figur, KleinFeld kf) {
            figur.bp = SpielfigurenView.BerechneBewegungspunkte(figur);
            figur.rp = SpielfigurenView.BerechneRaumpunkte(figur);
        }

        /// <summary>
        /// Berechnet die aktuellen Baupunkte und Bewegungspunkte von Zauberern und Charakteren
        /// </summary>
        /// <param name="figur"></param>
        /// <param name="kf"></param>
        private static void Calculate(ref NamensSpielfigur figur, KleinFeld kf) {
            figur.bp = SpielfigurenView.BerechneBewegungspunkte(figur);
            figur.rp = SpielfigurenView.BerechneRaumpunkte(figur);
        }

        /// <summary>
        /// erzeuge eine Anzahl von Schiffen an die Küste der aktuell ausgewählten Nation
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static List<Schiffe> GenerateSchiffe(int count) {
            Random random = new();
            if (SharedData.Map == null)
                throw new Exception("Kartendaten fehlen");

            KleinFeld[] EigeneKüste = SharedData.Map.Values.Where(kf => kf.Nation == ProgramView.SelectedNation && KleinfeldView.IsKleinfeldAmMeer(kf) == true).ToArray();

            // schiffe aufs Meer
            List<Schiffe> list = [];
            int nummer = Schiffe.StartNummer+1;
            for (int i = 0; i < count; ++i) {
                KleinFeld? kf = EigeneKüste[random.Next(0, EigeneKüste.Length - 1)];
                var nachbarn = KleinfeldView.GetNachbarn(kf, 2);
                if (nachbarn != null) {
                    var wasser = nachbarn.Where(f => f.IsWasser == true).ToList();
                    kf = wasser[random.Next(0, wasser.Count() - 1)];
                    if (kf != null) {
                        var schiff = new Schiffe() {
                            Nummer = nummer++
                        };
                        TruppenSpielfigur truppenSpielfigur = schiff as TruppenSpielfigur;
                        Fill(ref truppenSpielfigur, kf);
                        Calculate(ref truppenSpielfigur, kf);
                        list.Add(schiff);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// erzeuge eine Anzahl von Kriegern in das Staatsgebiet der aktuelll ausgewählten Nation
        /// Katapulte werden auf die Rüstorte verteilt
        /// </summary>
        /// <param name="eigeneGebiet"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static List<Krieger> GenerateKrieger(KleinFeld[] eigeneGebiet, int count) {
            Random random = new();
            List<Krieger> list = [];
            int nummer = Krieger.StartNummer + 1;
            for (int i = 0; i < count; ++i) {
                KleinFeld kf = eigeneGebiet[random.Next(0, eigeneGebiet.Length - 1)];
                // auf Flotte
                Krieger krieger = new Krieger() {
                    Nummer = nummer++
                };
                TruppenSpielfigur truppenSpielfigur = krieger as TruppenSpielfigur;
                Fill(ref truppenSpielfigur, kf);
                Calculate(ref truppenSpielfigur, kf);
                krieger.Pferde = Zufall(random, 20, krieger.staerke / 2, krieger.staerke);
                krieger.Garde = i == 47;
                list.Add(krieger);
            }
            return list;
        }

        /// <summary>
        /// erzeuge eine Anzahl von Reitern in das Staatsgebiet der aktuelll ausgewählten Nation
        /// </summary>
        /// <param name="eigeneGebiet"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static List<Reiter> GenerateReiter(KleinFeld[] eigeneGebiet, int count) {
            Random random = new();
            List<Reiter> list = [];
            int nummer = Reiter.StartNummer + 1;
            for (int i = 0; i < count; ++i) {
                KleinFeld kf = eigeneGebiet[random.Next(0, eigeneGebiet.Length - 1)];
                // auf Flotte
                Reiter reiter = new Reiter() {
                    Nummer = nummer++
                };
                TruppenSpielfigur truppenSpielfigur = reiter as TruppenSpielfigur;
                Fill(ref truppenSpielfigur, kf);
                Calculate(ref truppenSpielfigur, kf);
                list.Add(reiter);
            }
            return list;
        }

      /// <summary>
      /// erzeuge eine Anzahl von Charakteren in das Staatsgebiet der aktuelll ausgewählten Nation
      /// </summary>
      /// <param name="eigeneGebiet"></param>
      /// <param name="count"></param>
      /// <returns></returns>
        private static List<Character> GenerateCharacter(KleinFeld[] eigeneGebiet, int count) {
            Random random = new();
            List<Character> list = [];
            int nummer = Character.StartNummer + 1;
            for (int i = 0; i < count; ++i) {
                KleinFeld kf = eigeneGebiet[random.Next(0, eigeneGebiet.Length - 1)];
                // auf Flotte
                Character wizard = new Character() {
                    Nummer = nummer++
                };
                NamensSpielfigur truppenSpielfigur = wizard as NamensSpielfigur;
                Fill(ref truppenSpielfigur, kf);
                Calculate(ref truppenSpielfigur, kf);
                list.Add(wizard);
            }
            return list;
        }
        
        /// <summary>
        /// erzeuge eine Anzahl von Zauperbern in das Staatsgebiet der aktuelll ausgewählten Nation
        /// </summary>
        /// <param name="eigeneGebiet"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static List<Zauberer> GenerateZauberer(KleinFeld[] eigeneGebiet, int count) {
            Random random = new();
            List<Zauberer> list = [];
            int nummer = Zauberer.StartNummer + 1;
            for (int i = 0; i < count; ++i) {
                KleinFeld kf = eigeneGebiet[random.Next(0, eigeneGebiet.Length - 1)];
                // auf Flotte
                Zauberer wizard = new Zauberer() {
                    Nummer = nummer++
                };
                NamensSpielfigur truppenSpielfigur = wizard as NamensSpielfigur;
                Fill(ref truppenSpielfigur, kf);
                Calculate(ref truppenSpielfigur, kf);
                list.Add(wizard);
            }
            return list;
        }

        /// <summary>
        /// erzeuge Schenkungen für und von dem Reich
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static List<Schenkungen> GenerateSchenkungen(int count) {
            Random random = new();
            List<Schenkungen> schenkungens = [];
            for (int i = 0; i < count; ++i) {
                Schenkungen schenkung = new Schenkungen();
                if (SharedData.Nationen == null || ProgramView.SelectedNation == null)
                    continue;
                schenkung.monat =i*8 + random.Next(7);
                var reich = SharedData.Nationen.ElementAt(random.Next(SharedData.Nationen.Count -1));
                if (random.Next(47) % 2 > 0) {
                    schenkung.Schenkung_an = random.Next(1, 10) * 1000;
                    schenkung.Schenkung_anID = reich.DBname;
                    schenkung.Schenkung_bekommenID = ProgramView.SelectedNation.DBname;
                }
                else {
                    schenkung.Schenkung_bekommen = random.Next(1, 10) * 1000;
                    schenkung.Schenkung_bekommenID = reich.DBname;
                    schenkung.Schenkung_anID = ProgramView.SelectedNation.DBname;
                }
                schenkungens.Add(schenkung);

            }
            return schenkungens;
        }
    }
}
