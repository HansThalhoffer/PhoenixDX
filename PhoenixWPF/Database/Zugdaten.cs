using PhoenixModel.Database;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixWPF.Program;
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
                    SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, ("Fehler beim Öffnen der PZE Datenbank: " + ex.Message)));
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
                    Load<PhoenixModel.dbZugdaten.BilanzEinnahmen>(connector, ref SharedData.BilanzEinnahmen_Zugdaten, Enum.GetNames(typeof(PhoenixModel.dbZugdaten.BilanzEinnahmen.Felder)));
                    Load<Character>(connector, ref SharedData.Character, Enum.GetNames(typeof(Character.Felder)));
                    Load<Diplomatiechange>(connector, ref SharedData.Diplomatiechange, Enum.GetNames(typeof(Diplomatiechange.Felder)));
                    Load<Kreaturen>(connector, ref SharedData.Kreaturen, Enum.GetNames(typeof(Kreaturen.Felder)));
                    Load<Krieger>(connector, ref SharedData.Krieger, Enum.GetNames(typeof(Krieger.Felder)));
                    Load<Lehensvergabe>(connector, ref SharedData.Lehensvergabe, Enum.GetNames(typeof(Lehensvergabe.Felder)));
                    Load<Personal>(connector, ref SharedData.Personal, Enum.GetNames(typeof(Personal.Felder)));
                    Load<Reiter>(connector, ref SharedData.Reiter, Enum.GetNames(typeof(Reiter.Felder)));
                    Load<RuestungBauwerke>(connector, ref SharedData.RuestungBauwerke, Enum.GetNames(typeof(RuestungBauwerke.Felder)));
                    Load<RuestungRuestorte>(connector, ref SharedData.RuestungRuestorte, Enum.GetNames(typeof(RuestungRuestorte.Felder)));
                    Load<Schatzkammer>(connector, ref SharedData.Schatzkammer, Enum.GetNames(typeof(Schatzkammer.Felder)));
                    Load<Schenkungen>(connector, ref SharedData.Schenkungen, Enum.GetNames(typeof(Schenkungen.Felder)));
                    Load<Schiffe>(connector, ref SharedData.Schiffe, Enum.GetNames(typeof(Schiffe.Felder)));
                    Load<PhoenixModel.dbZugdaten.Units>(connector, ref SharedData.Units_Zugdaten, Enum.GetNames(typeof(PhoenixModel.dbZugdaten.Units.Felder)));
                    Load<Zauberer>(connector, ref SharedData.Zauberer, Enum.GetNames(typeof(Zauberer.Felder)));

                }
                catch (Exception ex)
                {
                    SpielWPF.LogError("Fehler beim Öffnen der Zugdaten Datenbank: " + ex.Message);
                }
                connector?.Close();
            }
        }
    }
}
