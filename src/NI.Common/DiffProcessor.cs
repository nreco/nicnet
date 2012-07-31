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
using System.ComponentModel;

namespace NI.Common {


	/// <summary>
	/// Diff processor 
	/// </summary>
	public class DiffProcessor {
		
		IDiffHandler _DiffHandler;
		
		/// <summary>
		/// Get or set IDiffHandler used to process 'diff' actions
		/// </summary>
		public IDiffHandler DiffHandler {
			get { return _DiffHandler; }
			set { _DiffHandler = value; }
		}
		
		/// <summary>
		/// Synchronize one collection with another
		/// </summary>
		public void Sync(IEnumerable source, IEnumerable destination) {
			// delete actions (elements only from set "to")
			for (IEnumerator toIterator = destination.GetEnumerator() ; toIterator.MoveNext(); ) {
				// find equal element in "from" set
				bool found = false;
				for (IEnumerator fromIterator = source.GetEnumerator(); fromIterator.MoveNext(); )
					if (DiffHandler.Compare(toIterator.Current, fromIterator.Current)==0)
						found = true;
				if (!found)
					DiffHandler.Remove(toIterator.Current);
			}
			
			// add (elements only from set "from") and
			// merge actions (elements both from set "from" and "to")
			for (IEnumerator fromIterator = source.GetEnumerator() ; fromIterator.MoveNext(); ) {
				// find equal element in "to" set
				bool found = false;
				for (IEnumerator toIterator = destination.GetEnumerator() ; toIterator.MoveNext(); )
					if (DiffHandler.Compare(fromIterator.Current, toIterator.Current)==0) {
						found = true;
						
						// do also 'merge' action
						DiffHandler.Merge(fromIterator.Current, toIterator.Current);
					}
				if (!found)
					DiffHandler.Add(fromIterator.Current);
				
			}			
			
			
		}
		

	}
}
