using PhoenixModel.Commands.Parser;
using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {

    public enum Spell { none, Bannen, Teleport, Duell, Barriere }


    /// <summary>
    /// Bersana zaubert eine Magische Wand in den Nordosten von 701/33
    /// Magus Morbus zaubert einen Bann auf 701/33
    /// Gladius Tullius zaubert einen Teleport von 501/22 nach 755/22
    /// Bersana zaubert einen Teleport von 501/22 nach 755/22 mit den Kriegern 101, Reitern 203
    /// Gladius Tullius fordert den Zauberer 501 von Helborn zum Duell
    /// </summary>
    public class CastSpellCommando : BaseCommand, IPhoenixCommand, IEquatable<CastSpellCommando> {
          
        public CastSpellCommando(string commandString) : base(commandString) {
        }
        const FigurType figurType = FigurType.Zauberer;
        public int UnitID { get; set; } = 0;
        public Spell Spell { get; set; } = Spell.none;
        KleinfeldPosition? LocationFrom  = null;
        KleinfeldPosition? LocationTo = null;
        List<Spielfigur> TeleportPayLoad = [];

        public override bool CanAppliedTo(ISelectable selectable) {
           return (selectable != null && selectable is Zauberer);
        }

        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException();
        }

        public bool Equals(ChangeNameCommand? other) {
            throw new NotImplementedException();
        }

        public override CommandResult ExecuteCommand() {
            throw new NotImplementedException();
        }

        public override string ToString() {
            throw new NotImplementedException();
        }

        public override CommandResult UndoCommand() {
            throw new NotImplementedException();
        }

        public bool Equals(CastSpellCommando? other) {
            if (other == null)
                return false;

            return UnitID == other.UnitID &&
                   Spell == other.Spell &&
                   EqualityComparer<KleinfeldPosition?>.Default.Equals(LocationFrom, other.LocationFrom) &&
                   EqualityComparer<KleinfeldPosition?>.Default.Equals(LocationTo, other.LocationTo) &&
                   TeleportPayLoad.SequenceEqual(other.TeleportPayLoad);
        }

        public override bool Equals(object? obj) {
            return Equals(obj as CastSpellCommando);
        }

        public override int GetHashCode() {
            return HashCode.Combine(UnitID, Spell, LocationFrom, LocationTo, TeleportPayLoad);
        }
    }
}
