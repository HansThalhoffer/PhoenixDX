using PhoenixModel.Extensions;
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
        public int _gfNach;
        /// <summary>
        /// Die Startpostion Kleinfeld am Anfang des Zuges
        /// </summary>
        public int _kfNach;
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
        /// Die Werte x10-x19, y10-y19 werden nicht in Zauberern und Charaktern verwendet - warum auch immer
        /// anscheinend erwartet man da nicht so viele Bewegungen
        /// </summary>
        public int x10 { get; set; }
        public int y10 { get; set; }
        public int x11 { get; set; }
        public int y11 { get; set; }
        public int x12 { get; set; }
        public int y12 { get; set; }
        public int x13 { get; set; }
        public int y13 { get; set; }
        public int x14 { get; set; }
        public int y14 { get; set; }
        public int x15 { get; set; }
        public int y15 { get; set; }
        public int x16 { get; set; }
        public int y16 { get; set; }
        public int x17 { get; set; }
        public int y17 { get; set; }
        public int x18 { get; set; }
        public int y18 { get; set; }
        public int x19 { get; set; }
        public int y19 { get; set; }

        /// <summary>
        /// Der Bezeichner erzugt sich aus Typ und Nummer.
        /// Um den Kleinfeld Bezeichner der Spielfigur zu holen, wird aus der Basisklasse <see cref="KleinfeldPosition"/>.CreateBezeichner() verwendt. Dieser Wert kann direkt in der Map verwendet werden.
        /// </summary>
        public string Bezeichner => $"{Typ.ToString()} {Nummer.ToString()}";
        public dbPZE.Nation? Nation { get; set; } = null;

        /// <summary>
        /// Implementierung in <see cref="SpielfigurExtensions"/>
        /// wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        /// bei der Neuanlage, muss der Wert gf_von auf den ersten übergebenen Wert gesetzt werden
        /// Wichtig ist: beim Bewegen danach auch die xy Koordinaten zu nutzen, so lange die Altanwendung nicht ganz weg ist
        /// </summary>
        public override int gf {
            get =>  this.GetGf();
            set {
                this.SetGf(value);
            }
        }
        /// <summary>
        /// Implementierung in <see cref="SpielfigurExtensions"/>
        /// wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        /// bei der Neuanlage, muss der Wert kf_von auf den ersten übergebenen Wert gesetzt werden
        /// Wichtig ist: beim Bewegen danach auch die xy Koordinaten zu nutzen, so lange die Altanwendung nicht ganz weg ist
        /// </summary>
        public override int kf {
            get => this.GetKf();
            set {
                this.SetKf(value);
            }
        }
        /// <summary>
        /// Implementierung in <see cref="SpielfigurExtensions"/>
        /// wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        /// Wichtig ist: beim Bewegen danach auch die xy Koordinaten zu nutzen, so lange die Altanwendung nicht ganz weg ist
        /// </summary>
        public int gf_nach {
            get => this.GetGfNach();
            set {
                this.SetGfNach(value);
            }
        }
        /// <summary>
        /// Implementierung in <see cref="SpielfigurExtensions"/>
        /// wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        /// Wichtig ist: beim Bewegen danach auch die xy Koordinaten zu nutzen, so lange die Altanwendung nicht ganz weg ist
        /// </summary>
        public int kf_nach {
            get => this.GetKfNach();
            set {
                this.SetKfNach(value);
            }
        }

        /// <summary>
        /// Heerführer statt hf. Für die Listendarstellung
        /// </summary>
        public int Heerführer {
            get  => (this is TruppenSpielfigur truppe) ? truppe.hf: 0;
        }
        /// <summary>
        /// LeichteKP statt lkp. Für die Listendarstellung
        /// </summary>
        public int LeichteKP {
            get => (this is TruppenSpielfigur truppe) ? truppe.LKP : 0;
        }
        /// <summary>
        /// LeichteKP statt lkp. Für die Listendarstellung
        /// </summary>
        public int SchwereKP {
            get => (this is TruppenSpielfigur truppe) ? truppe.SKP : 0;
        }

        /// <summary>
        /// CharakterName statt Charname. Für die Listendarstellung und Bearbeitung
        /// </summary>
        [View.Editable]
        public string CharakterName {
            get => (this is NamensSpielfigur figur)? figur.Charname: string.Empty;
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
            get => (this is NamensSpielfigur figur) ? figur.Spielername : string.Empty;
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
            get => (this is NamensSpielfigur figur) ? figur.Beschriftung : string.Empty;
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
        /// für AssignToSelectedReich <see cref="SpielfigurExtensions"/>
        /// Implementierung des Interfaces IDatabaseTable als abstract
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Load(DbDataReader reader) {
            this.AssignToSelectedReich(); 
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
