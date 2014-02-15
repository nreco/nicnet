using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using NI.Ioc;

namespace NI.Ioc.Examples.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
			Console.WriteLine("Loading IoC-container configuration...");

			var config = ConfigurationManager.GetSection("appContainer") as IComponentFactoryConfiguration;
            // or variant that is marked as not deprecated

			Console.WriteLine("Creating IoC-container...");
            var factory = new ComponentFactory(config, true);

			Console.WriteLine("Created {0} non-lazy instances", factory.Counters.CreateInstance);

			var componentNames = new[] {"datetimenow",  "datetimenow-3days", "nonLazyNonSingletonTestComponent", "appName"};

			foreach (var componentName in componentNames) {
				Console.WriteLine("'{0}'.ToString(): {1}", componentName, factory.GetComponent(componentName));
			}

            Console.ReadLine();
        }
    }
}
