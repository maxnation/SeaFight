using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaFightLogic.Supplementary;

namespace SeaFight
{

    class Program
    {

        static void Main(string[] args)
        {
            HealingShip s = new HealingShip(3, 700, 3);
            ShipActionEventArgs e = new ShipActionEventArgs(ShipActionType.Heal, 2);
            s.ShipAction += TestHandler;
            s.Heal(distance:2);
        }

        public static void TestHandler(Ship sender, ShipActionEventArgs e)
        {
            Console.WriteLine(@"Data from sender:
            Max Action distance: {0}, 
            Max speed: {1}, 
            Size: {2}, ShipCells.Length: {3},
            Ship Type: {4}", 
            sender.MaxActionDistance, sender.MaxSpeed, sender.Size, sender.ShipCells.Length, sender.GetType().Name);

            Console.WriteLine();
            Console.WriteLine(@"Data from eventArgs:
            Current Action Distance: {0}, 
            Type of action: {1}", 
            e.ActionDistance, e.ShipAction);
        }
    }
}
