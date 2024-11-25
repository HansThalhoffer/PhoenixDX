using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Program
{
    using System.ComponentModel;

    public class UserSettings : INotifyPropertyChanged
    {
        private string _databaseLocationKarte;
        private bool _showWindowNavigator;
        private bool _showWindowProperties;
        private bool _showWindowDiplomaty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DatabaseLocationKarte
        {
            get => _databaseLocationKarte;
            set
            {
                if (_databaseLocationKarte != value)
                {
                    _databaseLocationKarte = value;
                    OnPropertyChanged(nameof(DatabaseLocationKarte));
                }
            }
        }

        public bool ShowWindowNavigator
        {
            get => _showWindowNavigator;
            set
            {
                if (_showWindowNavigator != value)
                {
                    _showWindowNavigator = value;
                    OnPropertyChanged(nameof(ShowWindowNavigator));
                }
            }
        }

        public bool ShowWindowProperties
        {
            get => _showWindowProperties;
            set
            {
                if (_showWindowProperties != value)
                {
                    _showWindowProperties = value;
                    OnPropertyChanged(nameof(ShowWindowProperties));
                }
            }
        }

        public bool ShowWindowDiplomaty
        {
            get => _showWindowDiplomaty;
            set
            {
                if (_showWindowDiplomaty != value)
                {
                    _showWindowDiplomaty = value;
                    OnPropertyChanged(nameof(ShowWindowDiplomaty));
                }
            }
        }

        public UserSettings()
        {
            _databaseLocationKarte = "_Data\\Database\\PZE.mdb";
            _showWindowNavigator = true;
            _showWindowProperties = true;
            _showWindowDiplomaty = true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
