#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
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
using System.Collections;
using System.Text;

using NI.Common.Providers;

namespace NI.Data {

	public class SubstQueryModifier : IQueryModifier, IObjectProvider {
		SubstQueryDescriptor[] _Descriptors;

		public SubstQueryDescriptor[] Descriptors {
			get { return _Descriptors; }
			set { _Descriptors = value; } 
		}

		public IQuery Modify(IQuery q) {
			return ModifyQueryValue(q, new SubstVisitor(null, null) ) as IQuery;
		}

		protected IQueryNode ModifyQueryNode(IQueryNode qNode, SubstVisitor substVisitor) {
			
			if (qNode is IQueryConditionNode) {
				IQueryConditionNode qCondNode = (IQueryConditionNode)qNode;
				IQueryValue modifiedLValue = ModifyQueryValue( qCondNode.LValue, substVisitor );
				IQueryValue modifiedRValue = ModifyQueryValue( qCondNode.RValue, substVisitor );
				// return original if condition values are not modified
				if (modifiedLValue!=qCondNode.LValue || modifiedRValue!=qCondNode.RValue) {
					QueryConditionNode newCondNode = new QueryConditionNode(
						modifiedLValue, qCondNode.Condition, modifiedRValue);
					if (qCondNode is INamedQueryNode)
						newCondNode.Name = ((INamedQueryNode)qCondNode).Name;
					qNode = newCondNode;
				}
			} else if (qNode is IQueryGroupNode) {
				IQueryGroupNode qGroupNode = (IQueryGroupNode)qNode;
				QueryGroupNode newGroupNode = new QueryGroupNode(qGroupNode.Group);
				bool isModifiedQuery = false;
				if (qGroupNode.Nodes!=null)
					foreach (IQueryNode childNode in qGroupNode.Nodes) {
						IQueryNode modifiedQuery = ModifyQueryNode(childNode, substVisitor);
						if (modifiedQuery!=childNode)
							isModifiedQuery = true;
						newGroupNode.Nodes.Add( modifiedQuery );
					}
				// return original node if no changes happens
				if (isModifiedQuery) {
					if (qGroupNode is INamedQueryNode)
						newGroupNode.Name = ((INamedQueryNode)qGroupNode).Name;
					// return modified node
					qNode = newGroupNode;
				}
			}

			qNode = substVisitor.SubstQueryNode(qNode);
			return qNode;
		}

		protected IQueryValue ModifyQueryValue(IQueryValue qValue, SubstVisitor substVisitor) {
			if (qValue is IQuery) {
				IQuery q = (IQuery)qValue;
				SubstQueryDescriptor qSubstDescriptor = FindSubstDescriptor(q.SourceName);
				if (qSubstDescriptor!=null) {
					Query newQuery = new Query(qSubstDescriptor.Query);
					// process virtual relation conditions
					IQueryNode conditions = ModifyQueryNode(q.Root, substVisitor);
					// generate new visitor for merging conditions
					substVisitor = new SubstVisitor(qSubstDescriptor, conditions);
					newQuery.Root = ModifyQueryNode(qSubstDescriptor.Query.Root, substVisitor );
					// remap Fields
					if (q.Fields!=null) {
						newQuery.Fields = q.Fields;
						for (int i=0; i<newQuery.Fields.Length; i++)
							newQuery.Fields[i] = substVisitor.SubstField( (QField)newQuery.Fields[i] ).Name;
					}

					q = newQuery;
				} else {
					// subst fields should be disabled for nested queries
					IQueryNode qRoot = ModifyQueryNode(q.Root,substVisitor);
					if (qRoot!=q.Root) {
						Query newQuery = new Query(q);
						newQuery.Root = qRoot;
						q = newQuery;
					}
				}
				qValue = q;
			}

			return qValue;
		}

		protected SubstQueryDescriptor FindSubstDescriptor(string sourceName) {
			for (int i=0; i<Descriptors.Length; i++)
				if (Descriptors[i].MatchSourceName==sourceName)
					return Descriptors[i];
			return null;
		}

		protected class SubstVisitor {
			SubstQueryDescriptor Descriptor;
			IQueryNode Condition;

			public SubstVisitor(SubstQueryDescriptor descriptor, IQueryNode condition) {
				Descriptor = descriptor;
				Condition = condition;
			}

			string FormatCollection(ICollection c) {
				StringBuilder sb = new StringBuilder();
				foreach (object o in c) {
					if (sb.Length>0)
						sb.Append(',');
					sb.Append( Convert.ToString(o) );
				}
				return sb.ToString();
			}

			protected GroupModifierDescriptor MatchGroupModifier(INamedQueryNode qNode) {
				for (int i=0; i<Descriptor.GroupDescriptors.Length; i++)
					if (Descriptor.GroupDescriptors[i].GroupName==qNode.Name)
						return Descriptor.GroupDescriptors[i];
				return null;
			}

			protected void BuildGroupRecursive(QueryGroupNode buildGroup, IQueryNode node, GroupModifierDescriptor descriptor) {
				if (node is IQueryGroupNode) {
					IQueryGroupNode group = (IQueryGroupNode)node;
					if (group.Group!=GroupType.And)
						throw new Exception("Virtual query can contain only 'AND' condition groups");
					foreach (IQueryNode childNode in group.Nodes)
						BuildGroupRecursive(buildGroup, childNode, descriptor);
				}
				if (node is IQueryConditionNode) {
					IQueryConditionNode condition = (IQueryConditionNode)node;
					if (condition.LValue is IQueryFieldValue) {
						IQueryFieldValue qFldValue = (IQueryFieldValue)condition.LValue;
						if (descriptor.FieldMapping.Contains(qFldValue.Name))
							buildGroup.Nodes.Add( 
								new QueryConditionNode(
									new QField(Convert.ToString(descriptor.FieldMapping[qFldValue.Name])),
									condition.Condition,
									condition.RValue) );
					} else if (condition.RValue is IQueryFieldValue) {
						IQueryFieldValue qFldValue = (IQueryFieldValue)condition.RValue;
						if (descriptor.FieldMapping.Contains(qFldValue.Name))
							buildGroup.Nodes.Add( 
								new QueryConditionNode(
									condition.LValue,
									condition.Condition,
									new QField(Convert.ToString(descriptor.FieldMapping[qFldValue.Name])) ) );
					}
						
				}
			}

			protected QueryGroupNode BuildGroup(IQueryNode condition, GroupModifierDescriptor descriptor) {
				QueryGroupNode groupAnd = new QueryGroupNode(GroupType.And);
				BuildGroupRecursive(groupAnd, condition, descriptor);
				return groupAnd;
			}

			public IQueryFieldValue SubstField(IQueryFieldValue fld) {
				if (Descriptor.FieldMapping.Contains(fld.Name)) {
					return new QField( Convert.ToString( Descriptor.FieldMapping[fld.Name] ) );
				} else
					throw new Exception(
							String.Format("Invalid field name '{0}' of virtual relation '{1}' (allowed field names: {2})",
								fld.Name, Descriptor.MatchSourceName, FormatCollection(Descriptor.FieldMapping.Keys) ) );
			}

			public IQueryNode SubstQueryNode(IQueryNode qNode) {
				if (Descriptor==null) return qNode;
				if (qNode is INamedQueryNode) {
					GroupModifierDescriptor groupModifierDescriptor = MatchGroupModifier( (INamedQueryNode)qNode );
					if (groupModifierDescriptor!=null) {
						QueryGroupNode groupAnd = BuildGroup(Condition,groupModifierDescriptor);
						groupAnd.Name = ((INamedQueryNode)qNode).Name;
						groupAnd.Nodes.Add( qNode );
						return groupAnd;
					}
				}
				return qNode;
			}

		}
		
		public class GroupModifierDescriptor {
			string _GroupName;
			IDictionary _FieldMapping;

			public string GroupName {
				get { return _GroupName; }
				set { _GroupName = value; }
			}

			public IDictionary FieldMapping {
				get { return _FieldMapping; }
				set { _FieldMapping = value; }
			}

			public GroupModifierDescriptor() {
			}

			public GroupModifierDescriptor(string groupName, IDictionary fldMapping) {
				GroupName = groupName;
				FieldMapping = fldMapping;
			}

		}

		public class SubstQueryDescriptor {
			string _MatchSourceName;
			IDictionary _FieldMapping;
			IQuery _Query;
			GroupModifierDescriptor[] _GroupDescriptors;

			public SubstQueryDescriptor() { }

			public SubstQueryDescriptor(string matchSourceName, IQuery substQuery, IDictionary fieldMapping, GroupModifierDescriptor[] groupDescriptors) {
				MatchSourceName = matchSourceName;
				Query = substQuery;
				FieldMapping = fieldMapping;
				GroupDescriptors = groupDescriptors;
			}

			/// <summary>
			/// Get or set query that represents virtual relation
			/// </summary>
			public IQuery Query {
				get { return _Query; }
				set { _Query = value; }
			}

			/// <summary>
			/// Get or set source name to substitute (actually, name of virtual relation)
			/// </summary>
			public string MatchSourceName {
				get { return _MatchSourceName; }
				set { _MatchSourceName = value; }
			}

			/// <summary>
			/// Get or set field mapping for 'virtual' relation
			/// </summary>
			public IDictionary FieldMapping {
				get { return _FieldMapping; }
				set { _FieldMapping = value; }
			}

			/// <summary>
			/// Get or set query group modifiers
			/// </summary>
			public GroupModifierDescriptor[] GroupDescriptors {
				get { return _GroupDescriptors; }
				set { _GroupDescriptors = value; }
			}


		}



		#region IObjectProvider Members

		public object GetObject(object context) {
			return Modify( (IQuery)context );
		}

		#endregion
	}

}
