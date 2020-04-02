using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaFightLogic.Supplementary;
using SeaFightLogic;

namespace SeaFight
{

    class Program
    {
        static void Main(string[] args)
        {
            GameField gameField = new GameField(8);
            Warship s1 = new Warship(5, 3, 3);
 
            gameField.AddShip(s1, 0, 2, 3, Direction.West);
         }

        
    }
}
