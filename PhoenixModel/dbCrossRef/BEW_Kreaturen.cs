using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.dbCrossRef
{
    public class BEW_Kreaturen : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_Kreaturen";
        string IDatabaseTable.TableName => TableName;
    }
}
