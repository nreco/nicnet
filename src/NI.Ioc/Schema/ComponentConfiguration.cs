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
using System.Xml;
using System.ComponentModel;
using System.Threading;

namespace NI.Ioc
{
	/// <summary>
	/// Generic in-memory implementation of IComponentFactoryConfiguration.
	/// </summary>
	public class ComponentConfiguration : IComponent, IComponentFactoryConfiguration
	{
		IComponentInitInfo[] Components;
		IDictionary<string,IComponentInitInfo> ComponentsByName;
		
		/// <summary>
		/// Components collection description
		/// Null by default
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Get number of top-level component definitions
		/// </summary>
		public int Count {
			get {
				return Components.Length;
			}
		}

		ISite site;

		public virtual ISite Site {
			get { return site; }
			set {
				site = value;
			}
		}

		public event EventHandler Disposed;

		public ComponentConfiguration(IComponentInitInfo[] components) {
			Components = components;
			ComponentsByName = new Dictionary<string, IComponentInitInfo>(components.Length);
			for (int i = 0; i < Components.Length; i++) {
				var cInfo = Components[i];
				if (cInfo.Name != null) {
					ComponentsByName[cInfo.Name] = cInfo;
				}
			}

		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				var flag = false;
				try {
					Monitor.Enter(this, ref flag);

					if (this.site != null && this.site.Container != null) {
						this.site.Container.Remove(this);
					}

					// lets remove references
					if (ComponentsByName != null)
						ComponentsByName.Clear();
					ComponentsByName = null;
					Components = null;

					if (Disposed != null)
						Disposed(this, EventArgs.Empty);

				} finally {
					if (flag) {
						Monitor.Exit(this);
					}
				}
			}
		}
		
		public IComponentInitInfo this[string name] {
			get {
				if (name==null)
					return null; // legacy compatibility behavour
				return ComponentsByName.ContainsKey(name) ? ComponentsByName[name] : null;
			}
		}
		
		public IComponentInitInfo this[Type type] {
			get {
				for (int i=0; i<Components.Length; i++)
					if (Components[i]!=null && type.IsAssignableFrom(Components[i].ComponentType) )
						return Components[i];
				return null;
			}
		}
		
		public IEnumerator GetEnumerator() {
			return Components.GetEnumerator();
		}		
		
	}
}
