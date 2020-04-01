using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SeaFightLogic.Supplementary;

namespace SeaFight
{
    public class MixedShip : Ship, IHealing, IShooting
    {
        public void Heal(int distance)
        {
            if (distance < 0 || distance > ActionDistance)
            {
                throw new ArgumentOutOfRangeException($"distance of action must be from 0 to {ActionDistance}");
            }

            ShipActionEventArgs eventArgs = new ShipActionEventArgs(ShipActionType.Heal);
            OnShipAction(this, eventArgs);
        }

        public void Shoot(int distance)
        {
            if (distance < 0 || distance > ActionDistance)
            {
                throw new ArgumentOutOfRangeException($"distance of action must be from 0 to {ActionDistance}");
            }

            ShipActionEventArgs eventArgs = new ShipActionEventArgs(ShipActionType.Shoot);
            OnShipAction(this, eventArgs);
        }

        public MixedShip(int size, int speed, int actionDistance) : base(size, speed)
        {
            int maxDistance = Convert.ToInt32(ConfigurationManager.AppSettings["mixedShipDistance"]);

            if (actionDistance < 1 || actionDistance > maxDistance)
            {
                throw new ArgumentOutOfRangeException($"Action distance of ship must be from 1 to {maxDistance}");
            }
            this.ActionDistance = actionDistance;
        }
    }
}
