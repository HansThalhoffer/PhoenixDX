using Microsoft.Xna.Framework;
using PhoenixDX.Structures;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixDX.Program {
    public class BackgroundUpdater: IDisposable
    {
        private BackgroundWorker _backgroundWorker;
        private volatile bool _isWorkerRunning;
        ConcurrentQueue<KleinfeldPosition> _updateQueue;
        Welt _weltkarte;
        public BackgroundUpdater(ref Welt weltkarte, ref ConcurrentQueue<KleinfeldPosition> updateQueue)
        {
            _updateQueue =  updateQueue;
            _weltkarte = weltkarte;
            // Set up the BackgroundWorker
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += ProcessUpdateQueueInBackground;
            _backgroundWorker.RunWorkerCompleted += OnWorkerCompleted;
        }

        private void ProcessUpdateQueueInBackground(object sender, DoWorkEventArgs e)
        {
            while (_updateQueue.TryDequeue(out var gemarkPosition))
            {
                // Process the dequeued item
                _weltkarte.UpdateGemark(gemarkPosition);
            }
            _isWorkerRunning = false;
        }

        private void OnWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
                return;
            // Check if there are still items to process and restart the worker if needed
            if (!_updateQueue.IsEmpty && !_backgroundWorker.IsBusy)
            {
                _isWorkerRunning = true;
                _backgroundWorker.RunWorkerAsync();
            }
        }

        public void Update()
        {
            // Check if the update queue has items and start the worker if not running
            if (!_updateQueue.IsEmpty && !_isWorkerRunning)
            {
                _isWorkerRunning = true;
                _backgroundWorker.RunWorkerAsync();
            }
        }

        public void Dispose()
        {
            _backgroundWorker.CancelAsync();
        }
    }
}
