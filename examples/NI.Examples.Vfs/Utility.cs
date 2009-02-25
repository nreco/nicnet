using System;
using System.IO;
using System.Diagnostics;

using NI.Vfs;

namespace NI.Examples.Vfs
{
    public class Utility
    {
        public static void IllustrateSystem(IFileSystem fileSystem)
        {
            // create folder
            Console.WriteLine(
                    String.Format(
                        "Creating folder 'test' in filesystem root"
                    )
                );
            IFileObject testFolder = fileSystem.ResolveFile("test");
            testFolder.CreateFolder();

            // create 10 files
            Console.WriteLine(
                    String.Format(
                        "Creating 10 files in 'test' folder"
                    )
                );
            for (int i = 0; i < 10; i++)
            {
                string fName = String.Format("test/test{0}.{1}", i, i % 2 == 0 ? "txt" : "doc");
                IFileObject testFile = fileSystem.ResolveFile(fName);
                testFile.CreateFile();
                StreamWriter streamWr = new StreamWriter(testFile.GetContent().OutputStream);
                string fileContent = "This is test content #" + i.ToString();
                streamWr.Write(fileContent);
                Console.WriteLine(
                        String.Format(
                            "File {0} created in 'test' folder, content: {1}", fName, fileContent
                        )
                    );
                streamWr.Close();
            }

            Console.WriteLine(
                    String.Format(
                        "Children count in 'test' folder: {0}", testFolder.GetChildren().Length
                    )
                );
            if (testFolder.GetChildren().Length != 10)
            {
                throw new Exception("GetChildren failed");
            }

            // create another dir with subdir
            Console.WriteLine(
                    String.Format(
                        "Create folder 'test' as subfolder for folder 'test2'", testFolder.GetChildren().Length
                    )
                );
            IFileObject testTest2Folder = fileSystem.ResolveFile("test2/test/");
            Console.WriteLine(
                    String.Format(
                        "Copy all '<root>/test' files to '<root>/test2/test'"
                    )
                );
            testTest2Folder.CopyFrom(testFolder);
            Console.WriteLine(
                    String.Format(
                        "Copy all '<root>/test' files to '<root>/test2'"
                    )
                );
            testTest2Folder.Parent.CopyFrom(testFolder);

            // find files
            IFileObject[] txtTest2Files = testTest2Folder.Parent.FindFiles(new MaskFileSelector("*.txt"));
            Console.WriteLine(
                    String.Format(
                        "{0} .txt files found in '<root>/test2'", txtTest2Files.Length
                    )
                );
            if (txtTest2Files.Length != 10)
            {
                throw new Exception("FindFiles failed");
            }

            Console.WriteLine(
                    String.Format(
                        "Delete all .txt files from <root>/test2"
                    )
                );
            foreach (IFileObject f in txtTest2Files)
            {
                f.Delete();
            }
            if (testTest2Folder.GetChildren().Length != 5)
            {
                throw new Exception("Delete failed");
            }

            // check copied file content
            Console.WriteLine(
                    String.Format(
                        "Check '<root>/test2/test/test1.doc' file"
                    )
                );
            IFileObject test1docFile = fileSystem.ResolveFile("test2/test/test1.doc");
            if (!test1docFile.Exists())
            {
                throw new Exception("ResolveFile failed");
            }
            StreamReader rdr = new StreamReader(test1docFile.GetContent().InputStream);
            string content = rdr.ReadToEnd();
            rdr.Close();
            Console.WriteLine(
                    String.Format(
                        "Content of '<root>/test2/test/test1.doc' file: {0}", content
                    )
                );
            if (content != "This is test content #1")
            {
                throw new Exception("GetContent failed");
            }

            // deep tree copy test
            Console.WriteLine(
                    String.Format(
                        "Create '<root>/test3' folder"
                    )
                );
            IFileObject test3Folder = fileSystem.ResolveFile("test3");
            Console.WriteLine(
                    String.Format(
                        "Deep copy: from '<root>/test2' to '<root>/test3'"
                    )
                );
            test3Folder.CopyFrom(fileSystem.ResolveFile("test2"));

            // count doc files
            IFileObject[] docs = fileSystem.Root.FindFiles(new MaskFileSelector("*.doc"));
            Console.WriteLine(
                    String.Format(
                        "Total number of .doc files in filesystem root: {0}", docs.Length
                    )
                );
            if (docs.Length != 25)
            {
                throw new Exception("CopyFrom (subtree) or FindFiles failed");
            }
        }
    }
}
