﻿using PhoenixModel.dbPZE;
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
    public class Armee : List<Spielfigur>, IEigenschaftler
    {
        /// <summary>
        ///  dieses Property wird für die Anzeige in den Eigenschaften eines Kleinfeldes genutzt, die Darstellung in der Truppenliste wird in der Page selbst erzeugt
        /// </summary>
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
                            eigenschaften.Add(new Eigenschaft("Kreatur", wert, false, this));
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
                            eigenschaften.Add(new Eigenschaft("Krieger", wert, false, this));
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
                            eigenschaften.Add(new Eigenschaft("Reiter", wert, false, this));
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
                            eigenschaften.Add(new Eigenschaft("Schiff", wert, false, this ));
                        }
                    }
                    else if (figur is Zauberer)
                    {
                        var k = figur as Zauberer;
                        if (k != null)
                        {
                            string wert = $"{k.Bezeichner} {k.Beschriftung} {k.charname} GP {k.GP_akt}";
                            eigenschaften.Add(new Eigenschaft("Zauberer", wert, false, this));
                        }
                    }
                    else if (figur is Character)
                    {
                        var k = figur as Character;
                        if (k != null)
                        {
                            string wert = $"{k.Bezeichner} {k.Beschriftung} {k.Charname} GP {k.GP_akt}";
                            eigenschaften.Add(new Eigenschaft("Charakter", wert, false, this));
                        }
                    }

                }
                return eigenschaften;
            }
        }

        public string Bezeichner => "Armee";
    }

    /// <summary>
    /// Vereinfacht die Nutzung von Truppensammlungen, die aus verschiedenen Klassen bestehen
    /// </summary>
    public static class SpielfigurenView
    {
        public static int BerechneBewegungspunkte(Spielfigur figur)
        {
            //throw new NotImplementedException();
            return 0;
        }

        public static int BerechneBaukosten(Spielfigur figur)
        {
            // throw new NotImplementedException();
            return 0;
        }

        public static int BerechneRaumpunkte(Spielfigur figur)
        {
            //throw new NotImplementedException();
            return 0;
        }

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

        public static Armee GetSpielfiguren(Nation nation)
        {
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
        public static bool BelongsToUser(Spielfigur figur)
        {
            return ProgramView.BelongsToUser(figur);
        }
    }
}
