﻿using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database.Generatoren
{
    internal static class TestDataGenerator
    {
        public static void GeneriereTestdatenFürZug999()
        {
            if(ViewModel.SelectedNation == null)
                throw new Exception("Reich muss ausgewählt sein");
            
            string path = Main.Instance.Settings.UserSettings.DatabaseLocationZugdaten;
            path = StorageSystem.ExtractBasePath(path, "Zugdaten");
            path = Path.Combine(path, "999");
            path = Path.Combine(path, $"{ViewModel.SelectedNation.Name}.mdb");
            if (File.Exists(path) == false)
            {
                SpielWPF.LogError("Der Zug 999 mus angelegt werden", $"Bitte kopiere eine beliebige Zudaten datenbank in {path}");
                return;
            }
            PasswordHolder password = new ((EncryptedString)Main.Instance.Settings.UserSettings.PasswordReich);
            string connectionString = $"Provider = Microsoft.ACE.OLEDB.16.0; Data Source = {path}; Persist Security Info = False;";
            connectionString += $"Jet OLEDB:Database Password={password.DecryptedPassword};";

            if (SharedData.Map == null)
                throw new Exception("Kartendaten fehlen");

            KleinFeld[] eigeneGebiet = SharedData.Map.Values.Where(kf => kf.Nation == ViewModel.SelectedNation).ToArray();
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand($"DELETE FROM Krieger", connection);
                var result = command.ExecuteNonQuery();
                var schiffe = GenerateSchiffe(command, 50);
                var krieger = GenerateKrieger(eigeneGebiet, command, 50, schiffe);
                PutOnSchiffe(schiffe, krieger, 10);
               
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
        private static void PutOnSchiffe(IEnumerable<Schiffe> schiffe, IEnumerable<TruppenSpielfigur> figuren, int count)
        {
            Random random = new Random();
            int anzahlFiguren = figuren.Count();
            int anzahlSchiffe = schiffe.Count();
            for (int i = 0; i < count; i++)
            {
                Schiffe schiff = schiffe.ElementAt(random.Next(0, anzahlSchiffe - 1));
                TruppenSpielfigur truppenSpielfigur = figuren.ElementAt(random.Next(0,anzahlFiguren - 1));
                schiff.auf_Flotte = $"#{truppenSpielfigur.Nummer}";
                truppenSpielfigur.auf_Flotte = $"#{schiff.Nummer}";
            }
        }

        private static int Zufall(Random random, int wahrscheinlichkeit, int minValue, int maxValue)
        {
            return random?.Next(1, 100) < wahrscheinlichkeit ? random.Next(minValue, maxValue) : 0;
        }


        private static void Fill(ref Spielfigur figur, KleinFeld kf)
        {
            figur.ph_xy = kf.ph_xy;
            figur.gf = kf.gf;
            figur.gf_von = kf.gf;
            figur.kf = kf.kf;
            figur.kf_von = kf.kf;
        }

        private static void Fill(ref TruppenSpielfigur figur, KleinFeld kf)
        {
            Spielfigur spielfigur = figur as Spielfigur;
            Fill(ref spielfigur, kf);
            Random random = new();
            int lKP = Zufall(random,20,1,20);
            int sKP = Zufall(random, 10, 1, 5); ;

            figur.staerke = random.Next(500, 6000);
            figur.hf = random.Next(1, 20);
            figur.LKP = lKP;
            figur.lkp_alt = lKP;
            figur.SKP = sKP;
            figur.skp_alt = sKP;
        }

        private static void Calculate(ref TruppenSpielfigur figur, KleinFeld kf)
        {
            figur.bp = SpielfigurenView.BerechneBaupunkte(figur);
            figur.rp = SpielfigurenView.BerechneRaumpunkte(figur);
        }

        private static List<Schiffe> GenerateSchiffe(DbCommand command, int count)
        {
            Random random = new();
            if (SharedData.Map == null)
                throw new Exception("Kartendaten fehlen");
            // schiffe aufs Meer
            KleinFeld[] meer = SharedData.Map.Values.Where(kf => kf.Terrain.IsWasser).ToArray(); 
            List<Schiffe> list = [];
            int nummer = 301;
            for (int i = 0; i < count; ++i)
            {
                KleinFeld kf = meer[random.Next(0, meer.Length - 1)];
                var schiff = new Schiffe()
                {
                    Nummer = nummer++
                };
                TruppenSpielfigur truppenSpielfigur = schiff as TruppenSpielfigur;
                Fill(ref truppenSpielfigur, kf);
                Calculate(ref truppenSpielfigur, kf);
                list.Add(schiff);
            }
            return list;
        }

        private static List<Krieger> GenerateKrieger(KleinFeld[] eigeneGebiet, DbCommand command, int count, IEnumerable<Schiffe> schiffe)
        {
            Random random = new();
            List<Krieger> list = new List<Krieger>();
            for (int i = 0; i < count; ++i)
            {
                KleinFeld kf = eigeneGebiet[random.Next(0, eigeneGebiet.Length - 1)];
                int lKP = random.Next(0, 9) > 6 ? random.Next(0, 20) : 0;
                int sKP = random.Next(0, 9) > 8 ? random.Next(0, 6) : 0;
                // auf Flotte
                Krieger krieger = new Krieger()
                {
                    staerke = random.Next(1000, 60000),
                    hf = random.Next(1, 60),
                    gf = kf.gf,
                    gf_von = kf.gf,
                    kf = kf.kf,
                    kf_von = kf.kf,
                    LKP = lKP,
                    lkp_alt = lKP,
                    SKP = sKP,
                    skp_alt = sKP,
                };
                list.Add(krieger);
            }
            return list;
        }
    }
}
