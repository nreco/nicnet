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
	
	public class FileObjectTrigger {
		IFileObjectEventsMediator _EventsMediator;
		FileObjectEvents _TriggerEvents = FileObjectEvents.None;
		Action<FileTriggerEventArgs> _Operation;
		
		public FileObjectEvents TriggerEvents {
			get { return _TriggerEvents; }
			set { _TriggerEvents = value; }
		}
		
		public IFileObjectEventsMediator EventsMediator {
			get { return _EventsMediator; }
			set { _EventsMediator = value; }
		}
		
		public Action<FileTriggerEventArgs> Operation {
			get { return _Operation; }
			set { _Operation = value; }
		}
		
		public FileObjectTrigger() {
			
		}
		
		public void Init() {
			if (EventsMediator == null) return;

			FileObjectEventHandler fileHandler = new FileObjectEventHandler(this.OnFileEvent);
			FileObjectMoveEventHandler fileMoveHandler = new FileObjectMoveEventHandler(this.OnFileMoveEvent);
			FileObjectOpenEventHandler fileOpenHandler = new FileObjectOpenEventHandler(this.OnFileOpenEvent);
			FileObjectErrorEventHandler fileErrorHandler = new FileObjectErrorEventHandler(this.OnFileErrorEvent);
			if ((TriggerEvents & FileObjectEvents.FileCreating) == FileObjectEvents.FileCreating)
				EventsMediator.FileCreating += fileHandler;
			if ((TriggerEvents & FileObjectEvents.FolderCreating) == FileObjectEvents.FolderCreating)
				EventsMediator.FolderCreating += fileHandler;
			if ((TriggerEvents & FileObjectEvents.Copied) == FileObjectEvents.Copied)
				EventsMediator.FileCopied += fileHandler;
			if ((TriggerEvents & FileObjectEvents.Copying) == FileObjectEvents.Copying)
				EventsMediator.FileCopying += fileHandler;
			if ((TriggerEvents & FileObjectEvents.Created) == FileObjectEvents.Created)
				EventsMediator.FileCreated += fileHandler;
			if ((TriggerEvents & FileObjectEvents.Deleted) == FileObjectEvents.Deleted)
				EventsMediator.FileDeleted += fileHandler;
			if ((TriggerEvents & FileObjectEvents.Deleting) == FileObjectEvents.Deleting)
				EventsMediator.FileDeleting += fileHandler;
			if ((TriggerEvents & FileObjectEvents.FileError) == FileObjectEvents.FileError)
				EventsMediator.FileError += fileErrorHandler;
			if ((TriggerEvents & FileObjectEvents.FileOpening) == FileObjectEvents.FileOpening)
				EventsMediator.FileOpening += fileOpenHandler;
			if ((TriggerEvents & FileObjectEvents.Moved) == FileObjectEvents.Moved)
				EventsMediator.FileMoved += fileMoveHandler;
			if ((TriggerEvents & FileObjectEvents.Moving) == FileObjectEvents.Moving)
				EventsMediator.FileMoving += fileMoveHandler;
							
		}
		
		protected void OnFileEvent(object sender, FileObjectEventArgs args) {
			ExecuteOperation(FileObjectEvents.None, args);
		}
		protected void OnFileMoveEvent(object sender, FileObjectMoveEventArgs args) {
			ExecuteOperation(FileObjectEvents.None, args);
		}
		protected void OnFileOpenEvent(object sender, FileObjectOpenEventArgs args) {
			ExecuteOperation(FileObjectEvents.FileOpening, args);
		}
		protected void OnFileErrorEvent(object sender, FileObjectErrorEventArgs args) {
			ExecuteOperation(FileObjectEvents.FileError, args);
		}
		
		protected virtual void ExecuteOperation(FileObjectEvents eventType, FileObjectEventArgs args) {
			if (Operation!=null)
				Operation( new FileTriggerEventArgs(eventType, args) );			
		}
		
		public class FileTriggerEventArgs {
			public FileObjectEvents Event { get; private set; }

			public FileObjectEventArgs Args { get; private set; }

			public FileTriggerEventArgs(FileObjectEvents eventType, FileObjectEventArgs args) {
				Event = eventType;
				Args = args;
			}
		}
		
		
	}
}
