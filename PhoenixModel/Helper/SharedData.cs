using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static PhoenixModel.Helper.SharedData;

namespace PhoenixModel.Helper
{
    public static class SharedData
    {
        public interface IBlockable
        {
            bool IsBlocked { get; set; }
        }


        public class BlockGuard : IDisposable
        {
            IBlockable? dictionary = null;
            public BlockGuard(IBlockable dic) { 
                dictionary = dic; 
                dictionary.IsBlocked = true;
            }
            public void Dispose() 
            {
                if (dictionary != null) 
                    dictionary.IsBlocked = false;
            }
        }

        public interface IUpdatable
        {
            bool IsUpdated { get; set; }
        }


        public class UpdateGuard : IDisposable
        {
            IUpdatable? dictionary = null;
            public UpdateGuard(IUpdatable dic)
            {
                dictionary = dic;
            }
            public void Dispose()
            {
                if (dictionary != null)
                    dictionary.IsUpdated = true;
            }
        }

        public class BlockingDictionary<Tvalue> : ConcurrentDictionary<string, Tvalue>, IBlockable 
        {
            public BlockingDictionary(int access, int capacity): base(access, capacity) { }

            bool _isAddingCompleted = false;
            bool _isBlocked = false;
            bool _isUpdated = false;

            public BlockingDictionary() { }
            // dann kommwen keine weiteren Elemente dazu
            public void CompleteAdding() { IsAddingCompleted = true; }
            public bool Add(string key, Tvalue obj) { return TryAdd(key, obj); }
            public bool IsAddingCompleted { get => _isAddingCompleted; set => _isAddingCompleted = value; }
            public bool IsBlocked { get => _isBlocked; set => _isBlocked = value; }
            // Elemente haben sich geändert
            public bool IsUpdated { get => _isUpdated; set => _isUpdated = value; }
        }

        // Karte
        public static BlockingDictionary<KleinFeld>? Map = null;
        public static BlockingDictionary<Gebäude>? Gebäude = null; // bauwerkliste

        // PZE
        public static BlockingCollection<Nation>? Nationen = null;
        public static BlockingCollection<Feindaufklaerung>? Feindaufklaerung = null;
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
        public static BlockingCollection<RuestungBauwerke>? RuestungBauwerke = null;
        public static BlockingCollection<RuestungRuestorte>? RuestungRuestorte = null;
        public static BlockingCollection<Schatzkammer>? Schatzkammer = null;
        public static BlockingCollection<Schenkungen>? Schenkungen = null;
        public static BlockingCollection<Schiffe>? Schiffe = null;
        public static BlockingCollection<Units>? Units_Zugdaten = null;
        public static BlockingCollection<Zauberer>? Zauberer = null;

    }
}
