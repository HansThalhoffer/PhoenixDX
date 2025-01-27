using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;

namespace PhoenixModel.ViewModel {

    /// <summary>
    /// Gegner wird als Listenklasse der Feinde benutzt
    /// </summary>
    public class Gegner : List<Feinde>, IEigenschaftler {
        /// <summary>
        /// IEigenschaftler Anforderung
        /// </summary>
        public string Bezeichner => "Gegner";
        /// <summary>
        /// IEigenschaftler Anforderung
        ///  dieses Property wird für die Anzeige in den Eigenschaften eines Kleinfeldes genutzt, die Darstellung in der Truppenliste wird in der Page selbst erzeugt
        /// </summary>
        public List<Eigenschaft> Eigenschaften {
            get {
                List<Eigenschaft> eigenschaften = [];
                foreach (var figur in this) {
                    eigenschaften.Add(new Eigenschaft($"{figur.Typ}", $"{figur.Reich} #{figur.Nummer} {figur.Notiz}", false, this));
                }
                return eigenschaften;
            }
        }
    }

    /// <summary>
    /// Armee wird als Listenklasse der Spielfiguren benutzt
    /// </summary>
    public class Armee : List<Spielfigur>, IEigenschaftler {

        public Spielfigur[] Filter(Type type) {

            return this.Where(item => type.IsAssignableFrom(item.GetType())).ToArray();
        }

        /// <summary>
        /// IEigenschaftler Anforderung
        /// </summary>
        public string Bezeichner => "Armee";
        /// <summary>
        /// IEigenschaftler Anforderung
        ///  dieses Property wird für die Anzeige in den Eigenschaften eines Kleinfeldes genutzt, die Darstellung in der Truppenliste wird in der Page selbst erzeugt
        /// </summary>
        public List<Eigenschaft> Eigenschaften {
            get {
                List<Eigenschaft> eigenschaften = [];
                foreach (var figur in this) {
                    if (figur is TruppenSpielfigur t) {
                        string wert = $"{t.Bezeichner} Str {t.staerke.ToString("n0")} HF {t.hf}";
                        if (t.Garde)
                            wert += " Garde";
                        if (figur.Gold > 0)
                            wert += $" Gold {figur.Gold}";
                        if (t.LKP > 0)
                            if (t is Schiffe)
                                wert += $" LKS {t.LKP}";
                            else
                                wert += $" LKP {t.LKP}";
                        if (t.SKP > 0)
                            if (t is Schiffe)
                                wert += $" SKS {t.LKP}";
                            else
                                wert += $" SKP:{t.SKP}";
                        if (t.Pferde > 0)
                            wert += $" Pferde {t.Pferde}";
                        eigenschaften.Add(new Eigenschaft(t.BaseTyp.ToString(), wert, false, this));

                    }
                    else {
                        if (figur is Zauberer) {
                            var z = figur as Zauberer;
                            if (z != null) {
                                string wert = $"{z.Beschriftung} {z.charname} GP {z.GP_akt}/{z.GP_ges}";
                                if (figur.Typ == FigurType.CharakterZauberer)
                                    eigenschaften.Add(new Eigenschaft("Charakter Zauberer", wert, false, this));
                                else
                                    eigenschaften.Add(new Eigenschaft("Zauberer", wert, false, this));
                            }
                        }
                        else if (figur is Character) {
                            var c = figur as Character;
                            if (c != null) {
                                string wert = $"{c.Beschriftung} {c.Charname} GP {c.GP_akt}/{c.GP_ges}";
                                eigenschaften.Add(new Eigenschaft("Charakter", wert, false, this));
                            }
                        }
                    }

                }
                return eigenschaften;
            }
        }
    }
}
