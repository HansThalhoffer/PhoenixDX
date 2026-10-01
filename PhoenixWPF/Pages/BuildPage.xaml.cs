using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using static PhoenixModel.View.RuestplatzView;

namespace PhoenixWPF.Pages {

    /// <summary>
    /// Die Rüstungskammer: Rüstort wählen, Stückzahlen eintragen, rüsten.
    ///
    /// Das Vorbild ist das gleichnamige Fenster der Altanwendung - links die eigenen Rüstorte,
    /// oben die Kapazität mit ihrem Verbrauch und der Reichsschatz vorher und nachher, darunter
    /// die Stückzahlen je Rüstgut.
    ///
    /// <b>Gerüstet wird über den Befehlsweg.</b> Die Seite baut denselben Text, den auch die
    /// Befehlseingabe versteht, lässt ihn vom Parser lesen und führt ihn aus. Damit landet die
    /// Rüstung unter "Aktueller Zug" und lässt sich dort zurücknehmen. Direkt in die
    /// Rüstungstabelle zu schreiben wäre kürzer und würde die Prüfung, das Protokoll und das
    /// Zurücknehmen umgehen.
    ///
    /// Gerechnet wird nichts hier: was ein Korb kostet, was frei ist und ob er erlaubt ist, sagt
    /// <see cref="RuestplatzView"/>. Diese Seite zeigt es an.
    /// </summary>
    public partial class BuildPage : Page {

        private readonly ObservableCollection<Ruestkorbzeile> _korb = [];
        private readonly ObservableCollection<string> _eingestellt = [];

        /// <summary>
        /// Die Einheiten, zu denen sich hinzurüsten lässt. None heisst: ein neues Heer.
        /// </summary>
        private static readonly (string Name, ConstructionElementType Art)[] Zielarten = [
            ("neues Heer", ConstructionElementType.None),
            ("Krieger", ConstructionElementType.K),
            ("Reiter", ConstructionElementType.R),
            ("Schiffe", ConstructionElementType.S),
        ];

        public BuildPage() {
            InitializeComponent();

            foreach (var gut in GetRüstgüter()) {
                var zeile = new Ruestkorbzeile(gut);
                zeile.Geändert += (s, e) => RechneNeu();
                _korb.Add(zeile);
            }
            KorbGrid.ItemsSource = _korb;
            EingestelltListe.ItemsSource = _eingestellt;

            ZielArt.ItemsSource = Zielarten.Select(ziel => ziel.Name).ToList();
            ZielArt.SelectedIndex = 0;

            Loaded += (s, e) => FülleRüstorte();
            ProgramView.OnViewEvent += ViewEventHandler;
        }

        /// <summary>
        /// Die Liste der Rüstorte hängt an den geladenen Daten - sie steht erst, wenn alles da ist,
        /// und ändert sich beim Zugwechsel.
        /// </summary>
        private void ViewEventHandler(object? sender, PhoenixModel.EventsAndArgs.ViewEventArgs e) {
            if (e.EventType != PhoenixModel.EventsAndArgs.ViewEventArgs.ViewEventType.EverythingLoaded
             && e.EventType != PhoenixModel.EventsAndArgs.ViewEventArgs.ViewEventType.UpdateEverything)
                return;
            // Die Ereignisse kommen auch von einem Ladethread; die Liste gehört der Oberfläche.
            if (Dispatcher.CheckAccess() == false)
                Dispatcher.Invoke(FülleRüstorte);
            else
                FülleRüstorte();
        }

        /// <summary>
        /// Eine Zeile der Rüstortliste. Die ListBox bindet an Beschriftung.
        /// </summary>
        private class Ortzeile {
            public required KleinFeld Gemark { get; init; }
            public required string Beschriftung { get; init; }
            public override string ToString() => Beschriftung;
        }

        private void FülleRüstorte() {
            try {
                var vorherigeAuswahl = (RüstortListe.SelectedItem as Ortzeile)?.Gemark.Bezeichner;

                var zeilen = FindeRüstorte()
                    .Select(gemark => new Ortzeile {
                        Gemark = gemark,
                        Beschriftung = $"{gemark.Bezeichner} - {GetÜbersicht(gemark).Rüstortname}",
                    })
                    .ToList();
                RüstortListe.ItemsSource = zeilen;

                // die vorherige Auswahl wiederfinden, damit das Auffrischen nicht herausreisst
                var wieder = zeilen.FirstOrDefault(zeile => zeile.Gemark.Bezeichner == vorherigeAuswahl);
                if (wieder != null)
                    RüstortListe.SelectedItem = wieder;
                else if (zeilen.Count > 0 && RüstortListe.SelectedItem == null)
                    RüstortListe.SelectedIndex = 0;

                Zeige();
            }
            catch (Exception ex) {
                Absturzbericht.Berichte("Die Rüstorte liessen sich nicht auflisten", ex, tödlich: false);
            }
        }

        /// <summary>Die gewählte Gemark, oder null</summary>
        private KleinFeld? GewählterOrt => (RüstortListe.SelectedItem as Ortzeile)?.Gemark;

        /// <summary>Der Korb, wie ihn das Modell versteht</summary>
        private List<Posten> AlsKorb()
            => _korb.Where(zeile => zeile.Anzahl > 0)
                    .Select(zeile => new Posten(zeile.Art, zeile.Anzahl))
                    .ToList();

        /// <summary>Die Einheit, zu der gerüstet wird - None heisst ein neues Heer</summary>
        private (ConstructionElementType Art, int? Nummer) GewähltesZiel() {
            int index = ZielArt.SelectedIndex;
            if (index <= 0 || index >= Zielarten.Length)
                return (ConstructionElementType.None, null);
            if (int.TryParse(ZielNummer.Text?.Trim(), out int nummer) == false || nummer <= 0)
                return (ConstructionElementType.None, null);
            return (Zielarten[index].Art, nummer);
        }

        private void RüstortListe_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var ort = GewählterOrt;
            if (ort != null) {
                // dieselbe Geste wie in der Zugliste: die Karte folgt der Auswahl
                try { Program.Main.Instance.Spiel?.SelectGemark(new KleinfeldPosition(ort.gf, ort.kf)); }
                catch (Exception ex) { SpielWPF.LogWarning("Die Karte folgte der Auswahl nicht", ex.Message); }
            }
            Zeige();
        }

        private void Einstellung_Geändert(object sender, RoutedEventArgs e) => RechneNeu();
        private void Einstellung_Geändert(object sender, TextChangedEventArgs e) => RechneNeu();
        private void Einstellung_Geändert(object sender, SelectionChangedEventArgs e) => RechneNeu();

        /// <summary>
        /// Baut den Kopf neu auf: Ort, Kapazität, Reichsschatz, und was schon eingestellt ist.
        /// </summary>
        private void Zeige() {
            try {
                var ort = GewählterOrt;
                var übersicht = GetÜbersicht(ort, AusBesonderenEinnahmen.IsChecked == true);

                OrtText.Text = ort == null
                    ? "Kein Rüstort gewählt"
                    : $"{ort.Bezeichner} - {übersicht.Rüstortname}";

                PhasenText.Text = ZugView.KannRüsten
                    ? string.Empty
                    : $"In der {ZugView.PhasenBeschreibung} wird nicht gerüstet. Die Rüstphase ist der "
                      + "erste Abschnitt des Zuges; über das Menü \"Zug\" lässt sie sich beenden.";

                BelagerungsText.Text = übersicht.Belagerung != null && übersicht.Belagerung.Besteht
                    ? übersicht.Belagerung.Beschreibung
                    : string.Empty;

                KapaGS.Text = $"{übersicht.Kapazität.Goldstücke:N0}";
                VergebenGS.Text = $"{übersicht.Vergeben.Goldstücke:N0}";
                FreiGS.Text = $"{übersicht.Frei.Goldstücke:N0}";
                KapaHF.Text = $"{übersicht.Kapazität.Heerführer}";
                VergebenHF.Text = $"{übersicht.Vergeben.Heerführer}";
                FreiHF.Text = $"{übersicht.Frei.Heerführer}";
                KapaZ.Text = $"{übersicht.Kapazität.Zauberer}";
                VergebenZ.Text = $"{übersicht.Vergeben.Zauberer}";
                FreiZ.Text = $"{übersicht.Frei.Zauberer}";

                SchatzText.Text = $"Reichsschatz {übersicht.ReichsschatzAlt:N0} GS, "
                    + $"in diesem Zug verrüstet {übersicht.VerrüstetGesamt:N0} GS, "
                    + $"bleiben {übersicht.ReichsschatzNeu:N0} GS."
                    + (übersicht.BesondereEinnahmenHier > 0
                        ? $" Besondere Einnahmen auf dieser Gemark: {übersicht.BesondereEinnahmenHier:N0} GS."
                        : string.Empty);

                _eingestellt.Clear();
                foreach (var rüstung in GetEingestellte(ort))
                    _eingestellt.Add(Beschreibe(rüstung));

                RechneNeu();
            }
            catch (Exception ex) {
                Absturzbericht.Berichte("Die Rüstoberfläche liess sich nicht aufbauen", ex, tödlich: false);
            }
        }

        /// <summary>
        /// Rechnet den Korb durch und sagt, ob er so gerüstet werden kann - bevor jemand drückt.
        /// </summary>
        private void RechneNeu() {
            try {
                var ort = GewählterOrt;
                var korb = AlsKorb();
                bool besondere = AusBesonderenEinnahmen.IsChecked == true;

                var umfang = BerechneKorb(korb);
                SummeText.Text = korb.Count == 0
                    ? "Nichts eingetragen."
                    : $"Zusammen {umfang.Goldstücke:N0} GS, {umfang.Heerführer} Heerführer, {umfang.Zauberer} Zauberer.";

                var (zielart, zielnummer) = GewähltesZiel();
                string befehl = BaueBefehl(korb, ort, zielart, zielnummer);
                BefehlText.Text = befehl;

                if (korb.Count == 0 || ort == null) {
                    HinweisText.Text = string.Empty;
                    RüstenKnopf.IsEnabled = false;
                    return;
                }

                var geprüft = Prüfe(ort, korb, besondere);
                HinweisText.Text = geprüft.HasErrors ? $"{geprüft.Title}: {geprüft.Message}" : geprüft.Title;
                HinweisText.Foreground = geprüft.HasErrors
                    ? System.Windows.Media.Brushes.DarkRed
                    : System.Windows.Media.Brushes.DarkGreen;
                RüstenKnopf.IsEnabled = geprüft.HasErrors == false;
            }
            catch (Exception ex) {
                Absturzbericht.Berichte("Der Rüstauftrag liess sich nicht durchrechnen", ex, tödlich: false);
            }
        }

        /// <summary>
        /// Erzeugt den Befehl und führt ihn aus - denselben Weg, den auch die Befehlseingabe geht.
        /// </summary>
        private void Rüsten_Click(object sender, RoutedEventArgs e) {
            try {
                var ort = GewählterOrt;
                var korb = AlsKorb();
                var (zielart, zielnummer) = GewähltesZiel();
                string befehl = BaueBefehl(korb, ort, zielart, zielnummer);
                if (string.IsNullOrWhiteSpace(befehl))
                    return;

                if (CommandParser.ParseCommand(befehl, out var command) == false || command == null) {
                    SpielWPF.LogError("Der Rüstbefehl liess sich nicht lesen", befehl);
                    return;
                }

                var ergebnis = command.ExecuteCommand();
                if (ergebnis.HasErrors) {
                    SpielWPF.LogError(ergebnis.Title, ergebnis.Message);
                    return;
                }

                SpielWPF.LogInfo(ergebnis.Title, $"{befehl}\r\nZurücknehmen lässt sich das unter \"Aktueller Zug\".");
                Leere();
                Zeige();
            }
            catch (Exception ex) {
                Absturzbericht.Berichte("Das Rüsten ist fehlgeschlagen", ex, tödlich: false);
            }
        }

        private void Leeren_Click(object sender, RoutedEventArgs e) {
            Leere();
            RechneNeu();
        }

        private void Leere() {
            foreach (var zeile in _korb)
                zeile.Anzahl = 0;
            ZielArt.SelectedIndex = 0;
            ZielNummer.Text = string.Empty;
            KorbGrid.Items.Refresh();
        }
    }
}
