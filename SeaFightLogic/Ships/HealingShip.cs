using SeaFightLogic.Supplementary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFightLogic
{
    public class HealingShip : Ship, IHealing
    {
        public void Heal(int distance)
        {
            if (distance < 0 || distance > MaxActionDistance)
            {
                throw new ArgumentOutOfRangeException($"Distance of action must be from 0 to {MaxActionDistance}");
            }

            ShipActionEventArgs eventArgs = new ShipActionEventArgs(ShipActionType.Heal, distance);
            OnShipAction(this, eventArgs);
        }

        public HealingShip(int size, int speed, int actionDistance) : base(size, speed)
        {
            int maxDistance = Convert.ToInt32(ConfigurationManager.AppSettings["healingShipDistance"]);

            if (actionDistance < 1 || actionDistance > maxDistance)
            {
                throw new ArgumentOutOfRangeException($"Action distance of ship must be from 1 to {maxDistance}");
            }
            this.MaxActionDistance = actionDistance;
        }
    }
}
