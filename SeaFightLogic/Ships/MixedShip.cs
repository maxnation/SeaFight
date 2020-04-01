﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SeaFightLogic.Supplementary;

namespace SeaFightLogic
{
    public class MixedShip : Ship, IHealing, IShooting
    {
        public void Heal(int distance)
        {
            if (distance < 0 || distance > MaxActionDistance)
            {
                throw new ArgumentOutOfRangeException($"distance of action must be from 0 to {MaxActionDistance}");
            }

            ShipActionEventArgs eventArgs = new ShipActionEventArgs(ShipActionType.Heal, distance);
            OnShipAction(this, eventArgs);
        }

        public void Shoot(int distance)
        {
            if (distance < 0 || distance > MaxActionDistance)
            {
                throw new ArgumentOutOfRangeException($"distance of action must be from 0 to {MaxActionDistance}");
            }

            ShipActionEventArgs eventArgs = new ShipActionEventArgs(ShipActionType.Shoot, distance);
            OnShipAction(this, eventArgs);
        }

        public MixedShip(int size, int speed, int actionDistance) : base(size, speed)
        {
            int maxDistance = Convert.ToInt32(ConfigurationManager.AppSettings["mixedShipDistance"]);

            if (actionDistance < 1 || actionDistance > maxDistance)
            {
                throw new ArgumentOutOfRangeException($"Action distance of ship must be from 1 to {maxDistance}");
            }
            this.MaxActionDistance = actionDistance;
        }
    }
}
