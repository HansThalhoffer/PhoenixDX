using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Program
{
    public interface ISelectable:IEigenschaftler
    {
        /// <summary>
        /// das aktuelle Element kann auf die Auswahl reagieren
        /// Truppen und Rüstorte sind nur dann auswählbar, wenn das Element auch zu der ausgewählten Nation gehört
        /// sie verweigern das ausgewählt werden
        /// </summary>
        public bool Select();

        /// <summary>
        /// das aktuelle Element kann bearbeitet werden
        /// Vorbedingung, es ist auswählbar
        /// Gemarken sind dann bearbeitbar, wenn sie auch zu der ausgewählten Nation gehört
        /// sie verweigern das ausgewählt werden
        /// </summary>
        public bool Edit();
    }
}
