using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;

namespace PhoenixModel.CrossRef
{
    public class BEW_Reiter : BEW, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "BEW_Reiter";
        string IDatabaseTable.TableName => TableName;
    }

}
