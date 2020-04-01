using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaFightLogic.Supplementary;

namespace SeaFightLogic
{
    public class GameField
    {
        private Cell[,] cells;
        private readonly int quadrantSideLength;

        public GameField(int quadrantSideLength)
        {
            cells = new Cell[quadrantSideLength * 2, quadrantSideLength * 2];
            this.quadrantSideLength = quadrantSideLength;

            for (int i = 0; i <= cells.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= cells.GetUpperBound(1); j++)
                {
                    cells[i, j] = new Cell();
                }
            }
        }


        //TODO: Arr index to Cartesian coords
        private CartesianCoordinate ConvertArrIndexToCartesianCoords(int line, int colummn)
        {
            CartesianCoordinate cartesianCoordinate = new CartesianCoordinate(0, 0, 0);
            return cartesianCoordinate;
        }

        private void ConvertCartesianCoordsToArrIndex(CartesianCoordinate cartCoord, ref int str, ref int column)
        {
            // Convert cartesian coordinate to 2dim-array index
            switch (cartCoord.Quadrant)
            {
                case 0:
                    column = cartCoord.X + quadrantSideLength;
                    str = quadrantSideLength - cartCoord.Y - 1;
                    break;
                case 1:
                    column = quadrantSideLength - cartCoord.X - 1;
                    str = quadrantSideLength - cartCoord.Y - 1;
                    break;
                case 2:
                    column = quadrantSideLength - cartCoord.X - 1;
                    str = cartCoord.Y + quadrantSideLength;
                    break;
                case 3:
                    column = cartCoord.X + quadrantSideLength;
                    str = cartCoord.Y + quadrantSideLength;
                    break;
            }
        }

        public Cell this[int x, int y, int quadrant]
        {
            get
            {
                if (quadrant < 0 || quadrant > 3)
                {
                    throw new ArgumentOutOfRangeException("Quadrant value must be from 0 to 3");
                }
                if (x < 0 || x > quadrantSideLength)
                {
                    throw new ArgumentOutOfRangeException($"value of X must be from 0 to {quadrantSideLength - 1}");
                }
                if (y < 0 || y > quadrantSideLength)
                {
                    throw new ArgumentOutOfRangeException($"value of Y must be from 0 to {quadrantSideLength - 1}");
                }

                int line = -1;
                int column = -1;
                CartesianCoordinate cartesianCoord = new CartesianCoordinate(x, y, quadrant);
                ConvertCartesianCoordsToArrIndex(cartesianCoord, ref line, ref column);

                return cells[line, column];
            }
        }
    }
}
