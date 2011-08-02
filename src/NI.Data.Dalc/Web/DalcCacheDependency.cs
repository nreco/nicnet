using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

namespace NI.Data.Dalc.Web {
	
	public class DalcCacheDependency : CacheDependency {

		internal static List<DalcCacheDependency> DependencyPool = new List<DalcCacheDependency>();

		public string DataSource { get; private set; }
		public string SourceName { get; private set; }

		public DalcCacheDependency(string dataSource, string sourceName) {
			DataSource = dataSource;
			SourceName = sourceName;
			lock (DependencyPool) {
				DependencyPool.Add(this);
			}
		}

		public static void NotifyChanged(string dataSource, string sourceName) {
			for (int i = 0; i < DependencyPool.Count; i++) {
				var dep = DependencyPool[i];
				if (!dep.Disposed && dep.SourceName==sourceName && dep.DataSource==dataSource)
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
