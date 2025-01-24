using PhoenixModel.Commands;
using PhoenixModel.Database;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using System.Collections.Concurrent;

namespace PhoenixModel.ViewModel {

    public static class SharedData {
        /// <summary>
        /// In dieser Queue werden die Objekte abgelegt, die in der Datenbank gespeichert werden sollen. Das geschieht asynchron
        /// </summary>
        public static DatabaseQueue StoreQueue = [];
        public static ConcurrentQueue<KleinFeld> UpdateQueue = [];
        public static BlockingSet<ISelectable, ICommand> Commands = new();

        // Karte
        public static BlockingDictionary<KleinFeld>? Map = null;
        public static BlockingDictionary<Gebäude>? Gebäude = null; // bauwerkliste
        public static BlockingDictionary<ReichCrossref>? Diplomatie = null;

        // PZE
        public static BlockingCollection<Nation>? Nationen = null;
        // ist nicht aktuell public static BlockingCollection<Feindaufklaerung>? Feindaufklaerung = null;
        public static BlockingCollection<Handout>? Handout = null;
        public static BlockingCollection<Infolog>? Infolog = null;

        // crossref
        // public static BlockingDictionary<Gebäude>? BauwerkeRef= null; // kosten für wälle etc - steht aber auch in der Kosten Tabelle
        public static BlockingCollection<Rüstort>? RüstortReferenz = null;
        public static BlockingCollection<BEW_chars>? BEW_chars = null;
        public static BlockingCollection<BEW_Kreaturen>? BEW_Kreaturen = null;
        public static BlockingCollection<BEW_Krieger>? BEW_Krieger = null;
        public static BlockingCollection<BEW_LKP>? BEW_LKP = null;
        public static BlockingCollection<BEW_LKS>? BEW_LKS = null;
        public static BlockingCollection<BEW_PiratenChars>? BEW_PiratenChars = null;
        public static BlockingCollection<BEW_PiratenLKS>? BEW_PiratenLKS = null;
        public static BlockingCollection<BEW_PiratenSchiffe>? BEW_PiratenSchiffe = null;
        public static BlockingCollection<BEW_PiratenSKS>? BEW_PiratenSKS = null;
        public static BlockingCollection<BEW_Reiter>? BEW_Reiter = null;
        public static BlockingCollection<BEW_SKP>? BEW_SKP = null;
        public static BlockingCollection<BEW_SKS>? BEW_SKS = null;
        public static BlockingCollection<Kosten>? Kosten = null;
        public static BlockingCollection<Teleportpunkte>? Teleportpunkte = null;
        public static BlockingCollection<Units_crossref>? Units_crossref = null;
        public static BlockingCollection<Wall_crossref>? Wall_crossref = null;
        public static BlockingCollection<Crossref_zauberer_teleport>? Crossref_zauberer_teleport = null;

        // Zugdaten
        public static BlockingCollection<BilanzEinnahmen>? BilanzEinnahmen_Zugdaten = null;
        public static BlockingCollection<Character>? Character = null;
        public static BlockingCollection<Diplomatiechange>? Diplomatiechange = null;
        public static BlockingCollection<Kreaturen>? Kreaturen = null;
        public static BlockingCollection<Krieger>? Krieger = null;
        public static BlockingCollection<Lehensvergabe>? Lehensvergabe = null;
        // public static BlockingCollection<Personal>? Personal = null;
        public static BlockingCollection<Reiter>? Reiter = null;
        public static BlockingCollection<Ruestung>? Ruestung = null;
        public static BlockingCollection<RuestungBauwerke>? RuestungBauwerke = null;
        public static BlockingCollection<RuestungRuestorte>? RuestungRuestorte = null;
        public static BlockingCollection<Schatzkammer>? Schatzkammer = null;
        public static BlockingCollection<Schenkungen>? Schenkungen = null;
        public static BlockingCollection<Schiffe>? Schiffe = null;
        public static BlockingCollection<Units>? Units_Zugdaten = null;
        public static BlockingCollection<Zauberer>? Zauberer = null;
        public static BlockingCollection<ZugdatenSettings>? ZugdatenSettings = null;   

        // Feinderkennung - aus der Textdatei
        public static BlockingCollection<Feinde>? Feinde = null;
    }
}
