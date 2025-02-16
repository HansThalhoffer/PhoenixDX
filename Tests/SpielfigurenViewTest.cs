using LiveCharts.Wpf;
using PhoenixModel.Commands;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Commands.DiplomacyCommand;

namespace Tests {
    public class SpielfigurenViewTest {
        [StaFact]
        public void RaumpunkteTest() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            /*TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, true);*/

            Spielfigur[] expected = [
               new Krieger() { Nummer = 1, staerke = 1000, hf = 17, Pferde = 0, LKP = 2, SKP = 4, rp = 12700, }, // 1000 + 1700 + 2000 + 8000
                new Krieger() { Nummer = 2, staerke = 23034, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 23134, },
                new Krieger() { Nummer = 3, staerke = 79000, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 79100, },
                new Krieger() { Nummer = 4, staerke = 14034, hf = 14, Pferde = 0, LKP = 0, SKP = 1, rp = 17434, },
                new Krieger() { Nummer = 5, staerke = 8000, hf = 9, Pferde = 0, LKP = 0, SKP = 0, rp = 8900, },
                new Krieger() { Nummer = 6, staerke = 1000, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Krieger() { Nummer = 7, staerke = 32000, hf = 9, Pferde = 0, LKP = 0, SKP = 0, rp = 32900, },
                new Krieger() { Nummer = 8, staerke = 78000, hf = 3, Pferde = 0, LKP = 0, SKP = 0, rp = 78300, },
                new Krieger() { Nummer = 9, staerke = 66360, hf = 13, Pferde = 0, LKP = 0, SKP = 0, rp = 67660, },
                new Krieger() { Nummer = 10, staerke = 50275, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 50375, },
                new Krieger() { Nummer = 11, staerke = 1000, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Krieger() { Nummer = 12, staerke = 1000, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Krieger() { Nummer = 13, staerke = 25000, hf = 25, Pferde = 0, LKP = 0, SKP = 0, rp = 27500, },
                new Krieger() { Nummer = 14, staerke = 20000, hf = 2, Pferde = 0, LKP = 0, SKP = 0, rp = 20200, },
                new Reiter() { Nummer = 15, staerke = 4000, hf = 8, Pferde = 0, LKP = 0, SKP = 0, rp = 8800, },
                new Reiter() { Nummer = 16, staerke = 1000, hf = 2, Pferde = 0, LKP = 0, SKP = 0, rp = 2200, },
                new Reiter() { Nummer = 17, staerke = 1, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 102, },
                new Reiter() { Nummer = 18, staerke = 21500, hf = 27, Pferde = 0, LKP = 0, SKP = 0, rp = 45700, },
                new Reiter() { Nummer = 19, staerke = 18254, hf = 43, Pferde = 0, LKP = 0, SKP = 0, rp = 40808, },
                new Reiter() { Nummer = 20, staerke = 1500, hf = 3, Pferde = 0, LKP = 0, SKP = 0, rp = 3300, },
                new Reiter() { Nummer = 21, staerke = 500, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Reiter() { Nummer = 22, staerke = 1000, hf = 2, Pferde = 0, LKP = 0, SKP = 0, rp = 2200, },

                new Schiffe() { Nummer = 23, staerke = 300, hf = 15, Pferde = 0, LKP = 0, SKP = 0, rp = 31500, },
                new Schiffe() { Nummer = 24, staerke = 10, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Schiffe() { Nummer = 25, staerke = 10, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Schiffe() { Nummer = 26, staerke = 110, hf = 11, Pferde = 0, LKP = 0, SKP = 0, rp = 12100, },
                new Schiffe() { Nummer = 27, staerke = 90, hf = 9, Pferde = 0, LKP = 0, SKP = 0, rp = 9900, },
                new Schiffe() { Nummer = 28, staerke = 30, hf = 3, Pferde = 0, LKP = 0, SKP = 0, rp = 3300, },
                new Schiffe() { Nummer = 29, staerke = 10, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Schiffe() { Nummer = 30, staerke = 640, hf = 225, Pferde = 0, LKP = 0, SKP = 3, rp = 92500, },
                new Schiffe() { Nummer = 31, staerke = 10, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Schiffe() { Nummer = 32, staerke = 10, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Schiffe() { Nummer = 33, staerke = 630, hf = 70, Pferde = 0, LKP = 17, SKP = 0, rp = 87000, },
                new Schiffe() { Nummer = 34, staerke = 260, hf = 9, Pferde = 0, LKP = 0, SKP = 0, rp = 26900, },
                new Schiffe() { Nummer = 35, staerke = 10, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Schiffe() { Nummer = 36, staerke = 20, hf = 2, Pferde = 0, LKP = 0, SKP = 0, rp = 2200, },
                new Schiffe() { Nummer = 37, staerke = 10, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Schiffe() { Nummer = 38, staerke = 20, hf = 2, Pferde = 0, LKP = 0, SKP = 0, rp = 2200, },
                new Schiffe() { Nummer = 39, staerke = 621, hf = 34, Pferde = 0, LKP = 13, SKP = 0, rp = 78500, },
                new Schiffe() { Nummer = 40, staerke = 20, hf = 2, Pferde = 0, LKP = 0, SKP = 0, rp = 2200, },
                new Schiffe() { Nummer = 41, staerke = 10, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Schiffe() { Nummer = 42, staerke = 610, hf = 31, Pferde = 0, LKP = 0, SKP = 0, rp = 64100, },
                new Schiffe() { Nummer = 43, staerke = 10, hf = 1, Pferde = 0, LKP = 0, SKP = 0, rp = 1100, },
                new Schiffe() { Nummer = 44, staerke = 111, hf = 10, Pferde = 0, LKP = 0, SKP = 0, rp = 12100, },
                new Schiffe() { Nummer = 45, staerke = 630, hf = 110, Pferde = 0, LKP = 2, SKP = 0, rp = 76000, },

                // Adding Character
                new Character() { Nummer = 50, GP_akt = 65, rp = 3250, SpielerName="A" },
                new Character() { Nummer = 51, GP_akt = 50, rp = 2500, SpielerName="A" },
                new Character() { Nummer = 52, GP_akt = 42, rp = 2100, SpielerName="A" },
                new Character() { Nummer = 53, GP_akt = 30, rp = 1500, SpielerName="A" },

                // Adding Zauberer
                new Zauberer() { Nummer = 60, GP_akt = 4, rp = 200, },
                new Zauberer() { Nummer = 61, GP_akt = 5, rp = 250, },
                new Zauberer() { Nummer = 62, GP_akt = 4, rp = 200, },
                new Zauberer() { Nummer = 63, GP_akt = 2, rp = 100, },
                new Zauberer() { Nummer = 64, GP_akt = 4, rp = 200, },
                new Zauberer() { Nummer = 65, GP_akt = 4, rp = 200, },
                new Zauberer() { Nummer = 66, GP_akt = 2, rp = 100, },
                new Zauberer() { Nummer = 67, GP_akt = 6, rp = 300, },
                new Zauberer() { Nummer = 68, GP_akt = 8, rp = 400, },
                new Zauberer() { Nummer = 69, GP_akt = 17, rp = 850, },
                new Zauberer() { Nummer = 70, GP_akt = 19, rp = 950, },
                new Zauberer() { Nummer = 71, GP_akt = 9, rp = 450, },
            ];

            /*if (SharedData.Zauberer == null || SharedData.Zauberer.Count == 0) {
                Assert.Fail("Zauberer hat keine Daten");
                return;
            }
            foreach (var item in SharedData.Zauberer) {
                if (item.GP_akt == 0)
                    continue;
                string output = @$"new Zauberer() {{ GP_akt = {item.GP_akt.ToString()}, rp = {item.rp}, }},";
                Debug.WriteLine(output);
                int rp = SpielfigurenView.BerechneRaumpunkte(item);
                // Assert.True(rp == item.rp);
            }*/
            foreach (var item in expected) {
                int rp = SpielfigurRules.BerechneRaumpunkte(item);
                Assert.True(rp == item.rp);
            }

        }
    }
}
