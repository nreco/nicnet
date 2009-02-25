using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using NI.Winter;

namespace NI.Examples.Winter
{
    class Program
    {
        static void Main(string[] args)
        {
            IComponentsConfig config = ConfigurationManager.GetSection("components") as IComponentsConfig;
        }
    }
}
