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
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using NI.Vfs;
using NI.Data;

namespace NI.Data.Vfs {

    /// <summary>
    /// DalcFileSystem helper class
    /// </summary>
    public class DalcFileSystemHelper {

        private IDictionary _FileObjectMap;
        private IDictionary _FileContentMap;
        
        /// <summary>
        /// Map [Property] => [FieldName] of DalcFileObject => Storage Record
        /// </summary>
        public IDictionary FileObjectMap {
            get { return _FileObjectMap; }
            set { _FileObjectMap = value; }
        }

        /// <summary>
        /// Map [Property] => [FieldName] of DalcFileContent => Storage Record
        /// </summary>
        public IDictionary FileContentMap {
            get { return _FileContentMap; }
            set { _FileContentMap = value; }
        }
        
       
        public DalcFileObject SetFileProperties (DalcFileObject file, IDictionary data) {
            return (DalcFileObject)SetProperties(file, data, FileObjectMap);
        }
       
        public DalcFileContent SetContentProperties(DalcFileContent content, IDictionary data) {
			var res = (DalcFileContent)SetProperties(content, data, FileContentMap);
			var dataBuf = (data[FileContentMap["Stream"]] as byte[]) ?? new byte[0];
			content.GetStream(FileAccess.Write).Write(dataBuf, 0, dataBuf.Length);
			return res;
        }

        public IDictionary SetFileDictionaryValues(IDictionary data,DalcFileObject file) {
            return SetValues(data, file, FileObjectMap);
        }

        public IDictionary SetContentDictionaryValues(IDictionary data, DalcFileContent content) {
            var res = SetValues(data, content, FileContentMap);
			res[FileContentMap["Stream"]] = ((MemoryStream)content.GetStream(FileAccess.Read)).ToArray();
			return res;
        }

        /// <summary>
        /// Method sets object properties from dictionary values using map
        /// </summary>
        /// <param name="o">Object</param>
        /// <param name="data">Dictionary</param>
        /// <param name="map">Map</param>
        /// <returns>Object with setted properties</returns>
        private object SetProperties(object o, IDictionary data,IDictionary map) {
            PropertyInfo[] props = o.GetType().GetProperties();
            for (int i = 0; i < props.Length; i++)
                    if (map.Contains(props[i].Name) && data.Contains(map[props[i].Name]) && props[i].CanWrite) {
                     object fldName = props[i].Name;
                     object typedValue = null;
                     if (props[i].PropertyType == typeof(FileType)) {
                         typedValue = Enum.Parse(typeof(FileType),(string)ConvertTo(data[map[fldName]],typeof(string)));
                     } else {
                        typedValue = ConvertTo(data[map[fldName]], props[i].PropertyType);
                     }
                     if (typedValue != null)
                         props[i].SetValue(o, typedValue, null);
                    
                }
            
            return o;
        }
        /// <summary>
        /// Method sets dictionary values from object properties using map
        /// </summary>
        /// <param name="data">Dictionary</param>
        /// <param name="o">Object</param>
        /// <param name="map">Map</param>
        /// <returns>Dictionary with setted values</returns>
        private IDictionary SetValues(IDictionary data, object o, IDictionary map) {
            PropertyInfo[] props = o.GetType().GetProperties();
            for (int i = 0; i < props.Length; i++)                
                if (map.Contains(props[i].Name) && props[i].CanRead) {                    
                    object fldName = props[i].Name;
                    object propValue = props[i].GetValue(o, null);

                    if (props[i].PropertyType.IsEnum) {
                        propValue = Enum.GetName(props[i].PropertyType, propValue);
                    }
                    if (data.Contains(map[props[i].Name]))
                        data[map[props[i].Name]] = propValue;
                    else data.Add(map[props[i].Name], propValue);
                }
            return data;
        }

        #region Service methods
        protected object ConvertTo(object o, Type targetType) {
            object res = null;
            if (o == null || o == DBNull.Value)
            {
                res = null;
            }
            else if (o.GetType().IsAssignableFrom(targetType))
            {
                res = o;
            }
            else
            {
                TypeConverter fromTypeConv = TypeDescriptor.GetConverter(o);
                if (fromTypeConv.CanConvertTo(targetType))
                {
                    res = fromTypeConv.ConvertTo(o, targetType);
                }
                else
                {
                    TypeConverter toTypeConv = TypeDescriptor.GetConverter(targetType);
                    res = toTypeConv.ConvertFrom(o);
                }
            }
            // process datetime limitations
            if (res is DateTime)
            {
                DateTime resDt = (DateTime)res;
                if (resDt == DateTime.MinValue || resDt == DateTime.MaxValue)
                    return null;
            }
            return res;
        }
        #endregion
    }
}
