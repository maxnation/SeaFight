﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaFightLogic.Supplementary;
using SeaFightLogic;

namespace SeaFight
{

    class Program
    {
        static void Main(string[] args)
        {
            GameField gameField = new GameField(8);
            Cell c = gameField[4, 7, 0];
        }

        
    }
}
