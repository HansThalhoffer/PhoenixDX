using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixModel.Database
{
    public interface ILoadableDatabase : IDisposable
    {
        public EncryptedString Encryptedpassword { get; set; }
        public string DatabaseFileName { get; set; }
        public void Load();
    }

    public interface IDatabaseTable
    {
        public abstract string TableName { get; }
    }
}
