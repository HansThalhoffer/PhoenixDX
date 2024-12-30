﻿using Microsoft.VisualBasic.FileIO;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Program
{
    public static class ViewModel
    {
        public static Nation? SelectedNation { get; set; }

        public delegate void ViewEventHandler(object? sender, ViewEventArgs e);
        public static event ViewEventHandler? OnViewEvent;

        private static void _OnViewEvent(ViewEventArgs args)
        {
            if (OnViewEvent == null)
                return;
            OnViewEvent(null, args);
        }

        public static void LogError(int gf, int kf, string msg)
        {
            _OnViewEvent(new ViewEventArgs(gf,kf, new LogEntry(LogEntry.LogType.Error,msg)));
        }

        /// <summary>
        /// überprüft den Besitz der KleinFeld/des Kleinfelds
        /// </summary>
        /// <param name="position">eine Gemarkpostion mit validem Bezeichner</param>
        /// <returns>wahr, wenn die übergebene KleinFeld/Kleinfeld zu der Nation/Nation des authentifizeriten Nutzers gehört</returns>
        public static bool BelongsToUser(Spielfigur figur)
        {
            if (SharedData.Map == null)
            {
                ViewModel.LogError($"Die Kartendaten sind nicht geladen, daher kann ein Besitz einer Spielfigur nicht ermittelt werden");
                return false;
            }            
            if (ViewModel.SelectedNation == null)
            {
                ViewModel.LogError($"Ohne eine ausgewählte Nation kann kein Besitz einer Spielfigur ermittelt werden");
                return false;
            }
            if (figur.Nation == null)
            {
                ViewModel.LogError($"Der Spielfigur {figur.Bezeichner} ist keine Nation zugeordnet, daher kann der Besitz nicht ermittelt werden");
                return false;
            }

            // das Kleinfeld des Rüstotes gehört evtl. zum Nation des aktuellen Nutzers und kann daher ausgeählt werden
            return ViewModel.SelectedNation == figur.Nation;
        }

        /// <summary>
        /// überprüft den Besitz der KleinFeld/des Kleinfelds
        /// </summary>
        /// <param name="position">eine Gemarkpostion mit validem Bezeichner</param>
        /// <returns>wahr, wenn die übergebene KleinFeld/Kleinfeld zu der Nation/Nation des authentifizeriten Nutzers gehört</returns>
        public static bool BelongsToUser( KleinfeldPosition position)
        {
            if (SharedData.Map == null)
            {
                ViewModel.LogError($"Die Kartendaten sind nicht geladen, daher kann ein Besitz eines Kleinfeldes nicht ermittelt werden");
                return false;
            }
            if (SharedData.Map.ContainsKey(position.CreateBezeichner()) == false)
            {
                ViewModel.LogError($"Die Position {position.CreateBezeichner()} befindet sich auf einem nicht existenten Kleinfeld");
                return false;
            }
            if (ViewModel.SelectedNation == null)
            {
                ViewModel.LogError($"Ohne eine ausgewählte Nation kann kein Besitz eines Kleinfeldes ermittelt werden");
                return false;
            }
            // das Kleinfeld des Rüstotes gehört evtl. zum Nation des aktuellen Nutzers und kann daher ausgeählt werden
            return ViewModel.SelectedNation == SharedData.Map[position.CreateBezeichner()].Nation;
        }

        public static void LogError(string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Error, msg)));
        }
        public static void LogWarning(string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Warning, msg)));
        }
        public static void LogInfo(string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Info, msg)));
        }
        public static void LogError(KleinfeldPosition pos, string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Error, $"[{pos.CreateBezeichner()}] {msg}")));
        }
        public static void LogWarning(KleinfeldPosition pos, string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Warning, $"[{pos.CreateBezeichner()}] {msg}")));
        }
        public static void LogInfo(KleinfeldPosition pos, string msg)
        {
            _OnViewEvent(new ViewEventArgs(0, 0, new LogEntry(LogEntry.LogType.Info, $"[{pos.CreateBezeichner()}] {msg}")));
        }
    }
}
