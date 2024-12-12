﻿using PhoenixModel.CrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
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
            public bool IsAddingCompleted { get => _isAddingCompleted; set => _isAddingCompleted = value; }
            public bool IsBlocked { get => _isBlocked; set => _isBlocked = value; }
            // Elemente haben sich geändert
            public bool IsUpdated { get => _isUpdated; set => _isUpdated = value; }
        }

        // Karte
        public static BlockingDictionary<Gemark>? Map { get; set; }
        public static BlockingDictionary<Gebäude>? Gebäude { get; set; }

        // PZE
        public static BlockingCollection<Nation>? Nationen { get; set; }

        // crossref
        public static BlockingCollection<Bauwerk>? Bauwerke { get; set; }
        public static BlockingCollection<Rüstort>? Rüstorte { get; set; }
    }
}
