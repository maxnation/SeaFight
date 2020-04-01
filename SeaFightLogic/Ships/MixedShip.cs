using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SeaFight
{
    public class MixedShip : Ship, IHealing, IShooting
    {
        public void Heal(int distance)
        {
            throw new NotImplementedException();
        }

        public void Shoot(int distance)
        {
            throw new NotImplementedException();
        }

        public MixedShip(int size, int speed, int actionDistance) : base(size, speed)
        {
            int maxDistance = Convert.ToInt32(ConfigurationManager.AppSettings["mixedShipDistance"]);

            if (actionDistance < 1 || actionDistance > maxDistance)
            {
                throw new ArgumentOutOfRangeException("Size of ship must be from 1 to 5");
            }
            this.ActionDistance = actionDistance;
        }
    }
}
