using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

using System.Data;
using NI.Data;

namespace NI.Data.Web {

	public class DalcCacheDependencyDataRowTrigger : DbDalcDataRowTrigger {

		public string DataSource { get; set; }

		protected override bool IsMatch(DataRow r, EventType eventType) {
			// catch only inserted/updated/deleted events
			if (eventType != EventType.Inserted && eventType != EventType.Updated && eventType != EventType.Deleted)
				return false;
			return true;
		}

		protected override void Execute(DbDalcDataRowTrigger.EventType eventType, DataRow r, object sender, EventArgs args) {
			var sourceName = r.Table.TableName;
			
			try {
				LogDebug(String.Format("NotifyChanged (DataSource={0}, SourceName={1}) deps count = {2}", DataSource, sourceName, DalcCacheDependency.DependencyPool.Count));
				DalcCacheDependency.NotifyChanged(DataSource, sourceName);
			} catch (Exception ex) {
				LogError(String.Format("During NotifyChanged (DataSource={1}, SourceName={2}): {0}", ex, DataSource, sourceName));
			}			
		}		
		
	}
}
