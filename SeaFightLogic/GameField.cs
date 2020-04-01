using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
