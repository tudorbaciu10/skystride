using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skystride
{
    internal class Program
    {
        static void Main(string[] args)
        {
            vendor.Engine engine = new vendor.Engine();
            engine.Run(60.0);
        }
    }
}
