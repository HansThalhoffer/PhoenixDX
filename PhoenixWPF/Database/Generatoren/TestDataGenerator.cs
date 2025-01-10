using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database.Generatoren
{
    internal class TestDataGenerator
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
                var schiffe = GenerateSchiffe(eigeneGebiet, command, 50);
                GenerateKrieger(eigeneGebiet, command, 50, schiffe);
            }
            
        }

        private static IEnumerable<Schiffe> GenerateSchiffe(KleinFeld[] eigeneGebiet, DbCommand command, int count)
        {
            Random random = new();
            List<Schiffe> list = [];
            for (int i = 0; i < count; ++i)
            {
                KleinFeld kf = eigeneGebiet[random.Next(0, eigeneGebiet.Length - 1)];
                int lKP = random.Next(0, 9) > 6 ? random.Next(0, 20) : 0;
                int sKP = random.Next(0, 9) > 8 ? random.Next(0, 6) : 0;
                var schiff = new Schiffe()
                {
                    staerke = random.Next(500, 6000),
                    hf = random.Next(1, 20),
                    gf = kf.gf,
                    gf_von = kf.gf,
                    kf = kf.kf,
                    kf_von = kf.kf,
                    LKP = lKP,
                    lkp_alt = lKP,
                    SKP = sKP,
                    skp_alt = sKP,
                };
            }
            return list;
        }

        private static void GenerateKrieger(KleinFeld[] eigeneGebiet, DbCommand command, int count, IEnumerable<Schiffe> schiffe)
        {
            Random random = new();
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
            }
        }
    }
}
