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
    /// Die Klasse kann einfach erweitert werden. Einfach ein neues Property mit Backing Field hinzufügen und den Change Event aufrufen. Fertig.
    /// wenn alles neu gemacht werden soll, dann müssen die Settings gelöscht werden im
    /// Roaming App Data des Benutzers
    /// </summary>
    public class UserSettings : INotifyPropertyChanged
    {
        private string _databaseLocationKarte = string.Empty;
        private string _passwordKarte = string.Empty; 
        private string _databaseLocationPZE = string.Empty;
        private string _passwordPZE = string.Empty;
        private string _defaultValuesReiche = string.Empty;
        private string _databaseLocationCrossRef = string.Empty;
        private string _passwordCrossRef = string.Empty ;
        private string _databaseLocationZugdaten = string.Empty;
        private string _passwordReich = string.Empty;
        private string _databaseLocationFeinde = string.Empty;
        private int _selectedReich = -1;
        private int _selectedZug = -1;

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
                    OnPropertyChanged(nameof(DefaultValuesReiche));
                }
            }
        }

        #region AccessDatabase
        public string DatabaseLocationCrossRef
        {
            get => _databaseLocationCrossRef;
            set
            {
                if (_databaseLocationCrossRef != value)
                {
                    _databaseLocationCrossRef = value;
                    OnPropertyChanged(nameof(DatabaseLocationCrossRef));
                }
            }
        }
        public string PasswordCrossRef
        {
            get => _passwordCrossRef;
            set
            {
                if (_passwordCrossRef != value)
                {
                    _passwordCrossRef = value;
                    OnPropertyChanged(nameof(PasswordCrossRef));
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
        public string PasswordPZE
        {
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

   
        #endregion

        #region Windows
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

        public string PasswordReich 
        { 
            get => _passwordReich; 
            set { 
                _passwordReich = value;
                OnPropertyChanged(nameof(PasswordReich));
            }
        }
        public int SelectedReich 
        { 
            get => _selectedReich;
            set
            {
                _selectedReich = value;
                OnPropertyChanged(nameof(SelectedReich));
            }

        }

        public string DatabaseLocationZugdaten
        {
            get => _databaseLocationZugdaten;
            set
            {
                if (_databaseLocationZugdaten != value)
                {
                    _databaseLocationZugdaten = value;
                    OnPropertyChanged(nameof(DatabaseLocationZugdaten));
                }
            }
        }
        public int SelectedZug
        {
            get => _selectedZug;
            set
            {
                _selectedZug = value;
                OnPropertyChanged(nameof(SelectedZug));
            }

        }
        #endregion


        public UserSettings()
        { }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
