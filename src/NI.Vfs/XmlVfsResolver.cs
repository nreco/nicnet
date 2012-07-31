#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
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
using System.Xml;
using System.IO;
using System.Text;


namespace NI.Vfs {

	/// <summary>
	/// Resolves external XML resources named by a URI using VFS.
	/// </summary>
	public class XmlVfsResolver : XmlResolver {
		IFileSystem _FileSystem;
		string _BasePath;
		static readonly Uri AbsoluteBaseUri = new Uri("http://vfs/");

		protected IFileSystem FileSystem {
			get { return _FileSystem; }
		}

		protected string BasePath {
			get { return _BasePath; }
		}

		public XmlVfsResolver(IFileSystem fileSystem, string basePath) {
			_FileSystem = fileSystem;
			_BasePath = basePath;
		}

		public override System.Net.ICredentials Credentials {
			set { /* ignore */ }
		}

		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
			if ((ofObjectToReturn != null) && (ofObjectToReturn != typeof(Stream))) {
				throw new XmlException("Unsupported object type");
			}
			string relativePath = AbsoluteBaseUri.MakeRelative(absoluteUri);
			IFileObject file = FileSystem.ResolveFile( Path.Combine(BasePath, relativePath ) );
			return file.GetContent().InputStream;
		}

		public override Uri ResolveUri(Uri baseUri, string relativeUri) {
#if NET_1_1
			string baseUriString = baseUri!=null ? 
				baseUri.ToString().Substring(0, baseUri.ToString().LastIndexOf('/') ) :
				String.Empty;
			if ( baseUriString.StartsWith(AbsoluteBaseUri.ToString()) || relativeUri.StartsWith(AbsoluteBaseUri.ToString()) ) {
				
				return new Uri( baseUriString+relativeUri);
			} else {
				return new Uri( AbsoluteBaseUri.ToString()+relativeUri );
			}
#else

			if (baseUri!=null && baseUri.IsAbsoluteUri) {
				return new Uri(baseUri, relativeUri);
			} else {
				return new Uri(AbsoluteBaseUri, relativeUri);
			}
#endif

		}

	}

}
