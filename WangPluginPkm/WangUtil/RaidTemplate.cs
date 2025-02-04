﻿using System;
using System.Collections.Generic;
using PKHeX.Core;

namespace WangPluginPkm
{
    internal class RaidTemplate
    {
        public readonly ushort Species;
        public readonly uint[] Probabilities;
        public readonly int FlawlessIVs;
        public readonly int MinRank;
        public readonly int MaxRank;
        public readonly byte AltForm;
        public readonly bool IsGigantamax;
        public readonly int Ability;
        public readonly int Nature;
        public readonly int Gender;
        public readonly int[] FixedIV;
        public readonly sbyte ShinyType;

        public static readonly int[] ToxtricityAmplifiedNatures = { 0x03, 0x04, 0x02, 0x08, 0x09, 0x13, 0x16, 0x0B, 0x0D, 0x0E, 0x00, 0x06, 0x18 };
        public static readonly int[] ToxtricityLowKeyNatures = { 0x01, 0x05, 0x07, 0x0A, 0x0C, 0x0F, 0x10, 0x11, 0x12, 0x14, 0x15, 0x17 };

        public RaidTemplate(ushort species, uint[] probabilities, int flawlessIVs, byte altForm, int ability, int gender, bool giga, sbyte shinytype = 0) :
            this(species, probabilities, flawlessIVs, altForm, ability, gender, 25, giga, shinytype)
        {
        }

        public RaidTemplate(ushort species, uint[] probabilities, int flawlessIVs, byte altForm, int ability, int gender, int nature, bool giga, sbyte shinytype = 0)
        {
            Species = species;
            Probabilities = probabilities;
            FlawlessIVs = flawlessIVs;
            MinRank = Array.FindIndex(Probabilities, z => z != 0);
            MaxRank = Array.FindLastIndex(Probabilities, z => z != 0);
            AltForm = altForm;
            IsGigantamax = giga;
            Ability = ability;
            Gender = gender;
            Nature = nature;
            ShinyType = shinytype;
            FixedIV = new[] { -1, -1, -1, -1, -1, -1 };
        }

        public RaidTemplate(ushort species, int[] ivs, int rank, bool giga)
        {
            Species = species;
            MinRank = rank;
            MaxRank = rank;
            FixedIV = ivs;
            IsGigantamax = giga;
            Ability = 3;
            ShinyType = 0;
            FlawlessIVs = -1;
            Probabilities = new[] { 0u, 0u, 0u, 0u, 0u };
        }

        public bool CanObtainWith(int stars) => Probabilities[stars] > 0;

        public RaidPKM ConvertToPKM(ulong seed, uint TID16, uint SID16)
        {
            var rng = new Xoroshiro128Plus(seed);
            uint EC = (uint)rng.NextInt();
            uint SID16TID16 = (uint)rng.NextInt();
            uint PID = (uint)rng.NextInt();

            var shinytype = WangRandUtil.GetShinyType(PID, SID16TID16);
            if (ShinyType == 2 && shinytype == 0)
            {
                shinytype = 2;
            }
            if (ShinyType == 1)
            {
                shinytype = 3;
            }
            uint tsv = WangRandUtil.GetShinyValue((SID16 << 16) | TID16);
            PID = GetFinalPID(TID16, SID16, PID, SID16TID16, tsv, ShinyType);

            int[] ivs = { -1, -1, -1, -1, -1, -1 };
            for (int i = 0; i < 6; i++)
            {
                ivs[i] = FixedIV[i];
            }
            int deviation = -FlawlessIVs;
            for (int i = 0; i < FlawlessIVs; i++)
            {
                int idx;
                do
                {
                    do
                    {
                        idx = (int)rng.Next() & 7;
                        deviation++;
                    } while (idx >= 6);
                } while (ivs[idx] != -1);
                ivs[idx] = 31;
            }
            for (int i = 0; i < 6; i++)
            {
                if (ivs[i] == -1)
                    ivs[i] = (int)rng.NextInt(32);
            }

            int abilityIdx = 0;
            Span<int> abilities = stackalloc int[10]; ;
            PersonalTable.SWSH.GetFormEntry(Species, AltForm).GetAbilities(abilities);
            if (Ability < 3)
            {
                abilityIdx = Ability;
            }
            if (Ability == 3)
            {
                abilityIdx = (int)rng.NextInt(2);
            }
            else if (Ability == 4)
            {
                abilityIdx = (int)rng.NextInt(3);
            }
            var ability = abilities[abilityIdx];
            // gender 
            int gt = PersonalTable.SWSH[Species].Gender;
            var gender = gt switch
            {
                255 => 2, // Genderless
                254 => 1, // Female-Only
                0 => 0, // Male-Only
                _ => ((int)rng.NextInt(253) + 1 < gt ? 1 : 0)
            };

            int nature;
            if (Nature == 25)
            {
                if (Species == (int)PKHeX.Core.Species.Toxtricity)
                {
                    var table = AltForm == 0 ? ToxtricityAmplifiedNatures : ToxtricityLowKeyNatures;
                    nature = table[rng.NextInt((uint)table.Length)];
                }
                else
                {
                    nature = (int)rng.NextInt(25);
                }
            }
            else
            {
                nature = Nature;
            }

            return new RaidPKM(Species, AltForm, EC, PID, ivs, ability, abilityIdx, gender, nature, deviation, shinytype, IsGigantamax, ShinyType);
        }

        public static uint GetFinalPID(uint TID16, uint SID16, uint new_pid, uint TID16SID16, uint tsv, sbyte fixedShiny)
        {
            var shinytype = WangRandUtil.GetShinyType(new_pid, TID16SID16);
            if (fixedShiny == 2 && shinytype == 0)
            {
                shinytype = 2;
            }
            if (fixedShiny == 1)
            {
                shinytype = 0;
            }
            uint psv = WangRandUtil.GetShinyValue(new_pid);
            if (shinytype == 0)
            {
                if (psv == tsv)
                    return new_pid ^ 0x10000000; // ensure no shiny
                return new_pid;
            }

            if (psv == tsv)
                return new_pid; // already shiny
            return (new_pid & 0xFFFF) | (TID16 ^ SID16 ^ new_pid ^ (2 - shinytype)) << 16;
        }
    }
}
