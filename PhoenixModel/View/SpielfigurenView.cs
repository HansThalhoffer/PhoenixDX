using PhoenixModel.dbCrossRef;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.View.SpielfigurenView.SpielfigurenFilter;

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
        /// erlaubt das Filtern der Figuren
        /// </summary>
        public class SpielfigurenFilter {
            public readonly FigurType FigurType = FigurType.None;
            public enum Search {
                None,
                Gold,
                Fernkampf
            }

            public readonly Search SearchFor = Search.None;
            public SpielfigurenFilter(FigurType figurType) {
                FigurType = figurType;
            }
            public SpielfigurenFilter(Search searchFor) {
                SearchFor = searchFor;
            }
        }



        /// <summary>
        /// Holt alle Spielfiguren einer Nation als Armee
        /// </summary>
        /// <param name="nation"></param>
        /// <returns></returns>
        public static Armee GetSpielfiguren(Nation? nation, SpielfigurenFilter filter) {
            if (nation == null)
                return [];
            Armee result = [];
            if (filter.FigurType != FigurType.None) {
                switch (filter.FigurType) {
                    case FigurType.Kreatur:
                        var kreaturen = SharedData.Kreaturen?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                        if (kreaturen != null)
                            result.AddRange(kreaturen);
                        break;
                    case FigurType.Krieger:
                        var krieger = SharedData.Krieger?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                        if (krieger != null)
                            result.AddRange(krieger);
                        break;
                    case FigurType.Reiter:
                        var reiter = SharedData.Reiter?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                        if (reiter != null)
                            result.AddRange(reiter);
                        break;
                    case FigurType.Schiff:
                        var schiffe = SharedData.Schiffe?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                        if (schiffe != null)
                            result.AddRange(schiffe);
                        break;
                    case FigurType.Charakter:
                        var charaktere = SharedData.Character?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                        if (charaktere != null)
                            result.AddRange(charaktere);
                        break;
                    case FigurType.Zauberer:

                        var zauberer = SharedData.Zauberer?.Where(s => s.Nation == nation && Plausibilität.IsValid(s));
                        if (zauberer != null)
                            result.AddRange(zauberer);
                        break;
                    default:
                        break;
                }
            }
            // erlaubt die Filter zu kombinieren, sofern der Konstruktor des Filters das zulässt
            if (result.Count == 0)
                result = GetSpielfiguren(nation);

            if (filter.SearchFor != Search.None) {
                switch (filter.SearchFor) {
                    case Search.Fernkampf:
                        var fernkämpfer = result.Where(figur => figur.LeichteKP > 0 || figur.SchwereKP > 0).ToArray();
                        result.Clear();
                        result.AddRange(fernkämpfer);
                        break;
                    case Search.Gold:
                        var kapitalisten = result.Where(figur => figur.Gold > 0).ToArray();
                        result.Clear();
                        result.AddRange(kapitalisten);
                        break;
                }
            }
            return result;

        }



        /// <summary>
        /// Hole alle Charaktere und Zauberer, die Spielernamen haben oder Spieler sein können
        /// </summary>
        /// <param name="figur"></param>
        /// <returns></returns>
        public static List<NamensSpielfigur> HoleSpielerfiguren() {
            List<NamensSpielfigur> result = [];
            var charaktere = SharedData.Character?.Where(s => s.IsSpielerFigur == true && Plausibilität.IsValid(s));
            if (charaktere != null)
                result.AddRange(charaktere);
            var zauberer = SharedData.Zauberer?.Where(s => s.IsSpielerFigur == true && Plausibilität.IsValid(s));
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
