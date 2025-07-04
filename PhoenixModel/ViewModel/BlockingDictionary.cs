﻿using System.Collections.Concurrent;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Eine erweiterte ConcurrentDictionary-Implementierung mit zusätzlichen Steuerungsmechanismen.
    /// </summary>
    /// <typeparam name="Tvalue">Der Typ der gespeicherten Werte.</typeparam>
    public class BlockingDictionary<Tvalue> : ConcurrentDictionary<string, Tvalue> {
        /// <summary>
        /// Konstruktor zur Initialisierung des Dictionaries mit Zugriffseinstellungen.
        /// </summary>
        /// <param name="access">Die Anzahl der zulässigen gleichzeitigen Zugriffe.</param>
        /// <param name="capacity">Die Anfangskapazität des Dictionaries.</param>
        public BlockingDictionary(int access, int capacity) : base(access, capacity) { }

        private bool _isAddingCompleted = false;
        private bool _isBlocked = false;
        private bool _isUpdated = false;

        /// <summary>
        /// Standardkonstruktor.
        /// </summary>
        public BlockingDictionary() { }

        /// <summary>
        /// Setzt das Dictionary in den Modus, in dem keine weiteren Elemente hinzugefügt werden können.
        /// </summary>
        public void CompleteAdding() { IsAddingCompleted = true; }

        /// <summary>
        /// Fügt ein neues Element mit dem angegebenen Schlüssel hinzu.
        /// </summary>
        /// <param name="key">Der Schlüssel des Elements.</param>
        /// <param name="obj">Das hinzuzufügende Objekt.</param>
        /// <returns>Gibt true zurück, wenn das Hinzufügen erfolgreich war.</returns>
        public bool Add(string key, Tvalue obj) { return TryAdd(key, obj); }

        /// <summary>
        /// Gibt an, ob das Hinzufügen von Elementen abgeschlossen ist.
        /// </summary>
        public bool IsAddingCompleted { get => _isAddingCompleted; set => _isAddingCompleted = value; }

        /// <summary>
        /// Gibt an, ob das Dictionary blockiert ist.
        /// </summary>
        public bool IsBlocked { get => _isBlocked; set => _isBlocked = value; }

        /// <summary>
        /// Gibt an, ob sich Elemente im Dictionary geändert haben.
        /// </summary>
        public bool IsUpdated { get => _isUpdated; set => _isUpdated = value; }
    }   
}