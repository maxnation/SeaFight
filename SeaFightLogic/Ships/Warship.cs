using SeaFightLogic.Supplementary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight
{
    public class Warship : Ship, IShooting
    {        
        public void Shoot(int distance)
        {
            if (distance < 0 || distance > ActionDistance)
            {
                throw new ArgumentOutOfRangeException($"Distance of action must be from 0 to {ActionDistance}");
            }
            ShipActionEventArgs eventArgs = new ShipActionEventArgs(ShipActionType.Shoot);
            OnShipAction(this, eventArgs);
        }

        public Warship(int size, int speed, int actionDistance) : base(size, speed)
        {
            int maxDistance = Convert.ToInt32(ConfigurationManager.AppSettings["warShipDistance"]);

            if (actionDistance < 1 || actionDistance > maxDistance)
            {
                throw new ArgumentOutOfRangeException($"Action distance of ship must be from 1 to {maxDistance}");
            }
            this.ActionDistance = actionDistance;
        }
    }
}
