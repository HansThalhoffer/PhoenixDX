using PhoenixModel.dbCrossRef;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {

    /// <summary>
    /// Vereinfacht die Nutzung von Truppensammlungen, die aus verschiedenen Klassen bestehen
    /// </summary>

    public static class SpielfigurenView {

        /// <summary>
        /// TODO Berechnugn der Beweungspunkte aus bereits geschriebenen Daten
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static int BerechneBewegungspunkte(Spielfigur figur) {
            //throw new NotImplementedException();
            return 0;
        }

        /// <summary>
        /// TODO Berechnugn der Raumpunkte aus bereits geschriebenen Daten einer Spielfigur
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static int BerechneRaumpunkte(Spielfigur figur) {
            //throw new NotImplementedException();
            return 0;
        }

        /// <summary>
        /// Holt alle Spielfiguren eines Kleinfeldes als eine Armee
        /// </summary>
        /// <param name="gem"></param>
        /// <returns></returns>
        public static Armee GetSpielfiguren(KleinfeldPosition gem) {
            Armee result = [];
            var kreaturen = SharedData.Kreaturen?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
            if (kreaturen != null)
                result.AddRange(kreaturen);

            var krieger = SharedData.Krieger?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
            if (krieger != null)
                result.AddRange(krieger);

            var reiter = SharedData.Reiter?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
            if (reiter != null)
                result.AddRange(reiter);

            var schiffe = SharedData.Schiffe?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
            if (schiffe != null)
                result.AddRange(schiffe);
            var charaktere = SharedData.Character?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
            if (charaktere != null)
                result.AddRange(charaktere);
            var zauberer = SharedData.Zauberer?.Where(s => s.gf == gem.gf && s.kf == gem.kf && Plausibilität.IsValid(s));
            if (zauberer != null)
                result.AddRange(zauberer);
            return result;
        }

        /// <summary>
        /// Holt alle Spielfiguren einer Nation als Armee
        /// </summary>
        /// <param name="nation"></param>
        /// <returns></returns>
        public static Armee GetSpielfiguren(Nation? nation) {
            if (nation == null)
                return [];
            Armee result = [];
            var kreaturen = SharedData.Kreaturen?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
            if (kreaturen != null)
                result.AddRange(kreaturen);

            var krieger = SharedData.Krieger?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
            if (krieger != null)
                result.AddRange(krieger);

            var reiter = SharedData.Reiter?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
            if (reiter != null)
                result.AddRange(reiter);

            var schiffe = SharedData.Schiffe?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
            if (schiffe != null)
                result.AddRange(schiffe);
            var charaktere = SharedData.Character?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
            if (charaktere != null)
                result.AddRange(charaktere);
            var zauberer = SharedData.Zauberer?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
            if (zauberer != null)
                result.AddRange(zauberer);
            return result;
        }

        /// <summary>
        /// eine klare Zuordnung zu einer Klasse ist hier schwierig, daher die Weiterleitung
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static bool BelongsToUser(Spielfigur figur) {
            return ProgramView.BelongsToUser(figur);
        }
    }
}
