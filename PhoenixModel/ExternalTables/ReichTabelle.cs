using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PhoenixModel.ExternalTables
{
    public class ReichTabelle
    {
        public ReichTabelle(int nummer, HashSet<string> alias, string farbname, string farbe)
        { Alias = alias; FarbeHex = farbe; Farbname = farbname; }
        public HashSet<string> Alias { get; private set; }
        public string Farbname { get; private set; }
        public string FarbeHex { get; private set; }

        public static int Piraten = 9;

        public static ReichTabelle? Find(string reichbezeichner)
        {
            foreach (var defData in ReichTabelle.Vorbelegung)
            {
                if (defData.Alias.Contains(reichbezeichner.ToLower()))
                    return defData;
            }
            return null;
        }
        /// <summary>
        /// es gibt leider so viele Schreibweisen und Umbenennungen
        /// </summary>
        public static ReichTabelle[] Vorbelegung =
        {
                new ReichTabelle( 0, ["keine nation", "keinreich",  "kein reich"], "grau", "#808080"),
                new ReichTabelle( 1, ["theostelos", "theostelos"], "hellblau", "#04D9FF") ,
                new ReichTabelle( 2, ["yaromo", "yaromo"], "gelb", "#FFFF00") ,
                new ReichTabelle( 3, ["usa", "variedat", "uk d.v.k", "u.s.a.", "unabhängige stämme asimilans"], "orange","#FFA500") ,
                new ReichTabelle( 4, ["vir vachal", "virvachal","union von vir'vachal und asimilan", "uva", "u.v.a."], "hellgrün", "#A0FFB0") ,
                new ReichTabelle( 5, ["avallon"], "dunkelgrün","#005000") ,
                new ReichTabelle( 6, ["northeim"], "weiß","#FFFFFF" ) ,
                new ReichTabelle( 7, ["helborn"],"schwarz", "#000000") ,
                new ReichTabelle( 8, ["eoganachta", "eoganochta"], "lila", "#A020F0") ,
                new ReichTabelle( 9, ["piraten", "choson", "pirat", "purple puppy pirates", "ppp"],"blau", "#4040FF"),
                new ReichTabelle( 10, ["o'har", "ohar"], "rot", "#DD1010") ,
                new ReichTabelle( 11, ["consortium commertialis", "consortium", "konsortium", "gilde"],"gold", "#D4AF37") ,
                new ReichTabelle( 12, ["spielleitung", "sl"],"spielleitung", "#FF1493")
            };

    }
}
