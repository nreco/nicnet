#region License
/*
 * NIC.NET library
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
using System.IO;

namespace NI.Data.Vfs {
    /// <summary>
    /// Specific implementation of stream for DalcFileContent instances
    /// </summary>
    public class DalcFileStream : MemoryStream {

        private DalcFileContent Content;

        public override void Close() {
            Content.Close();
            base.Close();
        }

        public DalcFileStream(DalcFileContent content) {
            Content = content;
        }

        public void CloseMemoryStream()  {            
            base.Close();
        }

    }
}
