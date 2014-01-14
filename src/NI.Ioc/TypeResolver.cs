#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas,  Vitalii Fedorchenko (v.2 changes)
 * Distributed under the LGPL licence
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NI.Ioc {
	
	internal class TypeResolver {

		public TypeResolver() {

		}

		protected int FindBracketClose(string s, int start) {
			int nestedLev = 0;
			int idx = start;
			while (idx < s.Length) {
				switch (s[idx]) {
					case '[':
						nestedLev++;
						break;
					case ']':
						if (nestedLev > 0)
							nestedLev--;
						else
							return idx;
						break;
				}
				idx++;
			}
			return -1;
		}

		public Type ResolveType(string type_description) {
			const char Separator = ',';

			int aposPos = type_description.IndexOf('`');
			bool isGenericType = aposPos >= 0;
			string genericTypePart = String.Empty;

			if (isGenericType) {
				int genericStartArgPos = type_description.IndexOf('[', aposPos);
				if (genericStartArgPos >= 0) { /* real generic type, not definition */
					genericTypePart = type_description.Substring(genericStartArgPos, type_description.Length - genericStartArgPos);
					int genericPartEnd = FindBracketClose(genericTypePart, 1);
					genericTypePart = genericTypePart.Substring(0, genericPartEnd + 1);
					// get generic type definition str
					type_description = type_description.Replace(genericTypePart, String.Empty);
				}
			}

			string[] parts = type_description.Split(new char[] { Separator }, 2);

			if (parts.Length > 1) {
				// assembly name provided
				Assembly assembly;
				try {
					assembly = Assembly.LoadWithPartialName(parts[1]);
				} catch (Exception ex) {
					throw new TypeLoadException("Cannot load assembly " + parts[1], ex);
				}
				if (assembly == null)
					throw new TypeLoadException("Cannot load assembly " + parts[1]);

				try {
					Type t = assembly.GetType(parts[0], true, false);
					if (!String.IsNullOrEmpty(genericTypePart)) {
						// lets get generic type by generic type definition
						List<Type> genArgType = new List<Type>();
						// get rid of [ ]
						string genericTypeArgs = genericTypePart.Substring(1, genericTypePart.Length - 2);
						int genParamStartIdx = -1;
						while ((genParamStartIdx = genericTypeArgs.IndexOf('[', genParamStartIdx + 1)) >= 0) {
							int genParamEndIdx = FindBracketClose(genericTypeArgs, genParamStartIdx + 1);
							if (genParamEndIdx < 0)
								throw new Exception("Invalid generic type definition " + parts[0] + genericTypePart);
							string genArgTypeStr = genericTypeArgs.Substring(genParamStartIdx + 1, genParamEndIdx - genParamStartIdx - 1);
							genArgType.Add(ResolveType(genArgTypeStr));
							// skip processed
							genParamStartIdx = genParamEndIdx;
						}
						t = t.MakeGenericType(genArgType.ToArray());
					}
					return t;
				} catch (Exception ex) {
					throw new TypeLoadException("Cannot resolve type " + type_description, ex);
				}
			} else {
				int lastDotIndex = type_description.LastIndexOf('.');
				if (lastDotIndex >= 0) {
					// try suggest assembly name by namespace
					try {
						return ResolveType(String.Format("{0}{1}", type_description, genericTypePart) + "," + type_description.Substring(0, lastDotIndex));
					} catch {
						//bag suggestion. 
					}
				}
				// finally, lets just try all loaded assemblies
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
					Type type = assembly.GetType(type_description, false);
					if (type != null)
						return type;
				}

			}

			throw new TypeLoadException("Cannot resolve type " + type_description);
		}

	}
}
