using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Data.Permissions {
	
	/// <summary>
	/// Represents query rule used by <see cref="DbPermissionCommandGenerator"/>
	/// </summary>
	public interface IQueryRule {
		QueryNode ComposeCondition(PermissionContext context);
	}
}
