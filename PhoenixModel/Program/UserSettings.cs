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
        private bool _showWindowDiplomacy;
        private string _passworPZE;

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

        public bool ShowWindowDiplomacy
        {
            get => _showWindowDiplomacy;
            set
            {
                if (_showWindowDiplomacy != value)
                {
                    _showWindowDiplomacy = value;
                    OnPropertyChanged(nameof(ShowWindowDiplomacy));
                }
            }
        }

        public string PassworPZE { 
            get => _passworPZE; 
            set
            {
                if (_passworPZE != value)
                {
                    _passworPZE = value;
                    OnPropertyChanged(nameof(PassworPZE));
                }
            }
        }

        public UserSettings()
        {
            _databaseLocationKarte = "_Data\\Kartendaten\\Erkenfarakarte.mdb";
            _showWindowNavigator = true;
            _showWindowProperties = true;
            _showWindowDiplomacy = true;
            _passworPZE = string.Empty;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
