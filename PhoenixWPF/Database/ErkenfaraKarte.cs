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
        /// Die Bauwerkliste ist kaputt in der Datenbank. Die Karte ist die gepflegte Mastertabelle und mit dieser Wird die Bauwerkliste korrigiert
        /// </summary>
        private void RepairBauwerklistePhase1()
        {
            if (SharedData.Map != null && SharedData.Gebäude != null)
            {
                List<string> ergänzt = [];
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
                            ergänzt.Add(gemark.Bezeichner);
                    }
                    if (gebäude != null)
                        gebäude.Bauwerknamen = gemark.Bauwerknamen;
                }

                // Eine Meldung statt einer je Gemark. Vorher standen beim Start zwei Dutzend
                // gleichlautende Warnungen im Infotab, die jedes Mal wiederkamen - so oft, dass
                // niemand mehr hinsieht.
                if (ergänzt.Count > 0)
                    ProgramView.LogWarning($"{ergänzt.Count} Gebäude fehlen in der Bauwerkliste",
                        $"In der Karte stehen Gebäude, zu denen die Tabelle [bauwerkliste] der Erkenfarakarte.mdb "
                        + $"keinen Eintrag führt: {string.Join(", ", ergänzt)}.\r\r"
                        + "Die Karte ist die gepflegte Tabelle und gibt den Ausschlag; die Einträge wurden für "
                        + "diese Sitzung ergänzt. In der Datenbank fehlen sie weiterhin, deshalb kommt diese "
                        + "Meldung bei jedem Start wieder.");
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
            if (SharedData.Map != null && SharedData.Gebäude != null)
            {
                foreach (var gebäude in SharedData.Gebäude.Values)
                {
                    var gemark = SharedData.Map[gebäude.Bezeichner];
                    
                    if (gemark.Nation != null)
                        gebäude.Reich = gemark.Nation.Reich;
                    if (gemark.Baupunkte == 0)
                    {
                        ProgramView.LogWarning(gemark, $"Zerstörtes Gebäude in der Bauwerktabelle mit dem Namen {gebäude.Bauwerknamen}", $"Durch einen Datenbankfehler existiert das zerstörte Gebäude auf {gebäude.Bezeichner} noch in der Tabelle [bauwerkliste] in der Datenbank Ekrenfarakarte.mdb.\r\rDieser Fehler wurde automatisch korrigiert");
                        gebäude.Zerstört = true;
                    }
                }
            }
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgramView.Update(ViewEventArgs.ViewEventType.UpdateGebäude);
            }));
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
