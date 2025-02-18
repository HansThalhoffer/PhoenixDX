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
            Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
            // this.Visibility = Visibility.Hidden;
        }

        private void SetKleinfeldVisibility(KleinFeld kf) {
            Visibility = Visibility.Visible;
            foreach (Direction dir in Enum.GetValues(typeof(Direction))) {
                if (this.FindName($"button_bridge_{dir}") is Button buttonBridge) {
                    buttonBridge.Visibility = ConstructRules.CanConstructBridge(kf, dir) ? Visibility.Visible : Visibility.Hidden;
                }
                else
                    throw new Exception($"Da fehlt der Button button_bridge_{dir}");

                if (this.FindName($"button_kai_{dir}") is Button buttonKai) {
                    buttonKai.Visibility = ConstructRules.CanConstructKai(kf, dir) ? Visibility.Visible : Visibility.Hidden;
                }
                else
                    throw new Exception($"Da fehlt der Button button_kai_{dir}");

                if (this.FindName($"button_road_{dir}") is Button buttonRoad) {
                    buttonRoad.Visibility = ConstructRules.CanConstructRoad(kf, dir) ? Visibility.Visible : Visibility.Hidden;
                }
                else
                    throw new Exception($"Da fehlt der Button button_road_{dir}");

                if (this.FindName($"button_wall_{dir}") is Button buttonWall) {
                    buttonWall.Visibility = ConstructRules.CanConstructWall(kf, dir) ? Visibility.Visible : Visibility.Hidden;
                }
                else
                    throw new Exception($"Da fehlt der Button button_wall_{dir}");
            }
            button_burg.Visibility = ConstructRules.CanConstructCastle(kf) ? Visibility.Visible : Visibility.Hidden;
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
                Construct(constructionType.Value, direction);
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

        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var selected = Main.Instance.SelectionHistory.Current;
            // wenn ein Kleinfeld ausgewählt ist und es zum Reich des Users gehört, dann kann gebaut werden
            if (selected != null && selected is KleinFeld kf && ProgramView.BelongsToUser(kf)) {
                SetKleinfeldVisibility(kf);
                return;
            }
            this.Visibility = Visibility.Hidden;
        }

        private void button_burg_Click(object sender, RoutedEventArgs e) {
            Construct(ConstructionElementType.Burg, Direction.W);
        }
    }
}
