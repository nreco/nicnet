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
using System.Text.RegularExpressions;

namespace NI.Vfs {

	/// <summary>
	/// Resolves external XML resources named by a URI using VFS.
	/// </summary>
	public class VfsXmlResolver : XmlResolver {
		IFileSystem _FileSystem;
		string _BasePath;
		
		public static readonly Uri AbsoluteBaseUri = new Uri("http://vfs/");

		protected IFileSystem FileSystem {
			get { return _FileSystem; }
		}

		protected string BasePath {
			get { return _BasePath; }
		}

		public VfsXmlResolver(IFileSystem fileSystem, string basePath) {
			_FileSystem = fileSystem;
			_BasePath = basePath;
		}

		public override System.Net.ICredentials Credentials {
			set { /* ignore */ }
		}

		static Regex MatchXmlDeclaration = new Regex(@"^\s*[<][?]xml[^>]*[?][>]", RegexOptions.Compiled|RegexOptions.Singleline);

		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
			try {
				if ((ofObjectToReturn != null) && (ofObjectToReturn != typeof(Stream))) {
					throw new XmlException("Unsupported object type");
				}
				string relativePath = AbsoluteBaseUri.MakeRelative(absoluteUri);

				if (relativePath.IndexOfAny(new char[] { '*', '?' }) >= 0) {
					// several files
					var startPath = MaskFileSelector.GetMaskParentPath(relativePath) ?? String.Empty;
					var startFile = FileSystem.ResolveFile(startPath);
					var sb = new StringBuilder();
					sb.Append("<root>"); 
					var matchedFiles = startFile.FindFiles(new MaskFileSelector(relativePath));
					foreach (var f in matchedFiles) {
						using (var input = f.GetContent().InputStream) {
							var fileText = new StreamReader(input).ReadToEnd();
							fileText = MatchXmlDeclaration.Replace(fileText, String.Empty);
							sb.Append(fileText);
						}
					}
					sb.Append("</root>");
					return new MemoryStream( Encoding.UTF8.GetBytes( sb.ToString() ) );
				} else {
					// one file
					IFileObject file = FileSystem.ResolveFile(Path.Combine(BasePath, relativePath));
					byte[] fileContent;
					using (var input = file.GetContent().InputStream) {
						fileContent = new byte[input.Length];
						input.Read(fileContent, 0, fileContent.Length);
					}
					return new MemoryStream(fileContent);
				}
			} catch (Exception ex) {
				throw new FileSystemException(String.Format("Cannot resolve {0}: {1}", absoluteUri, ex.Message), ex);
			}
		}

		public override Uri ResolveUri(Uri baseUri, string relativeUri) {
			if (baseUri!=null && baseUri.IsAbsoluteUri) {
				return new Uri(baseUri, relativeUri);
			} else {
				return new Uri(AbsoluteBaseUri, relativeUri);
			}
		}

	}

}
