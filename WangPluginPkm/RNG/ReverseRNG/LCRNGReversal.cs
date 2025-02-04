﻿using System;
using PKHeX.Core;
namespace WangPluginPkm
{
    internal class LCRNGReversal
    {
        private const int cacheSize = 1 << 16;
        private static readonly byte[] low8 = new byte[cacheSize];
        private static readonly bool[] flags = new bool[cacheSize];
        private const uint Mult = LCRNG.Mult;
        private const uint Add = LCRNG.Add;
        private const uint k2 = Mult << 8;
        static LCRNGReversal()
        {
            // Populate Meet Middle Arrays
            var f = flags;
            var b = low8;
            for (uint i = 0; i <= byte.MaxValue; i++)
            {
                uint right = (Mult * i) + Add;
                ushort val = (ushort)(right >> 16);
                f[val] = true; b[val] = (byte)i;
                --val;
                f[val] = true; b[val] = (byte)i;
            }
        }
        public static int GetSeeds(Span<uint> result, uint pid,bool IsUnown=false)
        {
            uint first = pid << 16;
            uint second = pid & 0xFFFF_0000;
            if(IsUnown)
            {
                first= pid & 0xFFFF_0000;
                second= pid << 16;
            }
            return GetSeeds(result, first, second);
        }
        public static int GetSeeds(Span<uint> result, uint first, uint second)
        {
            int ctr = 0;
            uint k1 = second - (first * Mult);
            for (uint i = 0, k3 = k1; i <= 255; ++i, k3 -= k2)
            {
                ushort val = (ushort)(k3 >> 16);
                if (!flags[val])
                    continue;
                // Verify PID calls line up
                var seed = first | (i << 8) | low8[val];
                var next = LCRNG.Next(seed);
                if ((next & 0xFFFF0000) == second)
                    result[ctr++] = LCRNG.Prev(seed);
            }
            return ctr;
        }
        public static int GetSeedsIVs(Span<uint> result, uint hp, uint atk, uint def, uint spa, uint spd, uint spe)
        {
            uint first = (hp | (atk << 5) | (def << 10)) << 16;
            uint second = (spe | (spa << 5) | (spd << 10)) << 16;
            return GetSeedsIVs(result, first, second);
        }
        public static int GetSeedsIVs(Span<uint> result, uint first, uint second)
        {
            int ctr = 0;
            // Check with the top bit of the first call both
            // flipped and unflipped to account for only knowing 15 bits
            uint search1 = second - (first * Mult);
            uint search2 = second - ((first ^ 0x80000000) * Mult);

            for (uint i = 0; i <= 255; i++, search1 -= k2, search2 -= k2)
            {
                ushort val = (ushort)(search1 >> 16);
                if (flags[val])
                {
                    // Verify PID calls line up
                    var seed = first | (i << 8) | low8[val];
                    var next = LCRNG.Next(seed);
                    if ((next & 0x7FFF0000) == second)
                    {
                        var origin = LCRNG.Prev(seed);
                        result[ctr++] = origin;
                        result[ctr++] = origin ^ 0x80000000;
                    }
                }

                val = (ushort)(search2 >> 16);
                if (flags[val])
                {
                    // Verify PID calls line up
                    var seed = first | (i << 8) | low8[val];
                    var next = LCRNG.Next(seed);
                    if ((next & 0x7FFF0000) == second)
                    {
                        var origin = LCRNG.Prev(seed);
                        result[ctr++] = origin;
                        result[ctr++] = origin ^ 0x80000000;
                    }
                }
            }
            return ctr;
        }
        public static int[] SetValuesFromSeedLCRNG(PKM pk,  uint seed)
        {
            var A = LCRNG.Next(seed);
            var B = LCRNG.Next(A);
            pk.PID = (B & 0xFFFF0000) | (A >> 16);
            var C = LCRNG.Next(B);
            var D = LCRNG.Next(C);
            Span<int> IVs = stackalloc int[6];
            GetIVsInt32(IVs, C >> 16, D >> 16);
            pk.SetIVs(IVs);
            var r=IVs.ToArray();
            return r;
        }
        internal static void GetIVsInt32(Span<int> result, uint r1, uint r2)
        {
            result[5] = (int)r2 >> 10 & 31;
            result[4] = (int)r2 >> 5 & 31;
            result[3] = (int)r2 & 31;
            result[2] = (int)r1 >> 10 & 31;
            result[1] = (int)r1 >> 5 & 31;
            result[0] = (int)r1 & 31;
        }

    }

}
