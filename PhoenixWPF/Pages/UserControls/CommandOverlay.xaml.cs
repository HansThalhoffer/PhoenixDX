using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.Extensions;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF.Pages.UserControls {
    /// <summary>
    /// Interaktionslogik für CommandOverlay.xaml
    /// </summary>
    public partial class CommandOverlay : UserControl {

        /// <summary>
        /// Die Figur, auf die sich die angezeigten Befehle beziehen
        /// </summary>
        private Spielfigur? _figur = null;

        public CommandOverlay() {
            InitializeComponent();
            Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
            this.Visibility = Visibility.Hidden;
        }

        private void SetSpielfigurVisibility(Spielfigur figur) {
            Visibility = Visibility.Visible;
            buttonShoot.Visibility = figur.CanShoot() ? Visibility.Visible : Visibility.Collapsed;
            buttonBarriere.Visibility = figur.CanCastBarriere() ? Visibility.Visible : Visibility.Collapsed;
            buttonBannen.Visibility = figur.CanCastBannen() ? Visibility.Visible : Visibility.Collapsed;
            buttonTeleport.Visibility = figur.CanCastTeleport() ? Visibility.Visible : Visibility.Collapsed;
            buttonDuell.Visibility = figur.CanCastDuell() ? Visibility.Visible : Visibility.Collapsed;
            buttonFusion.Visibility = figur.CanFustion() ? Visibility.Visible : Visibility.Collapsed;
            buttonSplit.Visibility = figur.CanSplit() ? Visibility.Visible : Visibility.Collapsed;

            buttonHorse.Visibility = Visibility.Collapsed;
            if (figur.CanEmbark()) {
                buttonEmbark.Visibility = Visibility.Visible;
                buttonEmbark.Content = "Einschiffen";
            }
            else if (figur.CanDisEmbark()) {
                buttonEmbark.Visibility = Visibility.Visible;
                buttonEmbark.Content = "Ausschiffen";
            }
            else
                buttonEmbark.Visibility = Visibility.Collapsed;


            if (figur.CanSattleUp()) {
                buttonHorse.Visibility = Visibility.Visible;
                buttonHorse.Content = "Aufsitzen";
            }
            if (figur.CanSattleDown()) {
                buttonHorse.Visibility = Visibility.Visible;
                buttonHorse.Content = "Absitzen";
            }

            SetBewegungVisibility(figur);
        }

        /// <summary>
        /// Zeigt nur die Richtungen an, in die die Figur mit ihren verbleibenden Bewegungspunkten
        /// auch tatsächlich ziehen darf, und blendet die restlichen Bewegungspfeile aus.
        /// </summary>
        private void SetBewegungVisibility(Spielfigur figur) {
            var möglich = BewegungsRules.GetMöglicheSchritte(figur);
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                if (FindName($"button{richtung}") is not Button pfeil)
                    continue;
                if (möglich.TryGetValue(richtung, out var schritt)) {
                    pfeil.Visibility = Visibility.Visible;
                    pfeil.ToolTip = $"{(DirectionNames)richtung} nach {schritt.Ziel?.CreateBezeichner()} ({schritt.Ziel?.Terrain.Name}) - {schritt.BPKosten} BP";
                }
                else {
                    pfeil.Visibility = Visibility.Hidden;
                    pfeil.ToolTip = null;
                }
            }
            textBewegungspunkte.Text = figur.bp_max > 0 ? $"{figur.bp} / {figur.bp_max} BP" : string.Empty;

            // in der Karte den bisherigen Weg und die verbleibende Reichweite zeigen
            BewegungView.ZeigeBewegung(figur);
        }

        /// <summary>
        /// Bewegt die ausgewählte Figur ein Kleinfeld weit in die Richtung des angeklickten Pfeils
        /// </summary>
        private void Bewegung_Button_Click(object sender, RoutedEventArgs e) {
            if (sender is not Button button || _figur == null)
                return;
            string richtungsName = button.Name.Replace("button", string.Empty);
            if (Enum.TryParse(richtungsName, out Direction richtung) == false)
                return;
            Bewege(_figur, richtung);
        }

        /// <summary>
        /// Erzeugt den Bewegungsbefehl, führt ihn aus und aktualisiert die Anzeige
        /// </summary>
        private void Bewege(Spielfigur figur, Direction richtung) {
            var result = Bewegungssteuerung.BewegeInRichtung(figur, richtung);
            if (result.HasErrors)
                SpielWPF.LogError(result.Title, result.Message);
            SetSpielfigurVisibility(figur);
        }

        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var selected = Main.Instance.SelectionHistory.Current;
            if (selected != null && selected is Spielfigur figur) {
                _figur = figur;
                SetSpielfigurVisibility(figur);
                return;
            }
            _figur = null;
            BewegungView.VersteckeBewegung();
            this.Visibility = Visibility.Hidden;
        }
    }
}
