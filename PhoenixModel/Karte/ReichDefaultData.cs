using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Karte
{
    public class ReichDefaultData
    {
        public ReichDefaultData(string[] alias, string farbname, string farbe)
        { Alias = alias; FarbeHex = farbe; Farbname = farbname; }
        public string[] Alias { get; set; }
        public string Farbname { get; set; }
        public string FarbeHex { get; set; }

        public static ReichDefaultData[] Vorbelegung =
        {
                new ReichDefaultData( new string[] { "Kein Reich", "keinreich",  "kein reich" }, "grau", "#808080"),
                new ReichDefaultData( new string[] { "Theostelos", "theostelos" }, "hellblau", "#04D9FF") ,
                new ReichDefaultData( new string[] { "Yaromo", "yaromo" }, "gelb", "#FFFF00") ,
                new ReichDefaultData( new string[] { "USA.", "variedat", "uk d.v.k", "u.s.a.", "unabhängige stämme asimilans" }, "orange","#FFA500") ,
                new ReichDefaultData( new string[] { "Vir Vachal", "virvachal", "vir vachal", "Union von Vir'Vachal und Asimilan", "UVA", "U.V.A." }, "hellgrün", "#90EE90") ,
                new ReichDefaultData( new string[] { "Avallon", "avallon" }, "dunkelgrün","#4a6741") ,
                new ReichDefaultData( new string[] { "Northeim" , "northeim" }, "weiß","FFFFFF" ) ,
                new ReichDefaultData( new string[] { "Helborn" , "helborn" },"schwarz", "000000") ,
                new ReichDefaultData( new string[] { "Eoganachta", "eoganachta", "eoganochta" }, "lila", "#A020F0") ,
                new ReichDefaultData( new string[] { "Piraten", "choson", "piraten", "purple puppy pirates", "ppp"},"blau", "0000FF"),
                new ReichDefaultData( new string[] { "O'Har" , "o'har", "ohar" }, "rot", "FF0000") ,
                new ReichDefaultData( new string[] { "Consortium Commertialis", "consortium commertialis", "consortium", "Konsortium", "gilde" },"gold", "#D4AF37") ,
                new ReichDefaultData( new string[] { "Spielleitung" , "spielleitung", "sl" },"spielleitung", "#FF1493")
            };

    }
}
