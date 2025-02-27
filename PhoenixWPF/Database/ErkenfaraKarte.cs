﻿using PhoenixModel.Database;
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
                Task.Run(() => RepairBauwerklistePhase1());
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
                var gebäudeInKarte = SharedData.Map.Values.Where(gemark => gemark.Baupunkte > 0);
                foreach (var gemark in gebäudeInKarte)
                {
                    Gebäude? gebäude = null;
                    // lookup in Bauwerktabelle
                    if (SharedData.Gebäude.ContainsKey(gemark.Bezeichner))
                        gebäude = SharedData.Gebäude[gemark.Bezeichner];
                    // ergänzt die Datenbank falls notwendig
                    if (gebäude == null)
                    {
                        ProgramView.LogWarning(gemark, $"Fehlendes Gebäude in der Bauwerktabelle mit dem Namen {gemark.Bauwerknamen}", $"Durch einen Datenbankfehler hat das Gebäude auf {gemark.Bezeichner} keinen Eintrag in der Tabelle [bauwerkliste] in der Datenbank Ekrenfarakarte.mdb.\r\rDieser Fehler wurde automatisch korrigiert");
                        gebäude = new Gebäude();
                        gebäude.kf = gemark.kf;
                        gebäude.gf = gemark.gf;
                        SharedData.Gebäude.Add(gebäude.Bezeichner, gebäude);
                    }
                    gebäude.Bauwerknamen = gemark.Bauwerknamen;
                }
            }
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
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
