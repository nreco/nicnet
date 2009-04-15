using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;

using NI.Common;
using NI.Common.Providers;
using NI.Common.Globalization;

namespace NI.Common.Expressions
{
	/// <summary>
	/// Expression resolver based on underlying IResourceProvider.
	/// </summary>
	public class ResourceExprResolver : IExpressionResolver
	{
		IResourceProvider _ResourceProvider;
		IStringProvider _LanguageIdProvider = null;
		IStringProvider _PlaceIdProvider = null;
		
		/// <summary>
		/// Get or set resource provider
		/// </summary>
		public IResourceProvider ResourceProvider {
			get { return _ResourceProvider; }
			set { _ResourceProvider = value; }
		}

		public IStringProvider LanguageIdProvider {
			get { return _LanguageIdProvider; }
			set { _LanguageIdProvider = value; }
		}

		public IStringProvider PlaceIdProvider {
			get { return _PlaceIdProvider; }
			set { _PlaceIdProvider = value; }
		}

		public ResourceExprResolver() {
		}

		public object Evaluate(IDictionary context, string expression) 
		{
			CultureInfo cultureInfo = LanguageIdProvider!=null ? 
				CultureInfo.GetCultureInfo(LanguageIdProvider.GetString(context)) : null;
			string placeId = PlaceIdProvider!=null ? PlaceIdProvider.GetString(context) : null;
			
			if (cultureInfo!=null && placeId!=null)
				return ResourceProvider.GetResource(expression, placeId, cultureInfo);
			if (cultureInfo!=null)
				return ResourceProvider.GetResource(expression, cultureInfo);
			if (placeId != null)
				return ResourceProvider.GetResource(expression, placeId);
			
			return ResourceProvider.GetResource(expression);
		}

	}
}
