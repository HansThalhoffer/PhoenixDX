using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;

namespace PhoenixModel.ViewModel {

    public class Gegner: List<Feinde>, IEigenschaftler {

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
                    eigenschaften.Add(new Eigenschaft("Fremdtruppen", $"{figur.Typ}({figur.Reich}/{figur.Nation.Reich})", false, this));
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
                    if (figur is Kreaturen) {
                        var k = figur as Kreaturen;
                        if (k != null) {
                            string wert = $"{k.Bezeichner}";
                            eigenschaften.Add(new Eigenschaft("Kreatur", wert, false, this));
                        }
                    }
                    else if (figur is Krieger) {
                        var k = figur as Krieger;
                        if (k != null) {
                            string wert = $"{k.Bezeichner} Str {k.staerke.ToString("n0")} HF {k.hf}";
                            if (k.Garde)
                                wert += " Garde";
                            if (k.GS > 0)
                                wert += $" Gold {k.GS}";
                            if (k.LKP > 0)
                                wert += $" LKP {k.LKP}";
                            if (k.SKP > 0)
                                wert += $" SKP:{k.SKP}";
                            if (k.Pferde > 0)
                                wert += $" Pferde {k.Pferde}";
                            eigenschaften.Add(new Eigenschaft("Krieger", wert, false, this));
                        }
                    }
                    else if (figur is Reiter) {
                        var k = figur as Reiter;
                        if (k != null) {
                            string wert = $"{k.Bezeichner} Str {k.staerke} HF {k.hf}";
                            if (k.Garde)
                                wert += " Garde";
                            if (k.GS > 0)
                                wert += $" Gold {k.GS}";
                            if (k.LKP > 0)
                                wert += $" LKP {k.LKP}";
                            if (k.SKP > 0)
                                wert += $" SKP {k.SKP}";
                            if (k.Pferde > 0)
                                wert += $" Pferde {k.Pferde}";
                            eigenschaften.Add(new Eigenschaft("Reiter", wert, false, this));
                        }
                    }
                    else if (figur is Schiffe) {
                        var k = figur as Schiffe;
                        if (k != null) {
                            string wert = $"{k.Bezeichner} Str {k.staerke}";
                            if (k.Garde)
                                wert += " G";
                            if (k.GS > 0)
                                wert += $" Gold {k.GS}";
                            if (k.LKP > 0)
                                wert += $" LKP {k.LKP}";
                            if (k.SKP > 0)
                                wert += $" SKP {k.SKP}";
                            if (k.Pferde > 0)
                                wert += $" Pferde {k.Pferde}";
                            eigenschaften.Add(new Eigenschaft("Schiff", wert, false, this));
                        }
                    }
                    else if (figur is Zauberer) {
                        var k = figur as Zauberer;
                        if (k != null) {
                            string wert = $"{k.Bezeichner} {k.Beschriftung} {k.charname} GP {k.GP_akt}";
                            eigenschaften.Add(new Eigenschaft("Zauberer", wert, false, this));
                        }
                    }
                    else if (figur is Character) {
                        var k = figur as Character;
                        if (k != null) {
                            string wert = $"{k.Bezeichner} {k.Beschriftung} {k.Charname} GP {k.GP_akt}";
                            eigenschaften.Add(new Eigenschaft("Charakter", wert, false, this));
                        }
                    }

                }
                return eigenschaften;
            }
        }

        
    }
}
