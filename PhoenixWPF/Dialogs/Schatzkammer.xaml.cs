﻿using System;
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

namespace PhoenixWPF.Dialogs
{
    /// <summary>
    /// Interaktionslogik für SchatzkammerDialog.xaml
    /// </summary>
    public partial class SchatzkammerDialog : Window
    {
        public SchatzkammerDialog()
        {
            Owner = Application.Current.MainWindow; // Set the owner to the current window
            WindowStartupLocation = WindowStartupLocation.CenterOwner; 
            InitializeComponent();            
        }
    }
}
