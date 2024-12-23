﻿using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View
{
    public static class SpielfigurenView
    {
        public static List<Spielfigur> GetSpielfiguren(GemarkPosition gem)
        {
            List<Spielfigur> result = [];
            var kreaturen = SharedData.Kreaturen?.Where(s => s.gf == gem.gf && s.kf == gem.kf);
            if (kreaturen != null)
                result.AddRange( kreaturen);

            var krieger = SharedData.Krieger?.Where(s => s.gf == gem.gf && s.kf == gem.kf);
            if (krieger != null) 
                result.AddRange(krieger);
            
            var reiter = SharedData.Reiter?.Where(s => s.gf == gem.gf && s.kf == gem.kf);
            if (reiter != null)
                result.AddRange(reiter);
            
            var schiffe = SharedData.Schiffe?.Where(s => s.gf == gem.gf && s.kf == gem.kf).ToArray();
            if (schiffe != null)
                result.AddRange(schiffe);
            
            return result;
        }

        
    }
}
