﻿using PhoenixModel.Database;
using PhoenixModel.ViewModel;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Program;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using static PhoenixModel.Database.PasswordHolder;
using PhoenixModel.View;
using System.IO;
using Ionic.Zip;
using static PhoenixWPF.Database.PasswortProvider;
using System.Text;
using PhoenixModel.Program;
using PhoenixModel.Helper;

namespace PhoenixWPF.Database {

    public class Zugdaten : DatabaseLoader, ILoadableDatabase {
        EncryptedString _encryptedpassword;
        string _databaseFileName;
        public EncryptedString Encryptedpassword { get => _encryptedpassword; set => _encryptedpassword = value; }
        public string DatabaseFileName { get => _databaseFileName; set => _databaseFileName = value; }


        public Zugdaten(string databaseFileName, EncryptedString encryptedpassword) {
            _databaseFileName = databaseFileName;
            _encryptedpassword = encryptedpassword;
        }

        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e) {
            if (e.EventType == ViewEventArgs.ViewEventType.EverythingLoaded && SharedData.Diplomatie != null && SharedData.Diplomatiechange != null
                && ProgramView.SelectedNation != null) {
                Task.Run(() => RepairDiplomatieChange());
                ProgramView.OnViewEvent -= ViewModel_OnViewEvent;
            }
        }

        private void RepairDiplomatieChange() {
            if (SharedData.Diplomatie != null && SharedData.Diplomatiechange != null) {
                if (SharedData.Diplomatiechange.IsAddingCompleted == false)
                    SpielWPF.LogError("Daten noch nicht vollständig geladen", "Die Tabelle Diplomatiechange aus den Zugdaten fehlt");
                if (SharedData.Diplomatie.IsAddingCompleted == false)
                    SpielWPF.LogError("Daten noch nicht vollständig geladen", "Die Tabelle Reich_crossref aus der Erkenfarakarte fehlt");
                var keys = SharedData.Diplomatie.Keys.OrderBy(str => str).ToList();

                foreach (var diplo in SharedData.Diplomatiechange) {
                    if (diplo.ReferenzNation != ProgramView.SelectedNation) {
                        string key = diplo.Bezeichner;
                        if (keys.Contains(key) == false)
                            SpielWPF.LogWarning($"Die Kombination {diplo.Bezeichner} wurde nicht in der Reich_crossref gefunden", "Die Datenbanken der Erkenfarakarte und der Zugdaten sind auseinander gelaufen. Bitte die Spielleitung informieren");
                        if (SharedData.Diplomatie.ContainsKey(key)) {
                            diplo.CopyValues(SharedData.Diplomatie[key]);
                        }
                        else
                            SpielWPF.LogWarning($"Die Kombination {diplo.Bezeichner} wurde nicht in der Reich_crossref gefunden", "Die Datenbanken der Erkenfarakarte und der Zugdaten sind auseinander gelaufen. Bitte die Spielleitung informieren");
                    }
                }
            }
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                ProgramView.Update(ViewEventArgs.ViewEventType.UpdateDiplomatie);
            }));

        }

        public void Save(IDatabaseTable table) {
            Save(table, _encryptedpassword, _databaseFileName);
        }

        public void Insert(IDatabaseTable table) {
            Insert(table, _encryptedpassword, _databaseFileName);
        }

        public void Delete(IDatabaseTable table) {
            Delete(table, _encryptedpassword, _databaseFileName);
        }

        public void Load() {
            PasswordHolder holder = new(_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptedPassword)) {
                if (connector?.Open() == false)
                    return;
                try {

                }
                catch (Exception ex) {
                    SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Datenbank {_databaseFileName}", ex.Message));
                }
                connector?.Close();
            }
        }

        public void Dispose() {
        }

        public void LoadBackgroundSynchronous() {
            LoadInBackground();
        }

        protected override void LoadInBackground() {
            PasswordHolder holder = new(_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptedPassword)) {
                if (connector?.Open() == false)
                    return;
                try {
                    Load<ZugdatenSettings>(connector, ref SharedData.ZugdatenSettings, Enum.GetNames(typeof(ZugdatenSettings.Felder)));
                    Load<BilanzEinnahmen>(connector, ref SharedData.BilanzEinnahmen_Zugdaten, Enum.GetNames(typeof(BilanzEinnahmen.Felder)));
                    Load<Character>(connector, ref SharedData.Character, Enum.GetNames(typeof(Character.Felder)));
                    Load<Diplomatiechange>(connector, ref SharedData.Diplomatiechange, Enum.GetNames(typeof(Diplomatiechange.Felder)));
                    Load<Kreaturen>(connector, ref SharedData.Kreaturen, Enum.GetNames(typeof(Kreaturen.Felder)));
                    Load<Krieger>(connector, ref SharedData.Krieger, Enum.GetNames(typeof(Krieger.Felder)));
                    Load<Lehensvergabe>(connector, ref SharedData.Lehensvergabe, Enum.GetNames(typeof(Lehensvergabe.Felder)));
                    // unused Load<Personal>(connector, ref SharedData.Personal, Enum.GetNames(typeof(Personal.Felder)));
                    Load<Reiter>(connector, ref SharedData.Reiter, Enum.GetNames(typeof(Reiter.Felder)));
                    Load<Ruestung>(connector, ref SharedData.Ruestung, Enum.GetNames(typeof(Ruestung.Felder)));
                    Load<RuestungBauwerke>(connector, ref SharedData.RuestungBauwerke, Enum.GetNames(typeof(RuestungBauwerke.Felder)));
                    Load<RuestungRuestorte>(connector, ref SharedData.RuestungRuestorte, Enum.GetNames(typeof(RuestungRuestorte.Felder)));
                    Load<Schatzkammer>(connector, ref SharedData.Schatzkammer, Enum.GetNames(typeof(Schatzkammer.Felder)));
                    Load<Schenkungen>(connector, ref SharedData.Schenkungen, Enum.GetNames(typeof(Schenkungen.Felder)));
                    Load<Schiffe>(connector, ref SharedData.Schiffe, Enum.GetNames(typeof(Schiffe.Felder)));
                    Load<Units>(connector, ref SharedData.Units_Zugdaten, Enum.GetNames(typeof(Units.Felder)));
                    Load<Zauberer>(connector, ref SharedData.Zauberer, Enum.GetNames(typeof(Zauberer.Felder)));
                    ProgramView.OnViewEvent += ViewModel_OnViewEvent;
                }
                catch (Exception ex) {
                    SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: ", ex.Message);
                }
                connector?.Close();
            }
        }

        public static List<BilanzEinnahmen> LoadBilanzEinnahmenHistory() {
            if (Main.Instance.Settings == null || SharedData.Nationen == null || Main.Instance.Settings.UserSettings.SelectedReich < 0)
                return [];

            string databaseLocation = Main.Instance.Settings.UserSettings.DatabaseLocationZugdaten;
            string databaseFileName = System.IO.Path.GetFileName(databaseLocation);
            string zugdatenPath = Helper.StorageSystem.ExtractBasePath(databaseLocation, "Zugdaten");
            var zugDatenListe = Helper.StorageSystem.GetNumericDirectories(zugdatenPath);

            var zugDaten = new Zugdaten(databaseLocation, (Main.Instance.Settings.UserSettings.PasswordReich));
            PasswordHolder holder = new(new EncryptedString(Main.Instance.Settings.UserSettings.PasswordReich));
            List<BilanzEinnahmen> result = [];
            int tl = 100;
            foreach (string alteZugdaten in zugDatenListe) {
                string aktuellesDatenbank = System.IO.Path.Combine(zugdatenPath, alteZugdaten);
                aktuellesDatenbank = System.IO.Path.Combine(aktuellesDatenbank, databaseFileName);
                using (AccessDatabase connector = new(aktuellesDatenbank, holder.DecryptedPassword)) {
                    if (connector?.Open() == false)
                        return result;
                    try {
                        BlockingCollection<BilanzEinnahmen>? bilanzEinnahmen = [];
                        zugDaten.Load<BilanzEinnahmen>(connector, ref bilanzEinnahmen, Enum.GetNames(typeof(BilanzEinnahmen.Felder)));
                        if (bilanzEinnahmen != null && bilanzEinnahmen.Count > 0) {
                            var einnahmen = bilanzEinnahmen.ElementAt(0);
                            einnahmen.monat = int.Parse(alteZugdaten);
                            if (tl != einnahmen.Tiefland)
                                tl = einnahmen.Tiefland ?? 100;
                            result.Add(einnahmen);
                        }
                    }
                    catch (Exception ex) {
                        SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: ", ex.Message);
                    }
                    connector?.Close();
                }
            }
            return result;
        }


        private static void PrepareHistory() {
            if (Main.Instance.Settings == null || SharedData.Nationen == null || Main.Instance.Settings.UserSettings.SelectedReich < 0)
                return;

            string databaseLocation = Main.Instance.Settings.UserSettings.DatabaseLocationZugdaten;
            string databaseFileName = System.IO.Path.GetFileName(databaseLocation);
            string zugdatenPath = Helper.StorageSystem.ExtractBasePath(databaseLocation, "Zugdaten");
            var zugDatenListe = Helper.StorageSystem.GetNumericDirectories(zugdatenPath);

            var zugDaten = new Zugdaten(databaseLocation, (Main.Instance.Settings.UserSettings.PasswordReich));
            PasswordHolder holder = new(new EncryptedString(Main.Instance.Settings.UserSettings.PasswordReich));
            List<RuestungBauwerke> result = [];
            foreach (string alteZugdaten in zugDatenListe) {

                string rüstungDatenbank = $"Rüstung_{databaseFileName}";
                string aktuellerPfad = Path.Combine(zugdatenPath, alteZugdaten);
                string aktuelleDatenbank = Path.Combine(aktuellerPfad, rüstungDatenbank);
                // es gibt hier keine Rüstung Datenbank
                if (File.Exists(aktuelleDatenbank) == false) {
                    // gibt es ein Zip für Rüstung?
                    string? firstMatchingFile = Directory.EnumerateFiles(aktuellerPfad, "Ruestung_*.zip").FirstOrDefault();
                    if (firstMatchingFile == null) {
                        // nein, dann wurde hier nicht gerüstet
                        continue;
                    }
                    // es gibt ein Zip - also auspacken
                    using (ZipFile zip = ZipFile.Read(firstMatchingFile)) {
                        // suche nach dem File
                        ZipEntry? entry = zip.Entries.FirstOrDefault(e => e.FileName.EndsWith(databaseFileName));
                        if (entry != null) {
                            entry.Password = Encoding.UTF8.GetString(Convert.FromBase64String($"MTIzc2llYmVu{PasswortProvider.End}lcmdlIQ==")); // steht unverschlüsselt im alten Source-Code und der exe 
                            entry.Extract(aktuellerPfad, ExtractExistingFileAction.OverwriteSilently);
                            string extracted = Path.Combine(aktuellerPfad, entry.FileName);
                            if (File.Exists(extracted) == false) {
                                SpielWPF.LogError($"Fehler beim Auspacken von {aktuelleDatenbank}", entry.Info);
                            }
                            File.Move(extracted, aktuelleDatenbank, true);
                            Directory.Delete(Path.Combine(aktuellerPfad, "PZE.NET"), true);
                            if (File.Exists(aktuelleDatenbank) == false) {
                                SpielWPF.LogError($"Fehler beim Umbennen von {aktuelleDatenbank}", entry.Info);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        public static List<IEigenschaftler> LoadBaukostenHistory() {
            if (Main.Instance.Settings == null || SharedData.Nationen == null || Main.Instance.Settings.UserSettings.SelectedReich < 0)
                return [];
            PrepareHistory();
            string databaseLocation = Main.Instance.Settings.UserSettings.DatabaseLocationZugdaten;
            string databaseFileName = System.IO.Path.GetFileName(databaseLocation);
            string zugdatenPath = Helper.StorageSystem.ExtractBasePath(databaseLocation, "Zugdaten");
            var zugDatenListe = Helper.StorageSystem.GetNumericDirectories(zugdatenPath);

            var zugDaten = new Zugdaten(databaseLocation, (Main.Instance.Settings.UserSettings.PasswordReich));
            PasswordHolder holder = new(new EncryptedString(Main.Instance.Settings.UserSettings.PasswordReich));
            List<IEigenschaftler> result = [];
            foreach (string alteZugdaten in zugDatenListe) {

                string rüstungDatenbank = $"Rüstung_{databaseFileName}";
                string aktuellerPfad = Path.Combine(zugdatenPath, alteZugdaten);
                string aktuelleDatenbank = Path.Combine(aktuellerPfad, rüstungDatenbank);
                // es gibt hier keine Rüstung Datenbank
                if (File.Exists(aktuelleDatenbank) == false) {
                    continue;
                }
                using (AccessDatabase connector = new(aktuelleDatenbank, holder.DecryptedPassword)) {
                    if (connector?.Open() == false)
                        return result;
                    try {
                        BlockingCollection<RuestungBauwerke>? collection = [];
                        zugDaten.Load<RuestungBauwerke>(connector, ref collection, Enum.GetNames(typeof(RuestungBauwerke.Felder)));
                        if (collection != null && collection.Count > 0) {
                            // var einnahmen = collection.Where(k => k.Art.StartsWith("Bruecke"));
                            foreach (var item in collection) {
                                item.ZugMonat = Convert.ToInt32(alteZugdaten);
                            }
                            result.AddRange(collection);
                        }
                    }
                    catch (Exception ex) {
                        SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: ", ex.Message);
                    }
                    connector?.Close();
                }
                using (AccessDatabase connector = new(aktuelleDatenbank, holder.DecryptedPassword)) {
                    if (connector?.Open() == false)
                        return result;
                    try {
                        BlockingCollection<RuestungRuestorte>? collection = [];
                        zugDaten.Load<RuestungRuestorte>(connector, ref collection, Enum.GetNames(typeof(RuestungRuestorte.Felder)));
                        if (collection != null && collection.Count > 0) {
                            // var einnahmen = collection.Where(k => k.Art.StartsWith("Bruecke"));
                            foreach (var item in collection) {
                                item.ZugMonat = Convert.ToInt32(alteZugdaten);
                            }
                            result.AddRange(collection);
                        }
                    }
                    catch (Exception ex) {
                        SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: ", ex.Message);
                    }
                    connector?.Close();
                }
            }
            return result;
        }


        public static List<IEigenschaftler> LoadMobilisierungHistory() {
            if (Main.Instance.Settings == null || SharedData.Nationen == null || Main.Instance.Settings.UserSettings.SelectedReich < 0)
                return [];
            PrepareHistory();
            string databaseLocation = Main.Instance.Settings.UserSettings.DatabaseLocationZugdaten;
            string databaseFileName = System.IO.Path.GetFileName(databaseLocation);
            string zugdatenPath = Helper.StorageSystem.ExtractBasePath(databaseLocation, "Zugdaten");
            var zugDatenListe = Helper.StorageSystem.GetNumericDirectories(zugdatenPath);

            var zugDaten = new Zugdaten(databaseLocation, (Main.Instance.Settings.UserSettings.PasswordReich));
            PasswordHolder holder = new(new EncryptedString(Main.Instance.Settings.UserSettings.PasswordReich));
            List<IEigenschaftler> result = [];
            foreach (string alteZugdaten in zugDatenListe) {

                string rüstungDatenbank = $"Rüstung_{databaseFileName}";
                string aktuellerPfad = Path.Combine(zugdatenPath, alteZugdaten);
                string aktuelleDatenbank = Path.Combine(aktuellerPfad, rüstungDatenbank);
                // es gibt hier keine Rüstung Datenbank
                if (File.Exists(aktuelleDatenbank) == false) {
                    continue;
                }
                using (AccessDatabase connector = new(aktuelleDatenbank, holder.DecryptedPassword)) {
                    if (connector?.Open() == false)
                        return result;
                    try {
                        BlockingCollection<Ruestung>? collection = [];
                        zugDaten.Load<Ruestung>(connector, ref collection, Enum.GetNames(typeof(Ruestung.Felder)));
                        if (collection != null && collection.Count > 0) {
                            // var einnahmen = collection.Where(k => k.Art.StartsWith("Bruecke"));
                            foreach (var item in collection) {
                                item.ZugMonat = Convert.ToInt32(alteZugdaten);
                            }
                            result.AddRange(collection);
                        }
                    }
                    catch (Exception ex) {
                        SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: ", ex.Message);
                    }
                    connector?.Close();
                }
            }
            return result;
        }

        public class TruppenStatistik {
            public int Krieger = 0;
            public int KriegerHF = 0;
            public int Reiter = 0;
            public int ReiterHF = 0;
            public int Schiffe = 0;
            public int LKS = 0;
            public int SKS = 0;
            public int LKP = 0;
            public int SKP = 0;


            public enum Felder { Krieger, KriegerHF, Reiter, ReiterHF, Schiffe, LKS, SKS, LKP, SKP }
            public string Monat = string.Empty;
        }


        public static List<TruppenStatistik> LoadTruppenStatistik() {
            if (Main.Instance.Settings == null || SharedData.Nationen == null || Main.Instance.Settings.UserSettings.SelectedReich < 0)
                return [];

            string databaseLocation = Main.Instance.Settings.UserSettings.DatabaseLocationZugdaten;
            string databaseFileName = System.IO.Path.GetFileName(databaseLocation);
            string zugdatenPath = Helper.StorageSystem.ExtractBasePath(databaseLocation, "Zugdaten");
            var zugDatenListe = Helper.StorageSystem.GetNumericDirectories(zugdatenPath);

            var zugDaten = new Zugdaten(databaseLocation, (Main.Instance.Settings.UserSettings.PasswordReich));
            PasswordHolder holder = new(new EncryptedString(Main.Instance.Settings.UserSettings.PasswordReich));
            List<TruppenStatistik> result = [];
            foreach (string alteZugdaten in zugDatenListe) {
                string aktuelleDatenbank = System.IO.Path.Combine(zugdatenPath, alteZugdaten);
                aktuelleDatenbank = System.IO.Path.Combine(aktuelleDatenbank, databaseFileName);
                try {
                    using (AccessDatabase connector = new(aktuelleDatenbank, holder.DecryptedPassword)) {
                        if (connector?.Open() == false)
                            return result;
                        try {
                            int krieger = zugDaten.GetSum(connector, "Krieger", "staerke");
                            int kriegerHF = zugDaten.GetSum(connector, "Krieger", "hf");
                            int reiter = zugDaten.GetSum(connector, "Reiter", "staerke");
                            int reiterHF = zugDaten.GetSum(connector, "Reiter", "hf");
                            int Schiffe = zugDaten.GetSum(connector, "Schiffe", "staerke");
                            int LKS = zugDaten.GetSum(connector, "Schiffe", "LKP");
                            int SKS = zugDaten.GetSum(connector, "Schiffe", "SKP");
                            int LKP = zugDaten.GetSum(connector, "Krieger", "LKP");
                            LKP += zugDaten.GetSum(connector, "Reiter", "LKP");
                            int SKP = zugDaten.GetSum(connector, "Krieger", "SKP");
                            SKP += zugDaten.GetSum(connector, "Reiter", "SKP");

                            result.Add(new TruppenStatistik {
                                Krieger = krieger,
                                KriegerHF = kriegerHF,
                                Reiter = reiter,
                                ReiterHF = reiterHF,
                                Monat = alteZugdaten,
                                Schiffe = Schiffe,
                                LKS = LKS,
                                SKS = SKS,
                                LKP = LKP,
                                SKP = SKP,
                            });
                        }
                        catch (Exception ex) {
                            SpielWPF.LogError("Fehler beim Lesen der Zugdaten Datenbank: ", ex.Message);
                        }
                        connector?.Close();
                    }
                }
                catch (Exception ex) {
                    SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: ", ex.Message);
                }
            }
            return result;
        }
    }
}
