using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Data.Triggers {

	[Flags]
	public enum DataRowActionType {
		None = 0,

		/// <summary>
		/// Occurs before row insert.
		/// </summary>
		Inserting = 1,

		/// <summary>
		/// Occurs after row insert.
		/// </summary>
		Inserted = 2,

		/// <summary>
		/// Occurs before row update.
		/// </summary>
		Updating = 4,

		/// <summary>
		/// Occurs after row update
		/// </summary>
		Updated = 8,

		/// <summary>
		/// Occurs before row delete
		/// </summary>
		Deleting = 16,

		/// <summary>
		/// Occurs after row delete
		/// </summary>
		Deleted = 32,

		/// <summary>
		/// Inserting or Updating
		/// </summary>
		Saving = 1 + 4,

		/// <summary>
		/// Inserted or Updated
		/// </summary>
		Saved = 2 + 8
	}

}
