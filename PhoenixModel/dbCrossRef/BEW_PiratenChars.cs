using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class BEW_PiratenChars : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_PiratenChars";
        string IDatabaseTable.TableName => TableName;

    }
}
