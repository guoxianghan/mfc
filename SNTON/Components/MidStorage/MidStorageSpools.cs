using log4net;
using SNTON.Components.CleanUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC.Logging;
using NHibernate;
using SNTON.Entities.DBTables.MidStorage;
using SNTON.Entities.DBTables.Spools;
using VI.MFC.Utils;
using SNTON.Constants;

namespace SNTON.Components.MidStorage
{
    public class MidStorageSpools : CleanUpBrokerBase, IMidStorageSpools
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "MidStorageSpoolsEntity";
        private const string DatabaseDbTable = "MidStorageSpools";
        private VIThreadEx thread_realtimeequiptask;
        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();
        public List<MidStorageSpoolsEntity> RealTimeMidStoreCache { get; set; }
        void MidStoreCache()
        {
            RealTimeMidStoreCache = GetMidStorages("", null);
            if (RealTimeMidStoreCache == null)
                RealTimeMidStoreCache = new List<MidStorageSpoolsEntity>();
        }
        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static MidStorageSpools Create(XmlNode configNode)
        {
            var broker = new MidStorageSpools();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public MidStorageSpools()
        {
            thread_realtimeequiptask = new VIThreadEx(MidStoreCache, null, "thread for reading MidStoreCache ", 2000);
        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public MidStorageSpools(object dependency)
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
            thread_realtimeequiptask.Start();
            base.StartInternal();//start the cleanup thread
            logger.InfoMethod(EntityDbTable + " broker started.");
        }

        /// <summary>
        /// Here we may load data from the database during startup in case we were
        /// a caching broker. Non-caching broker (preferred) may not do anything here.
        /// </summary>
        public override void ReadBrokerData()
        {
        }

        public List<MidStorageSpoolsEntity> GetMidStorageByArea(short area, IStatelessSession session)
        {
            List<MidStorageSpoolsEntity> ret = null;
            try
            {
                protData.EnterReadLock();
                if (session == null)
                {
                    ret = BrokerDelegate(() => GetMidStorageByArea(area, session), ref session);
                    return ret;
                }

                var tmp = ReadSqlList<MidStorageSpoolsEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND StorageArea=" + area);
                if (tmp.Any())
                {
                    //StringBuilder sb = new StringBuilder();
                    //foreach (var item in tmp)
                    //{
                    //    if (item.SpoolId != 0)
                    //        sb.Append($"'{item.SpoolId.ToString()}',");
                    //}
                    //if (sb.ToString().Trim(',').Length != 0)
                    //{
                    //    var spools = ReadSqlList<SpoolsEntity>(null, "SELECT * FROM SNTON.Spools WHERE ID in (" + sb.ToString().Trim(',') + ")");
                    //    if (spools != null)
                    //        foreach (var item in tmp)
                    //        {
                    //            item.Spool = spools.FirstOrDefault(x => x.Id == item.SpoolId);
                    //        }
                    //}
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get  MidStorageSpoolsEntity list", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public MidStorageSpoolsEntity GetMidStorageById(int storageid, int OriginalId, IStatelessSession session)
        {
            MidStorageSpoolsEntity ret = null;
            try
            {
                protData.EnterReadLock();
                if (session == null)
                {
                    ret = BrokerDelegate(() => GetMidStorageById(storageid, OriginalId, session), ref session);
                    return ret;
                }

                var tmp = ReadSqlList<MidStorageSpoolsEntity>(session, string.Format("SELECT * FROM {0} where  SeqNo = {1} AND StorageArea={3} AND IsDeleted={2} order by ID desc", "dbo.MidStorageSpools", OriginalId, Constants.SNTONConstants.DeletedTag.NotDeleted, storageid));
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                    //ret.Spool = ReadSql<SpoolsEntity>(null, "SELECT * FROM SNTON.Spools WHERE ID=" + ret.SpoolId);

                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get " + EntityDbTable, e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public List<MidStorageSpoolsEntity> GetMidStorages(string sql, IStatelessSession session)
        {
            List<MidStorageSpoolsEntity> ret = null;
            if (session == null)
            {
                ret = BrokerDelegate(() => GetMidStorages(sql, session), ref session);
                return ret;
            }

            try
            {
                protData.EnterReadLock();
                #region MyRegion

                string sqls = "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted;
                if (!string.IsNullOrEmpty(sql))
                    sqls = sqls + " AND " + sql;
                var tmp = ReadSqlList<MidStorageSpoolsEntity>(session, sqls);
                if (tmp != null && tmp.Any())
                {
                    //StringBuilder sb = new StringBuilder();
                    //foreach (var item in tmp)
                    //{
                    //    if (item.SpoolId != 0)
                    //        sb.Append($"'{item.SpoolId.ToString()}',");
                    //}
                    //if (sb.ToString().Trim(',').Length != 0)
                    //{
                    //    var spools = ReadSqlList<SpoolsEntity>(null, "SELECT * FROM SNTON.Spools WHERE ID in (" + sb.ToString().Trim(',') + ")");
                    //    if (spools != null)
                    //        foreach (var item in tmp)
                    //        {
                    //            item.Spool = spools.FirstOrDefault(x => x.Id == item.SpoolId);
                    //        }
                    //}
                    ret = tmp.ToList();
                }
                #endregion
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetMidStorages", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
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
                Update(session, list);
                i = mids.Length;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("更新MidStorageSpoolsEntity[]失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }
        #endregion
    }
}
