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

using NI.Common;
using NI.Common.Caching;
using NI.Common.Providers;

namespace NI.Data {

	/// <summary>
	/// IDalc proxy that enables caching for 'load' results.
	/// </summary>
	public class CacheDalc : IDalc {

		private IDalc _UnderlyingDalc;
		private ICache _LoadCache;
		private ICache _LoadRecordCache;
		private ICache _RecordCountCache;
		private IStringProvider _CacheKeyProvider = new UniqueCacheKeyProvider();
		
        private ICacheEntryValidatorProvider opCacheValidator;
        private IBooleanProvider bpCachingAllowed;
        private IBooleanProvider bpCachingAllowedByQuery;

        private bool enabled = true;
		private bool collectStatistics = false;
		protected CacheDalcStatistics statistics = new CacheDalcStatistics();
		
		/// <summary>
		/// Get or set cache instace used for caching 'Load' results
		/// </summary>
		public ICache LoadCache {
			get { return _LoadCache; }
			set { _LoadCache = value; }
		}
		
		/// <summary>
		/// Get or set cache instace used for caching 'LoadRecord' results
		/// </summary>
		public ICache LoadRecordCache {
			get { return _LoadRecordCache; }
			set { _LoadRecordCache = value; }
		}
		
		/// <summary>
		/// Get or set cache instace used for caching 'RecordCount' results
		/// </summary>
		public ICache RecordCountCache {
			get { return _RecordCountCache; }
			set { _RecordCountCache = value; }
		}		
	
		/// <summary>
		/// Get or set underlying DALC instance
		/// </summary>
		public IDalc UnderlyingDalc {
			get { return _UnderlyingDalc; }
			set { _UnderlyingDalc = value; }
		}
		
		/// <summary>
		/// Get or set cache key provider
		/// </summary>
		public IStringProvider CacheKeyProvider {
			get { return _CacheKeyProvider; }
			set { _CacheKeyProvider = value; }
		}		
	
		/// <summary>
		/// Get or set cache validator provider by source name (s). <br/>
        /// Null validator provider means total caching. 
        /// And null validator for source name does not allow caching of value in CacheDalc.
		/// </summary>
        public ICacheEntryValidatorProvider CacheValidatorProvider {
            get { return opCacheValidator; }
            set { opCacheValidator = value; }
        }

		/// <summary>
		/// Get or set provider that determines if value must be cached by value as context.
		/// </summary>
        public IBooleanProvider ValueCachingAllowedProvider {
            get { return bpCachingAllowed; }
            set { bpCachingAllowed = value; }
        }

		/// <summary>
		/// Get or set provider that determines if value must be cached by query as context.
		/// </summary>
        public IBooleanProvider QueryCachingAllowedProvider {
            get { return bpCachingAllowedByQuery; }
            set { bpCachingAllowedByQuery = value; }
        }

        public bool Enabled {
            get { return enabled; }
            set { enabled = value; }
        }

		public bool CollectStatistics {
			get { return collectStatistics; }
			set { collectStatistics = value; }
		}        

        protected bool CachingAllowed(IQuery query){
            return QueryCachingAllowedProvider == null || QueryCachingAllowedProvider.GetBoolean(query);
        }
		
        protected bool CachingAllowed(LoadDataSetValueContext context) {
			return ValueCachingAllowedProvider == null || ValueCachingAllowedProvider.GetBoolean(context);
        }

		protected bool CachingAllowed(LoadRecordValueContext context) {
			return ValueCachingAllowedProvider == null || ValueCachingAllowedProvider.GetBoolean(context);
        }

        private void GetAccessibleSourceNames(IQueryNode node, List<string> names){
            if (node is IQueryConditionNode){
                IQueryConditionNode cn = (IQueryConditionNode) node;
                if (cn.LValue is IQuery) {
					IQuery q = (IQuery)cn.LValue;
					if (!names.Contains(q.SourceName))
						names.Add(q.SourceName);					
					GetAccessibleSourceNames( q.Root, names);
				}
				if (cn.RValue is IQuery) {
					IQuery q = (IQuery)cn.RValue;
					if (!names.Contains(q.SourceName))
						names.Add(q.SourceName);
					GetAccessibleSourceNames(q.Root, names);
				}
            } else if (node is IQueryGroupNode){
                foreach (IQueryNode child in node.Nodes){
                    GetAccessibleSourceNames(child, names);
                }
            }
        }

        protected ICacheEntryValidator GetValidator(IQuery query){
            if (CacheValidatorProvider != null){
                List<string> list = new List<string>();
				list.Add(query.SourceName);
				GetAccessibleSourceNames(query.Root, list);
				return CacheValidatorProvider.GetValidator(list.ToArray());
            }
            return null;
        }

        protected bool CanAdd(string key, object validator){
            return validator != null || CacheValidatorProvider == null;
        }

		/// <summary>
		/// <see cref="IDalc.Load"/>
		/// </summary>
		public void Load(DataSet ds, IQuery query) {
            if (collectStatistics) statistics.totalLoad += 1;
				
            if ( Enabled && CachingAllowed(query) ){
			    string key = CacheKeyProvider.GetString(query);
			    DataSet cachedDs = LoadCache.Get(key) as DataSet;
			    if (cachedDs == null) {
				    cachedDs = ds.Clone();
				    UnderlyingDalc.Load(cachedDs, query);
                    ICacheEntryValidator validator = GetValidator( query );
                    if ( CanAdd(key, validator) && CachingAllowed( new LoadDataSetValueContext(query, ds) ) ){
				        LoadCache.Put( key, cachedDs, validator );
                    }
			    } else {
					if (collectStatistics) statistics.loadHits += 1;
				}
			    
			    if (!ds.Tables.Contains(query.SourceName))
				    ds.Tables.Add( cachedDs.Tables[query.SourceName].Clone() );
			    for (int i=0; i<cachedDs.Tables[query.SourceName].Rows.Count; i++)
				    ds.Tables[query.SourceName].ImportRow( cachedDs.Tables[query.SourceName].Rows[i] );
            } else {
                UnderlyingDalc.Load(ds, query);
            }
		}

		/// <summary>
		/// <see cref="IDalc.LoadRecord"/>
		/// </summary>
		public bool LoadRecord(IDictionary data, IQuery query) {
			if (collectStatistics) statistics.totalLoadRecord += 1;
            
            if ( Enabled && CachingAllowed(query) ){
			    string key = CacheKeyProvider.GetString(query);
			    IDictionary cachedData = LoadRecordCache.Get(key) as IDictionary;
			    if (cachedData == null) {
				    cachedData = new Hashtable();
				    UnderlyingDalc.LoadRecord(cachedData, query);
                    ICacheEntryValidator validator = GetValidator( query );
                    if ( CanAdd(key, validator) && CachingAllowed( new LoadRecordValueContext(query, cachedData) ) ){
				        LoadRecordCache.Put( key, cachedData, validator );
                    }
			    } else {
					if (collectStatistics) statistics.loadRecordHits += 1;
				}
			    
			    // copy data
			    foreach (DictionaryEntry cachedDataEntry in cachedData)
				    data[cachedDataEntry.Key] = cachedDataEntry.Value;
			    return cachedData.Keys.Count > 0;
            }
            return UnderlyingDalc.LoadRecord(data, query);
		}

		/// <summary>
		/// <see cref="IDalc.RecordCount"/>
		/// </summary>
		public int RecordsCount(string sourceName, IQueryNode conditions) {
			if (collectStatistics) statistics.totalRecordsCount += 1;
			
			Query recordCountQuery = new Query(sourceName, conditions);
            if ( Enabled && CachingAllowed(recordCountQuery) ){
			    string key = CacheKeyProvider.GetString(recordCountQuery);
			    object cachedValue = RecordCountCache.Get(key);
			    if (cachedValue == null) {
				    cachedValue = UnderlyingDalc.RecordsCount( sourceName, conditions);
                    ICacheEntryValidator validator = GetValidator( recordCountQuery );
                    if ( CanAdd(key, validator) ){
				        RecordCountCache.Put( key, cachedValue, validator );
                    }
			    } else {
					if (collectStatistics) statistics.recordsCountHits += 1;
				}
				
			    return Convert.ToInt32( cachedValue );
            }
            return UnderlyingDalc.RecordsCount( sourceName, conditions );
		}
		
		/// <summary>
		/// <see cref="IDalc.Update"/>
		/// </summary>
		public void Update(DataSet ds, string sourceName) {
			UnderlyingDalc.Update(ds, sourceName);
		}

		/// <summary>
		/// <see cref="IDalc.Update"/>
		/// </summary>
		public int Update(IDictionary data, IQuery query) {
			return UnderlyingDalc.Update(data, query);
		}

		/// <summary>
		/// <see cref="IDalc.Insert"/>
		/// </summary>
		public void Insert(IDictionary data, string sourceName) {
			UnderlyingDalc.Insert(data, sourceName);
		}

		/// <summary>
		/// <see cref="IDalc.Delete"/>
		/// </summary>
		public int Delete(IQuery query) {
			return UnderlyingDalc.Delete(query);
		}
		
		public CacheDalcStatistics GetStatistics() {
			return statistics;
		}
		
		public class CacheDalcStatistics {
			internal int totalLoad = 0;
			internal int totalLoadRecord = 0;
			internal int totalRecordsCount = 0;

			internal int loadHits = 0;
			internal int loadRecordHits = 0;
			internal int recordsCountHits = 0;
			
			public int TotalLoad { get { return totalLoad; } }
			public int TotalLoadRecord { get { return totalLoadRecord; } }
			public int TotalRecordsCount { get { return totalRecordsCount; } }

			public int LoadHits { get { return loadHits; } }
			public int LoadRecordHits { get { return loadRecordHits; } }
			public int RecordsCountHits { get { return recordsCountHits; } }

			public int TotalAll { get { return totalRecordsCount + totalLoadRecord + totalLoad; } }
			public int AllHits { get { return loadHits + loadRecordHits + recordsCountHits; } }
		}
		
		public class LoadRecordValueContext {
			IDictionary _RecordData;
			IQuery _Query;
			public IQuery Query { get { return _Query; } }
			public IDictionary RecordData { get { return _RecordData; } }
			public LoadRecordValueContext(IQuery query, IDictionary recordData) {
				this._RecordData = recordData;
				this._Query = query;
			}
		}
		public class LoadDataSetValueContext {
			DataSet _Data;
			IQuery _Query;
			public IQuery Query { get { return _Query; } }
			public DataSet Data { get { return _Data; } }
			public LoadDataSetValueContext(IQuery query, DataSet data) {
				this._Data = data;
				this._Query = query;
			}
		}
		
		
	}
}
