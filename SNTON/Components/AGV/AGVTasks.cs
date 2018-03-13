using log4net;
using NHibernate;
using SNTON.Components.CleanUp;
using SNTON.Entities.DBTables.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC.Logging;
using SNTON.Constants;
using SNTON.Entities.DBTables.Spools;
using SNTON.Entities.DBTables.RobotArmTask;
using SNTON.Entities.DBTables.Equipments;
using System.Diagnostics;

namespace SNTON.Components.AGV
{
    public class AGVTasks : CleanUpBrokerBase, IAGVTasks
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "AGVTasksEntity";
        private const string DatabaseDbTable = "SNTON.AGVTasks";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static AGVTasks Create(XmlNode configNode)
        {
            var broker = new AGVTasks();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public AGVTasks()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public AGVTasks(object dependency)
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


        public AGVTasksEntity GetAGVTaskEntityById(long id, IStatelessSession session = null)
        {
            AGVTasksEntity ret = null;
            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVTaskEntityById(id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<AGVTasksEntity>(session, $"FROM AGVTasksEntity where  Id = {id} AND IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                    //var spools = ReadSqlList<SpoolsEntity>(session, "SELECT * FROM SNTON.Spools where AGVTaskID=" + ret.Id + " AND IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                    //ret.Spools = spools;
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVTasksEntity", e);
            }
            return ret;
        }
        public List<AGVTasksEntity> GetAllAGVTaskEntity(IStatelessSession session = null)
        {
            List<AGVTasksEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllAGVTaskEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<AGVTasksEntity>(session, $"FROM AGVTasksEntity where IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVTasksEntity list", e);
            }
            return ret;
        }

        public void CreateAGVTask(AGVTasksEntity entity, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => CreateAGVTask(entity, session), ref session);
                return;
            }
            try
            {
                Insert(session, entity);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to add AGVTasksEntity", e);
            }
            return;
        }

        public AGVTasksEntity GetAGVTaskEntityByTaskNo(long taskno, IStatelessSession session = null)
        {
            AGVTasksEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVTaskEntityByTaskNo(taskno, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<AGVTasksEntity>(session, $"FROM AGVTasksEntity where  taskno = {taskno} AND IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in ret.EquipIdListTarget.Trim().Split(';'))
                    {
                        sb.Append($"{item},");
                    }
                    ret._RobotArmTasks = ReadSqlList<RobotArmTaskEntity>(session, $"SELECT * FROM SNTON.RobotArmTask WHERE TaskGroupGUID='{ret.TaskGuid.ToString()}'");
                    //ret._EquipTasks = ReadSqlList<EquipTaskEntity>(session,$"SELECT * FROM SNTON.EquipTask WHERE [EquipContollerId] IN {sb.ToString().TrimEnd(',')} AND ");
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVTasksEntity", e);
            }
            return ret;
        }

        public void SaveAGVTaskStatus(long taskno, byte status, IStatelessSession session = null)
        {
            AGVTasksEntity ret = null;

            if (session == null)
            {
                BrokerDelegate(() => SaveAGVTaskStatus(taskno, status, session), ref session);
                return;
            }
            try
            {
                var tmp = ReadList<AGVTasksEntity>(session, $"FROM AGVTasksEntity where  taskno = {taskno} AND IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                    ret.Status = status;
                    Update(session, ret);
                }
                else
                {
                    throw new ArgumentNullException("没有找到实体");
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to save status,taskno is " + taskno, e);
            }
            return;
        }

        public void UpdateEntity(AGVTasksEntity entity, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => UpdateEntity(entity, session), ref session);
                return;
            }
            try
            {
                protData.EnterWriteLock();
                base.Update(session, entity);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("更新AGV状态失败,guid: " + entity.TaskGuid.ToString(), e);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return;
        }

        public List<AGVTasksEntity> GetAGVTasks(string sqlwhere, IStatelessSession session = null)
        {
            List<AGVTasksEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVTasks(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = " and " + sqlwhere;
                }
                var tmp = ReadSqlList<AGVTasksEntity>(session, $"SELECT * FROM [SNTON].[SNTON].AGVTasks where IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sqlwhere + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVTasksEntity list", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }


        public AGVTasksEntity GetAGVTask(string sqlwhere, IStatelessSession session = null)
        {
            AGVTasksEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVTask(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = " and " + sqlwhere;
                }
                var tmp = ReadList<AGVTasksEntity>(session, $"FROM AGVTasksEntity where IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sqlwhere + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVTasksEntity by sql:" + sqlwhere, e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }




        public bool UpdateStatus(long id, int status, IStatelessSession session = null)
        {
            bool ret = false;

            if (session == null)
            {
                ret = BrokerDelegate(() => UpdateStatus(id, status, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();
                AGVTasksEntity agv = GetAGVTaskEntityById(id, session);
                if (agv != null)
                {
                    agv.Status = status;
                    UpdateEntity(agv, session);
                    return true;
                }
                else return false;
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to update AGVTasksEntity list", e);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;
        }

        public int UpdateEntity(IStatelessSession session, params AGVTasksEntity[] entities)
        {
            int ret = 0;

            if (session == null)
            {
                ret = BrokerDelegate(() => UpdateEntity(session, entities), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();
                Update(session, entities);
                ret = entities.Length;
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to update AGVTasksEntity list", e);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;
        }

        public AGVTasksEntity GetAGVTaskEntityByTaskNo(long taskno, int equiptskstatus, IStatelessSession session = null)
        {
            AGVTasksEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVTaskEntityByTaskNo(taskno, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<AGVTasksEntity>(session, $"FROM AGVTasksEntity where  taskno = {taskno} AND IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in ret.EquipIdListTarget.Trim().Split(';'))
                    {
                        sb.Append($"{item},");
                    }
                    ret._RobotArmTasks = ReadSqlList<RobotArmTaskEntity>(session, $"SELECT * FROM SNTON.RobotArmTask WHERE TaskGroupGUID='{ret.TaskGuid.ToString()}'");
                    ret._EquipTasks = ReadSqlList<EquipTaskEntity>(session, $"SELECT * FROM SNTON.EquipTask WHERE [EquipContollerId] IN {sb.ToString().TrimEnd(',')} AND [Status]={equiptskstatus} AND [IsDeleted]=0");
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVTasksEntity", e);
            }
            return ret;
        }

        public int Insert(IStatelessSession session, params AGVTasksEntity[] agvs)
        {
            try
            {
                Insert<AGVTasksEntity>(session, agvs.ToList());
                return agvs.Length;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("添加AGVTasksEntity失败", ex);
                return 0;
            }
        }

        public List<AGVTasksEntity> GetAGVTasks(int top, string sqlwhere, IStatelessSession session = null)
        {
            List<AGVTasksEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVTasks(top, sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                Stopwatch watch = Stopwatch.StartNew();//创建一个监听器
                watch.Start();
                protData.EnterReadLock();
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = " and " + sqlwhere;
                }
                var tmp = ReadSqlList<AGVTasksEntity>(session, $"SELECT TOP {top} * FROM [SNTON].[SNTON].AGVTasks where IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sqlwhere + " order by ID desc");
                watch.Stop();
                Console.WriteLine("AGVTasksEntity查询耗时:" + watch.ElapsedMilliseconds);
                watch.Restart();
                if (tmp.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in tmp)
                    {
                        sb.Append("'" + item.TaskGuid.ToString() + "',");
                    }
                    var equiptsks = ReadSqlList<EquipTaskView2Entity>(session, "SELECT * FROM dbo.EquipTaskView3 WHERE [TaskGuid]IN(" + sb.ToString().Trim(',') + ")");
                    Console.WriteLine("EquipTaskView2Entity查询耗时:" + watch.ElapsedMilliseconds);
                    foreach (var item in tmp)
                    {
                        item._EquipTasks2 = equiptsks?.FindAll(x => x.TaskGuid == item.TaskGuid);
                    }
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVTasksEntity list", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
    }
}
