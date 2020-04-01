using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public MixedShip(int size, int speed, int distance) : base(size, speed, distance)
        {

        }
    }
}
