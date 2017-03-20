using System;
using System.Collections;

namespace NI.Common
{
	/// <summary>
	/// Token provider interface
	/// </summary>
	public interface ITokenProvider
	{
		/// <summary>
		/// Provide tokens by given context
		/// </summary>
		/// <param name="context">context dictionary</param>
		/// <returns>result dictionary</returns>
		IDictionary ProvideTokens(IDictionary context);
	}
}
