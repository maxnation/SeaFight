using System.Collections.Generic;
using System;

namespace SeaFightLogic
{
    public class RemotenessFromCenterComparer : IComparer<Ship>
    {
        public int Compare(Ship s1, Ship s2)
        {
            int s1X = s1.Head.X;
            int s1Y = s1.Head.Y;

            int s2X = s2.Head.X;
            int s2Y = s2.Head.Y;

            int s1Distance = (int)Math.Sqrt(Math.Pow(s1X, 2) + Math.Pow(s1Y, 2));
            int s2Distance = (int)Math.Sqrt(Math.Pow(s2X, 2) + Math.Pow(s2Y, 2));

            if (s1Distance == s2Distance)
            {
                return 0;
            }
            return s1Distance > s2Distance ? 1 : -1;

         }
    }
}