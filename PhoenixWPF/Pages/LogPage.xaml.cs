using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using PhoenixWPF.Dialogs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF.Pages
{
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
            if (pos != null)
            {
               Program.Main.Instance.Map?.Goto(pos);
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
