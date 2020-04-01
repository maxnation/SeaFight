using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFightLogic
{
    public class Cell
    {
        public bool IsOccupied { get; set; }
        public Ship Ship; // Reference on ship which occupies this cell   
    }
}
