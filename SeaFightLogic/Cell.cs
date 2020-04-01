﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaFight;

namespace SeaFightLogic
{
    class Cell
    {
        public bool IsOccupied { get; set; }
        public Ship Ship; // Reference on ship which occupies this cell   
    }
}
