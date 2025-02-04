﻿using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangPluginPkm.WangUtil.DexBase
{
    internal class SquawkabillyDex
    {
        public static List<PKM> SquawkabillySets(ISaveFileProvider SAV, IPKMView Editor)
        {
            List<PKM> PKL = new();
            PKM pk;
            switch (SAV.SAV.Version)
            {
                case GameVersion.SL or GameVersion.VL or GameVersion.SV:
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            pk = SearchDatabase.SearchPKM(SAV, Editor, (int)Species.Squawkabilly, 50, j, false);
                            pk.ClearNickname();
                            PKL.Add(pk);
                        }
                    }
                    break;
                
            }
            return PKL;
        }
    }
}
