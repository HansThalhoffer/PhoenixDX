using PhoenixModel.CrossRef;
using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixWPF.Dialogs;
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

namespace PhoenixWPF.Database
{
    public class CrossRef : ILoadableDatabase
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

        public class PasswortProvider : PasswordHolder.IPasswordProvider
        {
            public EncryptedString Password
            {
                get
                {
                    PasswordDialog dialog = new PasswordDialog("Das Passwort für die CrossRef.mdb Datenbank bitte eingeben");
                    dialog.ShowDialog();
                    return dialog.ProvidePassword();
                }
            }
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

        enum RüstOrtFelder
        { nummer, ruestort, Baupunkte, Kapazitaet_truppen, Kapazitaet_HF, Kapazitaet_Z, canSieged };

        public delegate T LoadObject<T>(DbDataReader reader);

        void Load<T>(LoadObject<T> objReader, AccessDatabase? connector, BlockingCollection<T>? collection, string[] felder )
        {
            if (connector == null || collection == null)
                return;
            int total = 0;
            SharedData.Nationen = new BlockingCollection<Nation>();
            string felderListe = string.Join(", ", felder);
            using (DbDataReader? reader = connector?.OpenReader($"SELECT {felderListe} FROM ORDER BY {felder[0]}"))
            {
                while (reader != null && reader.Read())
                {
                    T obj = objReader(reader);
                    collection.Add(obj);
                }
            }
            collection.CompleteAdding();
            total = collection.Count();
            Spiel.Log(new PhoenixModel.Program.LogEntry($"{total} {typeof(T)} geladen"));
        }

        public void Load()
        {
            PasswordHolder holder = new PasswordHolder(_encryptedpassword, new PasswortProvider());
            using (AccessDatabase connector = new AccessDatabase(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return;
                try
                {
                    Load<Rüstort>(this.LoadRuestort, connector, SharedData.Rüstorte, Enum.GetNames(typeof(RüstOrtFelder)));
                }
                catch (Exception ex)
                {
                    Spiel.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, ("Fehler beim Öffnen der PZE Datenbank: " + ex.Message)));
                }
                connector?.Close();
            }
        }

        Rüstort LoadRuestort(DbDataReader reader)
        {
            var obj = new Rüstort(AccessDatabase.ToInt(reader[(int)RüstOrtFelder.nummer]),
                AccessDatabase.ToInt(reader[(int)RüstOrtFelder.Baupunkte]),
                AccessDatabase.ToString(reader[(int)RüstOrtFelder.ruestort]))
            {
                KapazitätTruppen = AccessDatabase.ToInt(reader[(int)RüstOrtFelder.Kapazitaet_truppen]),
                KapazitätHF = AccessDatabase.ToInt(reader[(int)RüstOrtFelder.Kapazitaet_truppen]),
                KapazitätZ = AccessDatabase.ToInt(reader[(int)RüstOrtFelder.Kapazitaet_truppen]),
                canSieged = AccessDatabase.ToBool(reader[(int)RüstOrtFelder.Kapazitaet_truppen])
            };
            return obj;
        }

        public void Dispose()
        {
        }
    }
}
