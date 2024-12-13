using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Database
{
    public static class DatabaseConverter
    {
        /// <summary>
        /// converts a database field object to Int
        /// </summary>
        static public int ToInt32(object o)
        {
            try
            {
                if (o == null)
                    throw new ArgumentNullException("Unbekanntes Feld in der Tabelle");
                if (o is DBNull)
                    return 0;
                if (o.GetType() == typeof(int))
                    return Convert.ToInt32(o);
                if (o.GetType() == typeof(double))
                    return Convert.ToInt32(o);
                if (o.GetType() == typeof(float))
                    return Convert.ToInt32(o);

                string? s = o.ToString();
                if (String.IsNullOrEmpty(s))
                    return 0;
                return int.Parse(s);
            }
            catch { }
            return -1;
        }

        /// </summary>
        static public bool ToBool(object o)
        {
            try
            {
                if (o == null)
                    throw new ArgumentNullException("Unbekanntes Feld in der Tabelle");
                if (o is DBNull)
                    return false;
                if (o.GetType() == typeof(int))
                    return Convert.ToInt32(o) > 0;
                if (o.GetType() == typeof(double))
                    return Convert.ToInt32(o) > 0;
                if (o.GetType() == typeof(float))
                    return Convert.ToInt32(o) > 0;
                if (o.GetType() == typeof(bool))
                    return Convert.ToBoolean(o);
            }
            catch { }
            return false;
        }

        /// <summary>
        /// converts a database field object to Int
        /// </summary>
        static public string ToString(object o)
        {
            if (o == null)
                throw new ArgumentNullException("Unbekanntes Feld in der Tabelle");
            if (o is DBNull)
                return string.Empty;
            string? s = o.ToString();
            if (String.IsNullOrEmpty(s))
                return string.Empty;
            return s;
        }
    }
}
