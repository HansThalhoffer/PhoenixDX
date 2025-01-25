using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using PhoenixWPF.Pages.Converter;
using PhoenixWPF.Program;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für DiplomatiePage.xaml
    /// </summary>
    public partial class DiplomatiePage : Page
    {
        public DiplomatiePage()
        {
            InitializeComponent();
            ProgramView.OnViewEvent += ViewModel_OnViewEvent;
        }

        private void ViewModel_OnViewEvent(object? sender, ViewEventArgs e)
        {
            if (SharedData.Diplomatiechange != null && ProgramView.SelectedNation != null &&
                (e.EventType == ViewEventArgs.ViewEventType.UpdateDiplomatie))
            {
                EigenschaftlerList.Clear();
                var list = SharedData.Diplomatiechange;
                if (list != null)
                    EigenschaftlerList.AddRange(list);
                LoadEigenschaftler();
            }
        }
        public List<IEigenschaftler> EigenschaftlerList { get; set; } = [];


        public void LoadEigenschaftler()
        {

            if (EigenschaftlerList == null || EigenschaftlerList.Count == 0)
                return;
            string[] toIgnore = { };
            // string[] toIgnore = { };
            List<Eigenschaft> eigList = EigenschaftlerList[0].Eigenschaften;
            List<Eigenschaft> columns = eigList.Where(prop => !toIgnore.Contains(prop.Name)).ToList();

            EigenschaftlerDataGrid.Columns.Clear();
            // Add dynamic columns for Eigenschaften
            foreach (var eig in columns)
            {
                string name = eig.Name;
                int index = eigList.IndexOf(eig);
                DataGridColumn? column = null;
                if (name.EndsWith("recht") || name.EndsWith("recht_von"))
                {
                    var templColumn = new DataGridTemplateColumn
                    {
                        Header = name,
                        IsReadOnly = false
                    };
                    column = templColumn;

                    // hier wird ein Datatemplate mit einer Checkbox per Factory gestrickt
                    var cellTemplate = new DataTemplate();
                    var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
                    checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, new Binding(name) //  new Binding($"Eigenschaften[{index}].Wert") 
                    {
                        Converter = new IntToBoolConverter(),
                        Mode = BindingMode.TwoWay,
                    });
                    checkBoxFactory.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(CheckBox_UnChecked));
                    checkBoxFactory.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(CheckBox_UnChecked));
                    checkBoxFactory.SetValue(CheckBox.IsEnabledProperty, false);
                    checkBoxFactory.SetValue(CheckBox.TagProperty, name); // wird nicht verwendet, ist für Debug aber nützlich
                    checkBoxFactory.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    checkBoxFactory.SetBinding(CheckBox.IsEnabledProperty, new Binding(".")
                    {
                        Converter = new AutorizedConverter()
                    });
                    cellTemplate.VisualTree = checkBoxFactory;
                    templColumn.CellTemplate = cellTemplate;
                }
                else
                {
                    column = new DataGridTextColumn
                    {
                        Header = name,
                        Binding = new System.Windows.Data.Binding(name),
                        IsReadOnly = true
                    };
                }
                if (column != null)
                    EigenschaftlerDataGrid.Columns.Add(column);
            }
            // jetzt noch die Datenquelle setzen und los geht's
            EigenschaftlerDataGrid.ItemsSource = EigenschaftlerList;
        }


        /// <summary>
        /// hier werden die checked und unchecked Events der checkboxen gefangen
        /// erhaltene Rechte dürfen nicht geändert werden, der ursprüngliche wert kommt wieder in die Checkbox
        /// Werte des eigenen Reichs darf man vergeben und die werden dann gespeichert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                var bindingExpression = checkBox.GetBindingExpression(CheckBox.IsCheckedProperty);

                // Access the DataContext of the CheckBox (e.g., the row's data item)
                if (checkBox.DataContext is ReichCrossref crossref && bindingExpression != null)
                {
                    var propertyName = bindingExpression.ParentBinding.Path.Path;
                    if (string.IsNullOrEmpty(propertyName) == false)
                    {
                        bool oldValue = PropertyProcessor.GetIntValueIfExists(crossref, propertyName) > 0;
                        bool newValue = ! e.RoutedEvent.Name.ToLower().StartsWith("un");
                        if (oldValue == newValue) // nothing to do here
                            return;
                        if (propertyName.EndsWith("von"))
                        {
                            checkBox.IsChecked = oldValue;
                        }
                        else
                        {
                            string verb = newValue ? "Gebe" : "Entziehe";
                            string commandString = $"{verb} {crossref.DBname} {propertyName}";
                            bool isParsed = CommandParser.ParseCommand(commandString, out var command);
                            if (isParsed && command != null) {
                                var result = command.ExecuteCommand();
                                if (result.HasErrors) {
                                    SpielWPF.LogError(result.Title, result.Message);
                                    checkBox.IsChecked = oldValue;
                                }
                            }
                            else
                                SpielWPF.LogError("Das DiplomacyCommando wurde nicht von der Maschine verstanden", $"Kommando: {commandString}");
                        }
                    }

                }
            }
        }

        private void EigenschaftlerDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.DataContext is ReichCrossref vertrag)
                {
                    // Get the binding path of the edited column
                    if (e.Column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding)
                    {
                        string propertyName = binding.Path.Path;
                        /*if (propertyName == _BauwerkNamenBindingPath)
                        {
                            if (e.EditingElement is TextBox tb)
                            {
                                SaveBauwerknamen(gebäude, tb.Text);
                            }
                        }*/
                    }
                }
            }
        }
    }
}
