﻿using PKHeX.Core.Searching;
using PKHeX.Core;
using System.Collections.Generic;
using System.Linq;
namespace WangPluginPkm
{
    internal class SearchDatabase
    {
        public static GameStrings GameStrings = GameInfo.GetStrings("zh");
        public static PKM SearchPKM(ISaveFileProvider SAV, IPKMView Editor, ushort species, int version, int form = 0, bool egg = false, int location = 0, int gender = 0, int r = 0)
        {
            List<IEncounterInfo> Results;
            IEncounterInfo enc;
            var setting = new SearchSettings
            {
                Species = species,
                SearchEgg = egg,
                Version = version,
            };
            var search = EncounterUtil.SearchDatabase(setting, SAV.SAV);
            var results = search.ToList();
            IEnumerable<IEncounterInfo> res = results;
            if (form != 0)
            {
                res = res.Where(pkm => pkm.Form == form);
                if (res.Count() != 0)
                    results = res.ToList();
            }
            PKM pk = Editor.Data;
            if (results.Count != 0)
            {
                Results = results;
                enc = Results[r];
                pk = enc.ConvertToPKM(SAV.SAV);
                if (location != 0)
                {
                    for (int i = 0; ; i++)
                    {
                        enc = Results[i];
                        pk = enc.ConvertToPKM(SAV.SAV);
                        if (pk.Met_Location == location)
                            break;
                    }
                }
                if (gender != 0)
                {
                    for (int i = 0; ; i++)
                    {
                        enc = Results[i];
                        pk = enc.ConvertToPKM(SAV.SAV);
                        if (pk.Gender == 1)
                            break;
                    }
                }

            }
            return pk;
        }
        public static PKM GetGodPkm(ISaveFileProvider SAV, IPKMView Editor, ushort species)
        {
            List<IEncounterInfo> Results;
            IEncounterInfo enc;
            var setting = new SearchSettings
            {
                Species = species,
                SearchEgg = false,
                Version = (int)SAV.SAV.Version,
            };
            var search = EncounterUtil.SearchDatabase(setting, SAV.SAV);
            var results = search.ToList();
            PKM pk = Editor.Data;
            if (results.Count != 0)
            {
                for (int i = 0; ; i++)
                {
                    Results = results;
                    enc = Results[i];
                    pk = enc.ConvertToPKM(SAV.SAV);
                    if (pk.Met_Location == 244)
                        break;
                }
            }
            return pk;
        }
        public static PKM MytheryPK(ISaveFileProvider SAV, ushort species, int version, int form = 0)
        {
            var db = EncounterEvent.GetAllEvents();
            var RawDB = new List<MysteryGift>(db);
            PKM pkc;
            List<PKM> p = new();
            IEnumerable<MysteryGift> res = RawDB;
            res = res.Where(pkm => pkm.Species == species);
            res = res.Where(pkm => pkm.Form == form);
            res = res.Where(pkm => pkm.Generation == version);
            var results = res.ToArray();
            if (results.Count() != 0)
            {
                foreach (MysteryGift gift in results)
                {

                    pkc = gift.ConvertToPKM(SAV.SAV);
                    // EntityConverter.TryMakePKMCompatible(pkc, pk, out var c, out pkc);
                    p.Add(pkc);
                }

            }
            return p[0];
        }
        public static bool MytheryPKList(ref List<PKM> L,ISaveFileProvider SAV, ushort species, int version, int form = 0)
        {
            var db = EncounterEvent.GetAllEvents();
            var RawDB = new List<MysteryGift>(db);
            PKM pkc;

            IEnumerable<MysteryGift> res = RawDB;
            res = res.Where(pkm => pkm.Species == species);
            res = res.Where(pkm => pkm.Form == form);
            res = res.Where(pkm => pkm.Generation == version);
            var results = res.ToArray();
            if (results.Count() != 0)
            {
                foreach (MysteryGift gift in results)
                {

                    pkc = gift.ConvertToPKM(SAV.SAV);
                    // EntityConverter.TryMakePKMCompatible(pkc, pk, out var c, out pkc);
                    L.Add(pkc);
                   
                }
                return true;
            }
            
               
                return false;
            
        }
        public static PKM MytheryLanguage(ISaveFileProvider SAV, ushort species, int version, int form = 0,int language=1)
        {
            var db = EncounterEvent.GetAllEvents();
            var RawDB = new List<MysteryGift>(db);
            PKM pkc;
            List<PKM> p = new();
            string OT = "";
            IEnumerable<MysteryGift> res = RawDB;
            res = res.Where(pkm => pkm.Species == species);
            res = res.Where(pkm => pkm.Form == form);
            res = res.Where(pkm => pkm.Generation == version);
            var results = res.ToArray();
            if (results.Count() != 0)
            {
                foreach (MysteryGift gift in results)
                {
                    var g = (WC8)gift;
                    OT = g.GetOT(language);
                    pkc = gift.ConvertToPKM(SAV.SAV);
                    p.Add(pkc);
                }
            }
            p[0].Language = language;
            p[0].OT_Name = OT;
            p[0].ClearNickname();
            return p[0];
        }
        public static string SearchMytheryGift(ushort species, int Generation, string otname,int SID16,int TID16, int form = 0)
        {
            var db = EncounterEvent.GetAllEvents();
            var RawDB = new List<MysteryGift>(db);
            string L="";
            IEnumerable<MysteryGift> res = RawDB;
            res = res.Where(pkm => pkm.Species == species);
            res = res.Where(pkm => pkm.Form == form);
            res = res.Where(pkm => pkm.Generation == Generation);
            res = res.Where(pkm => pkm.OT_Name==otname);
            res = res.Where(pkm=>pkm.SID16==SID16);
            res = res.Where(pkm => pkm.TID16 == TID16);
            var results = res.ToArray();
            WC6 WC6gift=new();
           
            if (results.Count() != 0)
            {
                MysteryGift gift = results[0];
                    
                    if (gift.Type == "WC6")
                        WC6gift= (WC6)gift;
                    
                    L += $"Name:{GameStrings.Species[WC6gift.Species]}"+" "+$"CardHeader:{WC6gift.CardHeader}\n";
              
                
            }
            else
                L = "无法匹配";
            return L;
        }

    }
}

