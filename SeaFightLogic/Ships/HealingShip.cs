using System;
using System.Collections.Generic;
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

        public HealingShip(int size, int speed, int distance): base(size, speed, distance)
        {

        }
    }
}
