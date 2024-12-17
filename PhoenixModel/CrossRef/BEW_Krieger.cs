using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.CrossRef
{
    public class BEW_Krieger : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_Krieger";
        string IDatabaseTable.TableName => TableName;

    }
}
