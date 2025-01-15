using PhoenixModel.EventsAndArgs;
using System.Data.Common;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixModel.Database {

    public interface ILoadableDatabase : IDisposable
    {
        public EncryptedString Encryptedpassword { get; set; }
        public string DatabaseFileName { get; set; }
        public void Load();
        public void Save(IDatabaseTable table);
        public void BackgroundLoad(LoadCompleted loadCompletedDelegate);
    }

    public interface IDatabaseTable
    {
        public abstract string TableName { get; }
        public abstract string DatabaseName { get; set; }
        public string Bezeichner { get; }
        public abstract void Load(DbDataReader reader);
        public abstract void Save(DbCommand reader);
        public abstract void Insert(DbCommand reader);
    }
}
