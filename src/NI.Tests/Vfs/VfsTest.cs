using System;
using NI.Vfs;
using System.IO;
using System.Data;
using NUnit.Framework;

namespace NI.Tests.Vfs
{
	/// <summary>
	/// </summary>
	[TestFixture]
	[Category("NI.Vfs")]
	public class VfsTest
	{
		public VfsTest()
		{
		}
		
		protected void prepeare(IFileSystem fileSystem) {

			// create folder
			IFileObject testFolder = fileSystem.ResolveFile("test");
			testFolder.CreateFolder();
		
			// create 10 files
			for (int i=0; i<10; i++) {
				string fName = String.Format("test/test{0}.{1}", i, i%2==0 ? "txt" : "doc" );
				IFileObject testFile = fileSystem.ResolveFile(fName);
				testFile.CreateFile();
				StreamWriter streamWr = new StreamWriter( testFile.Content.GetStream(FileAccess.Write) );
				streamWr.Write("This is test content #"+i.ToString());
				streamWr.Close();
			}
		
			// GetChildren
			if (testFolder.GetChildren().Length!=10)
				throw new Exception("GetChildren failed");

			// create another dir with subdir
			IFileObject testTest2Folder = fileSystem.ResolveFile( "test2/test/" );
			testTest2Folder.CopyFrom( testFolder );
			testTest2Folder.Parent.CopyFrom( testFolder );

			
			// FindFiles
			IFileObject[] txtTest2Files = testTest2Folder.Parent.FindFiles( new MaskFileSelector("*.txt") );
			if (txtTest2Files.Length!=10)
				throw new Exception("FindFiles failed");
			
			foreach (IFileObject f in txtTest2Files) f.Delete();
			if (testTest2Folder.GetChildren().Length!=5)
				throw new Exception("Delete failed");
				
			// check copied file content
			IFileObject test1docFile = fileSystem.ResolveFile("test2/test/test1.doc");
			if (!test1docFile.Exists())
				throw new Exception("ResolveFile failed");
			StreamReader rdr = new StreamReader( test1docFile.Content.GetStream(FileAccess.Read) );
			string content = rdr.ReadToEnd();
			rdr.Close();
			Assert.AreEqual("This is test content #1",content);
			
			// deep tree copy test
			IFileObject test3Folder = fileSystem.ResolveFile("test3");
			test3Folder.CopyFrom( fileSystem.ResolveFile("test2") );
			
			// count doc files
			if (fileSystem.Root.FindFiles( new MaskFileSelector("*.doc") ).Length!=25)
				throw new Exception("CopyFrom (subtree) or FindFiles failed");
				
		}
		
		[Test]
		public void test_MemoryFileSystem() {
			MemoryFileSystem memFs = new MemoryFileSystem();
			
			prepeare(memFs);

			var f = memFs.ResolveFile("g1.wmv");
			f.CreateFile();
			f.WriteAllBytes( System.Text.Encoding.ASCII.GetBytes("test") );
			Assert.AreEqual(4, f.Content.Size);

			// remove everything
			memFs.Root.Delete();
		}
		
		[Test]
		public void test_LocalFileSystem() {
			string tempFileName = Path.GetTempFileName();
			File.Delete(tempFileName);
			Directory.CreateDirectory(tempFileName);
			
			LocalFileSystem localFs = new LocalFileSystem(tempFileName);
			
			prepeare(localFs);
			
			// remove everything
			localFs.Root.Delete();
			
		}
		
        [Test]
        public void test_DalcFileSystem() {
            
            //FileSystemHelper definition
            NI.Data.Vfs.DalcFileSystemHelper fsHelper = new NI.Data.Vfs.DalcFileSystemHelper();
            System.Collections.Hashtable fileObjMap =  new System.Collections.Hashtable();
            fileObjMap.Add("Name", "name");
            fileObjMap.Add("Type", "type");
            fileObjMap.Add("ParentName", "directory");
            fsHelper.FileObjectMap = fileObjMap;
            System.Collections.Hashtable fileContentMap = new System.Collections.Hashtable();
            fileContentMap.Add("Name", "name");
            fileContentMap.Add("LastModifiedTime", "modified_date");
            fileContentMap.Add("Stream", "content");
            fsHelper.FileContentMap = fileContentMap;

            //DataSet Dalc definition
            NI.Data.DataSetDalc dsDalc = new NI.Data.DataSetDalc();
            System.Data.DataSet ds = new System.Data.DataSet();
            ds.Tables.Add("resources");
            DataColumn idColumn = ds.Tables["resources"].Columns.Add("name", typeof(string));
            ds.Tables["resources"].Columns.Add("type", typeof(string));
            ds.Tables["resources"].Columns.Add("directory", typeof(string));
            ds.Tables["resources"].PrimaryKey = new DataColumn[] { idColumn };
            
            ds.Tables.Add("resource_data");
            idColumn = ds.Tables["resource_data"].Columns.Add("name", typeof(string));
            ds.Tables["resource_data"].Columns.Add("modified_date", typeof(DateTime));
            ds.Tables["resource_data"].Columns.Add("content", typeof(byte[]));
            ds.Tables["resource_data"].PrimaryKey = new DataColumn[] { idColumn };

            dsDalc.PersistedDS = ds;

            //DalcFileSystem definition
            NI.Data.Vfs.DalcFileSystem fileSystem = new NI.Data.Vfs.DalcFileSystem();
            fileSystem.TableName = "resources";
            fileSystem.ContentTableName = "resource_data";
            fileSystem.KeyFieldName = "name";
            fileSystem.ParentFieldName = "directory";
            fileSystem.FileSystemHelper = fsHelper;
            fileSystem.Dalc = dsDalc;
            prepeare(fileSystem);
        }
		
		/// <summary>
		/// MASK FILE SELECTOR
		/// </summary>
		[Test]
		public void test_MaskFileSelector() {
				
			MaskFileSelector maskSelector = new MaskFileSelector("a*.txt");
			
			Assert.True( maskSelector.IncludeFile( new StubFileObject("aaa/hehe/a1.txt")  ) );
			Assert.True( maskSelector.IncludeFile( new StubFileObject("a2.txt") ) );
			Assert.False(maskSelector.IncludeFile( new StubFileObject("a2.txt1") ) );
			Assert.False(maskSelector.IncludeFile( new StubFileObject("2.txt") ));

			var maskSelector2 = new MaskFileSelector(@"folder\**\*.xml");
			Assert.True(maskSelector2.IncludeFile(new StubFileObject("folder/hehe/a1.xml")));
			Assert.False(maskSelector2.IncludeFile(new StubFileObject("test/hehe/a1.xml")));
			Assert.False(maskSelector2.IncludeFile(new StubFileObject("folder/a1.txt")));
		}
		
		/// <summary>
		/// LIST FILE SELECTOR
		/// </summary>
		[Test]
		public void test_ListFileSelector() {
			ListFileSelector listSelector = new ListFileSelector(@"a/hehe.txt","a.doc");
			if (!listSelector.IncludeFile( new StubFileObject(@"a\hehe.txt") ) )
				throw new Exception("ListFileSelector.IncludeFile failed");
			if (!listSelector.IncludeFile( new StubFileObject("a.doc") ) )
				throw new Exception("ListFileSelector.IncludeFile failed");
			if (listSelector.IncludeFile( new StubFileObject("b/hehe.txt") ) )
				throw new Exception("ListFileSelector.IncludeFile failed");
			if (listSelector.IncludeFile( new StubFileObject("a.doc1") ) )
				throw new Exception("ListFileSelector.IncludeFile failed");
		}
		
		class StubFileObject : IFileObject {
			string _Name;
			
			public string Name { get { return _Name; } }
			public FileType Type { get { return FileType.File; } }
			public IFileObject Parent { get { return null; } }
			public void Close() { }
			public void CopyFrom(IFileObject srcFile) { }
			public void CopyFrom(Stream stream) { }
			public void CreateFile() { }
			public void CreateFolder() { }
			public void Delete() { }
			public bool Exists() { return true; }
			public IFileObject[] GetChildren() { return null; }
			public IFileContent Content { get { return null; } }
			public IFileObject[] FindFiles(IFileSelector selector) { return null; }
			public void MoveTo(IFileObject desfile) { }
			
			public StubFileObject(string name) {
				_Name = name;
			}
		}
		
	}
}
