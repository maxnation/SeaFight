using System;
using System.Collections.Generic;
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

        public Warship(int size, int speed, int actionDistance) : base(size, speed, actionDistance) { }
    }
}
