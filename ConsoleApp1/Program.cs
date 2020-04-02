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
            Warship s2 = new Warship(5, 3, 4);
            HealingShip s3 = new HealingShip(1, 3, 3);

            field.AddShip(s1, 0, 2, 3, Direction.North);
            field.AddShip(s2, 1, 1, 6, Direction.West);
            field.AddShip(s3, 0, 3, 6, Direction.West);
            s1.Shoot(3);

            s2.ShowState();
             s3.Heal(2);
            s2.ShowState();

        }

    }
}
