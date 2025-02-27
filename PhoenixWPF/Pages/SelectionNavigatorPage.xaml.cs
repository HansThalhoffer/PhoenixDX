﻿using PhoenixModel.dbZugdaten;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
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

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für SelectionNavigatorPage.xaml
    /// </summary>
    public partial class SelectionNavigatorPage : Page
    {
        public SelectionNavigatorPage()
        {
            InitializeComponent();
            Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
        }

        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Back.IsEnabled = Main.Instance.SelectionHistory.CanNavigateBack();
            Forward.IsEnabled = Main.Instance.SelectionHistory.CanNavigateForward();
            Display.Text = Main.Instance.SelectionHistory.CurrentDisplay;
            var marked = KleinfeldView.GetMarked(MarkerType.User);
            if (marked != null && marked.Count() > 0) {
                Mehrfachauswahl.Text = $"{marked.Count()} Kleinfelder ausgewählt";
            }
            else
                Mehrfachauswahl.Text = string.Empty;
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Main.Instance.SelectionHistory.NavigateBack();
            if (Main.Instance.SelectionHistory.Current is KleinfeldPosition pos)
                Program.Main.Instance.Spiel?.SelectGemark(pos);
        }

        private void ForwardButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Main.Instance.SelectionHistory.NavigateForward();
            if (Main.Instance.SelectionHistory.Current is KleinfeldPosition pos)
                Program.Main.Instance.Spiel?.SelectGemark(pos);
        }
    }
}
