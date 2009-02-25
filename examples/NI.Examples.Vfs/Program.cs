using System.IO;
using System;

using NI.Vfs;

namespace NI.Examples.Vfs
{
    class Program
    {
        static void Main(string[] args)
        {
            // Memory filesystem example
            Console.WriteLine(
                    String.Format(
                        "Memory filesystem example:"
                    )
                );
            MemoryFileSystem memFs = new MemoryFileSystem();
            Utility.IllustrateSystem(memFs);
            memFs.Root.Delete();
            Console.WriteLine("=== Press any key to proceed to local filesystem sample ===");
            Console.ReadLine();

            // Local filesystem example
            Console.WriteLine(
                String.Format(
                    "Local filesystem example:"
                )
            );
            string tempFileName = Path.GetTempFileName();
            File.Delete(tempFileName);
            Directory.CreateDirectory(tempFileName);
            Console.WriteLine(
                String.Format(
                    "Creating local fs with root {0}", tempFileName
                )
            );
            LocalFileSystem localFs = new LocalFileSystem(tempFileName);
            Utility.IllustrateSystem(localFs);
            localFs.Root.Delete();
            Console.ReadLine();
        }
    }
}
