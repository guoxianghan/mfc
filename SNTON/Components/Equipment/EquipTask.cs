using SNTON.Components.CleanUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System.Reflection;
using log4net;
using System.Xml;
using VI.MFC.Logging;
using static SNTON.Constants.SNTONConstants;
using VI.MFC.Utils;

namespace SNTON.Components.Equipment
{
    public class EquipTask : CleanUpBrokerBase, IEquipTask
    {
        //private VIThreadEx cleanupThread;
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipTaskEntity";
        private const string DatabaseDbTable = "SNTON.EquipTask";
        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipTask Create(XmlNode configNode)
        {
            var broker = new EquipTask();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipTask()
        {
            //cleanupThread = new VIThreadEx(Clearup, null, "Clearup", 60000);

        }

        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipTask(object dependency)
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
            //cleanupThread.Start();
            base.StartInternal();//start the cleanup thread
            logger.InfoMethod(EntityDbTable + " broker started.");
        }

        /// <summary>
        /// Here we may load data from the database during startup in case we were
        /// a caching broker. Non-caching broker (preferred) may not do anything here.
        /// </summary>
        public override void ReadBrokerData()
        {
            //var tmp = ReadSqlList<EquipTaskEntity>(null, $"SELECT top 1 * FROM {DatabaseDbTable}  order by Id desc");
        }
        #endregion


        public void CreateEquipTask(int id, int TaskType, int PlantNo, IStatelessSession session)
        {
            if (session == null)
            {
                BrokerDelegate(() => CreateEquipTask(id, TaskType, PlantNo, session), ref session);
                return;
            }
            try
            {
                EquipTaskEntity entity = new EquipTaskEntity() { Created = DateTime.Now, IsDeleted = DeletedTag.NotDeleted, Status = 0, EquipContollerId = id, TaskType = 0, PlantNo = 0 };
                Insert<EquipTaskEntity>(session, entity);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to add EquipTaskEntity", e);
                throw e;
            }

        }

        public List<EquipTaskEntity> GetEquipTaskEntitySqlWhere(string sqlwhere, IStatelessSession session = null)
        {
            List<EquipTaskEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTaskEntitySqlWhere(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = " and " + sqlwhere;
                }
                var tmp = ReadSqlList<EquipTaskEntity>(session, $"SELECT * FROM SNTON.EquipTask where IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sqlwhere + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTaskEntity list", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public bool UpdateEntity(EquipTaskEntity entity, IStatelessSession session = null)
        {
            bool r = false;
            if (session == null)
            {
                //r = UpdateEntity(entity, session);
                r = BrokerDelegate(() => UpdateEntity(entity, session), ref session);
                return r;
            }
            try
            {
                protData.EnterWriteLock();
                this.Update(session, entity);
                r = true;
            }
            catch (Exception ex)
            {
                logger.WarnMethod("更新EquipTaskEntity失败", ex);
                r = false;
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return r;
        }

        public EquipTaskEntity GetEquipTaskEntityByID(long id, IStatelessSession session = null)
        {
            EquipTaskEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTaskEntityByID(id, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                var tmp = ReadSqlList<EquipTaskEntity>(session, $"SELECT * FROM SNTON.EquipTask where id= '" + id.ToString() + "'and IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTaskEntity list", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
        protected override void MarkDataForDeletion(DateTime olderThan, int threadDeleteMaxRecords, IStatelessSession theSession = null)
        {
            if (theSession == null)
            {
                BrokerDelegate(() => MarkDataForDeletion(olderThan, threadDeleteMaxRecords, theSession), ref theSession);
                return;
            }
            try
            {
                protData.EnterWriteLock();
                string sql = $"UPDATE {DatabaseDbTable} SET ISDELETED=1 WHERE CREATED<='{olderThan.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}' AND [Status] IN (0,8,10,12) AND  ISDELETED=0";
                int result = RunSqlStatement(theSession, sql);
            }
            catch (Exception ex)
            {
                logger.WarnMethod("EquipTaskEntity更新删除标记失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }
        public override long DeleteRecordsOlderThan(DateTime theDate, long maxRecords)
        {
            int result = 0;
            try
            {
                string sql = $"DELETE {DatabaseDbTable} WHERE ISDELETED=1 AND  CREATED<='{theDate.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}";
                result = RunSqlStatement(null, sql);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("faild to delete", ex);
            }
            return result;
        }
        protected override int DeleteDataMarkedDeleted(IStatelessSession session = null)
        {
            return 0;
        }
        public bool CreateEquipTask(EquipTaskEntity entity, IStatelessSession session = null)
        {
            bool r = false;
            if (session == null)
            {
                r = BrokerDelegate(() => CreateEquipTask(entity, session), ref session);
                return r;
            }
            try
            {
                Insert(session, entity);
                r = true;
            }
            catch (Exception e)
            {
                logger.ErrorMethod("创建 EquipTaskEntity失败", e);
                //throw e;
                r = false;
            }
            return r;
        }

        public int UpdateEntity(List<EquipTaskEntity> entities, IStatelessSession session = null)
        {
            int i = 0;
            if (entities == null || entities.Count == 0)
                return 0;
            if (session == null)
            {
                i = BrokerDelegate(() => UpdateEntity(entities, session), ref session);
                return i;
            }
            try
            {
                protData.EnterWriteLock();
                Update(session, entities);
                i = entities.Count;
            }
            catch (Exception e)
            {
                i = 0;
                logger.ErrorMethod("Failed to update EquipTaskEntity", e);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }

        public bool UpdateStatus(int status, IStatelessSession session, params long[] ids)
        {
            bool r = false;

            if (session == null)
            {
                r = BrokerDelegate(() => UpdateStatus(status, session, ids), ref session);
                return r;
            }
            try
            {
                protData.EnterWriteLock();
                if (ids.Length == 0)
                    return false;
                StringBuilder sb = new StringBuilder();
                foreach (var item in ids)
                {
                    sb.Append(item + ",");
                }
                int i = RunSqlStatement(session, "UPDATE SNTON.EquipTask SET [STATUS]=" + status + " WHERE ID IN (" + sb.ToString().Trim(',') + ")");
                if (i == 0)
                    return false;
                else return true;
            }
            catch (Exception ex)
            {
                logger.WarnMethod("更新设备任务状态失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }

            return true;
        }

        public List<EquipTaskEntity> GetEquipTaskEntityNotDeleted(string sqlwhere, IStatelessSession session = null)
        {
            List<EquipTaskEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTaskEntitySqlWhere(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = " and " + sqlwhere;
                }
                var tmp = ReadSqlList<EquipTaskEntity>(session, $"SELECT * FROM SNTON.EquipTask where " + sqlwhere + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTaskEntity list", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
    }
}
