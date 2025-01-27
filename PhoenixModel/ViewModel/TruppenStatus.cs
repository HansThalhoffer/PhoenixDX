using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Die Klasse berechnet die Summenwerte einer Armee und zeigt diese in den Member-Properties an.
    /// </summary>
    public class TruppenStatus {
        /// <summary>
        /// Im Konstruktor erfolgt die Berechnung der Summenwerte einer Armee.
        /// </summary>
        /// <param name="amy">Die Armee, deren Werte berechnet werden sollen.</param>
        public TruppenStatus(Armee amy) {
            foreach (var item in amy) {
                if (item is Kreaturen kreatur) {
                    ProgramView.LogWarning("Kreaturen werden im Truppenstatus nicht erfasst",
                        "Eventuell muss dies noch implementiert werden");
                }
                else if (item is Krieger krieger) {
                    this.Krieger += krieger.staerke;
                    this.HF_Krieger += krieger.hf;
                    this.HF_Gesamt += krieger.hf;
                    this.SKP_Krieger += krieger.SchwereKP;
                    this.LKP_Krieger += krieger.LeichteKP;
                    this.Pferde_Krieger += krieger.Pferde;
                }
                else if (item is Reiter reiter) {
                    this.Reiter += reiter.staerke;
                    this.HF_Reiter += reiter.hf;
                    this.HF_Gesamt += reiter.hf;
                    this.SKP_Reiter += reiter.SchwereKP;
                    this.LKP_Reiter += reiter.LeichteKP;
                    this.Pferde_Reiter += reiter.Pferde;
                }
                else if (item is Schiffe schiffe) {
                    this.Schiffe += schiffe.staerke;
                    this.HF_Schiffe += schiffe.hf;
                    this.HF_Gesamt += schiffe.hf;
                    this.LKS_Schiffe += schiffe.LeichteKP;
                    this.SKS_Schiffe += schiffe.SchwereKP;
                }
                else if (item is Zauberer wiz) {
                    // Kategorisierung der Zauberer nach Klassen
                    if (wiz.Klasse == Zaubererklasse.ZA) { this.ZA++; }
                    else if (wiz.Klasse == Zaubererklasse.ZB) { this.ZB++; }
                    else if (wiz.Klasse == Zaubererklasse.ZC) { this.ZC++; }
                    else if (wiz.Klasse == Zaubererklasse.ZD) { this.ZD++; }
                    else if (wiz.Klasse == Zaubererklasse.ZE) { this.ZE++; }
                    else if (wiz.Klasse == Zaubererklasse.ZF) { this.ZF++; }
                }
            }
        }

        /// <summary>
        /// Anzahl der Krieger.
        /// </summary>
        public int Krieger { get; set; }
        /// <summary>
        /// Hitpoints der Krieger.
        /// </summary>
        public int HF_Krieger { get; set; }
        /// <summary>
        /// Leichte Katapulte der Krieger.
        /// </summary>
        public int LKP_Krieger { get; set; }
        /// <summary>
        /// Schwere Katapulte der Krieger.
        /// </summary>
        public int SKP_Krieger { get; set; }
        /// <summary>
        /// Anzahl der Pferde bei Kriegern.
        /// </summary>
        public int Pferde_Krieger { get; set; }

        /// <summary>
        /// Anzahl der Reiter.
        /// </summary>
        public int Reiter { get; set; }
        /// <summary>
        /// Hitpoints der Reiter.
        /// </summary>
        public int HF_Reiter { get; set; }
        /// <summary>
        /// Leichte Katapulte der Reiter.
        /// </summary>
        public int LKP_Reiter { get; set; }
        /// <summary>
        /// Schwere Katapulte der Reiter.
        /// </summary>
        public int SKP_Reiter { get; set; }
        /// <summary>
        /// Anzahl der Pferde bei Reitern.
        /// </summary>
        public int Pferde_Reiter { get; set; }

        /// <summary>
        /// Anzahl der Schiffe.
        /// </summary>
        public int Schiffe { get; set; }
        /// <summary>
        /// Hitpoints der Schiffe.
        /// </summary>
        public int HF_Schiffe { get; set; }
        /// <summary>
        /// Leichte Katapulte der Schiffe.
        /// </summary>
        public int LKS_Schiffe { get; set; }
        /// <summary>
        /// Schwere Katapulte der Schiffe.
        /// </summary>
        public int SKS_Schiffe { get; set; }

        /// <summary>
        /// Gesamthitpoints aller Einheiten.
        /// </summary>
        public int HF_Gesamt { get; set; }

        /// <summary>
        /// Anzahl der Zauberer der Klasse ZA.
        /// </summary>
        public int ZA { get; set; }
        /// <summary>
        /// Anzahl der Zauberer der Klasse ZB.
        /// </summary>
        public int ZB { get; set; }
        /// <summary>
        /// Anzahl der Zauberer der Klasse ZC.
        /// </summary>
        public int ZC { get; set; }
        /// <summary>
        /// Anzahl der Zauberer der Klasse ZD.
        /// </summary>
        public int ZD { get; set; }
        /// <summary>
        /// Anzahl der Zauberer der Klasse ZE.
        /// </summary>
        public int ZE { get; set; }
        /// <summary>
        /// Anzahl der Zauberer der Klasse ZF.
        /// </summary>
        public int ZF { get; set; }
    }
}