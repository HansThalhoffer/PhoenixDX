using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Database {
    /// <summary>
    /// Statische Hilfsklasse für die Konvertierung von Datenbankfeldern in gängige Datentypen.
    /// </summary>
    public static class DatabaseConverter {
        /// <summary>
        /// Maskiert einfache Anführungszeichen in Zeichenketten für SQL-Abfragen und behandelt null-Werte.
        /// </summary>
        /// <param name="value">Die zu maskierende Zeichenkette.</param>
        /// <returns>Die maskierte Zeichenkette oder ein leerer String, falls null.</returns>
        public static string EscapeString(string? value) {
            return value?.Replace("'", "''") ?? string.Empty;
        }

        /// <summary>
        /// Konvertiert ein Datenbankfeld-Objekt in einen Integer-Wert.
        /// </summary>
        /// <param name="o">Das zu konvertierende Objekt.</param>
        /// <returns>Den Integer-Wert des Objekts oder -1 im Fehlerfall.</returns>
        /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn das Objekt null ist.</exception>
        public static int ToInt32(object o) {
            try {
                if (o == null)
                    throw new ArgumentNullException("Unbekanntes Feld in der Tabelle");
                if (o is DBNull)
                    return 0;
                if (o.GetType() == typeof(int) || o.GetType() == typeof(double) || o.GetType() == typeof(float))
                    return Convert.ToInt32(o);

                string? s = o.ToString();
                if (String.IsNullOrEmpty(s))
                    return 0;

                return int.Parse(s);
            }
            catch { }
            return -1;
        }

        /// <summary>
        /// Konvertiert ein Datenbankfeld-Objekt in einen booleschen Wert.
        /// </summary>
        /// <param name="o">Das zu konvertierende Objekt.</param>
        /// <returns>True, wenn das Objekt einen positiven Wert darstellt, sonst false.</returns>
        /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn das Objekt null ist.</exception>
        public static bool ToBool(object o) {
            try {
                if (o == null)
                    throw new ArgumentNullException("Unbekanntes Feld in der Tabelle");
                if (o is DBNull)
                    return false;
                if (o.GetType() == typeof(int) || o.GetType() == typeof(double) || o.GetType() == typeof(float))
                    return Convert.ToInt32(o) > 0;
                if (o.GetType() == typeof(bool))
                    return Convert.ToBoolean(o);
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Konvertiert ein Datenbankfeld-Objekt in eine Zeichenkette.
        /// </summary>
        /// <param name="o">Das zu konvertierende Objekt.</param>
        /// <returns>Die Zeichenkette des Objekts oder ein leerer String, falls DBNull oder null.</returns>
        /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn das Objekt null ist.</exception>
        public static string ToString(object o) {
            if (o == null)
                throw new ArgumentNullException("Unbekanntes Feld in der Tabelle");
            if (o is DBNull)
                return string.Empty;

            string? s = o.ToString();
            return String.IsNullOrEmpty(s) ? string.Empty : s;
        }
    }
}
