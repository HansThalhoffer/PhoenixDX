using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.CrossRef
{
    public class BEW_PiratenSKS : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_PiratenSKS";
        string IDatabaseTable.TableName => TableName;

    }
}
