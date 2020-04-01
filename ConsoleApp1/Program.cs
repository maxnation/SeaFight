using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight
{

    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                Warship warship1 = new Warship(3, 3, 5);
                Warship warship2 = new Warship(3, 4, 5);
                Console.WriteLine(warship1 != warship2);

            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.ParamName);
            }


        }
    }
}
