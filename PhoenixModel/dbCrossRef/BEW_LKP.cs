using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class BEW_LKP : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_LKP";
        string IDatabaseTable.TableName => TableName;
    }
}
