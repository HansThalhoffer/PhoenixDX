using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für TruppenStatusPage.xaml
    /// </summary>
    public partial class TruppenStatusPage : Page {

        public class TruppenStatus: INotifyPropertyChanged {
            // Event for PropertyChanged notification
            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged(string propertyName) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            // Properties for Krieg section
            private int _krieger;
            public int Krieger {
                get => _krieger;
                set { _krieger = value; OnPropertyChanged(nameof(Krieger)); }
            }

            private int _hfKrieg;
            public int HF_Krieg {
                get => _hfKrieg;
                set { _hfKrieg = value; OnPropertyChanged(nameof(HF_Krieg)); }
            }

            private int _lkpKrieg;
            public int LKP_Krieg {
                get => _lkpKrieg;
                set { _lkpKrieg = value; OnPropertyChanged(nameof(LKP_Krieg)); }
            }

            private int _skKrieg;
            public int SK_Krieg {
                get => _skKrieg;
                set { _skKrieg = value; OnPropertyChanged(nameof(SK_Krieg)); }
            }

            private int _pferdeKrieg;
            public int Pferde_Krieg {
                get => _pferdeKrieg;
                set { _pferdeKrieg = value; OnPropertyChanged(nameof(Pferde_Krieg)); }
            }

            // Properties for Reiten section
            private int _reiter;
            public int Reiter {
                get => _reiter;
                set { _reiter = value; OnPropertyChanged(nameof(Reiter)); }
            }

            private int _hfReiten;
            public int HF_Reiten {
                get => _hfReiten;
                set { _hfReiten = value; OnPropertyChanged(nameof(HF_Reiten)); }
            }

            private int _lkpReiten;
            public int LKP_Reiten {
                get => _lkpReiten;
                set { _lkpReiten = value; OnPropertyChanged(nameof(LKP_Reiten)); }
            }

            private int _skpReiten;
            public int SKP_Reiten {
                get => _skpReiten;
                set { _skpReiten = value; OnPropertyChanged(nameof(SKP_Reiten)); }
            }

            private int _pferdeReiten;
            public int Pferde_Reiten {
                get => _pferdeReiten;
                set { _pferdeReiten = value; OnPropertyChanged(nameof(Pferde_Reiten)); }
            }

            // Properties for Schiffe section
            private int _schiffe;
            public int Schiffe {
                get => _schiffe;
                set { _schiffe = value; OnPropertyChanged(nameof(Schiffe)); }
            }

            private int _hfSchiffe;
            public int HF_Schiffe {
                get => _hfSchiffe;
                set { _hfSchiffe = value; OnPropertyChanged(nameof(HF_Schiffe)); }
            }

            private int _lkSchiffe;
            public int LK_Schiffe {
                get => _lkSchiffe;
                set { _lkSchiffe = value; OnPropertyChanged(nameof(LK_Schiffe)); }
            }

            private int _sksSchiffe;
            public int SKS_Schiffe {
                get => _sksSchiffe;
                set { _sksSchiffe = value; OnPropertyChanged(nameof(SKS_Schiffe)); }
            }

            private int _hfGesamt;
            public int HF_Gesamt {
                get => _hfGesamt;
                set { _hfGesamt = value; OnPropertyChanged(nameof(HF_Gesamt)); }
            }

            // Properties for Zauberer section
            private int _za;
            public int ZA {
                get => _za;
                set { _za = value; OnPropertyChanged(nameof(ZA)); }
            }

            private int _zb;
            public int ZB {
                get => _zb;
                set { _zb = value; OnPropertyChanged(nameof(ZB)); }
            }

            private int _zc = 100;
            public int ZC {
                get => _zc;
                set { _zc = value; OnPropertyChanged(nameof(ZC)); }
            }

            private int _zd;
            public int ZD {
                get => _zd;
                set { _zd = value; OnPropertyChanged(nameof(ZD)); }
            }

            private int _ze;
            public int ZE {
                get => _ze;
                set { _ze = value; OnPropertyChanged(nameof(ZE)); }
            }

            private int _zf;
            public int ZF {
                get => _zf;
                set { _zf = value; OnPropertyChanged(nameof(ZF)); }
            }
        }

        public TruppenStatus Status { get; private set; } = new TruppenStatus();

    public TruppenStatusPage() {
            InitializeComponent();
            this.DataContext = Status;
        }
    }
}
