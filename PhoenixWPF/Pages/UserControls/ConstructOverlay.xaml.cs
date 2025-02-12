using PhoenixModel.dbErkenfara;
using PhoenixModel.Extensions;
using PhoenixModel.Program;
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
                    buttonBridge.Visibility = KleinfeldView.CanConstructBridge(kf, dir) ? Visibility.Visible : Visibility.Hidden;
                }
                else
                    throw new Exception($"Da fehlt der Button button_bridge_{dir}");

                if (this.FindName($"button_road_{dir}") is Button buttonroad) {
                    buttonroad.Visibility = KleinfeldView.CanConstructRoad(kf, dir) ? Visibility.Visible : Visibility.Hidden;
                }
                else
                    throw new Exception($"Da fehlt der Button button_road_{dir}");

                if (this.FindName($"button_wall_{dir}") is Button buttonWall) {
                    buttonWall.Visibility = KleinfeldView.CanConstructWall(kf, dir) ? Visibility.Visible : Visibility.Hidden;
                }
                else
                    throw new Exception($"Da fehlt der Button button_wall_{dir}");
            }

        }

        public enum ConstructionType {
            Wall,
            Bridge,
            Road
        }


        private void Construction_Button_Click(object sender, RoutedEventArgs e) {
            // Get the button that was clicked
            var button = sender as Button;
            if (button == null)
                return;

            // Extract the construction type and direction from the button's name
            var (constructionType, direction) = GetConstructionInfoFromButtonName(button.Name);

            // Perform the construction logic based on the construction type and direction
            if (constructionType != null && direction != null) {
                Construct(constructionType.Value, direction.Value);
            }
        }

        
        /// <summary>
        /// Method to perform the construction logic based on the construction type and direction
        /// </summary>
        /// <param name="constructionType"></param>
        /// <param name="direction"></param>
        private void Construct(ConstructionType constructionType, Direction direction) {
            switch (constructionType) {
                case ConstructionType.Wall:
                    // Logic to construct a wall in the specified direction
                    MessageBox.Show($"Constructing Wall in direction: {direction}");
                    break;
                case ConstructionType.Bridge:
                    // Logic to construct a bridge in the specified direction
                    MessageBox.Show($"Constructing Bridge in direction: {direction}");
                    break;
                case ConstructionType.Road:
                    // Logic to construct a road in the specified direction
                    MessageBox.Show($"Constructing Road in direction: {direction}");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Helper method to extract the direction from the button name (e.g., "button_wall_NO" -> "NO")
        /// </summary>
        /// <param name="buttonName"></param>
        /// <returns></returns>
        private Direction? GetDirectionFromButtonName(string buttonName) {
            var directionPart = buttonName.Replace("button_wall_", "").Replace("button_bridge_", "").Replace("button_road_", "");
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
        private (ConstructionType?, Direction?) GetConstructionInfoFromButtonName(string buttonName) {
            // Extract the construction type (wall, bridge, road) and direction (e.g., NO, SW) from the button name
            if (buttonName.Contains("button_wall")) {
                var direction = GetDirectionFromButtonName(buttonName);
                return (ConstructionType.Wall, direction);
            }
            if (buttonName.Contains("button_bridge")) {
                var direction = GetDirectionFromButtonName(buttonName);
                return (ConstructionType.Bridge, direction);
            }
            if (buttonName.Contains("button_road")) {
                var direction = GetDirectionFromButtonName(buttonName);
                return (ConstructionType.Road, direction);
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
    }
}
