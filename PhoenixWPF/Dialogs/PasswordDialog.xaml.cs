﻿using PhoenixModel.Database;
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
using System.Windows.Shapes;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Dialogs
{
    /// <summary>
    /// Interaktionslogik für PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        public string? Password { get; private set; }

        public PasswordDialog(string context)
        {
            InitializeComponent();
            PromptLabel.Text = context;
            if (App.Current != null && App.Current.MainWindow != null && App.Current.MainWindow != this)
                Owner = Application.Current.MainWindow; // Set the owner to the current window
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Password = PasswordBox.Password;
            DialogResult = true;
            Close();
        }

        public EncryptedString ProvidePassword()
        {
            return string.IsNullOrEmpty(Password) ? "":Password;
        }

    }
}
