using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View
{
    public static class SpielfigurenView
    {
        public static List<Spielfigur> GetSpielfiguren(KleinfeldPosition gem)
        {
            List<Spielfigur> result = [];
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
        /// eine klare Zuordnung zu einer Klasse ist hier schwierig, daher die Weiterleitung
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static bool BelongsToUser(Spielfigur figur)
        {
            return ViewModel.BelongsToUser(figur);
        }
    }
}
