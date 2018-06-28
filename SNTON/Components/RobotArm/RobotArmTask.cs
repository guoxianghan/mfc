
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Components.CleanUp;
using System.Reflection;
using log4net;
using System.Xml;
using VI.MFC.Logging;
using SNTON.Entities.DBTables.RobotArmTask;

namespace SNTON.Components.RobotArm
{
    public class RobotArmTask : CleanUpBrokerBase, IRobotArmTask
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "RobotArmTaskEntity";
        private const string DatabaseDbTable = "SNTON.RobotArmTask";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static RobotArmTask Create(XmlNode configNode)
        {
            var broker = new RobotArmTask();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public RobotArmTask()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public RobotArmTask(object dependency)
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



        public RobotArmTaskEntity GetRobotArmTaskEntityByID(long Id, IStatelessSession session)
        {
            RobotArmTaskEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetRobotArmTaskEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<RobotArmTaskEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get " + EntityDbTable, e);
            }
            return ret;
        }

        public List<RobotArmTaskEntity> GetAllRobotArmTaskEntity(IStatelessSession session)
        {

            List<RobotArmTaskEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllRobotArmTaskEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<RobotArmTaskEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get RobotArmTaskEntityList", e);
            }
            return ret;
        }

        public List<RobotArmTaskEntity> GetRobotArmTasks(string sqlwhere, IStatelessSession session = null)
        {
            List<RobotArmTaskEntity> ret = new List<RobotArmTaskEntity>();

            if (session == null)
            {
                ret = BrokerDelegate(() => GetRobotArmTasks(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                string sql = $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted;
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sql = sql + " and " + sqlwhere;
                }
                var tmp = ReadSqlList<RobotArmTaskEntity>(session, sql);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get RobotArmTaskEntityList", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
        /// <summary>
        /// 如果是最后一个,更新龙门任务单元状态
        /// </summary>
        /// <param name="guid"></param>
        public bool SetArmTasksUnitStatus(Guid guid, IStatelessSession session = null)
        {
            bool r = false;
            if (session == null)
            {
                r = BrokerDelegate(() => SetArmTasksUnitStatus(guid, session), ref session);
                return r;
            }
            try
            {
                protData.EnterWriteLock();
                List<RobotArmTaskEntity> list = GetRobotArmTasks($"TaskGroupGUID='{guid.ToString()}'", session);//找到该任务单元
                if (list == null || list.Count == 0)
                    return false;
                if (!list.Exists(x => x.SpoolStatus != 2))
                {
                    list.ForEach(x => x.TaskStatus = 9);
                    UpdateArmTasks(list, null);
                    logger.InfoMethod("龙门任务单元已完成:" + list[0].TaskGroupGUID.ToString());
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("Failed to get update RobotArmTaskEntity", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return r;
        }
        public void CreateArmTask(RobotArmTaskEntity entity, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => CreateArmTask(entity, session), ref session);
                return;
            }
            try
            {
                Insert(session, entity);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("Failed to get insert RobotArmTaskEntity", ex);
            }
        }

        public void InsertArmTask(List<RobotArmTaskEntity> lst, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => InsertArmTask(lst, session), ref session);
                return;
            }
            try
            {
                Insert(session, lst);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("Failed to get insert RobotArmTaskEntity list", ex);
            }
        }

        public bool UpdateArmTask(RobotArmTaskEntity entity, IStatelessSession session = null)
        {
            bool r = false;
            if (session == null)
            {
                r = BrokerDelegate(() => UpdateArmTask(entity, session), ref session);
                return r;
            }
            try
            {
                protData.EnterWriteLock();
                Update(session, entity);
                r = true;
            }
            catch (Exception ex)
            {
                r = false;
                logger.ErrorMethod("Failed to get update RobotArmTaskEntity", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return r;
        }

        public void UpdateArmTasks(List<RobotArmTaskEntity> lst, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => UpdateArmTasks(lst, session), ref session);
                return;
            }

            try
            {
                protData.EnterWriteLock();
                Update(session, lst);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("Failed to get update RobotArmTaskEntity list", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }
        /// <summary>
        /// ?ú?????????????¤×÷×???:-1?§?§;0???¨;1????×?±???,×?±?×???;2????×???;3×????ê±?;4????AGV????;5AGV?????ê±?;6×????§°?;7?????§°?;8?????§°?;9?????????ì?????í????????
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="taskStatus"></param>
        /// <param name="session"></param>
        public void UpdateArmTaskStatus(Guid guid, int taskStatus, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => UpdateArmTaskStatus(guid, taskStatus, session), ref session);
                return;
            }

            try
            {
                protData.EnterWriteLock();
                this.RunSqlStatement(session, $"UPDATE [SNTON].[RobotArmTask] SET [TaskStatus]={taskStatus} , [Updated]='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE [TaskGroupGUID]='{guid.ToString()}'");
                //logger.InfoMethod("成功更新龙门任务状态 guid is " + guid.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("Failed to update RobotArmTaskEntity list taskStatus when guid is " + guid.ToString(), ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }

        public RobotArmTaskEntity GetRobotArmTask(string sqlwhere, IStatelessSession session = null)
        {
            RobotArmTaskEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetRobotArmTask(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                string sql = $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted;
                if (string.IsNullOrEmpty(sqlwhere))
                {
                    sql = sql + " and " + sqlwhere;
                }
                var tmp = ReadSqlList<RobotArmTaskEntity>(session, sql);
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get RobotArmTaskEntity", e);
            }
            return ret;
        }

        public int SetArmTaskDelete(IStatelessSession session = null, params long[] ids)
        {
            int ret = 0;
            if (session == null)
            {
                ret = BrokerDelegate(() => SetArmTaskDelete(session, ids), ref session);
                return ret;
            }

            try
            {
                if (ids != null || ids.Length == 0)
                    return 0;
                protData.EnterWriteLock();

                StringBuilder id = new StringBuilder();
                foreach (var item in ids)
                {
                    id.Append(item + ",");
                }

                ret = RunSqlStatement(session, "UPDATE SNTON.RobotArmTask SET ISDELETED=1 WHERE ID IN(" + id.ToString().Trim(',') + ")");
                return ret;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("更新龙门状态失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;
        }
    }
}

