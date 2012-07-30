#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2008 NewtonIdeas
 * Distributed under the LGPL licence
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections;
using System.Text;
using System.IO;

using NI.Common;
using NI.Common.Caching;
using NI.Common.Providers;

namespace NI.Vfs {

	public class FileCache : ICache {
		IFileSystem _FileSystem;
		IStringProvider _FileNameProvider;
		IBinarySerializer _Serializer;
		string _TocFileName = "cache_TOC";

		public string TocFileName {
			get { return _TocFileName; }
			set { _TocFileName = value; }
		}

		public IStringProvider FileNameProvider {
			get { return _FileNameProvider; }
			set { _FileNameProvider = value; }
		}

		public IBinarySerializer Serializer {
			get { return _Serializer; }
			set { _Serializer = value; }
		}

		public IFileSystem FileSystem {
			get { return _FileSystem; }
			set { _FileSystem = value; }
		}


		public FileCache() {
			FileNameProvider = new DumpFileNameProvider();
			Serializer = new BinarySerializer();
		}


		public void Put(string key, object value) {
			byte[] serializedValue = Serializer.ToByteArray(value);
			IFileObject fileObj = FileSystem.ResolveFile( FileNameProvider.GetString(key) );
			if (fileObj.Exists())
				fileObj.Delete();
			fileObj.CreateFile();
			using (Stream outStream = fileObj.GetContent().OutputStream)
				outStream.Write(serializedValue, 0, serializedValue.Length);
			// update TOC
			IDictionary toc = LoadTOC();
			toc[key] = value;
			SaveTOC(toc);
		}

		public void Put(string key, object value, ICacheEntryValidator validator) {
            // CHECK: for now does not implement internal validation
            Put(key, value);
        }

		public object Get(string key) {
			IFileObject fileObj = FileSystem.ResolveFile( FileNameProvider.GetString(key) );
			object value = null;
			
			if (fileObj.Exists())
				using (Stream inStream = fileObj.GetContent().InputStream) {
					byte[] serializedValue = new byte[inStream.Length];
					inStream.Read( serializedValue, 0, serializedValue.Length );
					value = Serializer.FromByteArray(serializedValue);
				}
			return value;
		}

		public void Remove(string key) {
			IFileObject fileObj = FileSystem.ResolveFile( FileNameProvider.GetString(key) );
			if (fileObj.Exists())
				fileObj.Delete();
			// update TOC
			IDictionary toc = LoadTOC();
			toc.Remove(key);
			SaveTOC(toc);
		}

		public void Clear() {
			// update TOC
			IDictionary toc = LoadTOC();
			// remove all files
			foreach (DictionaryEntry tocEntry in toc) {
				IFileObject fileObj = FileSystem.ResolveFile( FileNameProvider.GetString(tocEntry.Key) );
				if (fileObj.Exists())
					fileObj.Delete();
			}

			toc.Clear();
			SaveTOC(toc);
		}

		public IDictionaryEnumerator GetEnumerator() {
			return LoadTOC().GetEnumerator();
		}

		protected void SaveTOC(IDictionary toc) {
			IFileObject tocFileObj = FileSystem.ResolveFile(TocFileName);
			if (tocFileObj.Exists())
				tocFileObj.Delete();
			tocFileObj.CreateFile();
			byte[] serializedToc = Serializer.ToByteArray(toc);
			using (Stream outStream = tocFileObj.GetContent().OutputStream) {
				outStream.Write(serializedToc, 0, serializedToc.Length);
			}
		}

		protected IDictionary LoadTOC() {
			IFileObject tocFileObj = FileSystem.ResolveFile(TocFileName);
			if (tocFileObj.Exists()) {
				using (Stream inStream = tocFileObj.GetContent().InputStream) {
					byte[] serializedToc = new byte[inStream.Length];
					inStream.Read( serializedToc, 0, serializedToc.Length );
					IDictionary toc = Serializer.FromByteArray(serializedToc) as IDictionary;
					if (toc!=null)
						return toc;
				}
			}
			return new Hashtable();
		}


		class DumpFileNameProvider : IStringProvider {
			public string GetString(object context) {
				return "entry_"+Convert.ToString(context).Replace('\\', '=').Replace('/', '-');
			}
		}


	}
}
