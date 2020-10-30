using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace steg_poc
{
    class Program
    {
        static void Main(string[] args)
        {
            Stega stega = new Stega();
            stega.EncodeImage(null);

            //Console.Read();
        }
    }
}
