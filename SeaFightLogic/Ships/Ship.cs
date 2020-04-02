using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SeaFightLogic.Supplementary;

namespace SeaFightLogic
{

    public abstract class Ship
    {
        public delegate void ShipActionHandler(Ship sender, ShipActionEventArgs eventArgs);
        public delegate void ShipMovementHandler(Ship sender, ShipMovementEventArgs eventArgs);

        public int MaxSpeed { get; protected set; }
        public int MaxActionDistance { get; protected set; }
        public int Size { get; protected set; }
        public bool[] ShipCells { get; set; }
        public Direction Direction { get; set; }

        public CartesianCoordinate Head { get; set; }

        public event ShipActionHandler ShipAction;
        public event ShipMovementHandler ShipMovement;

        // Method for raising event from derived classes
        protected void OnShipAction(Ship sender, ShipActionEventArgs eventArgs)
        {
            ShipAction.Invoke(sender, eventArgs);
        }

        public void Move(int speed, Direction direction)
        {
            if (speed < 0 || speed > MaxSpeed)
            {
                throw new ArgumentOutOfRangeException($"Speed must be from 0 to {MaxSpeed}");
            }

            ShipMovementEventArgs eventArgs = new ShipMovementEventArgs(speed);
            ShipMovement.Invoke(this, eventArgs);
        }

        public Ship(int size, int speed)
        {
            int maxSize = Convert.ToInt32(ConfigurationManager.AppSettings["maxShipSize"]);
            if (size < 1 || size > maxSize)
            {
                throw new ArgumentOutOfRangeException($"Size of ship must be from 1 to {maxSize}");
            }
            this.MaxSpeed = speed;

            ShipCells = new bool[size];

            for (int i = 0; i < ShipCells.Length; i++)
            {
                ShipCells[i] = true;
            }

            Size = size;
        }

        public Dictionary<string, string> ShowState()
        {
            string shipType = this.GetType().Name;

            int uninjuredPartsQuantity = ShipCells.Count(c => c == true);
            string shipWholeness = string.Format("Ship wholeness: {0}/{1}", uninjuredPartsQuantity, ShipCells.Length);

            // In next updates method should return json object instead 
            Dictionary<string, string> shipState = new Dictionary<string, string>
            {
                { "shipType", shipType },
                { "shipWholeness", shipWholeness }
             };

            return shipState;
        }

        #region operators
        public static bool operator ==(Ship s1, Ship s2)
        {
            bool typesMatch = s1.GetType() == s2.GetType();
            bool speedMatch = s1.MaxSpeed == s2.MaxSpeed;
            bool actionDistancesMatch = s1.MaxActionDistance == s2.MaxActionDistance;
            return typesMatch && speedMatch && actionDistancesMatch;
        }

        public static bool operator !=(Ship s1, Ship s2)
        {
            return !(s1 == s2);
        }
        #endregion
    }
}
