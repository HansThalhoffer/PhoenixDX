using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class BEW_chars : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_chars";
        string IDatabaseTable.TableName => TableName;

    }
}
