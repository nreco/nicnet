using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Ioc {
	
	/// <summary>
	/// Common IComponentFactory interface extensions 
	/// </summary>
	public static class ComponentFactoryExt {

		/// <summary>
		/// Return an instance of the specified component that match specified type.
		/// </summary>
		/// <typeparam name="T">type the component must match</typeparam>
		/// <param name="name"></param>
		/// <returns>component instance of desired type</returns>
		public static T GetComponent<T>(this IComponentFactory factory, string name) {
			return (T)factory.GetComponent(name);
		}

	}
}
