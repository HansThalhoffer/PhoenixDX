using PhoenixDX.Structures;
using PhoenixModel.dbErkenfara;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace PhoenixDX.Program {
    /// <summary>
    /// Verwaltet einen Hintergrundprozess zur Verarbeitung einer Warteschlange von KleinFeld-Objekten.
    /// </summary>
    internal class BackgroundUpdater : IDisposable {
        private BackgroundWorker _backgroundWorker;
        private volatile bool _isWorkerRunning;
        ConcurrentQueue<KleinFeld> _updateQueue;
        Welt _weltkarte;

        /// <summary>
        /// Erstellt eine neue Instanz von <see cref="BackgroundUpdater"/>.
        /// </summary>
        /// <param name="weltkarte">Referenz zur Weltkarte, die aktualisiert wird.</param>
        /// <param name="updateQueue">Referenz zur Warteschlange mit den zu verarbeitenden KleinFeld-Objekten.</param>
        public BackgroundUpdater(ref Welt weltkarte, ref ConcurrentQueue<KleinFeld> updateQueue) {
            _updateQueue = updateQueue;
            _weltkarte = weltkarte;
            // Initialisiert den BackgroundWorker
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += ProcessUpdateQueueInBackground;
            _backgroundWorker.RunWorkerCompleted += OnWorkerCompleted;
        }

        /// <summary>
        /// Verarbeitet die Warteschlange von KleinFeld-Objekten im Hintergrund.
        /// </summary>
        /// <param name="sender">Das auslösende Objekt.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void ProcessUpdateQueueInBackground(object sender, DoWorkEventArgs e) {
            while (_updateQueue.TryDequeue(out var gemarkPosition)) {
                // Verarbeitet das entnommene Element
                _weltkarte.UpdateGemark(gemarkPosition);
            }
            _isWorkerRunning = false;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Verarbeitung im Hintergrund abgeschlossen ist.
        /// </summary>
        /// <param name="sender">Das auslösende Objekt.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void OnWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Cancelled == true)
                return;
            // Überprüft, ob noch Elemente vorhanden sind und startet den Worker ggf. neu
            if (!_updateQueue.IsEmpty && !_backgroundWorker.IsBusy) {
                _isWorkerRunning = true;
                _backgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Startet den Worker, falls Elemente in der Warteschlange vorhanden sind und der Worker nicht läuft.
        /// </summary>
        public void Update() {
            if (!_updateQueue.IsEmpty && !_isWorkerRunning) {
                _isWorkerRunning = true;
                _backgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Gibt die Ressourcen des <see cref="BackgroundUpdater"/>-Objekts frei.
        /// </summary>
        public void Dispose() {
            _backgroundWorker.CancelAsync();
        }
    }
}
