using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class BEW_SKP : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_SKP";
        string IDatabaseTable.TableName => TableName;

    }
}
