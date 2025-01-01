using PhoenixModel.Program;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF.Dialogs
{
    /// <summary>
    /// Interaktionslogik für LogDetailDialog.xaml
    /// </summary>
    public partial class LogDetailDialog : Window
    {
        public static LogDetailDialog? Instance { get; private set; }
        bool _showErrorsOnly = false;


        public LogDetailDialog(LogEntry selectedLogEntry, bool showErrorsOnly = false)
        {
            _showErrorsOnly = showErrorsOnly;
            Instance = this;
            InitializeComponent();
            Owner = Application.Current.MainWindow; // Set the owner to the current window
            WindowStartupLocation = WindowStartupLocation.CenterOwner; InitializeComponent();
            LogListFrame.LoadCompleted += LogListFrame_LoadCompleted;
            UpdateDetail(selectedLogEntry);
        }

        private void UpdateDetail(LogEntry selectedLogEntry)
        {
            this.Titel.Text = selectedLogEntry.Titel;
            this.Mesage.Text = selectedLogEntry.Message;
            this.LogType.Text = selectedLogEntry.Type.ToString();
        }

        private void LogListFrame_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Pages.LogPage? page = e.Content as Pages.LogPage;
            if (page != null)
            {
                if (_showErrorsOnly)
                {
                    page.FilterErrors = true;
                    page.FilterInfos= false; 
                    page.FilterWarnings = false;

                }
                page.LogListBox.SelectionChanged += LogListBox_SelectionChanged;
            }
        }

       
        private void LogListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 &&  e.AddedItems[0] is LogEntry selectedLogEntry)
            {
                UpdateDetail(selectedLogEntry);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Instance = null;
            Pages.LogPage? page = LogListFrame.Content as Pages.LogPage;
            if (page != null)
            {
                page.LogListBox.SelectionChanged -= LogListBox_SelectionChanged;
            }
            base.OnClosed(e);
        }
    }
}
