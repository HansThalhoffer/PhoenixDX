using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ExternalTables
{
    public class ReichTabelle
    {
        public ReichTabelle(int nummer, string[] alias, string farbname, string farbe)
        { Alias = alias; FarbeHex = farbe; Farbname = farbname; }
        public string[] Alias { get; set; }
        public string Farbname { get; set; }
        public string FarbeHex { get; set; }

        public static int Piraten = 9;

        public static ReichTabelle[] Vorbelegung =
        {
                new ReichTabelle( 0, new string[] { "Kein Nation", "keinreich",  "kein reich" }, "grau", "#808080"),
                new ReichTabelle( 1, new string[] { "Theostelos", "theostelos" }, "hellblau", "#04D9FF") ,
                new ReichTabelle( 2, new string[] { "Yaromo", "yaromo" }, "gelb", "#FFFF00") ,
                new ReichTabelle( 3, new string[] { "USA", "variedat", "uk d.v.k", "u.s.a.", "unabhängige stämme asimilans" }, "orange","#FFA500") ,
                new ReichTabelle( 4, new string[] { "Vir Vachal", "virvachal", "vir vachal", "Union von Vir'Vachal und Asimilan", "UVA", "U.V.A." }, "hellgrün", "#90EE90") ,
                new ReichTabelle( 5, new string[] { "Avallon", "avallon" }, "dunkelgrün","#4a6741") ,
                new ReichTabelle( 6, new string[] { "Northeim" , "northeim" }, "weiß","#FFFFFF" ) ,
                new ReichTabelle( 7, new string[] { "Helborn" , "helborn" },"schwarz", "#000000") ,
                new ReichTabelle( 8, new string[] { "Eoganachta", "eoganachta", "eoganochta" }, "lila", "#A020F0") ,
                new ReichTabelle( 9, new string[] { "Piraten", "choson", "piraten", "purple puppy pirates", "ppp"},"blau", "#0000FF"),
                new ReichTabelle( 10, new string[] { "O'Har" , "o'har", "ohar" , "Ohar"}, "rot", "#FF0000") ,
                new ReichTabelle( 11, new string[] { "Consortium Commertialis", "consortium commertialis", "consortium", "Konsortium", "gilde" },"gold", "#D4AF37") ,
                new ReichTabelle( 12, new string[] { "Spielleitung" , "spielleitung", "sl" },"spielleitung", "#FF1493")
            };

    }
}
