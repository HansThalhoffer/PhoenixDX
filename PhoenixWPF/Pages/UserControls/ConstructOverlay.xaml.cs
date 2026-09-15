using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Extensions;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;
using SharpDX;
using System.Windows;
using System.Windows.Controls;


namespace PhoenixWPF.Pages.UserControls {
    /// <summary>
    /// Interaktionslogik für ConstructOverlay.xaml
    /// </summary>
    public partial class ConstructOverlay : UserControl {
        public ConstructOverlay() {
            InitializeComponent();
            if (Main.Instance.SelectionHistory != null)
                Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
            Zeige(null);
        }

        /// <summary>
        /// Zu welcher Schaltfläche eine Bauoption gehört: button_road_NO, button_wall_W, ...
        /// </summary>
        private static string Schaltflächenname(BauoptionenView.Bauoption option) {
            string art = option.Art switch {
                ConstructionElementType.Strasse => "road",
                ConstructionElementType.Wall => "wall",
                ConstructionElementType.Bruecke => "bridge",
                ConstructionElementType.Kai => "kai",
                _ => "burg",
            };
            return option.Richtung == null ? "button_burg" : $"button_{art}_{option.Richtung}";
        }

        /// <summary>
        /// Zeigt den Baustern für die ausgewählte Gemark.
        ///
        /// Der Stern bleibt immer stehen - auch wenn nichts geht. Vorher verschwand er in diesem
        /// Fall ganz oder wurde leer, und der Spieler stand ohne Erklärung da. Jetzt steht über ihm,
        /// woran es liegt: keine Gemark ausgewählt, fremdes Gebiet, falsche Phase, oder an jeder
        /// Kante steht schon etwas.
        ///
        /// Was möglich ist, trägt seinen Preis in der Kurzhilfe; was nicht möglich ist, verschwindet
        /// wie bisher, damit der Stern lesbar bleibt - der Grund steht in der Lage darüber.
        /// </summary>
        private void Zeige(KleinFeld? kf) {
            Visibility = Visibility.Visible;

            var lage = BauoptionenView.BeschreibeLage(kf);
            LageText.Text = lage.Title;
            LageText.ToolTip = lage.Message;

            foreach (var option in BauoptionenView.Bestimme(kf)) {
                // Eine fehlende Schaltfläche ist ein Fehler im XAML und nicht im Spiel - gemeldet
                // wird er, aber er darf den Baustern nicht mitreissen.
                if (FindName(Schaltflächenname(option)) is not Button schaltfläche) {
                    SpielWPF.LogError($"Im Baustern fehlt die Schaltfläche {Schaltflächenname(option)}",
                        $"Die Möglichkeit '{option.Bezeichnung}' lässt sich deshalb nicht anzeigen.");
                    continue;
                }
                schaltfläche.Visibility = option.Möglich ? Visibility.Visible : Visibility.Hidden;
                schaltfläche.ToolTip = option.Hinweis;
            }
        }

        private void Construction_Button_Click(object sender, RoutedEventArgs e) {
            // Get the button that was clicked
            var button = sender as Button;
            if (button == null)
                return;

            // Extract the construction type and direction from the button's name
            var (constructionType, direction) = GetConstructionInfoFromButtonName(button.Name);

            // Perform the construction logic based on the construction type and direction
            if (constructionType != null) {
                try {
                    Construct(constructionType.Value, direction);
                }
                catch (Exception ex) {
                    // Ein Knopfdruck darf die Anwendung nicht beenden. Bis zur Ereignisschleife
                    // durchgereicht, täte er genau das - ohne ein Wort.
                    Absturzbericht.Berichte($"Beim Bauen von {constructionType} ging etwas schief", ex, tödlich: false);
                }
            }
        }

        
        /// <summary>
        /// Method to perform the construction logic based on the construction type and direction
        /// </summary>
        /// <param name="constructionType"></param>
        /// <param name="direction"></param>
        private void Construct(ConstructionElementType constructionType, Direction? direction) {
            var selected = Main.Instance.SelectionHistory.Current;
            // wenn ein Kleinfeld ausgewählt ist und es zum Reich des Users gehört, dann kann gebaut werden
            if (selected != null && selected is KleinFeld kf && ProgramView.BelongsToUser(kf)) {
                string commandString = string.Empty;
                switch (constructionType) {
                    case ConstructionElementType.Bruecke:
                        commandString = $"Errichte Brücke im {direction} von {kf.CreateBezeichner()}";
                        break;
                    case ConstructionElementType.Kai:
                        commandString = $"Errichte Kai im {direction} von {kf.CreateBezeichner()}";
                        break;
                    case ConstructionElementType.Strasse:
                        commandString = $"Errichte Straße im {direction} von {kf.CreateBezeichner()}";
                        break;
                    case ConstructionElementType.Wall:
                        commandString = $"Errichte Wall im {direction} von {kf.CreateBezeichner()}";
                        break;
                    case ConstructionElementType.Burg:
                        commandString = $"Errichte Burg auf {kf.CreateBezeichner()}";
                        break;
                    default:
                        break;
                }
                if (string.IsNullOrEmpty(commandString) == false) {
                    if (CommandParser.ParseCommand(commandString, out var cmd) && cmd != null) {
                        var result = cmd.ExecuteCommand();
                        Zeige(kf);
                        if (result.HasErrors)
                            SpielWPF.LogError(result.Title, result.Message);

                    }
                    else
                        SpielWPF.LogError("Der Name konnte nicht gespeichert werden", "Keine Ahnung warum");
                }
            }
        }

        /// <summary>
        /// Helper method to extract the direction from the button name (e.g., "button_wall_NO" -> "NO")
        /// </summary>
        /// <param name="buttonName"></param>
        /// <returns></returns>
        private Direction? GetDirectionFromButtonName(string buttonName) {
            var directionPart = buttonName.Replace("button_wall_", "").Replace("button_bridge_", "").Replace("button_road_", "").Replace("button_kai_", "");
            if (Enum.TryParse(directionPart, out Direction direction)) {
                return direction;
            }
            return null;
        }


        /// <summary>
        /// Helper method to extract construction type and direction from the button name
        /// </summary>
        /// <param name="buttonName"></param>
        /// <returns></returns>
        private (ConstructionElementType?, Direction?) GetConstructionInfoFromButtonName(string buttonName) {
            // Extract the construction type (wall, bridge, road) and direction (e.g., NO, SW) from the button name
            if (buttonName.Contains("button_bridge")) {
                var direction = GetDirectionFromButtonName(buttonName);
                return (ConstructionElementType.Bruecke, direction);
            }
            if (buttonName.Contains("button_kai")) {
                var direction = GetDirectionFromButtonName(buttonName);
                return (ConstructionElementType.Kai, direction);
            }
            if (buttonName.Contains("button_road")) {
                var direction = GetDirectionFromButtonName(buttonName);
                return (ConstructionElementType.Strasse, direction);
            }
            if (buttonName.Contains("button_wall")) {
                var direction = GetDirectionFromButtonName(buttonName);
                return (ConstructionElementType.Wall, direction);
            }

            return (null, null);  // Return null if we couldn't determine the construction type or direction
        }

        /// <summary>
        /// Die Auswahl hat gewechselt.
        ///
        /// Auch eine fremde Gemark wird gezeigt - dann steht über dem Stern, dass sie nicht zum
        /// eigenen Reich gehört. Nur wenn gar keine Gemark ausgewählt ist, etwa weil eine Spielfigur
        /// angeklickt wurde, bleibt die alte Anzeige stehen: der Baustern gehört zur Gemark, und die
        /// hat sich dann nicht geändert.
        /// </summary>
        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (Main.Instance.SelectionHistory.Current is KleinFeld kf)
                Zeige(kf);
        }

        private void button_burg_Click(object sender, RoutedEventArgs e) {
            Construct(ConstructionElementType.Burg, Direction.W);
        }
    }
}
