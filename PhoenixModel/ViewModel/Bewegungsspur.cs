using PhoenixModel.View;
using System.Reflection;

namespace PhoenixModel.ViewModel {

    /// <summary>
    /// Der zurückgelegte Weg einer Spielfigur innerhalb eines Zuges.
    ///
    /// Die Altanwendung legt die Wegpunkte in den Feldern x1/y1 bis x19/y19 der Zugdatenbank ab.
    /// Dabei ist x das Großfeld und y das Kleinfeld des jeweils betretenen Feldes; das Startfeld
    /// steht nicht in der Spur, sondern in gf_von/kf_von. Solange die Altanwendung nicht endgültig
    /// abgelöst ist, müssen diese Felder weiter gepflegt werden, damit die Spielleitung Konflikte
    /// entlang des Weges auswerten kann.
    /// </summary>
    public class Bewegungsspur {

        /// <summary>
        /// Truppen führen bis zu 19 Wegpunkte
        /// </summary>
        public const int MaxWegpunkteTruppe = 19;

        /// <summary>
        /// Charaktere und Zauberer führen in der Datenbank nur 9 Wegpunkte
        /// </summary>
        public const int MaxWegpunkteNamensfigur = 9;

        private static readonly PropertyInfo[] _gfFelder;
        private static readonly PropertyInfo[] _kfFelder;

        static Bewegungsspur() {
            var typ = typeof(Spielfigur);
            _gfFelder = new PropertyInfo[MaxWegpunkteTruppe];
            _kfFelder = new PropertyInfo[MaxWegpunkteTruppe];
            for (int i = 0; i < MaxWegpunkteTruppe; i++) {
                _gfFelder[i] = typ.GetProperty($"x{i + 1}") ?? throw new MissingMemberException(typ.FullName, $"x{i + 1}");
                _kfFelder[i] = typ.GetProperty($"y{i + 1}") ?? throw new MissingMemberException(typ.FullName, $"y{i + 1}");
            }
        }

        private readonly Spielfigur _figur;

        public Bewegungsspur(Spielfigur figur) {
            _figur = figur;
        }

        /// <summary>
        /// Die Anzahl der Wegpunkte, die diese Figur in der Datenbank speichern kann
        /// </summary>
        public int MaxWegpunkte => _figur is NamensSpielfigur ? MaxWegpunkteNamensfigur : MaxWegpunkteTruppe;

        private int GetGf(int index) => (int)(_gfFelder[index].GetValue(_figur) ?? 0);
        private int GetKf(int index) => (int)(_kfFelder[index].GetValue(_figur) ?? 0);

        private void Set(int index, int gf, int kf) {
            _gfFelder[index].SetValue(_figur, gf);
            _kfFelder[index].SetValue(_figur, kf);
        }

        /// <summary>
        /// Die Anzahl der bisher eingetragenen Wegpunkte
        /// </summary>
        public int Count {
            get {
                for (int i = 0; i < MaxWegpunkte; i++) {
                    if (GetGf(i) == 0)
                        return i;
                }
                return MaxWegpunkte;
            }
        }

        /// <summary>
        /// Der Wegpunkt an der übergebenen Position oder null, wenn dort keiner eingetragen ist
        /// </summary>
        public KleinfeldPosition? this[int index] {
            get {
                if (index < 0 || index >= MaxWegpunkte)
                    return null;
                int gf = GetGf(index);
                return gf == 0 ? null : new KleinfeldPosition(gf, GetKf(index));
            }
        }

        /// <summary>
        /// Hängt einen Wegpunkt an und erhöht den Schrittzähler der Figur.
        /// Schritte werden auch gezählt, wenn die Figur wieder auf dem Ursprungsfeld ankommt.
        /// </summary>
        /// <returns>false, wenn kein Platz mehr für weitere Wegpunkte ist</returns>
        public bool Add(KleinfeldPosition position) {
            int index = Count;
            if (index >= MaxWegpunkte) {
                ProgramView.LogError($"Die Figur {_figur.Bezeichner} kann keine weiteren Wegpunkte speichern",
                    $"In der Zugdatenbank ist nur Platz für {MaxWegpunkte} Wegpunkte je Figur");
                return false;
            }
            Set(index, position.gf, position.kf);
            _figur.schritt = index + 1;
            return true;
        }

        /// <summary>
        /// Entfernt den zuletzt eingetragenen Wegpunkt und verringert den Schrittzähler
        /// </summary>
        /// <returns>den entfernten Wegpunkt oder null, wenn die Spur leer war</returns>
        public KleinfeldPosition? RemoveLast() {
            int index = Count - 1;
            if (index < 0)
                return null;
            var position = this[index];
            Set(index, 0, 0);
            _figur.schritt = index;
            return position;
        }

        /// <summary>
        /// Löscht die gesamte Spur. Wird beim Zugwechsel benötigt.
        /// </summary>
        public void Clear() {
            for (int i = 0; i < MaxWegpunkteTruppe; i++)
                Set(i, 0, 0);
            _figur.schritt = 0;
        }

        /// <summary>
        /// Alle eingetragenen Wegpunkte in der Reihenfolge ihrer Entstehung
        /// </summary>
        public List<KleinfeldPosition> ToList() {
            List<KleinfeldPosition> result = [];
            for (int i = 0; i < MaxWegpunkte; i++) {
                var position = this[i];
                if (position == null)
                    break;
                result.Add(position);
            }
            return result;
        }

        public override string ToString() {
            var punkte = ToList();
            return punkte.Count == 0 ? "keine Bewegung" : string.Join(" → ", punkte.Select(p => p.CreateBezeichner()));
        }
    }
}
