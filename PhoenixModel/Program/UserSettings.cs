using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Program
{
    using System.ComponentModel;

    /// <summary>
    /// die Settings werden automatisch gespeichert, wenn Änderungen stattfinden.
    /// wenn alles neu gemacht werden soll, dann müssen die Settings gelöscht werden im
    /// Roaming App Data des Benutzers
    /// </summary>
    public class UserSettings : INotifyPropertyChanged
    {
        private string _databaseLocationKarte = "_Data\\Kartendaten\\Erkenfarakarte.mdb";
        private string _passwordKarte = string.Empty; 
        private string _databaseLocationPZE = "_Data\\Database\\PZE.mdb";
        private string _passwordPZE = string.Empty;
        private string _defaultValuesReiche = "_Data\\EinstellungenReiche.txt";
        private bool _showWindowNavigator = true;
        private bool _showWindowProperties = true;
        private bool _showWindowDiplomacy = true;
        

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DefaultValuesReiche
        {
            get => _defaultValuesReiche;
            set
            {
                if (_defaultValuesReiche != value)
                {
                    _defaultValuesReiche = value;
                    OnPropertyChanged(nameof(DatabaseLocationKarte));
                }
            }
        }

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

        public string DatabaseLocationPZE
        {
            get => _databaseLocationPZE;
            set
            {
                if (_databaseLocationPZE != value)
                {
                    _databaseLocationPZE = value;
                    OnPropertyChanged(nameof(DatabaseLocationPZE));
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

        public string PasswordPZE { 
            get => _passwordPZE; 
            set
            {
                if (_passwordPZE != value)
                {
                    _passwordPZE = value;
                    OnPropertyChanged(nameof(PasswordPZE));
                }
            }
        }

        public string PasswordKarte
        {
            get => _passwordKarte;
            set
            {
                if (_passwordKarte != value)
                {
                    _passwordKarte = value;
                    OnPropertyChanged(nameof(PasswordKarte));
                }
            }
        }

        public UserSettings()
        { }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
