using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFightLogic.Supplementary
{
    public class ShipActionEventArgs
    {
        public ShipActionType ShipAction;
        public int ActionDistance;

        public ShipActionEventArgs(ShipActionType shipActionType, int actionDistance)
        {
            ShipAction = shipActionType;
            ActionDistance = actionDistance;
        }
    }
}
