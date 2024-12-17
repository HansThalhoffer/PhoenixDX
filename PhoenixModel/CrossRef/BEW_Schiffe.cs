using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.CrossRef
{
    public class BEW_Schiffe : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_Schiffe";
        string IDatabaseTable.TableName => TableName;

    }
}
