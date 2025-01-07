using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;

namespace PhoenixModel.dbZugdaten
{
    public class Diplomatiechange :  ReichCrossref, IDatabaseTable
    {
        private static string _datebaseName = string.Empty;
        public override string DatabaseName { get { return Diplomatiechange._datebaseName; } set { Diplomatiechange._datebaseName = value; } }
        public new const string TableName = "Diplomatiechange";
    }
}
