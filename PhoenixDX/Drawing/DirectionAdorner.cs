using Microsoft.Xna.Framework.Graphics;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;

namespace PhoenixDX.Drawing {
    /// <summary>
    /// Repräsentiert eine Adorner-Klasse, die Richtungsinformationen speichert und Texturen entsprechend zuordnet.
    /// </summary>
    internal abstract class DirectionAdorner : GemarkAdorner {
        int[] _value = { 0, 0, 0, 0, 0, 0 };
        bool _empty = false;

        /// <summary>
        /// Standardkonstruktor für DirectionAdorner.
        /// </summary>
        public DirectionAdorner() { }

        /// <summary>
        /// Erstellt eine Instanz von DirectionAdorner mit angegebenen Richtungswerten.
        /// </summary>
        /// <param name="NW">Wert für Nordwest.</param>
        /// <param name="NO">Wert für Nordost.</param>
        /// <param name="O">Wert für Osten.</param>
        /// <param name="SO">Wert für Südost.</param>
        /// <param name="SW">Wert für Südwest.</param>
        /// <param name="W">Wert für Westen.</param>
        public DirectionAdorner(int? NW, int? NO, int? O, int? SO, int? SW, int? W) : base() {
            _value[(int)Direction.NW] = (int)NW;
            _value[(int)Direction.NO] = (int)NO;
            _value[(int)Direction.O] = (int)O;
            _value[(int)Direction.SO] = (int)SO;
            _value[(int)Direction.SW] = (int)SW;
            _value[(int)Direction.W] = (int)W;
            _empty = _value[0] + _value[1] + _value[2] + _value[3] + _value[4] + _value[5] == 0;
        }

        /// <summary>
        /// Gibt die Richtungstextur zurück.
        /// </summary>
        /// <returns>Die Richtungstextur.</returns>
        protected abstract Drawing.DirectionTexture GetDirectionTexture();

        /// <summary>
        /// Erstellt eine Liste von Texturen basierend auf den Richtungswerten.
        /// </summary>
        /// <returns>Eine Liste von <see cref="Texture2D"/>.</returns>
        private List<Texture2D> GetTextures() {
            List<Texture2D> textures = new List<Texture2D>();
            foreach (Direction direction in Enum.GetValues(typeof(Direction))) {
                if (HasDirection(direction) > 0) {
                    var t = GetDirectionTexture();
                    textures.Add(t.GetTexture(direction));
                }
            }
            return textures;
        }

        /// <summary>
        /// Prüft, ob eine bestimmte Richtung gesetzt ist.
        /// </summary>
        /// <param name="direction">Die zu überprüfende Richtung.</param>
        /// <returns>Der Wert der Richtung.</returns>
        public int HasDirection(Direction direction) {
            return _value[(int)direction];
        }

        /// <summary>
        /// Gibt zurück, ob die Adorner keine aktiven Richtungen hat.
        /// </summary>
        public bool IsEmpty => _empty;

        /// <summary>
        /// Erstellt eine kombinierte Textur basierend auf den aktuellen Richtungswerten.
        /// </summary>
        /// <returns>Die erstellte <see cref="BaseTexture"/> oder null, wenn keine Texturen vorhanden sind.</returns>
        public BaseTexture CreateTexture() {
            if (IsEmpty)
                return null;

            string cacheKey = $"{GetType().Name} {_value[0]} {_value[1]} {_value[2]} {_value[3]} {_value[4]} {_value[5]}";
            if (TextureCache.Contains(cacheKey))
                return TextureCache.Get(cacheKey);

            List<Texture2D> list = GetTextures();
            if (list != null && list.Count > 0) {
                var texture = TextureCache.MergeTextures(list);
                TextureCache.Set(cacheKey, texture);
            }
            return null;
        }

        private BaseTexture _texture = null;

        /// <summary>
        /// Gibt die berechnete Textur zurück.
        /// </summary>
        /// <returns>Die erzeugte <see cref="BaseTexture"/>.</returns>
        public override BaseTexture GetTexture() {
            if (_texture == null)
                _texture = CreateTexture();
            return _texture;
        }
    }
}
