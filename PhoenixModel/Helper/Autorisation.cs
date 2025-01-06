using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Helper
{
    public static class Autorisation
    {
        /// <summary>
        /// überprüft, ob der Benutzer die Daten ändern darf
        /// </summary>
        /// <param name="table"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsAllowedToChange(IDatabaseTable table, string property)
        {
            if (table == null || string.IsNullOrEmpty(property) )
                throw new ArgumentNullException("table or property dürfen nicht null sein");
            switch(property)
            {
                case "Bauwerknamen":
                    {
                        if (table is KleinFeld gemark)
                            return (ViewModel.BelongsToUser(gemark));
                        if (table is Gebäude gebäude)
                            return (ViewModel.BelongsToUser(gebäude));
                    }
                    break;
            }

            return false;
        }
    }
}
