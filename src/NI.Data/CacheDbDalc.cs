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
using System.Data;
using System.Collections;
using System.Collections.Generic;


namespace NI.Data {
	//TODO: rethink in System.Runtime.Caching
    /*public class CacheDbDalc : CacheDalc, ISqlDalc {

		/// <summary>
		/// Get or set underlying DB DALC component
		/// </summary>
		public ISqlDalc UnderlyingDbDalc {
			get { return (ISqlDalc) base.UnderlyingDalc; }
			set { base.UnderlyingDalc = value; }
		}

		private ICache _LoadReaderCache = null;

		/// <summary>
		/// Get or set cache instace used for caching 'LoadReader' results
		/// </summary>
		public ICache LoadReaderCache {
			get { return _LoadReaderCache; }
			set { _LoadReaderCache = value; }
		}

        public System.Data.IDbConnection Connection {
			get { return UnderlyingDbDalc.Connection; }
			set { UnderlyingDbDalc.Connection = value; }
        }

        public System.Data.IDbTransaction Transaction {
			get { return UnderlyingDbDalc.Transaction; }
			set { UnderlyingDbDalc.Transaction = value; }
        }

        public int Execute(string sqlText) {
            return UnderlyingDbDalc.Execute(sqlText);
        }

        public IDataReader ExecuteReader(string sqlText) {
            return UnderlyingDbDalc.ExecuteReader(sqlText);
        }

		protected bool CachingAllowed(LoadReaderValueContext context) {
			return ValueCachingAllowedProvider == null || ValueCachingAllowedProvider.GetBoolean(context);
		}

        public IDataReader LoadReader(Query q) {
			//if (collectStatistics) statistics.totalLoadRecord += 1;
			if (Enabled && CachingAllowed(q)) {
				string key = CacheKeyProvider.GetString(q);
				CacheDataReader cachedReader = LoadReaderCache.Get(key) as CacheDataReader;
				if (cachedReader == null) {
					IDataReader realReader = UnderlyingDbDalc.LoadReader(q);
					ICacheEntryValidator validator = GetValidator(q);
					if (LoadReaderCache!=null && CanAdd(key, validator) && CachingAllowed(new LoadReaderValueContext(q, realReader))) {
						// lets prepare data from real reader
						List<CacheDataReaderField> fields = new List<CacheDataReaderField>();
						List<object[]> data = new List<object[]>();
						// populate fields
						for (int i = 0; i < realReader.FieldCount; i++)
							fields.Add( new CacheDataReaderField( realReader.GetName(i), realReader.GetDataTypeName(i), realReader.GetFieldType(i) ) );
						// data
						while (realReader.Read()) {
							object[] values = new object[realReader.FieldCount];
							realReader.GetValues(values);
							data.Add(values);
						}
						// lets close real reader 
						DataTable realRdrSchemaTable = realReader.GetSchemaTable();
						realReader.Close();

						cachedReader = new CacheDataReader(fields.ToArray(), data, realRdrSchemaTable );
						LoadReaderCache.Put(key, cachedReader, validator);
						return cachedReader.Clone();
					}
					return realReader;
				} else {
					//if (collectStatistics) statistics.loadHits += 1;
					return cachedReader.Clone();
				}
			}
            
			return UnderlyingDbDalc.LoadReader(q);
        }

        public void Load(DataSet ds, string sqlText) {
            UnderlyingDbDalc.Load(ds, sqlText);
        }

        public bool LoadRecord(IDictionary data, string sqlCommandText) {
            return UnderlyingDbDalc.LoadRecord(data, sqlCommandText);
        }

		internal class CacheDataReaderField {
			public string FieldName;
			public string DataTypeName;
			public Type FieldType;
			public CacheDataReaderField(string fldName, string dataTypeName, Type t) {
				FieldName = fldName;
				DataTypeName = dataTypeName;
				FieldType = t;
			}
		}

		public class LoadReaderValueContext {
			IDataReader _DataReader;
			Query _Query;
			public Query Query { get { return _Query; } }
			public IDataReader DataReader { get { return _DataReader; } }
			public LoadReaderValueContext(Query query, IDataReader dataRdr) {
				this._DataReader = dataRdr;
				this._Query = query;
			}
		}

		internal class CacheDataReader : IDataReader {
			bool isClosed = false;
			DataTable schemaTable = null;
			int depth = 0;
			IList<object[]> readerData;
			CacheDataReaderField[] fields;
			int readerCurrentIndex = -1;

			public CacheDataReader(CacheDataReaderField[] fields, IList<object[]> data, DataTable schemaTbl) {
				this.schemaTable = schemaTbl;
				this.readerData = data;
				this.fields = fields;
			}

			public CacheDataReader Clone() {
				return new CacheDataReader(fields, readerData, schemaTable);
			}

			protected void CheckFieldIndex(int i) {
				if (i<0 || i>=fields.Length)
					throw new IndexOutOfRangeException();
			}

			public void Close() {
				isClosed = true;
			}

			public int Depth {
				get { return depth; }
			}

			public DataTable GetSchemaTable() {
				if (schemaTable==null)
					throw new NotImplementedException();
				return schemaTable;
			}

			public bool IsClosed {
				get { return isClosed; }
			}

			public bool NextResult() {
				return false; // we didn't support cached batch queries
			}

			public bool Read() {
				if ((readerCurrentIndex + 1) < readerData.Count) {
					readerCurrentIndex++;
					return true;
				} else {
					return false;
				}
			}

			public int RecordsAffected {
				get { return 0; //* cache data reader is used only for select queries  
                }
			}

			public void Dispose() {
				readerData = null;
				schemaTable = null;
			}

			public int FieldCount {
				get { return fields.Length; }
			}

			public bool GetBoolean(int i) {
				CheckFieldIndex(i);
				return Convert.ToBoolean( readerData[readerCurrentIndex][i] );
			}
			public byte GetByte(int i) {
				CheckFieldIndex(i);
				return Convert.ToByte( readerData[readerCurrentIndex][i] );
			}
			public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
				CheckFieldIndex(i);
				byte[] fldBytes = (byte[])readerData[readerCurrentIndex][i];
				if (buffer!=null) {
					Array.Copy(fldBytes, fieldOffset, buffer, bufferoffset, length);
				}
				return fldBytes.Length;
			}

			public char GetChar(int i) {
				CheckFieldIndex(i);
				return Convert.ToChar( readerData[readerCurrentIndex][i] );
			}

			public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
				CheckFieldIndex(i);
				char[] fldChars = (char[])readerData[readerCurrentIndex][i];
				if (buffer!=null) {
					Array.Copy(fldChars, fieldoffset, buffer, bufferoffset, length);
				}
				return fldChars.Length;
				
			}

			public IDataReader GetData(int i) {
				CheckFieldIndex(i);
				return new CacheDataReader(
					new CacheDataReaderField[] { fields[i] }, readerData, schemaTable );
			}

			public string GetDataTypeName(int i) {
				CheckFieldIndex(i);
				return fields[i].DataTypeName;
			}

			public DateTime GetDateTime(int i) {
				CheckFieldIndex(i);
				return Convert.ToDateTime( readerData[readerCurrentIndex][i] );
			}

			public decimal GetDecimal(int i) {
				CheckFieldIndex(i);
				return Convert.ToDecimal( readerData[readerCurrentIndex][i] );
			}

			public double GetDouble(int i) {
				CheckFieldIndex(i);
				return Convert.ToDouble( readerData[readerCurrentIndex][i] );
			}

			public Type GetFieldType(int i) {
				CheckFieldIndex(i);
				return fields[i].FieldType;
			}

			public float GetFloat(int i) {
				CheckFieldIndex(i);
				return Convert.ToSingle( readerData[readerCurrentIndex][i] );
			}

			public Guid GetGuid(int i) {
				CheckFieldIndex(i);
				return (Guid)readerData[readerCurrentIndex][i];
			}

			public short GetInt16(int i) {
				CheckFieldIndex(i);
				return Convert.ToInt16( readerData[readerCurrentIndex][i] );
			}

			public int GetInt32(int i) {
				CheckFieldIndex(i);
				return Convert.ToInt32( readerData[readerCurrentIndex][i] );
			}

			public long GetInt64(int i) {
				CheckFieldIndex(i);
				return Convert.ToInt64( readerData[readerCurrentIndex][i] );
			}

			public string GetName(int i) {
				CheckFieldIndex(i);
				return fields[i].FieldName;
			}

			public int GetOrdinal(string name) {
				for (int i=0; i<fields.Length; i++)
					if (fields[i].FieldName==name)
						return i;
				throw new IndexOutOfRangeException();
			}

			public string GetString(int i) {
				CheckFieldIndex(i);
				return readerData[readerCurrentIndex][i] as string;
			}

			public object GetValue(int i) {
				return readerData[readerCurrentIndex][i];
			}

			public int GetValues(object[] values) {
				int copyCnt = 0;
				for (int i=0; i<fields.Length; i++)
					if (i<values.Length) {
						values[i] = readerData[readerCurrentIndex][i];
						copyCnt++;
					} else break;
				return copyCnt;
			}

			public bool IsDBNull(int i) {
				CheckFieldIndex(i);
				object value = readerData[readerCurrentIndex][i];
				return value==DBNull.Value;
			}

			public object this[string name] {
				get {
					int i = GetOrdinal(name);
					return readerData[readerCurrentIndex][i];
				}
			}

			public object this[int i] {
				get { 
					CheckFieldIndex(i);
					return readerData[readerCurrentIndex][i];
				}
			}

		}


    }*/

}
