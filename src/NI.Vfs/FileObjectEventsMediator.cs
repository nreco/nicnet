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

namespace NI.Vfs {
	
	public class FileObjectEventsMediator : IFileObjectEventsMediator {
		
		public event FileObjectEventHandler FileCreating;

		public event FileObjectEventHandler FileCreated;

		public event FileObjectEventHandler FolderCreating;

		public event FileObjectEventHandler FileDeleting;

		public event FileObjectEventHandler FileDeleted;

		public event FileObjectMoveEventHandler FileMoving;

		public event FileObjectMoveEventHandler FileMoved;

		public event FileObjectEventHandler FileCopying;

		public event FileObjectEventHandler FileCopied;

		public event FileObjectOpenEventHandler FileOpening;

		public event FileObjectErrorEventHandler FileError;
		
		bool _SuppressNestedEvents = true;
		int nestedLevel = 0;

		public bool SuppressNestedEvents { get; set; }
				
		public FileObjectEventsMediator() {
			SuppressNestedEvents = true;
		}
		
		protected bool IncNestedLevel() {
			nestedLevel++;
			return (!SuppressNestedEvents || nestedLevel<=1);
		}
		protected bool DecNestedLevel() {
			if (nestedLevel>0)
				nestedLevel--;
			return (!SuppressNestedEvents || nestedLevel == 0);
		}
		
		public void OnFileCreating(FileObjectEventArgs e) {
			bool raise = IncNestedLevel();
			if (FileCreating != null && raise)
				FileCreating(this, e);
		}
		public void OnFolderCreating(FileObjectEventArgs e) {
			bool raise = IncNestedLevel();
			if (FolderCreating != null && raise)
				FolderCreating(this, e);
		}
		public void OnFileCreated(FileObjectEventArgs e) {
			bool raise = DecNestedLevel();
			if (FileCreated!=null && raise)
				FileCreated(this,e);
		}

		public void OnFileDeleting(FileObjectEventArgs e) {
			bool raise = IncNestedLevel();
			if (FileDeleting != null && raise)
				FileDeleting(this, e);			
		}

		public void OnFileDeleted(FileObjectEventArgs e) {
			bool raise = DecNestedLevel();
			if (FileDeleted != null && raise)
				FileDeleted(this, e);				
		}

		public void OnFileMoving(FileObjectMoveEventArgs e) {
			bool raise = IncNestedLevel();
			if (FileMoving != null && raise)
				FileMoving(this, e);			
		}

		public void OnFileMoved(FileObjectMoveEventArgs e) {
			bool raise = DecNestedLevel();
			if (FileMoved != null && raise)
				FileMoved(this, e);				
		}

		public void OnFileCopying(FileObjectEventArgs e) {
			bool raise = IncNestedLevel();
			if (FileCopying != null && raise)
				FileCopying(this, e);			
		}

		public void OnFileCopied(FileObjectEventArgs e) {
			bool raise = DecNestedLevel();
			if (FileCopied != null && raise)
				FileCopied(this, e);				
		}

		public void OnFileOpening(FileObjectOpenEventArgs e) {
			// this is not 'before'-'after' pair event - nothing to do with nested level
			if (FileOpening != null)
				FileOpening(this, e);				
		}

		public void OnFileError(FileObjectErrorEventArgs e) {
			// also lets decrease 'nested' level - error is alt flow for 'before-after' pair
			bool raise = DecNestedLevel();
			if (FileError != null && raise)
				FileError(this, e);				
			
		}
	}
	
}
