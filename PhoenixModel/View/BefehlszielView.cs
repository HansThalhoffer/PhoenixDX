using PhoenixModel.Commands;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {

    /// <summary>
    /// Zu welcher Gemark ein Befehl gehört.
    ///
    /// Gebraucht für die Liste des laufenden Zuges: wer dort einen Eintrag anklickt, soll auf der
    /// Karte dorthin springen, wo der Befehl gewirkt hat.
    ///
    /// Bei einer Bewegung ist das das <em>Ziel</em> und nicht der Ausgangspunkt - dort steht das
    /// Heer, wenn man nachsehen will. Führte die Bewegung nirgendwohin, bleibt der Ausgangspunkt.
    /// </summary>
    public static class BefehlszielView {

        /// <summary>
        /// Die Gemark, auf die ein Befehl zeigt - oder null, wenn er auf keine zeigt.
        ///
        /// Die Fälle stehen hier und nicht als überschriebene Eigenschaft in fünfzehn
        /// Befehlsklassen: es ist eine Frage der Anzeige, nicht des Befehls, und so lässt sie sich
        /// an einer Stelle prüfen.
        /// </summary>
        public static KleinfeldPosition? GetZielfeld(BaseCommand? befehl) {
            if (befehl == null)
                return null;

            return befehl switch {
                // Die Bewegung zeigt auf ihr Ziel
                MoveCommand bewegung => bewegung.ToLocation ?? bewegung.FromLocation,
                // Teleport und Zauber zeigen ebenfalls dorthin, wo sie gewirkt haben
                TeleportCommand teleport => teleport.LocationTo,

                // Was an oder auf einer Gemark gebaut und geändert wird
                ConstructCommand bau => bau.Location,
                RuestortBaubefehl baubefehl => baubefehl.Location,
                HauptstadtverlegungCommand verlegung => verlegung.Location,
                ChangeNameCommand umbenennung => umbenennung.Location,
                EquipCommand rüstung => rüstung.Location,
                CastSpellCommando zauber => zauber.LocationTo,
                DoNothingCommand nichts => nichts.Location,

                // Alles Übrige zeigt auf das, woran es gearbeitet hat - eine Figur steht ebenfalls
                // auf einer Gemark, denn Spielfigur ist eine KleinfeldPosition.
                _ => befehl.Betroffen as KleinfeldPosition,
            };
        }

        /// <summary>
        /// Zeigt dieser Befehl überhaupt auf eine Gemark?
        /// </summary>
        public static bool HatZielfeld(BaseCommand? befehl) => GetZielfeld(befehl) != null;

        /// <summary>
        /// Das Bauwerk, um das es geht, und die Kante, an der es liegt.
        ///
        /// Ein Wall, eine Strasse, eine Brücke und eine Kaianlage liegen an einer Gemarkkante; auf
        /// der Karte lässt sich genau dieses Stück hervorheben. Alles andere gehört der ganzen
        /// Gemark - dann ist die Richtung null, und es blinkt das Feld.
        /// </summary>
        /// <returns>Art und Richtung, oder zweimal null, wenn der Befehl kein Bauwerk betrifft</returns>
        public static (ConstructionElementType? Art, Direction? Richtung) GetZielbauwerk(BaseCommand? befehl) {
            if (befehl is not ConstructCommand bau)
                return (null, null);
            return (bau.What, bau.Direction);
        }
    }
}
