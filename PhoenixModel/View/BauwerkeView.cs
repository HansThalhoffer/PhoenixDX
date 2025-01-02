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

        public static int? GetRüstortNachKarte(KleinfeldPosition pos)
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
        }

        // die Funktion beseitigt Fehler in den Datenbanken
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
                        ViewModel.LogWarning( gemark, $"Fehlendes Gebäude in der Bauwerktabelle mit dem Namen {gemark.Bauwerknamen}", "Durch einen Datenbankfehler hat das Gebäude keinen Eintrag in der Referenztabelle");    
                        gebäude = new Gebäude();
                        gebäude.kf = gemark.kf;
                        gebäude.gf = gemark.gf;
                        gebäude.Bauwerknamen = gemark.Bauwerknamen;
                        SharedData.Gebäude.Add(gebäude.Bezeichner, gebäude);
                    }
                    // findt und bereinigt den Eintrag zum Rüstort im Gebäude
                    if (gebäude.Rüstort == null)
                    {
                        Rüstort? rüstortLautKarte = GetRuestortReferenz(gemark.Ruestort);
                        if (Rüstort.NachBaupunkten.ContainsKey(gemark.Baupunkte))
                            gebäude.Rüstort = Rüstort.NachBaupunkten[gemark.Baupunkte];
                        else
                        {
                            int bp = gemark.Baupunkte - gemark.Baupunkte % 250;
                            while (Rüstort.NachBaupunkten.ContainsKey(bp) == false && bp > 0)
                                bp -= 250;
                            if (bp > 0)
                            {
                                gebäude.InBau = true;
                                gebäude.Rüstort = Rüstort.NachBaupunkten[bp];
                            }
                            else
                            {
                                gebäude.Zerstört = true;
                            }
                        }
                    if (gebäude.Rüstort != rüstortLautKarte)
                            ViewModel.LogWarning(gemark, $"Unterschiedliches Gebäude nach Baupunkten ({gemark.Baupunkte}):{gebäude.Rüstort?.Bezeichner} und Karte: {rüstortLautKarte?.Bezeichner}", "Durch einen Datenbankfehler hat das Gebäude keinen oder fehlerhafte Einträg in den Tabellen. Diese sind nicht synchron.");
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

