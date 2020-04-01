using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SeaFightLogic.Supplementary;

namespace SeaFight
{

    public abstract class Ship
    {
        public delegate void ShipActionHandler(Ship sender, ShipActionEventArgs eventArgs);

        public int Speed { get; protected set; }
        public int ActionDistance { get; protected set; }
        public int Size { get; protected set; }
        public bool[] ShipCells { get; set; }
        public event ShipActionHandler ShipAction;

        // Method for raising event from derived classes
        public void OnShipAction(Ship sender, ShipActionEventArgs eventArgs)
        {
            ShipAction.Invoke(sender, eventArgs);
        }

        public Ship(int size, int speed)
        {
            int maxSize = Convert.ToInt32(ConfigurationManager.AppSettings["maxShipSize"]);
            if (size < 1 || size > maxSize)
            {
                throw new ArgumentOutOfRangeException("Size of ship must be from 1 to 5");
            }       
            this.Speed = speed;

            ShipCells = new bool[size];

            for (int i = 0; i < ShipCells.Length; i++)
            {
                ShipCells[i] = true;
            }

            Size = size;
        }

        public void ShowState()
        {
            Console.WriteLine("Ship type: {0}", this.GetType().Name);
            int uninjuredPartsQuantity = ShipCells.Count(c => c == true);
            Console.WriteLine("Ship wholeness: {0}/{1}", uninjuredPartsQuantity, ShipCells.Length);
        }


        public static bool operator ==(Ship s1, Ship s2)
        {
            bool typesMatch = s1.GetType() == s2.GetType();
            bool speedMatch = s1.Speed == s2.Speed;
            bool actionDistancesMatch = s1.ActionDistance == s2.ActionDistance;
            return typesMatch && speedMatch && actionDistancesMatch;
        }

        public static bool operator !=(Ship s1, Ship s2)
        {
            return !(s1 == s2);
        }
    }
}
