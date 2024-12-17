using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.CrossRef
{
    public class BEW_SKS : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_SKS";
        string IDatabaseTable.TableName => TableName;
    }
}
