using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Data {
	
	/// <summary>
	/// Internal NI.Data assembly logger
	/// </summary>
	/// <remarks>Default handlers use Trace and Debug classes</remarks>
	public class Logger {
		
		static Action<Type,string> error;
		static Action<Type,string> debug;
		static Action<Type,string> info;
		
		static Logger() {
			error = TraceError;
			info = TraceInfo;
			debug = DebugPrint;
		}

		Type t;

		internal Logger(Type t) {
			this.t = t;
		}

		public static void SetError(Action<Type,string> errorHandler) {
			error = errorHandler;
		}
		public static void SetDebug(Action<Type, string> debugHandler) {
			debug = debugHandler;
		}
		public static void SetInfo(Action<Type, string> infoHandler) {
			info = infoHandler;
		}

		static void TraceError(Type t, string m) {
			Trace.TraceError("[{0}] {1}", t, m);
		}

		static void TraceInfo(Type t, string m) {
			Trace.TraceInformation("[{0}] {1}", t, m);
		}

		static void DebugPrint(Type t, string m) {
			System.Diagnostics.Debug.Print( "[{0}] {1}", t, m );
		}

		public void Error(string s) {
			if (error!=null)
				error(t,s);
		}

		public void Error(string s, params object[] args) {
			Error( String.Format(s,args) );
		}

		public void Info(string s) {
			if (info != null)
				info(t,s);
		}

		public void Info(string s, params object[] args) {
			Info(String.Format(s, args));
		}

		public void Debug(string s) {
			if (debug != null)
				debug(t,s);
		}

		public void Debug(string s, params object[] args) {
			Debug(String.Format(s, args));
		}

	}
}
