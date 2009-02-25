using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using NI.Winter;
using NI.Common;

namespace NI.Examples.Winter
{
    class Program
    {
        static void Main(string[] args)
        {
            // get 'components' section
            Console.WriteLine(
                String.Format(
                        "===START CREATING CONFIG==="
                    )
                );
            IComponentsConfig config = ConfigurationSettings.GetConfig("components") as IComponentsConfig;
            Console.WriteLine(
                String.Format(
                        "===END CREATING CONFIG==="
                    )
                );
            // or variant that is marked as not deprecated
            // IComponentsConfig config = ConfigurationManager.GetSection("components") as IComponentsConfig;

            // create named service provider instance
            Console.WriteLine(
                String.Format(
                        "===START CREATING SERVICE PROVIDER==="
                    )
                );
            INamedServiceProvider srv = new ServiceProvider(config);
            Console.WriteLine(
                String.Format(
                        "===END CREATING SERVICE PROVIDER==="
                    )
                );

            Console.WriteLine();

            Console.WriteLine(
                String.Format(
                        "[Service] name: {0}, {1}", "datetimenow", srv.GetService("datetimenow")
                    )
                );

            Console.WriteLine(
                String.Format(
                        "[Service] name: {0}, {1}", "datetimenow-3days", srv.GetService("datetimenow-3days")
                    )
                );

            // create simple object via config
            TestClass t1 = srv.GetService("nonLazyNonSingletonTestComponent") as TestClass;
            Console.WriteLine(
                String.Format(
                        "[Service] name: {0}, {1}", "nonLazyNonSingletonTestComponent", t1.ToString()
                    )
                );

            // create 'lazy' object via config : object will be created only when GetService is called (i.e. by demand)
            TestClass t2 = srv.GetService("lazyNonSingletonTestComponent") as TestClass;
            Console.WriteLine(
                String.Format(
                        "[Service] name: {0}, {1}", "lazyNonSingletonTestComponent", t2.ToString()
                    )
                );

            Console.WriteLine(
                String.Format(
                        "[Service] name: {0}, {1}", "appName", srv.GetService("appName")
                    )
                );

            Console.ReadLine();
        }
    }
}
