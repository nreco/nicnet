using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Ioc.Exceptions {

	[Serializable]
	public class XmlConfigurationException : Exception {

		public XmlConfigurationException(string msg)
			: base(msg) {

		}

		public XmlConfigurationException(string msg, Exception inner)
			: base(msg, inner) {

		}
	}
}
