using System;
using System.Collections;
using System.Text;

using NI.Common;

namespace NI.Data.Dalc {
	
	/// <summary>
	/// Chain of another IQueryModifier implementations
	/// </summary>
	public class ChainQueryModifier : IQueryModifier {
		
		IQueryModifier[] _Modifiers;

		/// <summary>
		/// Get or set chain modifiers list
		/// </summary>
		[Dependency]
		public IQueryModifier[] Modifiers {
			get { return _Modifiers; }
			set { _Modifiers = value; }
		}

		public ChainQueryModifier() { }

		/// <summary>
		/// Modify specified query using external modifiers 'by chain'
		/// </summary>
		/// <param name="q">query to modify</param>
		/// <returns>modified query</returns>
		public IQuery Modify(IQuery q) {
			for (int i=0; i<Modifiers.Length; i++)
				q = Modifiers[i].Modify(q);
			return q;
		}

	}
}
