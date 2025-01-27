using PhoenixModel.Database;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands.Parser {
    /// <summary>
    /// Basisklasse für Befehle, die das IPhoenixCommand-Interface implementieren.
    /// </summary>
    public abstract class BaseCommand : IPhoenixCommand {
        /// <summary>
        /// Der zugrunde liegende Befehl als Zeichenkette.
        /// </summary>
        protected readonly string _CommandString;

        /// <summary>
        /// Wenn das Item innerhalb der aktuellen Session bearbeitet wurde, steht es noch hierdrin
        /// </summary>
        protected ISelectable? _Selectable = null;

        /// <summary>
        /// Wenn das Kommando das Selectable betrifft, gibt es true zurück
        /// Die Basisimplementierung schaut nach einem direkten Vergleich
        /// der funktioniert aber nur, wenn das Selectable nach Programmstart verändert wurde
        /// daher ist ein Vergleich der Werte immer notwendig
        /// </summary>
        /// <param name="selectable"></param>
        /// <returns></returns>
        public virtual bool HasEffectOn(ISelectable selectable) => _Selectable == selectable;

        /// <summary>
        /// Gibt an, ob der Befehl bereits ausgeführt wurde.
        /// </summary>
        public bool IsExecuted { get; protected set; }

        /// <summary>
        /// Erstellt eine neue Instanz der <see cref="BaseCommand"/>-Klasse.
        /// </summary>
        /// <param name="commandString">Die Befehlszeichenkette.</param>
        public BaseCommand(string commandString) {
            _CommandString = commandString;
        }

        /// <summary>
        /// Ruft die Befehlszeichenkette ab.
        /// </summary>
        public string CommandString => _CommandString;

        /// <summary>
        /// Kann der Befehl durchgeführt werden?
        /// CheckPreconditions().HasErrors = false;
        /// </summary>
        /// <returns>Ein Objekt vom Typ CommandResult.</returns>
        public virtual bool CanExecute { get { return CheckPreconditions().HasErrors == false; } }

        /// <summary>
        /// Kann der Befehl zurück genommen werden?
        /// Die Preconditions müssen passen und es muss ein ausgeführtes Command sein
        /// </summary>
        /// <returns>Ein Objekt vom Typ CommandResult.</returns>
        public virtual bool CanUndo => CanExecute && IsExecuted == true;

        /// <summary>
        /// Überprüft, ob die Vorbedingungen für die Ausführung des Befehls erfüllt sind.
        /// </summary>
        /// <returns>Ein <see cref="CommandResult"/>-Objekt mit dem Ergebnis der Prüfung.</returns>
        public abstract CommandResult CheckPreconditions();

        /// <summary>
        /// Führt den Befehl aus.
        /// </summary>
        /// <returns>Ein <see cref="CommandResult"/>-Objekt mit dem Ausführungsergebnis.</returns>
        public abstract CommandResult ExecuteCommand();

        /// <summary>
        /// Macht die Ausführung des Befehls rückgängig.
        /// </summary>
        /// <returns>Ein <see cref="CommandResult"/>-Objekt mit dem Ergebnis des Rückgängig-Machens.</returns>
        public abstract CommandResult UndoCommand();

        /// <summary>
        /// Aktualisiert den Status eines Elements in der Datenbank und speichert die Aktion in der Befehlswarteschlange.
        /// </summary>
        /// <param name="item">Das betroffene Datenbankobjekt.</param>
        /// <param name="viewEventType">Der Typ des Ansichtsereignisses, das aktualisiert werden soll.</param>
        /// <param name="callerName">Der Name der aufrufenden Methode (wird automatisch übermittelt).</param>
        protected void Update(IDatabaseTable item, ViewEventArgs.ViewEventType viewEventType, [CallerMemberName] string callerName = "") {
            IsExecuted = true;
            SharedData.StoreQueue.Enqueue(item);

            bool isUndo = callerName.StartsWith("Undo");
            if (isUndo) {
                SharedData.Commands.Remove(this);
            }
            else if (item is ISelectable selectable) {
                SharedData.Commands.Add(this);
                _Selectable = selectable;
            }

            // Aktualisiert die Ansicht für Diplomatie-Änderungen
            ProgramView.Update(ViewEventArgs.ViewEventType.UpdateDiplomatie);
        }

        /// <summary>
        /// Konvertiert das Kommando in eine Zeichenfolgendarstellung.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }

    /// <summary>
    /// Abstrakte Basisklasse für einen einfachen Befehlsparser.
    /// Implementiert die Schnittstelle <see cref="ICommandParser"/> zur Verarbeitung von Befehlsstrings.
    /// </summary>
    public abstract class SimpleParser : ICommandParser {
        /// <summary>
        /// Gibt einen Fehlerzustand zurück, indem das übergebene <paramref name="command"/>-Objekt auf <c>null</c> gesetzt wird.
        /// </summary>
        /// <param name="command">Ausgabeparameter, der auf <c>null</c> gesetzt wird.</param>
        /// <returns>Gibt immer <c>false</c> zurück, um einen Fehlschlag anzuzeigen.</returns>
        protected static bool Fail(out IPhoenixCommand? command) {
            command = null;
            return false;
        }

        /// <summary>
        /// Parst einen Befehlsstring und gibt das entsprechende <see cref="IPhoenixCommand"/>-Objekt zurück.
        /// </summary>
        /// <param name="commandString">Der zu parsende Befehlsstring.</param>
        /// <param name="command">Das geparste <see cref="IPhoenixCommand"/>-Objekt oder <c>null</c>, falls das Parsen fehlschlägt.</param>
        /// <returns><c>true</c>, wenn der Befehl erfolgreich geparst wurde; andernfalls <c>false</c>.</returns>
        public abstract bool ParseCommand(string commandString, out IPhoenixCommand? command);

        /// <summary>
        /// Analysiert eine Eingabe und extrahiert eine Kleinfeld-Position.
        /// </summary>
        /// <param name="input">Der zu analysierende Eingabestring.</param>
        /// <returns>Die extrahierte Kleinfeld-Position oder null, falls ungültig.</returns>
        public static KleinfeldPosition? ParseLocation(string input) {
            var numbers = input.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (numbers.Length == 2) {
                return new KleinfeldPosition(ParseInt(numbers[0]), ParseInt(numbers[1]));
            }
            return null;
        }

        /// <summary>
        /// Analysiert eine Eingabe und gibt den entsprechenden Einheitstyp zurück.
        /// </summary>
        /// <param name="input">Der eingegebene Einheitstyp als String.</param>
        /// <returns>Der erkannte Einheitstyp.</returns>
        public static FigurType ParseUnitType(string input) {
            return input.ToLower() switch {
                "reiter" => FigurType.Reiter,
                "krieger" => FigurType.Krieger,
                "kreatur" => FigurType.Kreatur,
                "schiff" => FigurType.Schiff,
                "zauberer" => FigurType.Zauberer,
                "charakter" => FigurType.Charakter,
                "charakterzauberer" => FigurType.CharakterZauberer,
                "lkp" => FigurType.LeichteArtillerie,
                "skp" => FigurType.SchwereArtillerie,
                "lks" => FigurType.LeichtesKriegsschiff,
                "sks" => FigurType.SchweresKriegsschiff,
                "leichtes katapult" => FigurType.LeichteArtillerie,
                "leichtem katapult" => FigurType.LeichteArtillerie,
                "leichten katapulten" => FigurType.LeichteArtillerie,
                "schweres katapult" => FigurType.SchwereArtillerie,
                "schwerem katapult" => FigurType.SchwereArtillerie,
                "schweren katapulten" => FigurType.SchwereArtillerie,
                "leichtes kriegsschiff" => FigurType.LeichtesKriegsschiff,
                "leichtem kriegsschiff" => FigurType.LeichtesKriegsschiff,
                "leichten kriegsschiffen" => FigurType.LeichtesKriegsschiff,
                "schweres kriegsschiff" => FigurType.SchweresKriegsschiff,
                "schwerem kriegsschiff" => FigurType.SchweresKriegsschiff,
                "schweren kriegsschiffen" => FigurType.SchweresKriegsschiff,
                "piratenschiff" => FigurType.PiratenSchiff,
                "piratenschiffen" => FigurType.PiratenSchiff,
                "schweres piratenkriegsschiff" => FigurType.PiratenSchweresKriegsschiff,
                "schwerem piratenkriegsschiff" => FigurType.PiratenSchweresKriegsschiff,
                "schweren piratenkriegsschiffen" => FigurType.PiratenSchweresKriegsschiff,
                "leichtes piratenkriegsschiff" => FigurType.PiratenLeichtesKriegsschiff,
                "leichtem piratenkriegsschiff" => FigurType.PiratenLeichtesKriegsschiff,
                "leichten piratenkriegsschiffen" => FigurType.PiratenLeichtesKriegsschiff,
                "berittene schwere artillerie" => FigurType.BeritteneSchwereArtillerie,
                "berittener schwerer artillerie" => FigurType.BeritteneSchwereArtillerie,
                "berittene leichte artillerie" => FigurType.BeritteneLeichteArtillerie,
                "berittener leichter artillerie" => FigurType.BeritteneLeichteArtillerie,
                _ => FigurType.None,
            };
        }

        /// <summary>
        /// die Umkehrfunktion zum Erstellen von Befehlen
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string UnitTypeToString(FigurType type) {
            return type switch {
                FigurType.Reiter => "Reiter",
                FigurType.Krieger => "Krieger",
                FigurType.Schiff => "Schiff",
                FigurType.Zauberer => "Zauberer",
                FigurType.Charakter => "Charakter",
                FigurType.CharakterZauberer => "Charakterzauberer",
                FigurType.LeichteArtillerie => "Leichtes Katapult",
                FigurType.SchwereArtillerie => "Schweres Katapult",
                FigurType.LeichtesKriegsschiff => "Leichtes Kriegsschiff",
                FigurType.SchweresKriegsschiff => "Schweres Kriegsschiff",
                FigurType.PiratenSchiff => "Piratenschiff",
                FigurType.PiratenSchweresKriegsschiff => "Schweres Piratenkriegsschiff",
                FigurType.PiratenLeichtesKriegsschiff => "Leichtes Piratenkriegsschiff",
                FigurType.BeritteneSchwereArtillerie => "Berittene schwere Artillerie",
                FigurType.BeritteneLeichteArtillerie => "Berittene leichte Artillerie",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Analysiert eine Eingabe und gibt den entsprechenden Einheitstyp zurück.
        /// </summary>
        /// <param name="input">Der eingegebene Einheitstyp als String.</param>
        /// <returns>Der erkannte Einheitstyp.</returns>
        public static Direction? ParseDirection(string input) {
            return input.ToLower() switch {
                "no" => Direction.NO,
                "nordost" => Direction.NO,
                "nordosten" => Direction.NO,
                "o" => Direction.O,
                "ost" => Direction.O,
                "osten" => Direction.O,
                "so" => Direction.SO,
                "südost" => Direction.SO,
                "südosten" => Direction.SO,
                "sw" => Direction.SW,
                "südwest" => Direction.SW,
                "südwesten" => Direction.SW,
                "w" => Direction.W,
                "west" => Direction.W,
                "westen" => Direction.W,
                "nw" => Direction.NW,
                "nordwest" => Direction.NW,
                "nordwesten" => Direction.NW,
                _ => null,
            };
        }

        /// <summary>
        /// liest eine Zahl und ignoriert Exceptions
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int ParseInt(string input) {
            try {
                input = input.Trim();
                if (string.IsNullOrEmpty(input))
                    return 0;
                if (input.All(char.IsDigit) == false)
                    return 0;
                return int.Parse(input);
            }
            catch { };
            return 0;
        }

        /// <summary>
        /// Um aus einem zu bauenden oder zu rüstendem Objekt wieder ein Strin zu machen
        /// die Namen entsprechen 1:1 der Kostentabelle in crossref.mdb
        /// </summary>
        public static string ConstructionElementTypeToString(ConstructionElementType type) {
            return type
            switch {
                ConstructionElementType.None => "",
                ConstructionElementType.K => "Krieger", // Krieger
                ConstructionElementType.S => "Schiffe", // 
                ConstructionElementType.R => "Reiter", // Reiter
                ConstructionElementType.P => "PFerde", // PFerde
                ConstructionElementType.LKP => "Leichte Katapulte",// 
                ConstructionElementType.SKP => "Schwere Katapulte",// 
                ConstructionElementType.LKS => "Leichte Kriegsschiffe ",// 
                ConstructionElementType.SKS => "Schwere Kriegsschiffe",// 
                ConstructionElementType.HF => "Heerführer", // HF 
                ConstructionElementType.ZA => "Zauberer Klasse A", // Zauberer Klasse A
                ConstructionElementType.ZB => "Zauberer Klasse B", // Zauberer Klasse B
                ConstructionElementType.Strasse => "Straße",
                ConstructionElementType.Bruecke => "Brücke",
                ConstructionElementType.Wall => "Wall",
                ConstructionElementType.Burg => "Burg",
                ConstructionElementType.ausbau => "ausbau",
                _ => string.Empty,
            };
        }


        /// <summary>
        /// Wenn etwas gerüstet oder gebaut werden soll, wird es hiermit im String identifihiert
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public ConstructionElementType parseConstructionElement(string input) {
            return input.ToLower()
            switch {
                "k" => ConstructionElementType.K,
                "krieger" => ConstructionElementType.K,
                "kriegern" => ConstructionElementType.K,
                "r" => ConstructionElementType.R,
                "reiter" => ConstructionElementType.R,
                "reitern" => ConstructionElementType.R,
                "s" => ConstructionElementType.S,
                "schiff" => ConstructionElementType.S,
                "schiffe" => ConstructionElementType.S,
                "schiffen" => ConstructionElementType.S,
                "pferde" => ConstructionElementType.P,
                "lkp" => ConstructionElementType.LKP,
                "leichte katapulte" => ConstructionElementType.LKP,
                "leichte kp" => ConstructionElementType.LKP,
                "skp" => ConstructionElementType.SKP,
                "schwere katapulte" => ConstructionElementType.SKP,
                "schwere kp" => ConstructionElementType.SKP,
                "lks" => ConstructionElementType.LKS,
                "leichte kriegsschiffe" => ConstructionElementType.LKS,
                "leichte ks" => ConstructionElementType.LKS,
                "sks" => ConstructionElementType.SKS,
                "schwere kriegsschiffe" => ConstructionElementType.SKS,
                "schwere ks" => ConstructionElementType.SKS,
                "heerführer" => ConstructionElementType.HF,
                "hf" => ConstructionElementType.HF,
                "za" => ConstructionElementType.ZA,
                "zauberer klasse a" => ConstructionElementType.ZA,
                "zb" => ConstructionElementType.ZB,
                "zauberer klasse b" => ConstructionElementType.ZB,
                "wall" => ConstructionElementType.Wall,
                "strasse" => ConstructionElementType.Strasse,
                "straße" => ConstructionElementType.Strasse,
                "brücke" => ConstructionElementType.Bruecke,
                "bruecke" => ConstructionElementType.Bruecke,
                "burg" => ConstructionElementType.Burg,
                _ => ConstructionElementType.None
            };
        }
    }
}
