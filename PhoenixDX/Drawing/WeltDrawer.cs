﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Helper;
using PhoenixDX.Structures;
using System;
using System.Collections.Generic;

using Vektor = Microsoft.Xna.Framework.Vector2;

namespace PhoenixDX.Drawing {
    /// <summary>
    /// Statische Klasse zur Darstellung der Weltkarte.
    /// </summary>
    internal static class WeltDrawer {
        static Texture2D _weiss = null;
        static Texture2D _selection1 = null;
        static Texture2D _selection2 = null;
        static public bool ShowReichOverlay = false;
        static public bool ShowKüsten = false;

        /// <summary>
        /// Lädt die benötigten Texturen in den Speicher.
        /// </summary>
        /// <param name="contentManager">Der ContentManager zum Laden der Ressourcen.</param>
        public static void LoadContent(ContentManager contentManager) {
            _weiss = contentManager.Load<Texture2D>("Images/hexweiss");
            _selection1 = contentManager.Load<Texture2D>("Images/maus1");
            _selection2 = contentManager.Load<Texture2D>("Images/maus2");
        }

        static double _tick = 0;
        static double _animated = 0;
        static bool _toggle = false;

        /// <summary>
        /// Zeichnet die Weltkarte mit allen Provinzen und Feldern.
        /// </summary>
        /// <param name="spriteBatch">Das SpriteBatch-Objekt zum Zeichnen der Texturen.</param>
        /// <param name="scale">Der Skalierungsfaktor der Karte.</param>
        /// <param name="mousePos">Die aktuelle Position der Maus (optional).</param>
        /// <param name="isMoving">Gibt an, ob sich die Karte bewegt.</param>
        /// <param name="provinzen">Die Liste aller Provinzen.</param>
        /// <param name="gameTime">Die aktuelle Spielzeit.</param>
        /// <param name="selected">Die aktuell ausgewählte Gemark.</param>
        /// <param name="visibleScreen">Der sichtbare Bereich des Bildschirms.</param>
        /// <returns>Die Gemark, über die sich die Maus befindet.</returns>
        public static Gemark Draw(SpriteBatch spriteBatch, Vektor scale, Vektor? mousePos, bool isMoving,
            ref Dictionary<int, Provinz> provinzen, TimeSpan gameTime, Gemark selected, Rectangle visibleScreen) {
            _animated += gameTime.TotalMilliseconds - _tick;
            _tick = gameTime.TotalMilliseconds;
            if (_animated > 100d) {
                _toggle = !_toggle;
                _animated = 0;
            }

            SpriteFont font = FontManager.Fonts["Small"];
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            Vektor offset = new Vektor(30f * scale.X, 10f * scale.Y);
            Vektor mausPos = Vektor.Zero;
            if (mousePos.HasValue) {
                mausPos = new Vektor(mousePos.Value.X, mousePos.Value.Y);
            }
            Gemark mouseover = null;

            // Zeichnet die Karte mit Culling
            foreach (var province in provinzen.Values) {
                var posP = province.GetMapPosition(scale); // Aktualisiert die MapSize - Reihenfolge wichtig
                posP.X += offset.X;
                posP.Y += offset.Y;
                if (posP.X > visibleScreen.Right || posP.Y > visibleScreen.Bottom)
                    continue;
                var sizeP = province.GetMapSize();
                if (posP.X + sizeP.X < visibleScreen.Left || posP.Y + sizeP.Y + 50 < visibleScreen.Top)
                    continue;

                foreach (var gemark in province.Felder.Values) {
                    var posG = gemark.GetMapPosition(posP, scale);
                    var sizeG = Gemark.GetMapSize();
                    Rectangle rScreenG = new Rectangle(Convert.ToInt32(posG.X), Convert.ToInt32(posG.Y), Convert.ToInt32(sizeG.X), Convert.ToInt32(sizeG.Y));
                    bool inKleinfeld = isMoving == false && mouseover == null && gemark.InKleinfeld(mausPos);
                    if (inKleinfeld)
                        mouseover = gemark;

                    var listTexture = gemark.GetTextures(0);
                    foreach (var baseTexture in listTexture) {
                        if (baseTexture is ColoredTexture colored)
                            spriteBatch.Draw(baseTexture.Texture, rScreenG, null, inKleinfeld ? Color.Plum : colored.Color);
                        else if (baseTexture is OpacityTexture opacity)
                            spriteBatch.Draw(baseTexture.Texture, rScreenG, null, inKleinfeld ? Color.Plum * opacity.Opacity : Color.White * opacity.Opacity);
                        else
                            spriteBatch.Draw(baseTexture.Texture, rScreenG, null, inKleinfeld ? Color.Plum : Color.White);
                    }
                }
            }

            // Zeichnet weitere Ebenen und Overlays
            foreach (var province in provinzen.Values) {
                var posP = province.GetMapPosition(scale);
                posP.X += offset.X;
                posP.Y += offset.Y;
                if (posP.X > visibleScreen.Right || posP.Y > visibleScreen.Bottom)
                    continue;
                var sizeP = province.GetMapSize();
                if (posP.X + sizeP.X < visibleScreen.Left || posP.Y + sizeP.Y + 50 < visibleScreen.Top)
                    continue;

                foreach (var gemark in province.Felder.Values) {
                    var posG = gemark.GetMapPosition(posP, scale);
                    var sizeG = Gemark.GetMapSize();
                    Rectangle rScreenG = new Rectangle(Convert.ToInt32(posG.X), Convert.ToInt32(posG.Y), Convert.ToInt32(sizeG.X), Convert.ToInt32(sizeG.Y));
                    bool inKleinfeld = isMoving == false && mouseover == null && gemark.InKleinfeld(mausPos);
                    if (inKleinfeld)
                        mouseover = gemark;

                    var listTexture = gemark.GetTextures(1);
                    foreach (var baseTexture in listTexture) {
                        if (baseTexture is ColoredTexture colored)
                            spriteBatch.Draw(baseTexture.Texture, rScreenG, null, inKleinfeld ? Color.Plum : colored.Color);
                        else if (baseTexture is OpacityTexture opacity)
                            spriteBatch.Draw(baseTexture.Texture, rScreenG, null, inKleinfeld ? Color.Plum * opacity.Opacity: Color.White * opacity.Opacity);
                        else
                            spriteBatch.Draw(baseTexture.Texture, rScreenG, null, inKleinfeld ? Color.Plum : Color.White);
                    }

                    if (ShowReichOverlay && gemark.ReichID > 0 && gemark.Reich != null) {
                        spriteBatch.Draw(_weiss, rScreenG, null, inKleinfeld ? Color.Plum : gemark.Reich.color * 0.5f);
                    }
                    if (gemark == selected) {
                        if (_toggle)
                            spriteBatch.Draw(_selection1, rScreenG, null, Color.White);
                        else
                            spriteBatch.Draw(_selection2, rScreenG, null, Color.White);
                    }
                }
            }
            spriteBatch.End();
            return mouseover;
        }
    }
}
