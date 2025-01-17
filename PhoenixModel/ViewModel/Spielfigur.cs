using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Die Klasse dient als Basis für alle Spielfiguren in diesem Spiel
    /// Sie hat zwei Spezialisierungen: <see cref="NamensSpielfigur"/> und <see cref="TruppenSpielfigur"/>.
    /// </summary>
    public abstract class Spielfigur : KleinfeldPosition, ISelectable {
        /// <summary>
        /// Jede Figur sollte einen Typ haben, um die Darstellung zu vereinfachen
        /// Der Basistyp entspricht der Klasse
        /// </summary>
        public abstract FigurType BaseTyp { get; }
        /// <summary>
        /// Ein Typ kann sich vom BasisTyp unterscheiden, wenn bestimmte Werte gesetzt sind. 
        /// Wenn eine Truppe Katapulte hat, dann ist es eine Katapultmannschaft. Die Anwesenheit von Katapulten verändert auch die Bewegungsweite
        /// </summary>
        public abstract FigurType Typ { get; }
        /// <summary>
        /// Die Stärke einer Figur ist abhängig von der Menge der Einheiten oder der Gutpunkte der Figur, sie ist immer aktuell
        /// </summary>
        public abstract string Stärke { get; }        
        /// <summary>
        /// Das transportierte Gold der Einheit, das keine Kampfeinnahmen sind, sondern an die Einheit übertragen wurde
        /// </summary>
        public int GS { get; set; } = 0;
        /// <summary>
        /// Das transportierte Gold der Einheit, am Anfang des Zuges
        /// </summary>
        public int GS_alt { get; set; } = 0;
        /// <summary>
        /// Das im Kampf durch diese Einheit erworbe Gold, dieser Wert bleibt auch bei einer Fusionierung bestehen
        /// </summary>
        public int Kampfeinnahmen { get; set; } = 0;
        /// <summary>
        /// Das im Kampf durch diese Einheit erworbe Gold, am Anfang des Zuges
        /// </summary>
        public int Kampfeinnahmen_alt { get; set; } = 0;
        /// <summary>
        /// Die Nummer wird je nach Einheit vergeben, siehe static Startnummer in den jeweiligen Derivaten
        /// </summary>
        public int Nummer { get; set; } = 0;
        /// <summary>
        /// Die Startpostion Großfeld am Anfang des Zuges
        /// </summary>
        public int gf_von { get; set; } = 0;
        /// <summary>
        /// Die Startpostion Kleinfeld am Anfang des Zuges
        /// </summary>
        public int kf_von { get; set; } = 0;
        /// <summary>
        /// Die Startpostion Großfeld am Anfang des Zuges
        /// </summary>
        private int _gfNach;
        /// <summary>
        /// Die Startpostion Kleinfeld am Anfang des Zuges
        /// </summary>
        private int _kfNach;
        /// <summary>
        /// Der Raumpunkteverbrauch durch die Spielfigur, die Menge der Raumpunkte sind je nach Beschaffenheit des Kleinfeldes und Rüstortes begrenzt
        /// </summary>
        public int rp { get; set; } = 0;
        /// <summary>
        /// Die aktuell verfügbaren Bewegungspunkte
        /// </summary>
        public int bp { get; set; } = 0;
        /// <summary>
        /// Die maximal verfügbaren Bewegungspunkte
        /// </summary>
        public int bp_max { get; set; }

        /// <summary>
        /// Die Anzal der überwundenen Höhenstufen in diesem Zug. Am Anfang des Zuges = 0
        /// </summary>
        public int hoehenstufen { get; set; }
        /// <summary>
        /// Die Anzal der Schritte in diesem Zug. Am Anfang des Zuges = 0
        /// Schritte werden auch gezählt, wenn sich die Figur hin- und herbewegt, also am ursprünglichen Feld wieder ankommt
        /// Somit kostet Hin- und Herbewegen auch Bewegungspunkte
        /// </summary>
        public int schritt { get; set; }

        /// <summary>
        /// Die Altanwendung speichert den xy wert des Kleinfeldes
        /// Bei Zugstart ist dieser also SharedData.Map[this.CreateBezeichner()].ph_xy
        /// </summary>
        public string ph_xy { get; set; } = string.Empty;
        /// <summary>
        /// die Felder x1-9 und y1-9 sind die Koordinaten in den db_xy Koordinaten der Kleinfelder und nicht gf/kf
        /// sie beschreiben die einzelnen Wegpunkte der Bewegung einer Spielfigur, damit diese als Konflikte ausgwertet werden können
        /// zur Kompatibilität müssen die Werte vorerst mit gepflegt werden
        /// </summary>
        public int x1 { get; set; }
        public int y1 { get; set; }
        public int x2 { get; set; }
        public int y2 { get; set; }
        public int x3 { get; set; }
        public int y3 { get; set; }
        public int x4 { get; set; }
        public int y4 { get; set; }
        public int x5 { get; set; }
        public int y5 { get; set; }
        public int x6 { get; set; }
        public int y6 { get; set; }
        public int x7 { get; set; }
        public int y7 { get; set; }
        public int x8 { get; set; }
        public int y8 { get; set; }
        public int x9 { get; set; }
        public int y9 { get; set; }

        /// <summary>
        /// Der Bezeichner erzugt sich aus Typ und Nummer.
        /// Um den Kleinfeld Bezeichner der Spielfigur zu holen, wird aus der Basisklasse <see cref="KleinfeldPosition"/>.CreateBezeichner() verwendt. Dieser Wert kann direkt in der Map verwendet werden.
        /// </summary>
        public string Bezeichner => $"{Typ.ToString()} {Nummer.ToString()}";
        public dbPZE.Nation? Nation { get; set; } = null;

        /// <summary>
        /// setzt eine Figur auf ein Kleinfeld - keine Bewegung
        /// </summary>
        /// <param name="kleinfeld"></param>
        public void PutOnKleinfeld(KleinFeld kleinfeld) {
            if (gf > 0 || string.IsNullOrEmpty(ph_xy) == false || kf > 0)
                ProgramView.LogError($"Fehler bei dem Setzen der Figur {Bezeichner} auf {kleinfeld.Bezeichner}", "Eine bereits auf dem Spielfeld befindlichen Figur, darf nicht erneut gesetzt werden, sondern muss bewegt werden.");
            ph_xy = kleinfeld.ph_xy;
            gf = kleinfeld.gf;
            kf = kleinfeld.kf;
        }

        /// <summary>
        /// wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        /// bei der Neuanlage, muss der Wert gf_von auf den ersten übergebenen Wert gesetzt werden
        /// Wichtig ist: beim Bewegen danach auch die xy Koordinaten zu nutzen, so lange die Altanwendung nicht ganz weg ist
        /// </summary>
        public override int gf {
            get {
                if (gf_nach > 0)
                    return gf_nach;
                return gf_von;
            }
            set {
                if (gf_von == 0) 
                    gf_von = value;
                gf_nach = value;
            }
        }
        /// <summary>
        /// wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        /// bei der Neuanlage, muss der Wert kf_von auf den ersten übergebenen Wert gesetzt werden
        /// Wichtig ist: beim Bewegen danach auch die xy Koordinaten zu nutzen, so lange die Altanwendung nicht ganz weg ist
        /// </summary>
        public override int kf {
            get {
                if (kf_nach > 0)
                    return kf_nach;
                return kf_von;
            }
            set {
                if (kf_von == 0)
                    kf_von = value;
                kf_nach = value;
            }
        }
        /// <summary>
        /// wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        /// Wichtig ist: beim Bewegen danach auch die xy Koordinaten zu nutzen, so lange die Altanwendung nicht ganz weg ist
        /// </summary>
        public int gf_nach {
            get => _gfNach;
            set {
                if (_gfNach != value) {
                    _gfNach = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        /// Wichtig ist: beim Bewegen danach auch die xy Koordinaten zu nutzen, so lange die Altanwendung nicht ganz weg ist
        /// </summary>
        public int kf_nach {
            get => _kfNach;
            set {
                if (_kfNach != value) {
                    _kfNach = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Heerführer statt hf. Für die Listendarstellung
        /// </summary>
        public int Heerführer {
            get {
                if (this is TruppenSpielfigur truppe)
                    return truppe.hf;
                return 0;
            }
        }
        /// <summary>
        /// LeichteKP statt lkp. Für die Listendarstellung
        /// </summary>
        public int LeichteKP {
            get {
                if (this is TruppenSpielfigur truppe)
                    return truppe.LKP;
                return 0;
            }
        }
        /// <summary>
        /// LeichteKP statt lkp. Für die Listendarstellung
        /// </summary>
        public int SchwereKP {
            get {
                if (this is TruppenSpielfigur truppe)
                    return truppe.SKP;
                return 0;
            }
        }

        /// <summary>
        /// CharakterName statt Charname. Für die Listendarstellung und Bearbeitung
        /// </summary>
        [View.Editable]
        public string CharakterName {
            get {
                if (this is NamensSpielfigur figur)
                    return figur.Charname;
                return string.Empty;
            }
            set {
                if (this is NamensSpielfigur figur && value != null)
                    figur.Charname = value;
            }
        }

        /// <summary>
        /// SpielerName statt Spielername. Für die Listendarstellung und Bearbeitung
        /// </summary>
        [View.Editable]
        public string SpielerName {
            get {
                if (this is NamensSpielfigur figur)
                    return figur.Spielername;
                return string.Empty;
            }
            set {
                if (this is NamensSpielfigur figur && value != null)
                    figur.Spielername = value;
            }
        }

        /// <summary>
        /// Titel statt Beschriftung. Für die Listendarstellung und Bearbeitung
        /// </summary>
        [View.Editable]
        public string Titel {
            get {
                if (this is NamensSpielfigur figur)
                    return figur.Beschriftung;
                return string.Empty;
            }
            set {
                if (this is NamensSpielfigur figur && value != null)
                    figur.Beschriftung = value;
            }
        }

        /// <summary>
        /// für manche Properties wie die Bewegung ist ein Change Event hilfreich
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Überschreibung der Laden Funktion, diese hier sollte zwingend von den abgeleiteten Klassen aufgerufen werden
        /// Implementierung des Interfaces IDatabaseTable als abstract
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Load(DbDataReader reader) {
            AssignToSelectedReich();
        }

        /// <summary>
        /// Implementierung des Interfaces <see cref="IDatabaseTable"/> als abstract
        /// </summary>
        /// <param name="command"></param>
        public abstract void Save(DbCommand command);
        /// <summary>
        /// Implementierung des Interfaces <see cref="IDatabaseTable"/> als abstract
        /// </summary>
        /// <param name="command"></param>public abstract void Insert(DbCommand command);
        public abstract void Insert(DbCommand command);

        /// <summary>
        /// wenn es aus der Zugdaten Datenbank kommt, dann ist es eine Spielfigur des ausgewählten Reiches
        /// Die Feinde sind eine eigene Klasse
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void AssignToSelectedReich() {
            if (ProgramView.SelectedNation == null)
                throw new InvalidOperationException("Zuext muss ein Nation ausgewählt sein");
            Nation = ProgramView.SelectedNation;
        }

        /// <summary>
        /// Implementierung von <see cref="ISelectable"/>
        /// </summary>
        /// <returns></returns>
        public bool Select() {
            if (SharedData.Map == null)
                return false;
            if (ProgramView.SelectedNation == null || Nation == null)
                return false;
            // die Spielfigur gehört evtl. zum Nation des aktuellen Nutzers und kann daher ausgeählt werden
            return ProgramView.SelectedNation == Nation;
        }

        /// <summary>
        /// Implementierung von <see cref="ISelectable"/>
        /// </summary>
        /// <returns></returns>
        public bool Edit() {
            return Select();
        }

        /// <summary>
        /// Die Felder, die nicht in der Eigenschaften Liste für den Benutzer sichtbar sein sollen
        /// </summary>
        private static readonly string[] PropertiestoIgnore = ["DatabaseName", "Bezeichner", "Nation", "GS_alt", "Kampfeinnahmen_alt", "Nummer", "gf_von", "kf_von", "gf", "kf", "gf_nach", "kf_nach", "ph_xy", "Key"];
        /// <summary>
        /// Implementierung von IEigenschaftler
        /// </summary>
        public virtual List<Eigenschaft> Eigenschaften {
            get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore);
        }
    }  
}
