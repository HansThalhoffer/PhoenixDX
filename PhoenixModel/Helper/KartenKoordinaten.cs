using PhoenixModel.Helper;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.ExternalTables.GeländeTabelle;

namespace PhoenixModel.dbErkenfara {

    /// <summary>
    /// Windrichtungen für die Texturen
    /// </summary>
    public enum Direction {
        NW = 0, NO = 1, O = 2, SO = 3, SW = 4, W = 5
    }

    public class KartenKoordinaten : KleinfeldPosition {
      
        public int dbx;
        public int dby;

        public string DebuggerDisplay() {
            return $"KartenKoordinaten {gf}/{kf} {dbx}/{dby}";
        }

        /// <summary>
        /// Kartenkoordinaten beinhaltet zusätzlich die dbx und dby aus der Datenbank
        /// Diese Werte werden nicht im Programm verwendet, jedoch zwecks Kompatibilität gehalten
        /// bis die Altanwendung definitiv abgelöst ist
        /// </summary>
        /// <param name="gf"></param>
        /// <param name="kf"></param>
        /// <param name="dbx"></param>
        /// <param name="dby"></param>
        public KartenKoordinaten(int gf, int kf, int dbx, int dby) {
            this.gf = gf;
            this.kf = kf;
            this.dbx = dbx;
            this.dby = dby;
        }

        /// <summary>
        /// gibt Spalte und Reihe in der Provinz zurück
        /// </summary>
        /// <returns></returns>
        public Position GetPositionInProvinz() {
            if (kf > 48)
                return new Position(0, 0);

            if (kf <= 4)
                return new Position(kf + 4, 1);
            if (kf <= 9)
                return new Position(kf - 1, 2);
            if (kf <= 15)
                return new Position(kf - 7, 3);
            if (kf <= 22)
                return new Position(kf - 15, 4);
            if (kf <= 30)
                return new Position(kf - 22, 5);
            if (kf <= 37)
                return new Position(kf - 30, 6);
            if (kf <= 43)
                return new Position(kf - 35, 7);

            return new Position(kf - 39, 8);
        }

        /// <summary>
        /// Das Dictionary beschleunigt das Finden von Nachbarn in den Hexfeldern
        /// Sie sind nach Windrichtung geordnet und somit können sie auch zur Pfadfindung dienen
        /// Der Zugriff über ein Dictionary ist deutlich schneller als die Berechnung
        /// </summary>
        static Dictionary<int, KleinfeldPosition[]> _nachbarn = new()
        {
            { 1, new KleinfeldPosition[] { new (-1, 44), new (-1, 45), new (0, 2), new (0, 6), new (0, 5), new (-101, 30) } },
            { 2, new KleinfeldPosition[] { new (-1, 45), new (-1, 46), new (0, 3), new (0, 7), new (0, 6), new(0, 1) } },
            { 3, new KleinfeldPosition[] { new (-1, 46), new (-1, 47), new (0, 4), new (0, 8), new (0, 7), new(0, 2) } },
            { 4, new KleinfeldPosition[] { new (-1, 47), new (-1, 48), new (100, 23), new (0, 9), new (0, 8), new(0, 3) } },
            { 5, new KleinfeldPosition[] { new (-101, 30), new (0, 1), new (0,6), new (0,11), new (0, 10), new(-101, 37) } },
             // 6..9:
            { 6, new KleinfeldPosition[] { new (0, 1), new (0, 2), new (0, 7), new (0, 12), new (0, 11), new (0, 5) } },
            { 7, new KleinfeldPosition[] { new (0, 2), new (0, 3), new (0, 8), new (0, 13), new (0, 12), new (0, 6) } },
            { 8, new KleinfeldPosition[] { new (0, 3), new (0, 4), new (0, 9), new (0, 14), new (0, 13), new (0, 7) } },
            { 9, new KleinfeldPosition[] { new (0, 4), new (100, 23), new (100, 31), new (0, 15), new (0, 14), new (0, 8) } },
            // 10..15:
            { 10, new KleinfeldPosition[] { new (-101, 37), new (0, 5), new (0, 11), new (0, 17), new (0, 16) , new (-101,43) } },
            { 11, new KleinfeldPosition[] { new (0, 5), new (0, 6), new (0, 12), new (0, 18), new (0, 17), new (0, 10) } },
            { 12, new KleinfeldPosition[] { new (0, 6), new (0, 7), new (0, 13), new (0, 19), new (0, 18), new (0, 11) } },
            { 13, new KleinfeldPosition[] { new (0, 7), new (0, 8), new (0, 14), new (0, 20), new (0, 19), new (0, 12) } },
            { 14, new KleinfeldPosition[] { new (0, 8), new (0, 9), new (0, 15), new (0, 21), new (0, 20), new (0, 13) } },
            { 15, new KleinfeldPosition[] { new (0, 9), new (100, 31), new (100, 38), new (0, 22), new (0, 21), new (0, 14) } },

            // 16..22 (Row 4)
            { 16, new KleinfeldPosition[] { new (-101, 43), new (0, 10), new (0, 17), new (0, 24), new (0, 23), new (-101, 48)  } },
            { 17, new KleinfeldPosition[] { new (0, 10), new (0, 11), new (0, 18), new (0, 25), new (0, 24), new (0, 16) } },
            { 18, new KleinfeldPosition[] { new (0, 11), new (0, 12), new (0, 19), new (0, 26), new (0, 25), new (0, 17) } },
            { 19, new KleinfeldPosition[] { new (0, 12), new (0, 13), new (0, 20), new (0, 27), new (0, 26), new (0, 18) } },
            { 20, new KleinfeldPosition[] { new (0, 13), new (0, 14), new (0, 21), new (0, 28), new (0, 27), new (0, 19) } },
            { 21, new KleinfeldPosition[] { new (0, 14), new (0, 15), new (0, 22), new (0, 29), new (0, 28), new (0, 20) } },
            { 22, new KleinfeldPosition[] { new (0, 15), new (100, 38), new (100, 44), new (0, 30), new (0, 29), new (0, 21) } },

            // Continue below (23..30) - Row 5
            { 23, new KleinfeldPosition[] { new (-101, 48), new (0, 16), new (0, 24), new (0, 31), new (-100, 9), new (-100, 4), } },
            { 24, new KleinfeldPosition[] { new (0, 16), new (0, 17), new (0, 25), new (0, 32), new (0, 31), new (0, 23) } },
            { 25, new KleinfeldPosition[] { new (0, 17), new (0, 18), new (0, 26), new (0, 33), new (0, 32), new (0, 24) } },
            { 26, new KleinfeldPosition[] { new (0, 18), new (0, 19), new (0, 27), new (0, 34), new (0, 33), new (0, 25) } },
            { 27, new KleinfeldPosition[] { new (0, 19), new (0, 20), new (0, 28), new (0, 35), new (0, 34), new (0, 26) } },
            { 28, new KleinfeldPosition[] { new (0, 20), new (0, 21), new (0, 29), new (0, 36), new (0, 35), new (0, 27) } },
            { 29, new KleinfeldPosition[] { new (0, 21), new (0, 22), new (0, 30), new (0, 37), new (0, 36), new (0, 28) } },
            { 30, new KleinfeldPosition[] { new (0, 22), new (100, 44), new (101, 1), new (101, 5), new (0, 37), new (0, 29) } },

            // 31..37 (Row 6)
            { 31, new KleinfeldPosition[] { new (0, 23), new (0, 24), new (0, 32), new (0, 38), new (-100, 15), new(-100, 9) } },
            { 32, new KleinfeldPosition[] { new (0, 24), new (0, 25), new (0, 33), new (0, 39), new (0, 38), new (0, 31) } },
            { 33, new KleinfeldPosition[] { new (0, 25), new (0, 26), new (0, 34), new (0, 40), new (0, 39), new (0, 32) } },
            { 34, new KleinfeldPosition[] { new (0, 26), new (0, 27), new (0, 35), new (0, 41), new (0, 40), new (0, 33) } },
            { 35, new KleinfeldPosition[] { new (0, 27), new (0, 28), new (0, 36), new (0, 42), new (0, 41), new (0, 34) } },
            { 36, new KleinfeldPosition[] { new (0, 28), new (0, 29), new (0, 37), new (0, 43), new (0, 42), new (0, 35) } },
            { 37, new KleinfeldPosition[] { new (0, 29), new (0, 30), new (101, 5), new (101, 10), new (0, 43), new (0, 36) } },

            // 38..43 (Row 7)
            { 38, new KleinfeldPosition[] { new (0, 31), new (0, 32), new (0, 39), new (0, 44), new (-100, 22), new (-100, 15) } },
            { 39, new KleinfeldPosition[] { new (0, 32), new (0, 33), new (0, 40), new (0, 45), new (0, 44), new (0, 38) } },
            { 40, new KleinfeldPosition[] { new (0, 33), new (0, 34), new (0, 41), new (0, 46), new (0, 45), new (0, 39) } },
            { 41, new KleinfeldPosition[] { new (0, 34), new (0, 35), new (0, 42), new (0, 47), new (0, 46), new (0, 40) } },
            { 42, new KleinfeldPosition[] { new (0, 35), new (0, 36), new (0, 43), new (0, 48), new (0, 47), new (0, 41) } },
            { 43, new KleinfeldPosition[] { new (0, 36), new (0, 37), new (101, 10), new (101, 16), new (0, 48), new (0, 42) } },

            // 44..48 (Row 8)
            { 44, new KleinfeldPosition[] { new (0, 38), new (0, 39), new (0, 45), new (1, 1), new (-100, 30), new (-100, 22) } },
            { 45, new KleinfeldPosition[] { new (0, 39), new (0, 40), new (0, 46), new (1, 2), new (1, 1), new (0, 44) } },
            { 46, new KleinfeldPosition[] { new (0, 40), new (0, 41), new (0, 47), new (1, 3), new (1, 2), new (0, 45) } },
            { 47, new KleinfeldPosition[] { new (0, 41), new (0, 42), new (0, 48), new (1, 4), new (1, 3), new (0, 46) } },
            { 48, new KleinfeldPosition[] { new (0, 42), new (0, 43), new (101, 16), new (101, 23), new(1, 4), new (0, 47) } },
        };

        /// <summary>
        /// holt den direkten Nachbarn in Windrichutng des übergebenen Kleinfeldes aus dem Dictionary
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static KleinfeldPosition? GetFromNachbarnArray(KleinfeldPosition pos, Direction direction) {
            if (pos.kf < 1 || pos.kf > 48)
                return null; 
            switch (direction) {
                case Direction.NW:
                    return _nachbarn[pos.kf][0];
                case Direction.NO:
                    return _nachbarn[pos.kf][1];
                case Direction.O:
                    return _nachbarn[pos.kf][2];
                case Direction.SO:
                    return _nachbarn[pos.kf][3];
                case Direction.SW:
                    return _nachbarn[pos.kf][4];
                case Direction.W:
                    return _nachbarn[pos.kf][5];
                default:
                    return null;
            }
        }

        /// <summary>
        /// berechnet den Nachbar des übergebenen Kleinfeldes anhand der Windrichtung
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static KleinfeldPosition? GetNachbar(KleinfeldPosition pos, Direction direction) {
            // aus dem dictionary holen
            var nachbar = GetFromNachbarnArray(pos,direction);
            if (nachbar == null)
                return null;

            // hier folgt nun die Berechnung
            return CreateFromNachbarnArray(pos,nachbar);
        }

        /// <summary>
        /// berechnet alle Nachbarn um das gegebene Kleinfeld herum
        /// </summary>
        /// <param name="original"></param>
        /// <param name="nachbar"></param>
        /// <returns></returns>
        private static KleinfeldPosition CreateFromNachbarnArray(KleinfeldPosition original, KleinfeldPosition nachbar) 
        {
            var result = new KleinfeldPosition(nachbar.gf + original.gf, nachbar.kf);
            if (original.gf > 700) {
                int cur = original.gf / 100;
                int nb = result.gf / 100;
                // wegen den halben eingeschobenen Provinzen ist das hier etwas schräg, aber ziemlich schnell
                if ((cur == 8 && nb == 7) ||
                    (cur == 10 && nb == 9) || (cur == 11 && nb == 10) ||
                    (cur == 12 && nb == 11) || (cur == 13 && nb == 12) ||
                    (cur == 14 && nb == 13) || (cur == 15 && nb == 14)
                    ) {
                    result.gf += 1;
                }
                else if ((cur == 7 && nb == 8) ||
                    (cur == 9 && nb == 10) || (cur == 10 && nb == 11) ||
                    (cur == 11 && nb == 12) || (cur == 12 && nb == 13) ||
                    (cur == 13 && nb == 14) || (cur == 14 && nb == 15)
                    ) {
                    result.gf -= 1;
                }
            }
            return result;
        }

        /// <summary>
        /// berechnet die 6 Nachbarn in alle Windrichtungen eines übergebenen Kleinfeldes
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static IEnumerable<KleinfeldPosition>? GetKleinfeldNachbarn(KleinfeldPosition pos) {

            if (pos.kf < 1 || pos.kf > 48)
                return null;
            KleinfeldPosition[] nachbarn =  new KleinfeldPosition[6];
            int i = 0;
            try {
                foreach (var kf in _nachbarn[pos.kf]) {
                    nachbarn[i++] = CreateFromNachbarnArray(pos, kf);
                }
            }
            catch (Exception ex) {
                ViewModel.LogError("Irgendwer hat das Array vergößert", ex.Message);
            }
            return nachbarn;
        }
    }
}
