using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using System.Data;
using NI.Data;

using NI.Data.Triggers;

namespace NI.Data.Web {
	
	/// <summary>
	/// Triggers DataCacheDependency invalidation by data modification events
	/// </summary>
	public class InvalidateDataDependencyTrigger {

		static Logger log = new Logger(typeof(DataRowTrigger));

		public string DataSourceID { get; set; }

		public InvalidateDataDependencyTrigger(string dataSourceId, DataEventBroker broker) {
			broker.Subscribe( IsMatch, new EventHandler<RowUpdatedEventArgs>(RowUpdatedHandler) );
		}

		public virtual void RowUpdatedHandler(object sender, RowUpdatedEventArgs e) {
			if (!IsMatch(e))
				return;

			var tblName = e.Row.Table.TableName;
			try {
				log.Debug("NotifyChanged (DataSource={0}, TableName={1}) deps count = {2}", DataSourceID, tblName, DataCacheDependency.DependencyPool.Count);
				DataCacheDependency.NotifyChanged(DataSourceID, tblName);
			} catch (Exception ex) {
				log.Error("Cache invalidation failed during NotifyChanged (DataSource={1}, TableName={2}): {0}",
					ex, DataSourceID, tblName);
			}			

		}

		protected virtual bool IsMatch(EventArgs e) {
			if (!(e is RowUpdatedEventArgs)) return false;
			var statementType = ((RowUpdatedEventArgs)e).StatementType;
			return 
				statementType==StatementType.Insert ||
				statementType==StatementType.Update ||
				statementType==StatementType.Delete;
		}
		
	}
}
