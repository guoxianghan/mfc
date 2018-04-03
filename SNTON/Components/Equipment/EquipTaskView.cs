using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Components.CleanUp;
using log4net;
using System.Reflection;
using System.Xml;
using VI.MFC.Logging;
using SNTON.Entities.DBTables.AGV;
using VI.MFC.Utils;
using SNTON.Constants;

namespace SNTON.Components.Equipment
{
    public class EquipTaskView : CleanUpBrokerBase, IEquipTaskView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipTaskViewEntity";
        private const string DatabaseDbTable = "dbo.EquipTaskView";
        public List<EquipTaskViewEntity> RealTimeEquipTaskStatus { get; set; }
        private VIThreadEx thread_realtimeequiptask;
        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipTaskView Create(XmlNode configNode)
        {
            var broker = new EquipTaskView();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipTaskView()
        {
            thread_realtimeequiptask = new VIThreadEx(RealTimeTask, null, "thread for reading realtimeequiptask ", SNTONConstants.ReadingCacheInternal);
        }
        void RealTimeTask()
        {
            RealTimeEquipTaskStatus = GetEquipTaskViewEntities($"[STATUS] IN (0,1,2,3,4,5,6,9,10) AND ISCANCEL IN (0,3,4)", null);
        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipTaskView(object dependency)
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
            var tmp = ReadSqlList<EquipTaskViewEntity>(null, $"SELECT top 1 * FROM {DatabaseDbTable}  order by ID desc");
            try
            {

            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to ReadBrokerData", e);
            }
        }
        #endregion
        public List<EquipTaskViewEntity> GetEquipTaskViewEntities(string sql, IStatelessSession session = null)
        {
            List<EquipTaskViewEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTaskViewEntities(sql, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                string sqll = "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted;
                if (!string.IsNullOrEmpty(sql))
                    sqll = sqll + " and " + sql;
                var tmp = ReadSqlList<EquipTaskViewEntity>(session, sqll);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTaskViewEntity", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
        public List<EquipTaskViewEntity> GetEquipTaskViewNotDeleted(string sql, IStatelessSession session = null)
        {
            List<EquipTaskViewEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTaskViewNotDeleted(sql, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                string sqll = "SELECT * FROM " + DatabaseDbTable + " WHERE " + sql;
                //if (!string.IsNullOrEmpty(sql))
                //    sqll = sqll + " and " + sql;
                var tmp = ReadSqlList<EquipTaskViewEntity>(session, sqll);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTaskViewEntity", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }



        public int Update(IStatelessSession session, params EquipTaskViewEntity[] equiptsks)
        {
            int r = 0;
            if (equiptsks == null || equiptsks.Length == 0)
                return 0;
            if (session == null)
            {
                r = BrokerDelegate(() => Update(session, equiptsks), ref session);
                return r;
            }
            try
            {
                protData.EnterWriteLock();
                var list = new List<EquipTaskEntity>();
                EquipTaskEntity tsk = null;
                foreach (var item in equiptsks)
                {
                    tsk = new EquipTaskEntity() { Length = item.Length, Created = item.Created, TaskGuid = item.TaskGuid, Deleted = item.Deleted, EquipContollerId = item.EquipContollerId, Id = item.Id, IsDeleted = item.IsDeleted, PlantNo = item.PlantNo, ProductType = item.ProductType, Source = item.Source, Status = item.Status, TaskLevel = item.TaskLevel, TaskType = item.TaskType, Updated = item.Updated, Supply1 = item.Supply1, IsCancel = item.IsCancel, Length2 = item.Length2, PLCNo = item.PLCNo, Supply2 = item.Supply2, SupplyQty1 = item.SupplyQty1, SupplyQty2 = item.SupplyQty2, TitleProdName = item.TitleProdName };
                    list.Add(tsk);
                }
                Update(session, list);
                r = equiptsks.Length;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("更新 EquipTaskEntity list失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return r;
        }

        public int UpdateStatus(IStatelessSession session, byte status, params long[] ids)
        {
            if (session == null)
            {
                int i = BrokerDelegate(() => UpdateStatus(session, status, ids), ref session);
                return i;
            }
            StringBuilder sb = new StringBuilder();
            try
            {
                protData.EnterWriteLock();
                foreach (var item in ids)
                {
                    sb.Append(item.ToString() + ",");
                }
                int i = RunSqlStatement(session, $"UPDATE SNTON.EquipTask SET [status]={status} WHERE ID IN ({sb.ToString().TrimEnd(',')})");
                return i;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("更新 EquipTaskEntity 状态失败,id=" + sb.ToString() + "status=" + status, ex);
                return 0;
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }


    }
}
