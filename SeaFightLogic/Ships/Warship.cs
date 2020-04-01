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
            throw new NotImplementedException();
        }

        public Warship(int size, int speed, int actionDistance) : base(size, speed)
        {
            int maxDistance = Convert.ToInt32(ConfigurationManager.AppSettings["warShipDistance"]);

            if (actionDistance < 1 || actionDistance > maxDistance)
            {
                throw new ArgumentOutOfRangeException("Size of ship must be from 1 to 5");
            }
            this.ActionDistance = actionDistance;
        }
    }
}
