using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

namespace NI.Data.Dalc.Web {
	
	public class DalcCacheDependency : CacheDependency {

		internal static List<DalcCacheDependency> DependencyPool = new List<DalcCacheDependency>();

		public string DataSource { get; private set; }
		public string[] SourceNames { get; private set; }

		public DalcCacheDependency(string dataSource, string sourceName) : this(dataSource,new[]{sourceName}) {
		}

		public DalcCacheDependency(string dataSource, string[] sourceNames) {
			DataSource = dataSource;
			SourceNames = sourceNames;
			lock (DependencyPool) {
				DependencyPool.Add(this);
			}
		}

		public static void NotifyChanged(string dataSource, string sourceName) {
			for (int i = 0; i < DependencyPool.Count; i++) {
				var dep = DependencyPool[i];
				if (!dep.Disposed && dep.DataSource==dataSource && Array.IndexOf(dep.SourceNames,sourceName)>=0)
					dep.NotifyDependencyChanged(dep, EventArgs.Empty);
			}
		}

		bool Disposed = false;
		protected override void DependencyDispose() {
			if (!Disposed) {
				Disposed = true;
				lock (DependencyPool) {
					DependencyPool.Remove(this);
				}
			}
 			base.DependencyDispose();
		}


	}
}
