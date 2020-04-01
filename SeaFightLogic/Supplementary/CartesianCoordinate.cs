using System;

namespace SeaFightLogic.Supplementary
{
    public struct CartesianCoordinate
    {
        public int X { get; }
        public int Y { get; }
        public int Quadrant { get; }

        public CartesianCoordinate(int x, int y, int quadrant)
        {
            if (quadrant < 0 || quadrant > 3)
            {
                throw new ArgumentOutOfRangeException("Quadrant value must be from 0 to 3");
            }
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException("X must be interger positive number");
            }
            if (y < 0)
            {
                throw new ArgumentOutOfRangeException("Y must be interger positive number");
            }

            X = x;
            Y = x;
            Quadrant = quadrant;

        }
    }
}
