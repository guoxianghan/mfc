using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Components.CleanUp;
using SNTON.Entities.DBTables.MidStorage;
using log4net;
using System.Reflection;
using System.Xml;
using VI.MFC.Logging;
using NHibernate;
using SNTON.Entities.DBTables.Spools;
using SNTON.Constants;
using VI.MFC.Utils;

namespace SNTON.Components.MidStorage
{
    public class MidStorage : CleanUpBrokerBase, IMidStorage
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "MidStorageEntity";
        private const string DatabaseDbTable = "SNTON.MidStorage";
        private VIThreadEx thread_realtimeequiptask;
        private const string MIDSTORAGECOUNT = @"SELECT  
                                                 [StorageArea] 
                                                ,[Length]
                                                ,[CName] 
                                                ,[StructBarCode]
	                                            ,COUNT(*)  AS [Count]
                                                FROM [SNTON].[dbo].[MidStorageSpools]
                                                WHERE [IsOccupied] IN (1,2) 
                                                GROUP BY [StorageArea],[Length],[StructBarCode],[CName]";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();
        public List<MidStorageSpoolsCountEntity> MidStorageAccountCache { get; set; }
        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static MidStorage Create(XmlNode configNode)
        {
            var broker = new MidStorage();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public MidStorage()
        {
            thread_realtimeequiptask = new VIThreadEx(ReadMidStorageAccountCache, null, "thread for reading ReadMidStorageAccountCache ", SNTONConstants.ReadingCacheInternal);
        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public MidStorage(object dependency)
        {
            if (dependency == null) // Not called by unittest. We have to instantiate the real object.
            {

            }
        }
        #endregion
        #region Override method
        /// <summary>
        /// Override from base class
        /// Get the class information
        /// </summary>
        /// <returns></returns>
        public override string GetInfo()
        {
            return EntityDbTable + " broker class";
        }

        /// <summary>
        /// Start the broker
        /// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();//start the cleanup thread
            thread_realtimeequiptask.Start();
            logger.InfoMethod(EntityDbTable + " broker started.");
        }

        /// <summary>
        /// Here we may load data from the database during startup in case we were
        /// a caching broker. Non-caching broker (preferred) may not do anything here.
        /// </summary>
        public override void ReadBrokerData()
        {
        }
        #endregion
        void ReadMidStorageAccountCache()
        {
            MidStorageAccountCache = GetMidStorageAccount(null);
        }
        public int UpdateMidStore(IStatelessSession session, params MidStorageSpoolsEntity[] mids)
        {
            int i = 0;
            if (session == null)
            {
                i = BrokerDelegate(() => UpdateMidStore(session, mids), ref session);
                return i;
            }
            try
            {
                protData.EnterWriteLock();
                List<MidStorageEntity> list = new List<MidStorageEntity>();
                foreach (var item in mids)
                {
                    list.Add(new MidStorageEntity() { Created = item.Created, Deleted = item.Deleted, Description = item.Description, HCoordinate = item.HCoordinate, Id = item.Id, IdsList = item.IdsList, IsDeleted = item.IsDeleted, IsEnable = item.IsEnable, IsOccupied = item.IsOccupied, IsVisible = item.IsVisible, SeqNo = item.SeqNo, StorageArea = item.StorageArea, Updated = item.Updated, VCoordinate = item.VCoordinate });
                }
                if (list.Count > 0)
                {
                    Update(session, list);
                }
                i = mids.Length;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("Failed to update Middle storage info", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }
        public List<MidStorageEntity> GetMidStorageByArea(short area, IStatelessSession session)
        {
            List<MidStorageEntity> ret = null;
            if (session == null)
            {
                ret = BrokerDelegate(() => GetMidStorageByArea(area, session), ref session);
                return ret;
            }

            try
            {
                protData.EnterReadLock();
                var tmp = ReadSqlList<MidStorageEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND StorageArea=" + area);
                if (tmp == null || !tmp.Any())
                {
                    return tmp;
                }
                foreach (var item in tmp)
                {
                    if (string.IsNullOrEmpty(item.IdsList.Trim()))
                    {
                        continue;
                    }
                    foreach (var spoolId in item.IdsList.Split(SNTONConstants.Splitors.IdsListSplitor))
                    {
                        if (string.IsNullOrEmpty(spoolId.Trim()))
                        {
                            continue;
                        }
                        var spool = ReadSql<SpoolsEntity>(null, "SELECT * FROM SNTON.Spools WHERE ID=" + spoolId);
                        if (spool != null)
                        {
                            item.Spools.Add(spool);
                        }
                    }
                }
                ret = tmp.ToList();
            }
            catch (Exception e)
            {
                logger.ErrorMethod(string.Format("Failed to get  middle storage by area({0})", area), e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
        /// <summary>
        /// Get the postion which is occupied by the area
        /// </summary>
        /// <param name="area"></param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        public List<MidStorageEntity> GetMidStoragePosOccupiedByArea(short area, IStatelessSession session)
        {
            List<MidStorageEntity> ret = null;
            //By Song@2018.01.14
            if (session == null)
            {
                ret = BrokerDelegate(() => GetMidStoragePosOccupiedByArea(area, session), ref session);
                return ret;
            }

            try
            {
                protData.EnterReadLock();

                var tmp = ReadSqlList<MidStorageEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    //+ Constants.SNTONConstants.DeletedTag.NotDeleted + " AND StorageArea=" + area + " AND IsOccupied=" + Constants.SNTONConstants.OccupiedTag.Occupied);
                    //Get the location which is occupied and booked
                    //By Song@2018.01.14
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND StorageArea=" + area + " AND IsOccupied >= " + Constants.SNTONConstants.OccupiedTag.Occupied);
                //if (tmp.Any())
                if (tmp == null || !tmp.Any())
                {
                    return ret;
                }
                foreach (var item in tmp)
                {
                    //By Song@2018.01.14
                    if (string.IsNullOrEmpty(item.IdsList) || string.IsNullOrWhiteSpace(item.IdsList))
                    {
                        continue;
                    }
                    //if (item.IsOccupied != 0)
                    foreach (var spoolid in item.IdsList.Split(SNTONConstants.Splitors.IdsListSplitor))
                    {
                        var spool = ReadSql<SpoolsEntity>(null, "SELECT * FROM SNTON.Spools WHERE ID=" + spoolid);
                        if (spool != null)
                        {
                            item.Spools.Add(spool);
                        }
                    }
                }
                ret = tmp.ToList();
            }
            catch (Exception e)
            {
                logger.ErrorMethod(string.Format("Failed to get middle storage occupied postion by area({0})", area), e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        /// <summary>
        /// Get the Spool Id in the Mid Storage location which is occupied
        /// </summary>
        /// <param name="midStorageId"></param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        public IList<long> GetSpoolIdListInMidStoragePosOccuiped(int midStorageId, IStatelessSession session)
        {
            IList<long> spoolIdList = null;

            if (session == null)
            {
                spoolIdList = BrokerDelegate(() => GetSpoolIdListInMidStoragePosOccuiped(midStorageId, session), ref session);
                return spoolIdList;
            }
            try
            {
                //Add reader and writer lock
                //By Song@2018.01.14
                protData.EnterReadLock();
                //Get the location which is occupied and booked
                //By Song@2018.01.14
                //var tmp = ReadList<MidStorageEntity>(session, string.Format(" FROM {0} where  ID = {1} AND IsDeleted={2}  AND IsOccupied =" + Constants.SNTONConstants.OccupiedTag.Occupied + " order by ID desc", EntityDbTable, midStorageId, Constants.SNTONConstants.DeletedTag.NotDeleted));
                var tmp = ReadList<MidStorageEntity>(session, string.Format(" FROM {0} where  ID = {1} AND IsDeleted={2}  AND IsOccupied >= {3} order by ID desc",
                                                                              EntityDbTable,
                                                                              midStorageId,
                                                                              SNTONConstants.DeletedTag.NotDeleted,
                                                                              SNTONConstants.OccupiedTag.Occupied));
                if (tmp != null && tmp.Any())
                {
                    MidStorageEntity ret = tmp.FirstOrDefault();
                    spoolIdList = ret.IdsList.Split(',').Cast<long>().ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod(string.Format("Failed to get spool Id list in middle storage (Id:{0})", midStorageId), e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return spoolIdList;
        }
        public List<MidStorageEntity> GetMidStorages(string sql, IStatelessSession session)
        {
            List<MidStorageEntity> ret = null;
            if (session == null)
            {
                ret = BrokerDelegate(() => GetMidStorages(sql, session), ref session);
                return ret;
            }

            try
            {
                protData.EnterReadLock();
                #region MyRegion
                string sqls = "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND " + sql;
                var tmp = ReadSqlList<MidStorageEntity>(session, sqls);
                if (tmp == null || !tmp.Any())
                {
                    return tmp;
                }

                foreach (var item in tmp)
                {
                    if (item.IsOccupied != 0 && !string.IsNullOrEmpty(item.IdsList.Trim()))
                        foreach (var spoolId in item.IdsList.Split(','))
                        {
                            if (string.IsNullOrEmpty(spoolId.Trim()))
                                continue;
                            //Use the current session instead of null value
                            //By Song@2018.01.14.
                            var spool = ReadSql<SpoolsEntity>(session, "SELECT * FROM SNTON.Spools WHERE ID=" + spoolId);
                            if (spool != null)
                            {
                                item.Spools.Add(spool);
                            }
                        }
                }
                ret = tmp.ToList();
                #endregion
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetMidStoragePosOccupiedByArea", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
        public MidStorageEntity GetMidStorageById(int storageId, IStatelessSession session)
        {
            MidStorageEntity ret = null;
            if (session == null)
            {
                ret = BrokerDelegate(() => GetMidStorageById(storageId, session), ref session);
                return ret;
            }

            try
            {
                protData.EnterReadLock();
                var tmp = ReadList<MidStorageEntity>(session, string.Format("FROM {0} where  ID = {1} AND IsDeleted={2} order by ID desc",
                                                                            EntityDbTable,
                                                                            storageId,
                                                                            SNTONConstants.DeletedTag.NotDeleted));
                if (tmp == null || !tmp.Any())
                {
                    return ret;
                }
                var spoolIdsList = tmp.FirstOrDefault().IdsList;
                if (!string.IsNullOrEmpty(spoolIdsList.Trim()))
                {
                    foreach (var spoolId in spoolIdsList.Trim().Split(SNTONConstants.Splitors.IdsListSplitor))
                    {
                        var spool = ReadSql<SpoolsEntity>(null, "SELECT * FROM SNTON.Spools WHERE ID='" + spoolId + "'");
                        if (spool != null)
                        {
                            ret.Spools.Add(spool);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod(string.Format("Failed to get middle storage(Id:{0}) info", storageId), e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }



        public int UpdateMidStore(IStatelessSession session, params MidStorageEntity[] mids)
        {
            int i = 0;
            if (mids.ToList().Count < 1)
            {
                return mids.Length;
            }
            if (session == null)
            {
                i = BrokerDelegate(() => UpdateMidStore(session, mids), ref session);
                return i;
            }

            try
            {
                protData.EnterWriteLock();
                Update(session, mids.ToList());
                logger.InfoMethod("Upadte middle storage data successfully");
                return mids.Length;
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to update middle storage data", e);
                return 0;
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }

        public MidStorageEntity GetMidStorage(string sql, IStatelessSession session)
        {
            MidStorageEntity ret = null;
            if (session == null)
            {
                ret = BrokerDelegate(() => GetMidStorage(sql, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                string condtion = string.IsNullOrEmpty(sql) ? string.Empty : (" AND " + sql);
                var tmp = ReadSqlList<MidStorageEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED = "
                                                                  + SNTONConstants.DeletedTag.NotDeleted
                                                                  + condtion);
                if (tmp.Any())
                {
                    foreach (var item in tmp)
                    {
                        if (string.IsNullOrEmpty(item.IdsList.Trim()))
                        {
                            continue;
                        }
                        foreach (var spoolId in item.IdsList.Split(SNTONConstants.Splitors.IdsListSplitor))
                        {
                            var spool = ReadSql<SpoolsEntity>(null, "SELECT * FROM SNTON.Spools WHERE ID=" + spoolId);
                            if (spool != null)
                            {
                                item.Spools.Add(spool);
                            }
                        }
                    }
                    ret = tmp.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetMidStoragePosOccupiedByArea", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
        public List<MidStorageSpoolsCountEntity> GetMidStorageAccount(IStatelessSession session)
        {
            List<MidStorageSpoolsCountEntity> ret = null;
            if (session == null)
            {
                ret = BrokerDelegate(() => GetMidStorageAccount(session), ref session);
                return ret;
            }

            try
            {
                protData.EnterReadLock();
                var tmp = ReadSqlList<MidStorageSpoolsCountEntity>(session, "SELECT * FROM MidStorageSpoolsCount");
                if (tmp != null && tmp.Any())
                {
                    ret = tmp.ToList();
                }

            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get MidStorageSpoolsCount", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public int ClearMidStoreNo(IStatelessSession session, int storeageareaid, params int[] seqnos)
        {
            int i = 0;
            if (session == null)
            {
                i = BrokerDelegate(() => ClearMidStoreNo(session, storeageareaid, seqnos), ref session);
                return i;
            }
            try
            {
                protData.EnterWriteLock();
                StringBuilder id = new StringBuilder();
                foreach (var item in seqnos)
                {
                    id.Append(item + ",");
                }
                i = RunSqlStatement(session, $"UPDATE SNTON.MidStorage SET IsOccupied=0,IdsList='' WHERE [StorageArea]={storeageareaid} AND [SeqNo] IN ({id.ToString().Trim(',')})");
                return i;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("重置龙门库状态异常", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }
    }
}
