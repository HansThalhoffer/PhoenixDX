using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class BEW_PiratenSchiffe : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_PiratenSchiffe";
        string IDatabaseTable.TableName => TableName;

    }
}
