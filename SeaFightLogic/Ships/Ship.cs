using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight
{

    public abstract class Ship
    {
        public int Speed { get; protected set; }
        public int ActionDistance { get; protected set; }
        public int Size { get; protected set; }

        public Ship(int size, int speed, int actionDistance)
        {
            if (size < 1 || size > 5)
            {
                throw new ArgumentOutOfRangeException("Size of ship must be from 1 to 5");
            }
            if (speed < 1 || speed > 5)
            {
                throw new ArgumentOutOfRangeException("Speed of ship must be from 1 to 5");
            }
            if (actionDistance < 1 || actionDistance > 5)
            {
                throw new ArgumentOutOfRangeException("Size of ship must be from 1 to 5");
            }

            Speed = speed;
            ActionDistance = actionDistance;
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
