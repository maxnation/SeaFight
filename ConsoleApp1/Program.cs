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

            Warship s1 = new Warship(1, 3, 3);

            field.AddShip(s1, 0, 2, 3, Direction.East);

            Warship s2 = new Warship(1, 3, 3);

            field.AddShip(s2,1, 2, 3, Direction.East);

            var state = field.FieldState();
            Console.WriteLine(state["shipsSortedByRemoteness"]);
        }

    }
}
