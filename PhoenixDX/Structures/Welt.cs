using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Helper;
using PhoenixDX.Drawing;
using PhoenixModel.Program;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PhoenixModel.ViewModel;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Die Klasse Welt repräsentiert die gesamte Spielwelt und verwaltet Provinzen, Reiche und Karteninformationen.
    /// </summary>
    public class Welt
    {
        /// <summary>
        /// Speicher der Provinzen
        /// </summary>
        Dictionary<int, Provinz> Provinzen = [] ;
        /// <summary>
        /// Speicher für die Reiche
        /// </summary>
        Dictionary<int, Reich> Reiche = [];

        /// <summary>
        /// Erstellt eine neue Instanz der Welt und initialisiert die Provinzen basierend auf der übergebenen Kartenstruktur.
        /// </summary>
        /// <param name="map">Die Kartenstruktur, die die KleinFelder aus SharedData.Map enthält.</param>
        public Welt(BlockingDictionary<KleinFeld> map) 
        {
            Provinzen.Add(701, new Provinz(7, 0, 701));
            Provinzen.Add(901, new Provinz(9, 0, 901));
            Provinzen.Add(712, new Provinz(7, 11, 712));
            Provinzen.Add(912, new Provinz(9, 11, 912));
            Provinzen.Add(6666, new Provinz(15, 11, 6666));
            Provinzen.Add(2001, new Provinz(1, 1, 2001));
            Provinzen.Add(4001, new Provinz(15, 1, 4001));

            for (int x = 1; x <= 15; x++)
            {
                for (int y = 1; y <= 11; y++)
                {
                    if ((x == 1 || x== 15) && y > 6)
                        continue;
                    if ((x == 2 || x == 14) && y > 7)
                        continue;
                    if ((x == 3 || x == 13) && y > 8)
                        continue;
                    if ((x == 4 || x == 12) && y > 9)
                        continue;
                    if ((x == 5 || x == 11) && y > 10)
                        continue;
                    int gf = x * 100 + y;
                    int yPos = y ;
                    int xPos = x; 
                    if (x < 6 )
                    {
                        yPos += 3 - x / 2 - x%2;
                    }
                    if (x > 6 && x <10)
                    {
                        yPos -= x %2;
                    }
                    if (x > 10)
                    {
                        yPos +=  x / 2 -5;
                    }
                    if (Provinzen.ContainsKey(gf) == false)
                        Provinzen.Add(gf, new Provinz(xPos,yPos, gf));
                }
            }

            foreach (KleinFeld gem in map.Values)
            {
                if (Plausibilität.IsValid(gem))
                {
                    if (Provinzen.ContainsKey(gem.gf) == false)
                    {
                        MappaMundi.Log(gem.gf, gem.kf, new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Großfeld {gem.gf} fehlt","In der Karte ist ein Großfeld eingetragen, das es rechnerisch nicht geben dürfte."));
                        continue;
                    }
                    var p = Provinzen[gem.gf];

                    if (p != null)
                    {
                        var k = p.GetOrCreateKleinfeld((int) gem.kf);
                        try
                        {
                            if (k.Initialize(gem) ==false)
                                continue; // TODO - doppelte Kleinfelder in der Datenbank
                        }
                        catch (Exception ex)
                        {
                            MappaMundi.Log(gem.gf, gem.kf,"Das Kleinfeld konnte nicht angelegt werden", ex);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Aktualisiert eine bestimmte Gemarkung (Kleinfeld) basierend auf der gegebenen Position.
        /// wird vom <see cref="BackgroundUpdater"/> verwendet, um aus der Queue die Gemarken zu holen und zu aktualisieren
        /// </summary>
        /// <param name="pos">Die Position der zu aktualisierenden Gemarkung.</param
        public void UpdateGemark(KleinfeldPosition pos)
        {
            if (pos.gf < 0 || pos.kf < 0 || pos.kf > 48)
                return;
            try
            {
                var provinz = Provinzen[pos.gf];
                var gemark = SharedData.Map[KleinFeld.CreateBezeichner(pos.gf, pos.kf)];
                Gemark updatedKleinfeld = new Gemark(pos.gf, pos.kf);
                updatedKleinfeld.Initialize(gemark);

                // reich verändert?
                if (Reiche.ContainsKey(updatedKleinfeld.ReichID) == true)
                    updatedKleinfeld.Reich = Reiche[updatedKleinfeld.ReichID];

                provinz.Felder[pos.kf] = updatedKleinfeld;
            }
            catch (Exception ex)
            {
                MappaMundi.Log(0, 0, $"Die Gemark {pos.gf}/{pos.kf} konnte nicht aktualisiert werden", ex);
            }

        }
        /// <summary>
        /// wir wollen nur 1x die Reiche hinzufügen
        /// </summary>
        public bool ReicheInitalized = false;
        /// <summary>
        /// Fügt der Welt Nationen hinzu und verknüpft sie mit den entsprechenden Reichen.
        /// </summary>
        /// <param name="nations">Die zu hinzuzufügenden Nationen.</param>
        public void AddNationen(BlockingCollection<Nation> nations)
        {
            ReicheInitalized = true;
            foreach (var nation in nations)
            {
                if (Reiche.ContainsKey(nation.Nummer) == false)
                    Reiche.Add(nation.Nummer, new Reich(nation));
                else
                    break;
            }

            foreach (var province in Provinzen.Values)
            {
                foreach (var kleinfeld in province.Felder.Values)
                {
                    if (Reiche.ContainsKey(kleinfeld.ReichID) == true)
                        kleinfeld.Reich = Reiche[kleinfeld.ReichID];
                }
            }
        }
        /// <summary>
        /// Lädt die notwendigen Inhalte für die Welt.
        /// </summary>
        /// <param name="contentManager">Der Content-Manager.</param>
        public static void LoadContent(ContentManager contentManager)
        {
            Gelaende.LoadContent(contentManager);
            Reich.LoadContent(contentManager);
            Gemark.LoadContent(contentManager);
        }
        /// <summary>
        /// Gibt das Kleinfeld einer bestimmten Provinz zurück.
        /// </summary>
        /// <param name="gf">Die ID des Großfelds.</param>
        /// <param name="kf">Die ID des Kleinfelds.</param>
        /// <returns>Das entsprechende Kleinfeld oder null, falls nicht vorhanden.</returns>
        internal Gemark GetKleinfeld(int gf, int kf)
        {
            if (Provinzen.TryGetValue(gf, out Provinz provinz))
            {
                return provinz.GetKleinfeld(kf);
            }
            return null;
        }
        /// <summary>
        /// Gibt eine Provinz basierend auf ihrer ID zurück.
        /// </summary>
        /// <param name="gf">Die ID des Großfelds.</param>
        /// <returns>Die entsprechende Provinz oder null, falls nicht vorhanden.</returns>
        internal Provinz GetProviz(int gf)
        {
            if (Provinzen.TryGetValue(gf, out Provinz provinz))
            {
                return provinz;
            }
            return null;
        }
        /// <summary>
        /// Gibt die physikalische Position eines bestimmten Kleinfelds in der Weltkarte zurück.
        /// </summary>
        /// <param name="gf">Die ID des Großfelds.</param>
        /// <param name="kf">Die ID des Kleinfelds.</param>
        /// <param name="scale">Der Skalierungsfaktor für die Position.</param>
        /// <returns>Die Position als Vector2 oder null, falls nicht vorhanden.</returns>
        public Vector2? GetPosition(int gf, int kf, Vector2 scale)
        {
            if (Provinzen.TryGetValue(gf, out Provinz provinz))
            {
               return provinz.GetMapPosition(scale);
            }
            return null;
        }

        /// <summary>
        /// Zeichnet die Weltkarte und gibt das ausgewählte Kleinfeld zurück.
        /// </summary>
        /// <param name="spriteBatch">Das SpriteBatch-Objekt für das Rendering.</param>
        /// <param name="scale">Der Skalierungsfaktor der Weltkarte.</param>
        /// <param name="mousePos">Die aktuelle Position der Maus.</param>
        /// <param name="isMoving">Gibt an, ob sich die Karte bewegt.</param>
        /// <param name="gameTime">Die aktuelle Spielzeit.</param>
        /// <param name="selected">Das aktuell ausgewählte Kleinfeld.</param>
        /// <param name="visibleScreen">Der sichtbare Bereich des Bildschirms.</param>
        /// <returns>Das gezeichnete und ausgewählte Kleinfeld.</returns>
        internal Gemark Draw(SpriteBatch spriteBatch, Vector2 scale, Vector2? mousePos, bool isMoving, TimeSpan gameTime, Gemark selected, Rectangle visibleScreen)
        {
            return WeltDrawer.Draw(spriteBatch, scale, mousePos, isMoving, ref Provinzen, gameTime, selected, visibleScreen );
        }    
    }
}
