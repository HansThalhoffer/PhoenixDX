using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class BEW_PiratenLKS : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_PiratenLKS";
        string IDatabaseTable.TableName => TableName;

    }
}
