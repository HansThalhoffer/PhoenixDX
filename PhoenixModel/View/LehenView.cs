using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {
    /// <summary>
    /// Die Sicht auf Lehen
    /// Dieser Teil wurde in der Altanwendung nicht vollständige implementiert
    /// </summary>
    public class LehenView {

        /// <summary>
        /// erzeug ein neues Lehen
        /// </summary>
        /// <param name="character"></param>
        /// <param name="lehensFelder"></param>
        /// <param name="lehensName"></param>
        public static void CreateLehen(NamensSpielfigur figur, List<KleinFeld> lehensFelder, string lehensName) {
            foreach (KleinFeld kf in lehensFelder) {
                string rang = string.Empty;
                if (figur is Character character)
                    rang = CharacterView.GetAssumedKlasse(character).ToString();
                else if (figur is Zauberer wiz)
                    rang = ZaubererView.GetKlasse(wiz).ToString();
                var lehen = (new Lehensvergabe { 
                    Charname = figur.Charname,
                    Charrang = rang,
                    gf = kf.gf,
                    kf = kf.kf,
                    Ruestortname = lehensName,
                    Ruestort = kf.Bauwerknamen  ?? string.Empty,
                    }); 
                SharedData.StoreQueue.Enqueue(lehen);
            }
        }

        /// <summary>
        /// Hole alle Charaktere und Zauberer, die Spielernamen haben oder Spieler sein können und noch kein Lehen haben
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static IEnumerable<NamensSpielfigur> HoleSpielerfigurenOhneLehen() {
            var result = SpielfigurenView.HoleSpielerfiguren();
            if (SharedData.Lehensvergabe == null || result.Count == 0)
                return result;
            
            // unique liste aller Charaktere aus der Lehensvergabe
            var lehensvergabe = SharedData.Lehensvergabe.GroupBy(p => p.Charname).Select(g => g.First()).ToList();
            if (lehensvergabe.Count == 0) 
                return result;
            // Filter Spielfiguren: Only those not in Lehensvergabe
            var filteredSpielfiguren = result
                .Where(s => !lehensvergabe.Any(l => l.Charname == s.CharakterName));
            return filteredSpielfiguren;
        }
    }
}
