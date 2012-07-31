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

namespace NI.Data
{
    public class RecordsCountQueryModifier : IQueryModifier
    {
        private int _RecordCount = 0;

        public int RecordCount
        {
            get { return _RecordCount; }
            set { _RecordCount = value; }
        }

        public IQuery Modify(IQuery q)
        {
            (q as Query).RecordCount = RecordCount;
            return q;
        }
    }
}
