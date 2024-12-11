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
                new ReichDefaultData( new string[] { "Theostelos", "theostelos" }, "hellblau", Color.LightBlue) ,
                new ReichDefaultData( new string[] { "Yaromo", "yaromo" }, "gelb", Color.Yellow) ,
                new ReichDefaultData( new string[] { "USA.", "variedat", "uk d.v.k", "u.s.a.", "unabhängige stämme asimilans" }, "orange", Color.Orange) ,
                new ReichDefaultData( new string[] { "Vir Vachal", "virvachal", "vir vachal", "Union von Vir'Vachal und Asimilan", "UVA", "U.V.A." }, "hellgrün", Color.LightGreen) ,
                new ReichDefaultData( new string[] { "Avallon", "avallon" }, "dunkelgrün", Color.DarkGreen) ,
                new ReichDefaultData( new string[] { "Northeim" , "northeim" }, "weiß", Color.White) ,
                new ReichDefaultData( new string[] { "Helborn" , "helborn" },"schwarz", Color.Black) ,
                new ReichDefaultData( new string[] { "Eoganachta", "eoganachta", "eoganochta" }, "lila", Color.Magenta) ,
                new ReichDefaultData( new string[] { "Piraten", "choson", "piraten", "purple puppy pirates", "ppp"}, "blau", Color.Blue ),
                new ReichDefaultData( new string[] { "O'Har" , "o'har", "ohar" }, "rot", Color.Red) ,
                new ReichDefaultData( new string[] { "Consortium Commertialis", "consortium commertialis", "consortium", "Konsortium", "gilde" },"gold", Color.Gold) ,
                new ReichDefaultData( new string[] { "Spielleitung" , "spielleitung", "sl" },"spielleitung", Color.DeepPink)
            };

    }
}
