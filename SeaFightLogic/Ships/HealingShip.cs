using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight
{
    public class HealingShip : Ship, IHealing
    {      
        public void Heal(int distance)
        {
            throw new NotImplementedException();
        }

        public HealingShip(int size, int speed, int actionDistance) : base(size, speed)
        {
            int maxDistance = Convert.ToInt32(ConfigurationManager.AppSettings["healingShipDistance"]);

            if (actionDistance < 1 || actionDistance > 5)
            {
                throw new ArgumentOutOfRangeException("Size of ship must be from 1 to 5");
            }
            this.ActionDistance = actionDistance;
        }
    }
}
