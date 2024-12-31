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
    public class Armee : List<Spielfigur>, IEigenschaftler
    {
        public List<Eigenschaft> Eigenschaften
        {
            get
            {
                List<Eigenschaft> eigenschaften = [];
                foreach (var figur in this)
                {
                    if (figur is Kreaturen)
                    {
                        var k = figur as Kreaturen;
                        if (k != null)
                        {
                            string wert = $"{k.Bezeichner}";
                            eigenschaften.Add(new Eigenschaft("Kreatur", wert, false));
                        }
                    }
                    else if (figur is Krieger)
                    {
                        var k = figur as Krieger;
                        if (k != null)
                        {
                            string wert = $"{k.Bezeichner} Str {k.staerke.ToString("n0")} HF {k.hf}";
                            if (k.Garde)
                                wert += " Garde";
                            if (k.GS > 0)
                                wert += $" Gold {k.GS}";
                            if (k.LKP > 0)
                                wert += $" LKP {k.LKP}";
                            if (k.SKP > 0)
                                wert += $" SKP:{k.SKP}";
                            if (k.Pferde > 0)
                                wert += $" Pferde {k.Pferde}";
                            eigenschaften.Add(new Eigenschaft("Krieger", wert, false));
                        }
                    }
                    else if (figur is Reiter)
                    {
                        var k = figur as Reiter;
                        if (k != null)
                        {
                            string wert = $"{k.Bezeichner} Str {k.staerke} HF {k.hf}";
                            if (k.Garde)
                                wert += " Garde";
                            if (k.GS > 0)
                                wert += $" Gold {k.GS}";
                            if (k.LKP > 0)
                                wert += $" LKP {k.LKP}";
                            if (k.SKP > 0)
                                wert += $" SKP {k.SKP}";
                            if (k.Pferde > 0)
                                wert += $" Pferde {k.Pferde}";
                            eigenschaften.Add(new Eigenschaft("Reiter", wert, false));
                        }
                    }
                    else if (figur is Schiffe)
                    {
                        var k = figur as Schiffe;
                        if (k != null)
                        {
                            string wert = $"{k.Bezeichner} Str {k.staerke}";
                            if (k.Garde)
                                wert += " G";
                            if (k.GS > 0)
                                wert += $" Gold {k.GS}";
                            if (k.LKP > 0)
                                wert += $" LKP {k.LKP}";
                            if (k.SKP > 0)
                                wert += $" SKP {k.SKP}";
                            if (k.Pferde > 0)
                                wert += $" Pferde {k.Pferde}";
                            eigenschaften.Add(new Eigenschaft("Schiff", wert, false));
                        }
                    }
                    else if (figur is Zauberer)
                    {
                        var k = figur as Zauberer;
                        if (k != null)
                        {
                            string wert = $"{k.Bezeichner} {k.Beschriftung} GP {k.GP_akt}";
                            eigenschaften.Add(new Eigenschaft("Zauberer", wert, false));
                        }
                    }
                    else if (figur is Character)
                    {
                        var k = figur as Character;
                        if (k != null)
                        {
                            string wert = $"{k.Bezeichner} {k.Beschriftung} GP {k.GP_akt}";
                            eigenschaften.Add(new Eigenschaft("Charakter", wert, false));
                        }
                    }

                }
                return eigenschaften;
            }
        }

        public string Bezeichner => "Armee";
    }

    public static class SpielfigurenView
    {
        public static Armee GetSpielfiguren(KleinfeldPosition gem)
        {
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
