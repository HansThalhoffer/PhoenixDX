using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {
    public static class SchatzkammerView {

        /// <summary>
        /// die Funktion holt den aktuellen Stand der Schatzkammer für den Zug
        /// </summary>
        /// <returns></returns>
        public static Schatzkammer GetActual() {
            if (SharedData.Schatzkammer != null && SharedData.Schatzkammer.Count > 0) {
                return SharedData.Schatzkammer.Where(k => k.monat == ProgramView.SelectedMonth).First();
            }
            return new Schatzkammer();
        }

        /// <summary>
        /// die Funktion berechnet den aktuellen Stand der Schatzkammer für den Zug
        /// </summary>
        /// <returns></returns>
        public static int MoneyToSpendThisTurn() {
            return new Schatzkammer().Reichschatz;
        }

        /// <summary>
        /// summiert die Kampfeinnahmen und besonderen Einnahmen der Truppen
        /// </summary>
        /// <param name="kf"></param>
        /// <returns></returns>
        public static int GetMoneyOnKleinfeld(KleinFeld kf) {
             var armee = SpielfigurenView.GetSpielfiguren(kf);
            int besondereEinnahmen = 0;
            int kampfEinnahmen = 0;
            foreach (var item in armee) {
                if (item is TruppenSpielfigur truppe) {
                    kampfEinnahmen +=truppe.Kampfeinnahmen;
                    besondereEinnahmen += truppe.GS;
                }
            }
            return besondereEinnahmen+ kampfEinnahmen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kf"></param>
        /// <param name="geplanteAusgaben"></param>
        /// <returns></returns>
        public static bool HasEnoughMoney(KleinFeld kf, int geplanteAusgaben) {
            // suche im Reichsschatz zuerst
            if (MoneyToSpendThisTurn() >= geplanteAusgaben)
                return true;
            // nun suche nach Heer auf dem Feld mit den notwendigen Geld in der Tasche
            return GetMoneyOnKleinfeld(kf) >= geplanteAusgaben;
        }


    }
}
