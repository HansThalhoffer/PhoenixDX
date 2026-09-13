using LiveCharts.Wpf;
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

        /// <summary>
        /// Ein Rechtsklick waehlt den Eintrag aus, auf dem er stattfindet. Von sich aus tut eine
        /// ListBox das nicht - dann bezoege sich das Kontextmenue auf den zuletzt linksgeklickten
        /// Eintrag, und kopiert wuerde etwas anderes als das, worauf der Mauszeiger steht.
        /// </summary>
        private void LogListBox_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Der Weg ueber ContainerFromElement statt ueber einen eigenen ItemContainerStyle:
            // ein Stil ohne BasedOn wuerde den ListBoxItem-Stil des Dark-Themes verdraengen.
            if (e.OriginalSource is DependencyObject angeklickt
                && ItemsControl.ContainerFromElement(LogListBox, angeklickt) is ListBoxItem eintrag)
            {
                eintrag.IsSelected = true;
            }
        }

        /// <summary>
        /// Was nichts zu kopieren hat, wird abgeblendet statt wirkungslos angeboten.
        /// </summary>
        private void LogListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (KopiereEintrag != null)
                KopiereEintrag.IsEnabled = LogListBox.SelectedItem is LogEntry;
            if (KopiereAlle != null)
                KopiereAlle.IsEnabled = FilteredLogEntries.Count > 0;
        }

        private void KopiereEintrag_Click(object sender, RoutedEventArgs e)
        {
            if (LogListBox.SelectedItem is LogEntry eintrag)
                KopiereInDieZwischenablage(eintrag.AlsText(), "Der Eintrag");
        }

        private void KopiereAlle_Click(object sender, RoutedEventArgs e)
        {
            KopiereInDieZwischenablage(LogEntry.AlsText(FilteredLogEntries),
                $"Die {FilteredLogEntries.Count} angezeigten Eintraege");
        }

        /// <summary>
        /// Legt Text in die Zwischenablage.
        ///
        /// Die Zwischenablage gehoert dem ganzen System und kann im Moment des Zugriffs einem
        /// anderen Programm gehoeren; dann scheitert das Kopieren mit einer COM-Ausnahme. Das darf
        /// nicht stillschweigend passieren - wer kopiert, fuegt gleich darauf woanders ein und
        /// merkt sonst erst dort, dass nichts angekommen ist.
        /// </summary>
        private static void KopiereInDieZwischenablage(string text, string was)
        {
            if (string.IsNullOrEmpty(text))
                return;
            try
            {
                // true: der Text bleibt auch dann erhalten, wenn die Anwendung beendet wird
                Clipboard.SetDataObject(text, true);
            }
            catch (Exception ex)
            {
                Program.SpielWPF.LogError($"{was} liess sich nicht in die Zwischenablage legen",
                    "Die Zwischenablage wird gerade von einem anderen Programm belegt. "
                    + $"Ein zweiter Versuch hilft meistens.{Environment.NewLine}{ex.Message}");
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
