﻿using PhoenixModel.Helper;
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

namespace PhoenixWPF.Pages
{
    /// <summary>
    /// Interaktionslogik für PropertyListPage.xaml
    /// </summary>
    public partial class PropertyListPage : Page, IPropertyDisplay
    {
        public PropertyListPage()
        {
            InitializeComponent();
            Main.Instance.PropertyDisplay = this;
        }

        public void Display(List<Eigenschaft> eigenschaften)
        {
            this.PropertyListBox.ItemsSource = eigenschaften;
        }
    }
}
