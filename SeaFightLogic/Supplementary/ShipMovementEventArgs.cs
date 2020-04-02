using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFightLogic.Supplementary
{
  public  class ShipMovementEventArgs : EventArgs
    {
        public int Speed { get; }

        public ShipMovementEventArgs(int speed)
        {
            this.Speed = speed;
        }
    }
}
