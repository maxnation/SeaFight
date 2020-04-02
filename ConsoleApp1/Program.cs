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
            GameField field = new GameField(8);

            Warship s1 = new Warship(5, 3, 3);

            field.AddShip(s1, 0, 2, 3, Direction.East);

            s1.Move(2, Direction.East);
 
        }

    }
}
