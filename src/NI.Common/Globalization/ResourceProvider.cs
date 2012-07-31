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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Globalization;
using System.Security.Principal;

using NI.Common.Providers;
using NI.Common.Expressions;

namespace NI.Common.Globalization {
	
	/// <summary>
	/// Context culture aware string resource provider
	/// </summary>
	public class ResourceProvider : IObjectProvider, IStringProvider, IResourceProvider, IExpressionResolver {
		
		ResourceEntry[] _Resources;
		string _DefaultLanguageName = "en";
		int _LanguagePriority = 25;
		int _RolePriority = 50;
		int _PlacePriority = 100;
		
		public int LanguagePriority {
			get { return _LanguagePriority; }
			set { _LanguagePriority = value; }
		}

		public int RolePriority {
			get { return _RolePriority; }
			set { _RolePriority = value; }
		}

		public int PlacePriority {
			get { return _PlacePriority; }
			set { _PlacePriority = value; }
		}
		
		
		public string DefaultLanguageName {
			get { return _DefaultLanguageName; }
			set { _DefaultLanguageName = value; }
		}
		
		public ResourceEntry[] Resources {
			get { return _Resources; }
			set { _Resources = value; }
		}
		
		protected IDictionary<string,IList<ResourceEntry>> IdToResourceEntries = null;
		
		public ResourceProvider() {
			
		}
		
		protected void Init() {
			// don't lock here - this method called very often so monitor is too expensive
			if (IdToResourceEntries==null) {
				lock (this) {
					// may be it was already populated from another thread during monitor wait?
					if (IdToResourceEntries!=null)
						return;
					
					IDictionary<string, IList<ResourceEntry>> idToResource = new Dictionary<string, IList<ResourceEntry>>();
					for (int i=0; i<Resources.Length; i++) {
						ResourceEntry entry = Resources[i];
						if (!idToResource.ContainsKey(entry.Id))
							idToResource[entry.Id] = new List<ResourceEntry>();
						idToResource[entry.Id].Add(entry);
					}
					IdToResourceEntries = idToResource;
				}
			}
		}

		public object Evaluate(IDictionary context, string expression) {
			return GetResource(expression);
		}
				
		public object GetObject(object context) {
			return GetString(context);
		}

		public string GetString(object context) {
			if (!(context is string))
				throw new ArgumentException("context object should be string");
			return Convert.ToString( GetResource( (string)context ) );
		}

		public object GetResource(string id) {
			return GetResource(id, null, null);
		}

		public object GetResource(string id, string placeId) {
			return GetResource(id, placeId, null);
		}

		public object GetResource(string id, CultureInfo culture) {
			return GetResource(id, null, culture);
		}

		public object GetResource(string id, string placeId, CultureInfo culture) {
			Init();
			if (id==null)
				throw new ArgumentNullException("String resource identifier cannot be null");
			
			IList<ResourceEntry> entriesList = IdToResourceEntries.ContainsKey(id) ? IdToResourceEntries[id] : null;
			if (entriesList==null)
				return id;
			if (culture==null)
				culture = Thread.CurrentThread.CurrentUICulture;
			IPrincipal principal = Thread.CurrentPrincipal;
			
			// compose 'similarity' entry
			ResourceEntry matchedEntry = null;
			int priority = 0;
			
			foreach (ResourceEntry entry in entriesList) {
				int entryPriority = entry.GetPriority(id, placeId, culture.TwoLetterISOLanguageName, principal, this);
				if (entryPriority>priority) {
					matchedEntry = entry;
					priority = entryPriority;
				}
			}
			
			return matchedEntry!=null ? matchedEntry.Resource : id;
		}
		
		
		public class ResourceEntry  {
			string _Id;
			string _LanguageId = null;
			string _PlaceId = null;
			string[] _Roles = null;
			object _Resource;
			
			public string Id { 
				get { return _Id;} 
				set { _Id = value; }
			}

			public string LanguageId { 
				get { return _LanguageId;} 
				set { _LanguageId = value; }
			}
			
			public string PlaceId { 
				get { return _PlaceId;} 
				set { _PlaceId = value; }
			}
			
			public string[] Roles { 
				get { return _Roles;} 
				set { _Roles = value; }
			}

			public object Resource { 
				get { return _Resource;} 
				set { _Resource = value; }
			}			
			
			public ResourceEntry() { }

			public ResourceEntry(string id, string placeId, string langId, string[] roles, object str) {
				Id = id;
				PlaceId = placeId;
				LanguageId = langId;
				Roles = roles;
				Resource = str;
			}

			public int GetPriority(string id, string placeId, string langId, IPrincipal principal, ResourceProvider resourceManager) {
				if (Id!=id) return -1;
				int priority = 0;
				// match language
				if (langId == LanguageId)
					priority += resourceManager.LanguagePriority;
				else if (LanguageId != resourceManager.DefaultLanguageName && LanguageId != null)
					return -1; // not matched at all
				
				// match place
				if (placeId == PlaceId) {
					priority += resourceManager.PlacePriority;
				} else if (PlaceId!=null)
					return -1; // not matched at all
				
				// match role
				if (Roles!=null) {
					int foundRoleIndex = -1;
					for (int i=0; i<Roles.Length; i++)
						if (principal.IsInRole(Roles[i])) {
							foundRoleIndex = i;
							break;
						}
					if (foundRoleIndex<0)
						return -1; // not matched at all

					priority += (resourceManager.RolePriority - Math.Min(foundRoleIndex, 10));
				}
				
				return priority;
			}
		}


	}

	
}
