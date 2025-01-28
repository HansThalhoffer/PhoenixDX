using PhoenixWPF.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhoenixWPF.Pages.UserControls
{
    /// <summary>
    /// Interaktionslogik für GlassButton.xaml
    /// </summary>
    public partial class GlassButton : UserControl
    {
        public GlassButton()
        {
            InitializeComponent();
            TopButton.MouseLeftButtonDown += Button_MouseLeftButtonDown;
        }

        void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        // Define the Dependency Property for Button Text
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(GlassButton),
                new PropertyMetadata("Button"));

        public string ButtonText {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }


        /// <summary>
        /// Routed Click event definition
        /// </summary>
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GlassButton));

        /// <summary>
        /// Event handler for Click event
        /// </summary>
        public event RoutedEventHandler Click {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }

    }
}
