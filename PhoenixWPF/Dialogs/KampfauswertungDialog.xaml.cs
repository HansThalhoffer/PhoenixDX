using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF.Dialogs {

    /// <summary>
    /// Die Kampfauswertung der Spielleitung (Regelwerk Kapitel 5).
    ///
    /// Der Ablauf im Fenster entspricht dem am Tisch: die Zugdaten aller Reiche holen, sehen wo
    /// sich Heere verfeindeter Reiche treffen, für eine Gemark die Würfe eintragen und rechnen
    /// lassen. Der Bericht steht daneben und lässt sich übernehmen oder verwerfen.
    ///
    /// Gewürfelt wird nicht: die Trefferpunkte des Beschusses und der W20 gehören der
    /// Spielleitung und werden eingetragen. Gerechnet wird auf Kopien, solange niemand
    /// "Übernehmen" drückt - siehe <see cref="KampfablaufRules"/>.
    /// </summary>
    public partial class KampfauswertungDialog : Window {

        private readonly ObservableCollection<Kampfauswertung.Heereszeile> _heere = [];
        private List<KonfliktRules.Konflikt> _konflikte = [];
        private KampfablaufRules.Schlachtbericht? _bericht;

        public KampfauswertungDialog() {
            InitializeComponent();
            if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow != this)
                Owner = Application.Current.MainWindow;

            HeereGrid.ItemsSource = _heere;
            ZeigeStand();
            ZeigeKonflikte();
        }

        private void ZeigeStand() {
            if (Spielleitungsdaten.IstGeladen == false) {
                StandLabel.Text = "Noch keine Zugdaten geladen. Für die Auswertung braucht es die "
                    + "Zugdatenbanken aller Reiche eines Zuges - sie liegen im Verzeichnis des Zuges, "
                    + "je Reich eine Datei.";
                return;
            }
            StandLabel.Text = $"Zug {Spielleitungsdaten.Zug}: {Spielleitungsdaten.GeladeneReiche.Count} Reiche geladen "
                + $"({string.Join(", ", Spielleitungsdaten.GeladeneReiche)}).";
        }

        private void ZeigeKonflikte() {
            _konflikte = Kampfauswertung.FindeKonflikte();
            KonfliktListe.ItemsSource = _konflikte.Select(k => k.Beschreibung).ToList();
            HinweisLabel.Text = _konflikte.Count == 0 && Spielleitungsdaten.IstGeladen
                ? "In diesem Zug treffen keine verfeindeten Heere aufeinander."
                : string.Empty;
        }

        /// <summary>
        /// Das Verzeichnis des Zuges auswählen und die Reiche lesen.
        ///
        /// Vorgeschlagen wird das Verzeichnis der eigenen Zugdatenbank - bei der Spielleitung
        /// liegen die Reiche gewöhnlich daneben.
        /// </summary>
        private void LadenButton_Click(object sender, RoutedEventArgs e) {
            var auswahl = new Microsoft.Win32.OpenFolderDialog {
                Title = "Verzeichnis des Zuges auswählen",
                InitialDirectory = VorschlagFürZugverzeichnis() ?? string.Empty,
            };
            if (auswahl.ShowDialog() != true)
                return;

            int zug = ZugView.AktuellerZug.Zug;
            if (int.TryParse(Path.GetFileName(auswahl.FolderName), out int ausDemNamen))
                zug = ausDemNamen;

            var ergebnis = Kampfauswertung.Lade(auswahl.FolderName, zug);
            if (ergebnis.Erfolgreich == false) {
                SpielWPF.LogError(ergebnis.Meldung, ergebnis.Details);
                MessageBox.Show(this, $"{ergebnis.Meldung}\r\n\r\n{ergebnis.Details}", "Kampfauswertung",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SpielWPF.LogInfo(ergebnis.Meldung, ergebnis.Details);
            ZeigeStand();
            ZeigeKonflikte();
        }

        private static string? VorschlagFürZugverzeichnis() {
            string? eigene = Main.Instance?.Settings?.UserSettings?.DatabaseLocationZugdaten;
            return string.IsNullOrEmpty(eigene) ? null : Path.GetDirectoryName(eigene);
        }

        private void KonfliktListe_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _bericht = null;
            BerichtText.Text = string.Empty;
            ÜbernehmenButton.IsEnabled = false;
            KopierenButton.IsEnabled = false;

            var konflikt = AusgewählterKonflikt();
            if (konflikt == null) {
                AngreiferAuswahl.ItemsSource = null;
                _heere.Clear();
                return;
            }

            AngreiferAuswahl.ItemsSource = konflikt.Parteien.Select(partei => partei.Reich.Reich).ToList();
            AngreiferAuswahl.SelectedIndex = 0;
        }

        private void AngreiferAuswahl_SelectionChanged(object sender, SelectionChangedEventArgs e) => BaueHeere();

        private void BaueHeere() {
            _heere.Clear();
            var konflikt = AusgewählterKonflikt();
            if (konflikt == null)
                return;

            Nation? angreifer = AngreiferAuswahl.SelectedItem is string name
                ? NationenView.GetNationFromString(name)
                : null;

            foreach (var zeile in Kampfauswertung.BaueHeereszeilen(konflikt, angreifer))
                _heere.Add(zeile);
        }

        private KonfliktRules.Konflikt? AusgewählterKonflikt() {
            int index = KonfliktListe.SelectedIndex;
            return index >= 0 && index < _konflikte.Count ? _konflikte[index] : null;
        }

        private KleinFeld? AusgewählteGemark() {
            var konflikt = AusgewählterKonflikt();
            return konflikt == null ? null : KleinfeldView.GetKleinfeld(konflikt.Gemark);
        }

        private void BerechnenButton_Click(object sender, RoutedEventArgs e) {
            if (_heere.Count == 0) {
                HinweisLabel.Text = "Für diese Gemark stehen keine Heere zur Auswahl.";
                return;
            }
            if (_heere.All(zeile => zeile.IstAngreifer) || _heere.Any(zeile => zeile.IstAngreifer) == false) {
                HinweisLabel.Text = "Es braucht auf beiden Seiten mindestens ein Heer.";
                return;
            }

            // Die Änderungen im Gitter müssen in die Zeilen, bevor gerechnet wird
            HeereGrid.CommitEdit(DataGridEditingUnit.Row, true);

            _bericht = Kampfauswertung.Werte(AusgewählteGemark(), _heere,
                LiesZahl(TrefferGegenVerteidiger), LiesZahl(TrefferGegenAngreifer));

            BerichtText.Text = _bericht.AlsText();
            ÜbernehmenButton.IsEnabled = true;
            KopierenButton.IsEnabled = true;
            HinweisLabel.Text = "Der Bericht ist gerechnet, aber noch nicht übernommen.";
        }

        private void ÜbernehmenButton_Click(object sender, RoutedEventArgs e) {
            if (_bericht == null)
                return;

            var antwort = MessageBox.Show(this,
                "Die Verluste werden von den Heeren abgezogen und Beute und Kampfeinnahmen dem Sieger "
                + "gutgeschrieben. Das lässt sich nicht zurücknehmen. Übernehmen?",
                "Kampfauswertung", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (antwort != MessageBoxResult.Yes)
                return;

            int verändert = KampfablaufRules.Übernimm(_bericht);
            SpielWPF.LogInfo($"Kampfauswertung {_bericht.Gemark.CreateBezeichner()}: {verändert} Heere verändert",
                _bericht.AlsText());

            ÜbernehmenButton.IsEnabled = false;
            HinweisLabel.Text = $"Übernommen: {verändert} Heere verändert. Gespeichert wird beim Speichern des Zuges.";
            BaueHeere();
        }

        private void KopierenButton_Click(object sender, RoutedEventArgs e) {
            if (_bericht == null)
                return;
            try {
                Clipboard.SetDataObject(_bericht.AlsText(), true);
                HinweisLabel.Text = "Der Bericht steht in der Zwischenablage.";
            }
            catch (Exception ex) {
                SpielWPF.LogError("Der Bericht konnte nicht in die Zwischenablage gelegt werden", ex.Message);
            }
        }

        private static double LiesZahl(TextBox feld)
            => double.TryParse(feld.Text, out double wert) && wert > 0 ? wert : 0;
    }
}
