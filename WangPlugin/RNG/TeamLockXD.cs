using PKHeX.Core;
using System;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace WangPlugin
{
    internal static class TeamLockXD
    {
        public static bool GenPkm(uint seed,ushort s,int N,int G,int Ratio)
        {
            var A = XDRNG.Next(seed); // IV1
            var B = XDRNG.Next(A); // IV2
            var C = XDRNG.Next(B); // Ability?
            var D = XDRNG.Next(C); // PID
            var E = XDRNG.Next(D); // PID
            var PID = (D & 0xFFFF0000) | E >> 16;
            var Gender = ((PID & 0xFF) < Ratio ? 1 : 0);
            var Nature = (int)(PID % 25);
            if (Gender == G && Nature == N)
            {
                return true;
            }
            return false;
        }
       
    }
}