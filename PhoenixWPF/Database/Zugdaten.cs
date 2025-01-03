﻿using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Program;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database
{

    public class Zugdaten : DatabaseLoader, ILoadableDatabase
    {
        EncryptedString _encryptedpassword;
        string _databaseFileName;
        public EncryptedString Encryptedpassword { get => _encryptedpassword; set => _encryptedpassword = value; }
        public string DatabaseFileName { get => _databaseFileName; set => _databaseFileName = value; }


        public Zugdaten(string databaseFileName, EncryptedString encryptedpassword)
        {
            _databaseFileName = databaseFileName;
            _encryptedpassword = encryptedpassword;
        }

        public void UpdateKarte()
        {
            Task.Run(() => { _UpdateKarte(); });
        }

        public void _UpdateKarte()
        {
            while (SharedData.Map == null || SharedData.Map.IsBlocked == true || SharedData.Map.IsAddingCompleted == false)
            {
                Thread.Sleep(100);
            }
            using (SharedData.BlockGuard guard = new(SharedData.Map))
            {
                foreach (var gem in SharedData.Map)
                {

                }
            }
        }

        public void Load()
        {
            PasswordHolder holder = new(_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return;
                try
                {
              
                }
                catch (Exception ex)
                {
                    SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Datenbank {_databaseFileName}" , ex.Message));
                }
                connector?.Close();
            }
        }

        public void Dispose()
        {
        }

        protected override void LoadInBackground()
        {
            PasswordHolder holder = new(_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return;
                try
                {
                    Load<BilanzEinnahmen>(connector, ref SharedData.BilanzEinnahmen_Zugdaten, Enum.GetNames(typeof(BilanzEinnahmen.Felder)));
                    Load<Character>(connector, ref SharedData.Character, Enum.GetNames(typeof(Character.Felder)));
                    Load<Diplomatiechange>(connector, ref SharedData.Diplomatiechange, Enum.GetNames(typeof(Diplomatiechange.Felder)));
                    Load<Kreaturen>(connector, ref SharedData.Kreaturen, Enum.GetNames(typeof(Kreaturen.Felder)));
                    Load<Krieger>(connector, ref SharedData.Krieger, Enum.GetNames(typeof(Krieger.Felder)));
                    Load<Lehensvergabe>(connector, ref SharedData.Lehensvergabe, Enum.GetNames(typeof(Lehensvergabe.Felder)));
                    // unused Load<Personal>(connector, ref SharedData.Personal, Enum.GetNames(typeof(Personal.Felder)));
                    Load<Reiter>(connector, ref SharedData.Reiter, Enum.GetNames(typeof(Reiter.Felder)));
                    Load<RuestungBauwerke>(connector, ref SharedData.RuestungBauwerke, Enum.GetNames(typeof(RuestungBauwerke.Felder)));
                    Load<RuestungRuestorte>(connector, ref SharedData.RuestungRuestorte, Enum.GetNames(typeof(RuestungRuestorte.Felder)));
                    Load<Schatzkammer>(connector, ref SharedData.Schatzkammer, Enum.GetNames(typeof(Schatzkammer.Felder)));
                    Load<Schenkungen>(connector, ref SharedData.Schenkungen, Enum.GetNames(typeof(Schenkungen.Felder)));
                    Load<Schiffe>(connector, ref SharedData.Schiffe, Enum.GetNames(typeof(Schiffe.Felder)));
                    Load<Units>(connector, ref SharedData.Units_Zugdaten, Enum.GetNames(typeof(Units.Felder)));
                    Load<Zauberer>(connector, ref SharedData.Zauberer, Enum.GetNames(typeof(Zauberer.Felder)));

                }
                catch (Exception ex)
                {
                    SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: " , ex.Message);
                }
                connector?.Close();
            }
        }

        public static List<BilanzEinnahmen> LoadBilanzEinnahmenHistory()
        {
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
            foreach(string alteZugdaten in zugDatenListe)
            { 
                string aktuellesDatenbank= System.IO.Path.Combine(zugdatenPath, alteZugdaten);
                aktuellesDatenbank = System.IO.Path.Combine(aktuellesDatenbank, databaseFileName);
                using (AccessDatabase connector = new(databaseLocation, holder.DecryptPassword()))
                {
                    if (connector?.Open() == false)
                        return result;
                    try
                    {
                        BlockingCollection<BilanzEinnahmen>? bilanzEinnahmen = [];
                        zugDaten.Load<BilanzEinnahmen>(connector, ref bilanzEinnahmen, Enum.GetNames(typeof(BilanzEinnahmen.Felder)));
                        if (bilanzEinnahmen != null && bilanzEinnahmen.Count > 0)
                        {
                            var einnahmen = bilanzEinnahmen.ElementAt(0);
                            einnahmen.monat = int.Parse(alteZugdaten);
                            if (tl != einnahmen.Tiefland)
                                tl = einnahmen.Tiefland ?? 100;
                            result.Add(einnahmen);
                        }
                    }
                    catch (Exception ex)
                    {
                        SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: ", ex.Message);
                    }
                    connector?.Close();
                }
            }
            return result;
        }

        public class TruppenStatistik
        {
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


        public static List<TruppenStatistik> LoadTruppenStatistik()
        {
            if (Main.Instance.Settings == null || SharedData.Nationen == null || Main.Instance.Settings.UserSettings.SelectedReich < 0)
                return [];

            string databaseLocation = Main.Instance.Settings.UserSettings.DatabaseLocationZugdaten;
            string databaseFileName = System.IO.Path.GetFileName(databaseLocation);
            string zugdatenPath = Helper.StorageSystem.ExtractBasePath(databaseLocation, "Zugdaten");
            var zugDatenListe = Helper.StorageSystem.GetNumericDirectories(zugdatenPath);

            var zugDaten = new Zugdaten(databaseLocation, (Main.Instance.Settings.UserSettings.PasswordReich));
            PasswordHolder holder = new(new EncryptedString(Main.Instance.Settings.UserSettings.PasswordReich));
            List<TruppenStatistik> result = [];
            foreach (string alteZugdaten in zugDatenListe)
            {
                string aktuelleDatenbank = System.IO.Path.Combine(zugdatenPath, alteZugdaten);
                aktuelleDatenbank = System.IO.Path.Combine(aktuelleDatenbank, databaseFileName);
                try
                {
                    using (AccessDatabase connector = new(aktuelleDatenbank, holder.DecryptPassword()))
                    {
                        if (connector?.Open() == false)
                            return result;
                        try
                        {
                            int krieger = zugDaten.GetSum(connector, "Krieger", "staerke");
                            int kriegerHF = zugDaten.GetSum(connector, "Krieger", "hf");
                            int reiter = zugDaten.GetSum(connector, "Reiter", "staerke");
                            int reiterHF = zugDaten.GetSum(connector, "Reiter", "hf");
                            int Schiffe = zugDaten.GetSum(connector, "Schiffe", "staerke", "LKP=0 AND SKP=0");
                            int LKS = zugDaten.GetSum(connector, "Schiffe", "LKP");
                            int SKS = zugDaten.GetSum(connector, "Schiffe", "SKP");
                            int LKP = zugDaten.GetSum(connector, "Krieger", "LKP");
                            LKP += zugDaten.GetSum(connector, "Reiter", "LKP");
                            int SKP = zugDaten.GetSum(connector, "Krieger", "SKP");
                            SKP += zugDaten.GetSum(connector, "Reiter", "SKP");

                            result.Add(new TruppenStatistik
                            {
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
                        catch (Exception ex)
                        {
                            SpielWPF.LogError("Fehler beim Lesen der Zugdaten Datenbank: " , ex.Message);
                        }
                        connector?.Close();
                    }
                }
                catch (Exception ex)
                {
                    SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: ", ex.Message);
                }
            }
            return result;
        }
    }
}
