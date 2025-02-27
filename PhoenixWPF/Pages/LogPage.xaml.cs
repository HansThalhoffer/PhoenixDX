﻿using LiveCharts.Wpf;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using PhoenixWPF.Dialogs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF.Pages {
    public partial class LogPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private static ObservableCollection<LogEntry> _logEntries = new();
        private ObservableCollection<LogEntry> _filteredLogEntries = new();

        public ObservableCollection<LogEntry> FilteredLogEntries
        {
            get => _filteredLogEntries;
            private set
            {
                _filteredLogEntries = value;
                OnPropertyChanged(nameof(FilteredLogEntries));
            }
        }

        private bool _filterInfo = false;
        public bool FilterInfos
        {
            get => _filterInfo;
            set
            {
                _filterInfo = value;
                OnPropertyChanged(nameof(FilterInfos));
                UpdateFilteredLogEntries();
            }
        }

        private bool _filterErrors = true;
        public bool FilterErrors
        {
            get => _filterErrors;
            set
            {
                _filterErrors = value;
                OnPropertyChanged(nameof(FilterErrors));
                UpdateFilteredLogEntries();
            }
        }

        private bool _filterWarnings = true;
        public bool FilterWarnings
        {
            get => _filterWarnings;
            set
            {
                _filterWarnings = value;
                OnPropertyChanged(nameof(FilterWarnings));
                UpdateFilteredLogEntries();
            }
        }

        public LogPage()
        {
            InitializeComponent();
            _logEntries.CollectionChanged += _logEntries_CollectionChanged;
            DataContext = this;
            FilteredLogEntries = new ObservableCollection<LogEntry>(_logEntries);
            UpdateFilteredLogEntries();
        }

        private void _logEntries_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFilteredLogEntries();
        }

        public static void AddToLog(LogEntry logentry)
        {
            // Wenn Tests durchgeführt werden, dann die Logs nicht in den Dispatcher schicken, da keine GUI da
            if (Application.Current.GetType() == typeof(System.Windows.Application))
                return;

            if (string.IsNullOrWhiteSpace(logentry.Titel)) 
                return;
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                logentry.Titel = $"{logentry.Type} {logentry.Titel}";
                // es wird ein Fehler angezeigt, die weiteren Nachrichten kommen nicht mehr rein
                _logEntries.Add(logentry);
                if (logentry.Type == LogEntry.LogType.Error)
                {
                    if (LogDetailDialog.Instance == null)
                    {
                        var dialog = new LogDetailDialog(logentry, true);
                        dialog.ShowDialog();
                    }
                }
            });
        }

        /// <summary>
        /// Zeige den Fehlerdialog mit aktuell ausgewähltem Eintrag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LogListBox.SelectedItem != null)
            {
                ExtractAndGoTo(LogListBox.SelectedItem);
            }
            if (LogListBox.SelectedItem is LogEntry selectedLogEntry)
            {
                if (LogDetailDialog.Instance == null)
                {
                    var dialog = new LogDetailDialog(selectedLogEntry);
                    dialog.ShowDialog();
                }                
            }
        }
         
        /// <summary>
        /// Bewege die Karte auf das Feld, sofern Koordinaten in dem Titel genannt sind
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogListBox_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LogListBox.SelectedItem != null)
            {
                ExtractAndGoTo(LogListBox.SelectedItem);
            }
        }

        [GeneratedRegex(@"\[(\d+)/(\d+)\]")]
        public static partial Regex KoordinatenRegex();
        
        public static KleinfeldPosition? ExtractPosition(LogEntry entry)
        {
            // Regex to match the pattern [number1/number2] 
            Regex regex = KoordinatenRegex();
            Match match = regex.Match(entry.Titel);

            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int gf) && int.TryParse(match.Groups[2].Value, out int kf))
                {
                    return new KleinfeldPosition(gf, kf);
                }
            }
            return null;
        }

        private void ExtractAndGoTo(object? input)
        {
            var entry = input as LogEntry;
            if (entry == null)
                return;
            KleinfeldPosition? pos = ExtractPosition(entry);
            if (pos != null && SharedData.Map != null && SharedData.Map.ContainsKey(pos.CreateBezeichner()))
            {
                var kleinfeld = SharedData.Map[pos.CreateBezeichner()];
                Program.Main.Instance.Spiel?.SelectGemark(kleinfeld);
            }
        }

        private void UpdateFilteredLogEntries()
        {
            FilteredLogEntries.Clear();

            foreach (var log in _logEntries)
            {
                if ((FilterInfos && log.Type == LogEntry.LogType.Info) ||
                    (FilterErrors && log.Type == LogEntry.LogType.Error) ||
                    (FilterWarnings && log.Type == LogEntry.LogType.Warning))
                {
                    FilteredLogEntries.Add(log);
                }
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        
    }


    
}
