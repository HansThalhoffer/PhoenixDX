﻿using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.View;
using System.ComponentModel;
using System.Windows.Controls;

namespace PhoenixWPF.Pages {

    /// <summary>
    /// Die Klasse berecchnet die Summenwerte einer Armee und zeigt diese in den Memberproperties an
    /// </summary>
    public class TruppenStatus : INotifyPropertyChanged {
        /// <summary>
        /// Ereignis für PropertyChanged-Benachrichtigungen
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Methode zur Auslösung der PropertyChanged-Benachrichtigung
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// im Konstruktor erfolgt die Berechnung der Summenwerte 
        /// </summary>
        /// <param name="amy"></param>
        public TruppenStatus(Armee amy) {
            /// Army ist eine Ableitung von List<Spielfigur>
            /// jede Spielfigur kann von der Klasse Truppenspielfigur
            foreach (var item in amy) {
                if (item is Kreaturen kreatur) {
                    ProgramView.LogWarning("Kreaturen werden im Truppenstatus nicht erfasst", "Eventuell muss dies noch implementiert werden");
                }
                else if (item is Krieger krieger) {
                    this._krieger += krieger.staerke;
                    this._hfKrieger += krieger.hf;
                    this._hfGesamt += krieger.hf;
                    this._skpKrieger += krieger.SchwereKP;
                    this._lkpKrieger += krieger.LeichteKP;
                    this._pferdeKrieger += krieger.Pferde;
                }
                else if (item is Reiter reiter) {
                    this._reiter += reiter.staerke;
                    this._hfReiten += reiter.hf;
                    this._hfGesamt += reiter.hf;
                    this._skpReiten += reiter.SchwereKP;
                    this._lkpReiten += reiter.LeichteKP;
                    this._pferdeReiten += reiter.Pferde;
                }
                else if (item is Schiffe schiffe) {
                    this._schiffe += schiffe.staerke;
                    this._hfSchiffe += schiffe.hf;
                    this._hfGesamt += schiffe.hf;
                    this._sksSchiffe += schiffe.SchwereKP;
                    this._lkSchiffe += schiffe.LeichteKP;                  
                }
                else if (item is Zauberer wiz){
                    if (SpielfigurenView.GetZaubererklasse(wiz) == Zaubererklasse.ZA) { this._za++; }
                    else if (SpielfigurenView.GetZaubererklasse(wiz) == Zaubererklasse.ZB) { this._zb++; }
                    else if (SpielfigurenView.GetZaubererklasse(wiz) == Zaubererklasse.ZC) { this._zc++; }
                    else if (SpielfigurenView.GetZaubererklasse(wiz) == Zaubererklasse.ZD) { this._zd++; }
                    else if (SpielfigurenView.GetZaubererklasse(wiz) == Zaubererklasse.ZE) { this._ze++; }
                    else if (SpielfigurenView.GetZaubererklasse(wiz) == Zaubererklasse.ZF) { this._zf++; }
                }

            }

        }

        // Eigenschaften für den Abschnitt "Krieg"
        private int _krieger;
        /// <summary>
        /// Die Anzahl der Krieger im Krieg.
        /// </summary>
        public int Krieger {
            get => _krieger;
            set { _krieger = value; OnPropertyChanged(nameof(Krieger)); }
        }

        private int _hfKrieger;
        /// <summary>
        /// Die Anzahl der Hauptführer im Krieg.
        /// </summary>
        public int HF_Krieg {
            get => _hfKrieger;
            set { _hfKrieger = value; OnPropertyChanged(nameof(HF_Krieg)); }
        }

        private int _lkpKrieger;
        /// <summary>
        /// Die Anzahl der Landkommandanten im Krieg.
        /// </summary>
        public int LKP_Krieg {
            get => _lkpKrieger;
            set { _lkpKrieger = value; OnPropertyChanged(nameof(LKP_Krieg)); }
        }

        private int _skpKrieger;
        /// <summary>
        /// Die Anzahl der Spezialkommandanten im Krieg.
        /// </summary>
        public int SK_Krieg {
            get => _skpKrieger;
            set { _skpKrieger = value; OnPropertyChanged(nameof(SK_Krieg)); }
        }

        private int _pferdeKrieger;
        /// <summary>
        /// Die Anzahl der Pferde im Krieg.
        /// </summary>
        public int Pferde_Krieg {
            get => _pferdeKrieger;
            set { _pferdeKrieger = value; OnPropertyChanged(nameof(Pferde_Krieg)); }
        }

        // Eigenschaften für den Abschnitt "Reiten"
        private int _reiter;
        /// <summary>
        /// Die Anzahl der Reiter.
        /// </summary>
        public int Reiter {
            get => _reiter;
            set { _reiter = value; OnPropertyChanged(nameof(Reiter)); }
        }

        private int _hfReiten;
        /// <summary>
        /// Die Anzahl der Hauptführer im Reiten.
        /// </summary>
        public int HF_Reiten {
            get => _hfReiten;
            set { _hfReiten = value; OnPropertyChanged(nameof(HF_Reiten)); }
        }

        private int _lkpReiten;
        /// <summary>
        /// Die Anzahl der Landkommandanten im Reiten.
        /// </summary>
        public int LKP_Reiten {
            get => _lkpReiten;
            set { _lkpReiten = value; OnPropertyChanged(nameof(LKP_Reiten)); }
        }

        private int _skpReiten;
        /// <summary>
        /// Die Anzahl der Spezialkommandanten im Reiten.
        /// </summary>
        public int SKP_Reiten {
            get => _skpReiten;
            set { _skpReiten = value; OnPropertyChanged(nameof(SKP_Reiten)); }
        }

        private int _pferdeReiten;
        /// <summary>
        /// Die Anzahl der Pferde im Reiten.
        /// </summary>
        public int Pferde_Reiten {
            get => _pferdeReiten;
            set { _pferdeReiten = value; OnPropertyChanged(nameof(Pferde_Reiten)); }
        }

        // Eigenschaften für den Abschnitt "Schiffe"
        private int _schiffe;
        /// <summary>
        /// Die Anzahl der Schiffe.
        /// </summary>
        public int Schiffe {
            get => _schiffe;
            set { _schiffe = value; OnPropertyChanged(nameof(Schiffe)); }
        }

        private int _hfSchiffe;
        /// <summary>
        /// Die Anzahl der Hauptführer bei den Schiffen.
        /// </summary>
        public int HF_Schiffe {
            get => _hfSchiffe;
            set { _hfSchiffe = value; OnPropertyChanged(nameof(HF_Schiffe)); }
        }

        private int _lkSchiffe;
        /// <summary>
        /// Die Anzahl der Landkommandanten bei den Schiffen.
        /// </summary>
        public int LK_Schiffe {
            get => _lkSchiffe;
            set { _lkSchiffe = value; OnPropertyChanged(nameof(LK_Schiffe)); }
        }

        private int _sksSchiffe;
        /// <summary>
        /// Die Anzahl der Spezialkommandanten bei den Schiffen.
        /// </summary>
        public int SKS_Schiffe {
            get => _sksSchiffe;
            set { _sksSchiffe = value; OnPropertyChanged(nameof(SKS_Schiffe)); }
        }

        private int _hfGesamt;
        /// <summary>
        /// Die Gesamtzahl der Hauptführer.
        /// </summary>
        public int HF_Gesamt {
            get => _hfGesamt;
            set { _hfGesamt = value; OnPropertyChanged(nameof(HF_Gesamt)); }
        }

        // Eigenschaften für den Abschnitt "Zauberer"
        private int _za;
        /// <summary>
        /// Die Anzahl der Zauberer vom Typ A.
        /// </summary>
        public int ZA {
            get => _za;
            set { _za = value; OnPropertyChanged(nameof(ZA)); }
        }

        private int _zb;
        /// <summary>
        /// Die Anzahl der Zauberer vom Typ B.
        /// </summary>
        public int ZB {
            get => _zb;
            set { _zb = value; OnPropertyChanged(nameof(ZB)); }
        }

        private int _zc = 100;
        /// <summary>
        /// Die Anzahl der Zauberer vom Typ C, standardmäßig auf 100 gesetzt.
        /// </summary>
        public int ZC {
            get => _zc;
            set { _zc = value; OnPropertyChanged(nameof(ZC)); }
        }

        private int _zd;
        /// <summary>
        /// Die Anzahl der Zauberer vom Typ D.
        /// </summary>
        public int ZD {
            get => _zd;
            set { _zd = value; OnPropertyChanged(nameof(ZD)); }
        }

        private int _ze;
        /// <summary>
        /// Die Anzahl der Zauberer vom Typ E.
        /// </summary>
        public int ZE {
            get => _ze;
            set { _ze = value; OnPropertyChanged(nameof(ZE)); }
        }

        private int _zf;
        /// <summary>
        /// Die Anzahl der Zauberer vom Typ F.
        /// </summary>
        public int ZF {
            get => _zf;
            set { _zf = value; OnPropertyChanged(nameof(ZF)); }
        }
    }

    /// <summary>
    /// Interaktionslogik für TruppenStatusPage.xaml
    /// </summary>
    public partial class TruppenStatusPage : Page {
        public TruppenStatus? Status { get; private set; } = null;

        public TruppenStatusPage() {
            InitializeComponent();
            var curArmy = SpielfigurenView.GetSpielfiguren(PhoenixModel.Program.ProgramView.SelectedNation);
            Status = new TruppenStatus(curArmy);
            this.DataContext = Status;
        }
    }
}
