using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

namespace NI.Data.Web {
	
	/// <summary>
	/// Establishes a relationship between an item stored in an ASP.NET application's Cache object and the results of data query
	/// </summary>
	public class DataCacheDependency : CacheDependency {

		internal static List<DataCacheDependency> DependencyPool = new List<DataCacheDependency>();

		public string DataSource { get; private set; }
		public string[] TableNames { get; private set; }

		/// <summary>
		/// Initializes a new instance of the DataCacheDependency and creates dependency from specified data table
		/// </summary>
		/// <param name="dataSourceId">unique data source identifier</param>
		/// <param name="tableName">data table name</param>
		public DataCacheDependency(string dataSourceId, string tableName) : this(dataSourceId,new[]{tableName}) {
		}

		/// <summary>
		/// Initializes a new instance of the DataCacheDependency and creates dependency from specified data tables
		/// </summary>
		/// <param name="dataSourceId">unique data source identifier</param>
		/// <param name="tableNames">list of data table names</param>
		public DataCacheDependency(string dataSourceId, string[] tableNames) {
			DataSource = dataSourceId;
			TableNames = tableNames;
			lock (DependencyPool) {
				DependencyPool.Add(this);
			}
		}
		
		public virtual bool IsMatch(string dataSource, string sourceName) {
			return DataSource==dataSource && Array.IndexOf(TableNames,sourceName)>=0;
		}
		
		public static void NotifyChanged(string dataSourceId, string tableName) {
			for (int i = 0; i < DependencyPool.Count; i++) {
				var dep = DependencyPool[i];
				if (!dep.Disposed && dep.IsMatch(dataSourceId, tableName))
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
