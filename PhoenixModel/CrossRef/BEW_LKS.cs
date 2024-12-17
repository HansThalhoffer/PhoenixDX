using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.CrossRef
{
    public class BEW_LKS : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_LKS";
        string IDatabaseTable.TableName => TableName;
    }
}
