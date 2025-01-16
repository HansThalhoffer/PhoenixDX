using PhoenixModel.dbZugdaten;
using PhoenixModel.Program;
using PhoenixModel.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Die Klasse berechnet die Summenwerte einer Armee und zeigt diese in den Memberproperties an
    /// </summary>
    public class TruppenStatus {
        /// <summary>
        /// im Konstruktor erfolgt die Berechnung der Summenwerte 
        /// </summary>
        /// <param name="amy"></param>
        public TruppenStatus(Armee amy) {
            foreach (var item in amy) {
                if (item is Kreaturen kreatur) {
                    ProgramView.LogWarning("Kreaturen werden im Truppenstatus nicht erfasst", "Eventuell muss dies noch implementiert werden");
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
                    if (wiz.Klasse == Zaubererklasse.ZA) { this.ZA++; }
                    else if (wiz.Klasse == Zaubererklasse.ZB) { this.ZB++; }
                    else if (wiz.Klasse == Zaubererklasse.ZC) { this.ZC++; }
                    else if (wiz.Klasse == Zaubererklasse.ZD) { this.ZD++; }
                    else if (wiz.Klasse == Zaubererklasse.ZE) { this.ZE++; }
                    else if (wiz.Klasse == Zaubererklasse.ZF) { this.ZF++; }
                }
            }
        }

        // Readonly properties (initialized via the constructor)
        public int Krieger { get; set; }
        public int HF_Krieger { get; set; }
        public int LKP_Krieger { get; set; }
        public int SKP_Krieger { get; set; }
        public int Pferde_Krieger { get; set; }

        public int Reiter { get; set; }
        public int HF_Reiter { get; set; }
        public int LKP_Reiter { get; set; }
        public int SKP_Reiter { get; set; }
        public int Pferde_Reiter { get; set; }

        public int Schiffe { get; set; }
        public int HF_Schiffe { get; set; }
        public int LKS_Schiffe { get; set; }
        public int SKS_Schiffe { get; set; }

        public int HF_Gesamt { get; set; }

        public int ZA { get; set; }
        public int ZB { get; set; }
        public int ZC { get; set; }
        public int ZD { get; set; }
        public int ZE { get; set; }
        public int ZF { get; set; }
    }

}
