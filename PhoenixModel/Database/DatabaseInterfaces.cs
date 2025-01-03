﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixModel.Database
{
    public delegate void LoadCompleted(ILoadableDatabase database);

    public interface ILoadableDatabase : IDisposable
    {
        public EncryptedString Encryptedpassword { get; set; }
        public string DatabaseFileName { get; set; }
        public void Load();
        public void BackgroundLoad(LoadCompleted loadCompletedDelegate);
    }

    public interface IDatabaseTable
    {
        public abstract string TableName { get; }
        public string Bezeichner { get;}
        public abstract void Load(DbDataReader reader);
        public abstract void Save(DbCommand reader);
        public abstract void Insert(DbCommand reader);
    }
}
