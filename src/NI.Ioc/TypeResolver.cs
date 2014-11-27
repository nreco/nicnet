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

		public Type ResolveType(string typeDescription) {
			const char Separator = ',';

			var typeName = typeDescription;
			int aposPos = typeName.IndexOf('`');
			bool isGenericType = aposPos >= 0;
			string genericTypePart = String.Empty;

			if (isGenericType) {
				int genericStartArgPos = typeName.IndexOf('[', aposPos);
				if (genericStartArgPos >= 0) { /* real generic type, not definition */
					genericTypePart = typeName.Substring(genericStartArgPos, typeName.Length - genericStartArgPos);
					int genericPartEnd = FindBracketClose(genericTypePart, 1);
					genericTypePart = genericTypePart.Substring(0, genericPartEnd + 1);
					// get generic type definition str
					typeName = typeName.Replace(genericTypePart, String.Empty);
				}
			}

			string[] parts = typeName.Split(new char[] { Separator }, 2);

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
					throw new TypeLoadException("Cannot resolve type " + typeName, ex);
				}
			} else {
				int lastDotIndex = typeName.LastIndexOf('.');
				if (lastDotIndex >= 0) {
					// try suggest assembly name by namespace
					try {
						return ResolveType(typeDescription + "," + typeName.Substring(0, lastDotIndex));
					} catch {
						//bad suggestion. 
					}
				}
				// finally, lets just try all loaded assemblies
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
					Type type = assembly.GetType(typeDescription, false);
					if (type != null)
						return type;
				}

			}

			throw new TypeLoadException("Cannot resolve type " + typeName);
		}

	}
}
