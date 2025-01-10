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
        protected override string GetTableName() { return TableName; }

        public Diplomatiechange():base()
        { }
        
        public void CopyValues(ReichCrossref baseObject) 
        {
            if (baseObject == null)
                throw new ArgumentNullException(nameof(baseObject));

            // Copy properties from base class
            Referenzreich = baseObject.ReferenzNation.Name;
            Wegerecht = baseObject.Wegerecht;
            Kuestenrecht = baseObject.Kuestenrecht;
            Reich = baseObject.Nation.Reich;
            Wegerecht_von = baseObject.Wegerecht_von;
            DBname = baseObject.DBname;
            Kuestenrecht_von = baseObject.Kuestenrecht_von;
            Flottenkey = baseObject.Flottenkey;
        }
    }
}
