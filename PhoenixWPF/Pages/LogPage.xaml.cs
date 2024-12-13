﻿using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static PhoenixModel.Program.LogEntry;

namespace PhoenixWPF.Pages
{
    /// <summary>
    /// Interaktionslogik für Log.xaml
    /// </summary>
    public partial class LogPage : Page
    {
        // ObservableCollection for thread-safe data binding to ListBox
        private static ObservableCollection<string> _logEntries = new ObservableCollection<string>();

        // Lock object to ensure thread-safety when adding log entries
        private static readonly object _logLock = new object();

        public LogPage()
        {
            InitializeComponent();

            // Set the data context for ListBox to bind to the log entries
            LogListBox.ItemsSource = _logEntries;
        }

        private void LogListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LogListBox.SelectedItem != null)
            {
                string selectedText = LogListBox.SelectedItem.ToString();
                ExtractAndGoTo(selectedText);
            }
        }

        private void ExtractAndGoTo(string input)
        {
            // Regex to match the pattern [number1/number2] 
            Regex regex = new Regex(@"\[(\d+)/(\d+)\]");
            Match match = regex.Match(input);

            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int gf) && int.TryParse(match.Groups[2].Value, out int kf))
                {
                    Program.Main.Instance.Map?.Goto(gf, kf);
                }                
            }          
        }
    

        /// <summary>
        /// Static method to add a message to the log from any thread.
        /// </summary>
        /// <param name="message">The message to be added to the log.</param>
        public static void AddToLog(LogEntry logentry)
        {
            if (string.IsNullOrWhiteSpace(logentry.Message)) return;

            // Ensure the log entries are updated on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_logLock)
                {
                    _logEntries.Add($"{logentry.Type.ToString()} {logentry.Message}");

                    // Optionally limit log size to avoid memory overuse
                    if (_logEntries.Count > 1000) // Keep last 1000 entries
                    {
                        _logEntries.RemoveAt(0);
                    }
                }
            });
        }
    }
}
