#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013-2014 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
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
using System.Threading.Tasks;

using NI.Data;
using NI.Data.Storage.Model;

namespace NI.Data.Storage {
	
	/// <summary>
	/// Represents abstract storage for <see cref="ObjectContainer"/>
	/// </summary>
	public interface IObjectContainerStorage {

		/// <summary>
		/// Load storage objects by specified IDs
		/// </summary>
		/// <param name="ids">list of object IDs to load</param>
		/// <returns>ID-to-ObjectContainer map for loaded objects</returns>
		/// <remarks>All object properties are loaded by this method.</remarks>
		IDictionary<long, ObjectContainer> Load(long[] ids);

		/// <summary>
		/// Load only certain object properties for specified IDs
		/// </summary>
		/// <param name="ids">list of object IDs to load</param>
		/// <param name="props">properties list to load</param>
		/// <returns>ID-to-ObjectContainer map</returns>
		IDictionary<long, ObjectContainer> Load(long[] ids, Property[] props = null);

		/// <summary>
		/// Insert new object represented by <see cref="ObjectContainer"/> into storage 
		/// </summary>
		/// <param name="obj">object data to insert</param>
		void Insert(ObjectContainer obj);

		/// <summary>
		/// Delete object represented by <see cref="ObjectContainer"/> from storage
		/// </summary>
		/// <param name="obj">object to delete</param>
		void Delete(ObjectContainer obj);
		
		/// <summary>
		/// Update object data represented by <see cref="ObjectContainer"/>
		/// </summary>
		/// <param name="obj">object with modified data</param>
		void Update(ObjectContainer obj);

		/// <summary>
		/// Delete objects from storage by specified IDs
		/// </summary>
		/// <param name="objIds">object IDs to delete</param>
		/// <returns>number of actually removed objects</returns>
		int Delete(params long[] objIds);

		/// <summary>
		/// Add new relations into storage
		/// </summary>
		/// <param name="relations">new relations</param>
		void AddRelation(params ObjectRelation[] relations);

		/// <summary>
		/// Remove relations from storage
		/// </summary>
		/// <param name="relations">relations to remove</param>
		void RemoveRelation(params ObjectRelation[] relations);
		
		/// <summary>
		/// Load object relations between specified object and related objects for given relationships 
		/// </summary>
		/// <param name="obj">target object</param>
		/// <param name="rels">relationships between target object and related objects</param>
		/// <returns>loaded relations</returns>
		IEnumerable<ObjectRelation> LoadRelations(ObjectContainer obj, IEnumerable<Relationship> rels);

		/// <summary>
		/// Load object relations between specified objects and related objects for given relationships 
		/// </summary>
		/// <param name="obj">target objects</param>
		/// <param name="rels">relationships between target objects and related objects</param>
		/// <returns>loaded relations</returns>
		IEnumerable<ObjectRelation> LoadRelations(ObjectContainer[] obj, IEnumerable<Relationship> rels);

		/// <summary>
		/// Load object relations by relationship ID and conditions reperesented by <see cref="QueryNode"/>
		/// </summary>
		/// <param name="relationshipId">relationship ID</param>
		/// <param name="condition">filtering conditions (can be null)</param>
		/// <returns>loaded object relations</returns>
		IEnumerable<ObjectRelation> LoadRelations(string relationshipId, QueryNode condition);
		
		/// <summary>
		/// Load object IDs for specified query
		/// </summary>
		/// <param name="q">object query</param>
		/// <returns>list of matched object IDs</returns>
		long[] GetObjectIds(Query q);

		/// <summary>
		/// Count number of objects that match specified query
		/// </summary>
		/// <param name="q">object query</param>
		/// <returns>number of matched objects</returns>
		int GetObjectsCount(Query q);

	}
}
