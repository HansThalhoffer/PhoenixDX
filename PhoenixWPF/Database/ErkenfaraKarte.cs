using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using System.Data;
using System.Windows;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Program {
    public class ErkenfaraKarte: DatabaseLoader, ILoadableDatabase
    {
        EncryptedString _encryptedpassword;
        string _databaseFileName;
        public EncryptedString Encryptedpassword { get => _encryptedpassword; set => _encryptedpassword = value; }
        public string DatabaseFileName { get => _databaseFileName; set => _databaseFileName = value; }

        public ErkenfaraKarte(string databaseFileName, EncryptedString encryptedpassword)
        {
           _databaseFileName = databaseFileName;
            _encryptedpassword = encryptedpassword;
        }
       
        public void Load()
        {
            PasswordHolder holder = new (_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptedPassword))
            {
                if (connector?.Open() == false)
                    return;
                Load<Gebäude>(connector, ref SharedData.Gebäude, Enum.GetNames(typeof(Gebäude.Felder)));
                Load<KleinFeld>(connector, ref SharedData.Map, Enum.GetNames(typeof(KleinFeld.Felder)));
                connector?.Close();
                // Die Reparatur läuft bewusst hier und nicht nebenher in einem Task.
                //
                // Sie ergänzt fehlende Einträge in SharedData.Gebäude. Lief sie nebenher, wetteiferte
                // sie mit dem Aufbau der Karte: BauwerkeView.GetGebäude meldet jedes Gemark, dessen
                // Eintrag noch fehlt, als Fehler - mit demselben Wortlaut, den die Reparatur als
                // Warnung verwendet. Je nachdem, wer zuerst an das Gemark kam, erschienen die
                // Meldungen beim Start also mal, mal nicht. Es ist eine Schleife über bereits
                // geladene Daten und dauert nicht nennenswert.
                RepairBauwerklistePhase1();
                ProgramView.OnViewEvent += ViewModel_OnViewEvent;
                return;
            }
        } 

        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e)
        {
            if (e.EventType == ViewEventArgs.ViewEventType.EverythingLoaded && SharedData.Gebäude != null && ProgramView.SelectedNation != null)
            {
                Task.Run(() => RepairBauwerklistePhase2());
                ProgramView.OnViewEvent -= ViewModel_OnViewEvent;
            }
        }

        /// <summary>
        /// Die Bauwerke, die Phase 1 in der Bauwerkliste ergänzt hat. Sie warten auf ihr Reich:
        /// das steht erst fest, wenn die Nationen geladen sind, und ohne Reich darf der Eintrag
        /// nicht in die Datenbank. Phase 2 holt das nach.
        /// </summary>
        private readonly List<PhoenixModel.dbErkenfara.Gebäude> _nachgetragen = [];

        /// <summary>
        /// Die Bauwerkliste ist kaputt in der Datenbank. Die Karte ist die gepflegte Mastertabelle und mit dieser Wird die Bauwerkliste korrigiert
        /// </summary>
        private void RepairBauwerklistePhase1()
        {
            if (SharedData.Map != null && SharedData.Gebäude != null)
            {
                _nachgetragen.Clear();
                var gebäudeInKarte = SharedData.Map.Values.Where(gemark => gemark.Baupunkte > 0);
                foreach (var gemark in gebäudeInKarte)
                {
                    Gebäude? gebäude = null;
                    // lookup in Bauwerktabelle
                    if (SharedData.Gebäude.ContainsKey(gemark.Bezeichner))
                        gebäude = SharedData.Gebäude[gemark.Bezeichner];
                    else
                    {
                        // ergänzt den Eintrag falls notwendig - dieselbe Reparatur, die auch der
                        // erste Zugriff auf ein Gemark auslöst, damit es nur eine Stelle dafür gibt
                        gebäude = BauwerkeView.ErgänzeFehlendesGebäude(gemark, stillschweigend: true);
                        if (gebäude != null)
                        {
                            // Geschrieben wird hier noch nichts: das Reich lässt sich zu diesem
                            // Zeitpunkt nicht ermitteln, weil KleinFeld.Nation die Nationen aus der
                            // PZE braucht und die noch nicht geladen ist. Ein Eintrag ohne Reich
                            // wäre unvollständig, deshalb schreibt erst Phase 2.
                            _nachgetragen.Add(gebäude);
                        }
                    }
                    if (gebäude != null)
                        gebäude.Bauwerknamen = gemark.Bauwerknamen;
                }

                // Eine Meldung statt einer je Gemark. Vorher standen beim Start zwei Dutzend
                // gleichlautende Warnungen im Infotab, die jedes Mal wiederkamen - so oft, dass
                // niemand mehr hinsieht.
                if (_nachgetragen.Count > 0)
                    ProgramView.LogInfo($"{_nachgetragen.Count} Gebäude in der Bauwerkliste nachgetragen",
                        $"In der Karte stehen Gebäude, zu denen die Tabelle [bauwerkliste] der Erkenfarakarte.mdb "
                        + $"keinen Eintrag führt: {string.Join(", ", _nachgetragen.Select(gebäude => gebäude.Bezeichner))}.\r\r"
                        + "Die Karte ist die gepflegte Tabelle und gibt den Ausschlag; die Einträge sind für diese "
                        + "Sitzung ergänzt. Geschrieben werden sie, sobald die Reiche geladen sind - ohne Reich wäre "
                        + "der Eintrag unvollständig. Der Spielleitung fehlen sie vermutlich ebenfalls.");
            }
            // Die Reparatur läuft jetzt im Ladevorgang und damit auch dort, wo es gar keine
            // Anwendung mit Oberfläche gibt - etwa im Testlauf.
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgramView.Update(ViewEventArgs.ViewEventType.UpdateGebäude);
            }));
        }

        /// <summary>
        /// Die Bauwerkliste ist kaputt in der Datenbank. In Phase 2 sind alle Daten geladen, somit können die Nationen in der Liste repariert werden
        /// </summary>
        private void RepairBauwerklistePhase2()
        {
            try
            {
                if (SharedData.Map != null && SharedData.Gebäude != null)
                {
                    foreach (var gebäude in SharedData.Gebäude.Values)
                    {
                        // Die Bauwerkliste kann Einträge zu Gemarken führen, die die Karte nicht
                        // kennt. Ein Indexzugriff flöge hier mit einer Ausnahme heraus, und die
                        // sähe niemand: Phase 2 läuft in einem Task.
                        if (SharedData.Map.TryGetValue(gebäude.Bezeichner, out var gemark) == false)
                            continue;

                        if (gemark.Nation != null)
                            gebäude.Reich = gemark.Nation.Reich;
                        if (gemark.Baupunkte == 0)
                        {
                            ProgramView.LogWarning(gemark, $"Zerstörtes Gebäude in der Bauwerktabelle mit dem Namen {gebäude.Bauwerknamen}", $"Durch einen Datenbankfehler existiert das zerstörte Gebäude auf {gebäude.Bezeichner} noch in der Tabelle [bauwerkliste] in der Datenbank Ekrenfarakarte.mdb.\r\rDieser Fehler wurde automatisch korrigiert");
                            gebäude.Zerstört = true;
                        }
                    }
                    SchreibeNachgetrageneBauwerke();
                }
            }
            catch (Exception ex)
            {
                SpielWPF.LogError("Die Bauwerkliste liess sich nicht vollständig reparieren", ex.Message);
            }
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgramView.Update(ViewEventArgs.ViewEventType.UpdateGebäude);
            }));
        }

        /// <summary>
        /// Schreibt die in Phase 1 ergänzten Bauwerke in die Datenbank.
        ///
        /// Erst hier ist das möglich: das Reich steht in der Karte als Nummer und wird über die
        /// Nationen aufgelöst, die beim Laden der Karte noch nicht da sind. Phase 1 hat die
        /// Einträge deshalb nur im Speicher angelegt - und genau daran ist die Reparatur bisher
        /// gescheitert: ohne Reich galt jeder Eintrag als unvollständig, geschrieben wurde keiner,
        /// und beim nächsten Start fehlten sie wieder.
        /// </summary>
        private void SchreibeNachgetrageneBauwerke()
        {
            if (_nachgetragen.Count == 0)
                return;

            var (vollständig, ohneReich) = BauwerkeView.VervollständigeReiche(_nachgetragen);
            foreach (var gebäude in vollständig)
                SharedData.StoreQueue.Insert(gebäude);
            _nachgetragen.Clear();

            if (vollständig.Count > 0)
                ProgramView.LogInfo($"{vollständig.Count} Gebäude werden in der Bauwerkliste nachgetragen",
                    $"Die Einträge zu {string.Join(", ", vollständig.Select(gebäude => gebäude.Bezeichner))} "
                    + "werden in die Tabelle [bauwerkliste] der Erkenfarakarte.mdb geschrieben.\r\r"
                    + "Beim nächsten Start sollte diese Meldung ausbleiben. Der Spielleitung fehlen diese "
                    + "Einträge vermutlich ebenfalls.");

            if (ohneReich.Count > 0)
                ProgramView.LogWarning($"{ohneReich.Count} Gebäude fehlen in der Bauwerkliste und haben kein Reich",
                    $"Zu diesen Gemarken führt die Tabelle [bauwerkliste] keinen Eintrag, und die Karte nennt "
                    + $"auch kein Reich dazu: {string.Join(", ", ohneReich)}.\r\r"
                    + "Sie wurden für diese Sitzung ergänzt, aber nicht in die Datenbank geschrieben - ein "
                    + "Eintrag ohne Reich wäre unvollständig. Hier sollte die Spielleitung nachsehen.");
        }

        public void SchreibeAlle(IEnumerable<PhoenixModel.Database.DatabaseQueue.DatabaseQueueItem> vorgänge) {
            SchreibeAlle(vorgänge, _encryptedpassword, _databaseFileName);
        }

        public void Save(IDatabaseTable table) {
            Save(table, _encryptedpassword, _databaseFileName);
        }

        public void Insert(IDatabaseTable table) {
            Insert(table, _encryptedpassword, _databaseFileName);
        }

        public void Delete(IDatabaseTable table) {
            Delete(table, _encryptedpassword, _databaseFileName);
        }

        public void Dispose()
        {
        }


        public void LoadBackgroundSynchronous() {
            LoadInBackground();
        }

        protected override void LoadInBackground()
        {
            PasswordHolder holder = new(_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptedPassword))
            {
                if (connector?.Open() == false)
                    return;
                try
                {
                    Load<ReichCrossref>(connector, ref SharedData.Diplomatie, Enum.GetNames(typeof(ReichCrossref.Felder)));
                    Load<Zugreihenfolge>(connector, ref SharedData.Zugreihenfolge, Enum.GetNames(typeof(Zugreihenfolge.Felder)));
                }
                catch (Exception ex)
                {
                    SpielWPF.LogError("Fehler beim Laden der Erkenfare Datenbank: " , ex.Message);
                }
                connector?.Close();
            }
        }
    }
}
