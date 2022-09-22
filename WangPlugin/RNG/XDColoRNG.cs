using PKHeX.Core;
using System;

namespace WangPlugin
{
    internal static class XDColoRNG
    {
        private const int shift = 16;
        private const int shift8 = 8;

        public static uint Next(uint seed) => XDRNG.Next(seed);
        public static bool GenPkm(ref PKM pk, uint seed, CheckRules r, uint Xor = 0)
        {
       
            var la = new LegalityAnalysis(pk);
            switch (pk.Species)
                {
                    case (int)Species.Umbreon or (int)Species.Eevee: // Colo Umbreon, XD Eevee
                        pk.TID = (int)((seed = XDRNG.Next(seed)) >> 16);
                        pk.SID = (int)((seed = XDRNG.Next(seed)) >> 16);
                        seed = XDRNG.Advance(seed, 2); // PID calls consumed
                        break;
                    case (int)Species.Espeon: // Colo Espeon
                        pk.TID = (int)((seed = XDRNG.Next(seed)) >> 16);
                        pk.SID = (int)((seed = XDRNG.Next(seed)) >> 16);
                        seed = XDRNG.Advance(seed, 9); // PID calls consumed, skip over Umbreon
                        break;
                }
                var A = XDRNG.Next(seed); // IV1
                var B = XDRNG.Next(A); // IV2
                var C = XDRNG.Next(B); // Ability?
                var D = XDRNG.Next(C); // PID
                var E = XDRNG.Next(D); // PID
                pk.PID = (D & 0xFFFF0000) | E >> 16;
            if (!r.CheckShiny(r, pk))
            {
                return false;
            }
            Span<int> IVs = stackalloc int[6];
            GetIVsInt32(IVs, A >> 16, B >> 16);
            pk.SetIVs(IVs);
            if (!r.CheckIV(r, pk))
            {
                return false;
            }
            la = new LegalityAnalysis(pk);
            if (!la.Info.PIDIVMatches)
            {
                return false;
            }
            pk.Nature = (int)(pk.PID % 100 % 25);
                pk.RefreshAbility((int)(pk.PID & 1));
               
            return true;
        }
        private static void GetIVsInt32(Span<int> result, uint r1, uint r2)
        {
            result[5] = (((int)r2 >> 10) & 0x1F);
            result[4] = (((int)r2 >> 5) & 0x1F);
            result[3] = (int)(r2 & 0x1F);
            result[2] = (((int)r1 >> 10) & 0x1F);
            result[1] = (((int)r1 >> 5) & 0x1F);
            result[0] = (int)(r1 & 0x1F);
        }
        private static bool CheckShiny(uint pid, int TID, int SID, bool[] shiny, uint Xor = 0)
        {
            var s = (uint)(TID ^ SID) ^ ((pid >> 16) ^ (pid & 0xFFFF));
            if (shiny[0])
                return true;
            else if (shiny[1] && s < 16)
                return true;
            else if (shiny[2] && s < 16 && s != 0)
                return true;
            else if (shiny[3] && s == 0)
                return true;
            else if (shiny[4] && s == 1)
                return true;
            else if (shiny[5] && s == Xor)
                return true;
            else
                return false;
        }

    }
}