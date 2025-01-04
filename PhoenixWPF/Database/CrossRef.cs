using PhoenixModel.dbCrossRef;
using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Program;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static PhoenixModel.Database.PasswordHolder;
using static PhoenixWPF.Database.CrossRef;
using System.ComponentModel;
using System.Windows;

namespace PhoenixWPF.Database
{
    public class CrossRef : DatabaseLoader, ILoadableDatabase
    {
        EncryptedString _encryptedpassword;
        string _databaseFileName;

        public EncryptedString Encryptedpassword { get => _encryptedpassword; set => _encryptedpassword = value; }
        public string DatabaseFileName { get => _databaseFileName; set => _databaseFileName = value; }

        public CrossRef(string databaseFileName, EncryptedString encryptedpassword)
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
                    Load<Rüstort>(connector, ref SharedData.RüstortReferenz, Enum.GetNames(typeof(Rüstort.Felder)));
                }
                catch (Exception ex)
                {
                    SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, "Fehler beim Öffnen der PZE Datenbank: " , ex.Message));
                }
                connector?.Close();
            }
        }

        public void Save(IDatabaseTable table)
        {
            PasswordHolder holder = new(_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return;
                try
                {
                    if (connector != null)
                    {
                        var command = connector.OpenDBCommand();
                        table.Save(command);
                    }
                }
                catch (Exception ex)
                {
                    SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Datenbank {_databaseFileName}", ex.Message));
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
                    Load<BEW_chars>(connector, ref SharedData.BEW_chars, Enum.GetNames(typeof(BEW_chars.Felder)));
                    Load<BEW_Kreaturen>(connector, ref SharedData.BEW_Kreaturen, Enum.GetNames(typeof(BEW_Kreaturen.Felder)));
                    Load<BEW_Krieger>(connector, ref SharedData.BEW_Krieger, Enum.GetNames(typeof(BEW_Krieger.Felder)));
                    Load<BEW_LKP>(connector, ref SharedData.BEW_LKP, Enum.GetNames(typeof(BEW_LKP.Felder)));
                    Load<BEW_LKS>(connector, ref SharedData.BEW_LKS, Enum.GetNames(typeof(BEW_LKS.Felder)));
                    Load<BEW_PiratenChars>(connector, ref SharedData.BEW_PiratenChars, Enum.GetNames(typeof(BEW_PiratenChars.Felder)));
                    Load<BEW_PiratenLKS>(connector, ref SharedData.BEW_PiratenLKS, Enum.GetNames(typeof(BEW_PiratenLKS.Felder)));
                    Load<BEW_PiratenSchiffe>(connector, ref SharedData.BEW_PiratenSchiffe, Enum.GetNames(typeof(BEW_PiratenSchiffe.Felder)));
                    Load<BEW_PiratenSKS>(connector, ref SharedData.BEW_PiratenSKS, Enum.GetNames(typeof(BEW_PiratenSKS.Felder)));
                    Load<BEW_Reiter>(connector, ref SharedData.BEW_Reiter, Enum.GetNames(typeof(BEW_Reiter.Felder)));
                    Load<BEW_SKP>(connector, ref SharedData.BEW_SKP, Enum.GetNames(typeof(BEW_SKP.Felder)));
                    Load<BEW_SKS>(connector, ref SharedData.BEW_SKS, Enum.GetNames(typeof(BEW_SKS.Felder)));
                    Load<Kosten>(connector, ref SharedData.Kosten, Enum.GetNames(typeof(Kosten.Felder)));
                    Load<Teleportpunkte>(connector, ref SharedData.Teleportpunkte, Enum.GetNames(typeof(Teleportpunkte.Felder)));
                    Load<Units_crossref>(connector, ref SharedData.Units_crossref, Enum.GetNames(typeof(Units_crossref.Felder)));
                    Load<Wall_crossref>(connector, ref SharedData.Wall_crossref, Enum.GetNames(typeof(Wall_crossref.Felder)));
                    Load<Crossref_zauberer_teleport>(connector, ref SharedData.Crossref_zauberer_teleport, Enum.GetNames(typeof(Crossref_zauberer_teleport.Felder)));
                }
                catch (Exception ex)
                {
                    SpielWPF.LogError("Fehler beim Laden der Crossref Datenbank ", ex.Message);
                }
                connector?.Close();
            }           
        }

 
    }
}
