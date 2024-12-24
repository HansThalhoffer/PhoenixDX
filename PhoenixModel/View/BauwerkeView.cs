using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View
{
    public static class BauwerkeView
    {
        // die Funktion beseitigt Fehler in den Datenbanken
        public static Gebäude? GetGebäude(Gemark gemark)
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
                    if (SharedData.Gebäude.ContainsKey(gemark.Bezeichner))
                        gebäude = SharedData.Gebäude[gemark.Bezeichner];
                    if (gebäude == null)
                    {
                        gebäude = new Gebäude();
                        gebäude.kf = gemark.kf;
                        gebäude.gf = gemark.gf;
                        gebäude.Bauwerknamen = gemark.Bauwerknamen;
                        SharedData.Gebäude.Add(gebäude.Bezeichner, gebäude);
                    }
                    if (gebäude.Rüstort == null)
                    {
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

