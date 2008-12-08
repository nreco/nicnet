using System;

namespace NI.Factory.Xml {
	/// <summary>
	/// IPropertyInfo
	/// </summary>
	public interface IMapEntryInfo {
		/// <summary>
		/// Entry key
		/// </summary>
		string Key { get; }
		
		/// <summary>
		/// Entry value
		/// </summary>
		IValueInfo Value { get; }
	}
}
