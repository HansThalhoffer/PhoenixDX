using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View
{
    public static class BauwerkeView
    {
        public static Rüstort? GetRuestortReferenz(int? nummer)
        {
            if (nummer == null || nummer < 1 || SharedData.RüstortReferenz == null) 
                return null;
            return SharedData.RüstortReferenz.ElementAt(nummer.Value-1);
        }

        public static Rüstort? GetRuestortNachBaupunkten(int baupunkte)
        {
            if (baupunkte < 1 || SharedData.RüstortReferenz == null)
                return null;
            Rüstort? rnbp = null;
            if (Rüstort.NachBaupunkten.ContainsKey(baupunkte))
                rnbp = Rüstort.NachBaupunkten[baupunkte];
            else
            {
                int bp = baupunkte - baupunkte % 250;
                while (Rüstort.NachBaupunkten.ContainsKey(bp) == false && bp > 0)
                    bp -= 250;
                if (bp > 0)
                {
                    rnbp = Rüstort.NachBaupunkten[bp];
                }
            }
            return rnbp;
        }

        public static IEnumerable<Gebäude>? GetGebäude(Nation nation)
        {
            if (SharedData.Gebäude != null)
                return SharedData.Gebäude.Values?.Where(s => s.Nation == nation);
            return null;
        }

        public static int? GetBaupunkteNachKarte(KleinfeldPosition pos)
        {
            if (SharedData.Map == null)
                return null;
            try
            {
                return SharedData.Map[pos.CreateBezeichner()].Baupunkte;
            }
            catch (Exception ex) 
            {
                ViewModel.LogError(pos, "Kleinfeld existiert nicht", ex.Message);
                return null; 
            }
        }

        /*public static int? GetRüstortNachKarte(KleinfeldPosition pos)
        {
            if (SharedData.Map == null)
                return null;
            try
            {
                return SharedData.Map[pos.CreateBezeichner()].Ruestort;
            }
            catch (Exception ex)
            {
                ViewModel.LogError(pos, "Kleinfeld existiert nicht", ex.Message);
                return null;
            }
        }*/

        public static Rüstort? GetRüstortNachKarte(KleinfeldPosition pos)
        {
            if (SharedData.Map == null)
                return null; 
            var gemark = SharedData.Map[pos.CreateBezeichner()];

            Rüstort? rüstortLautKarte = BauwerkeView.GetRuestortReferenz(gemark.Ruestort);
            if (rüstortLautKarte != null)
                return rüstortLautKarte;
            
            if (Rüstort.NachBaupunkten.ContainsKey(gemark.Baupunkte))
                return Rüstort.NachBaupunkten[gemark.Baupunkte];
            else
            {
                int bp = gemark.Baupunkte - gemark.Baupunkte % 250;
                while (Rüstort.NachBaupunkten.ContainsKey(bp) == false && bp > 0)
                    bp -= 250;
                if (bp > 0)
                {
                    return Rüstort.NachBaupunkten[bp];
                }
                return null;
            }
        }

        public static Gebäude? GetGebäude(KleinFeld gemark)
        {
                if (SharedData.Gebäude == null)
                    throw new Exception("Die Bauwerliste muss vor denen Kartendaten gelasen werden");
                if (SharedData.RüstortReferenz == null)
                    throw new Exception("Die Rüstort Referenzdaten müssen vor denen Kartendaten gelasen werden");
                if (gemark.Baupunkte == 0)
                    return null;
                try
                {
                    Gebäude? gebäude = null;
                    // lookup in Bauwerktabelle
                    if (SharedData.Gebäude.ContainsKey(gemark.Bezeichner))
                        gebäude = SharedData.Gebäude[gemark.Bezeichner];
                    // ergänzt die Datenbank falls notwendig
                    if (gebäude == null)
                    {
                        ViewModel.LogError( gemark, $"Fehlendes Gebäude in der Bauwerktabelle mit dem Namen {gemark.Bauwerknamen}", "Durch einen Datenbankfehler hat das Gebäude keinen Eintrag in der Tabelle [bauwerkliste] in der Datenbank Ekrenfarakarte.mdb");    
                    }
                    return gebäude;
                }
                catch (Exception ex)
                {
                    // auf Ebene des Modells werden keine Exceptions gefangen
                    throw new Exception($"Ausnahme bei der Festlegung des Geäudes auf Kleinfeld {gemark.Bezeichner}", ex);
                }
            }
        }
    }

