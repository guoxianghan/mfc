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
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.MidStorage;
using SNTON.Entities.DBTables.AGV;
using SNTON.Entities.DBTables.RobotArmTask;
using SNTON.Entities.DBTables.InStoreToOutStore;

namespace SNTON.Components.SQLCommand
{
    public class SQLCommandBroker : CleanUpBrokerBase, ISQLCommandBroker
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "SQLCommandBroker";
        private const string DatabaseDbTable = "SQLCommandBroker";
        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static SQLCommandBroker Create(XmlNode configNode)
        {
            var broker = new SQLCommandBroker();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public SQLCommandBroker()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public SQLCommandBroker(object dependency)
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

        public bool RunSqlCommand(string[] sqlcmdlist, IStatelessSession session = null)
        {
            bool ret = false;
            if (session == null)
            {
                ret = BrokerDelegate(() => RunSqlCommand(sqlcmdlist, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();
                foreach (var item in sqlcmdlist)
                {
                    RunSqlStatement(session, item);
                }
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
                logger.ErrorMethod("执行SQLCommand出现错误", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;

        }

        public bool OutStoreageTask(List<EquipTaskViewEntity> updateequiptsks, List<MidStorageSpoolsEntity> updatemids, AGVTasksEntity insertagvtsk, List<RobotArmTaskEntity> insetarmtsks, List<InStoreToOutStoreSpoolEntity> outspools, IStatelessSession session = null)
        {
            bool ret = false;
            if (session == null)
            {
                ret = BrokerDelegate(() => OutStoreageTask(updateequiptsks, updatemids, insertagvtsk, insetarmtsks, outspools, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();

                var equiptsklist = new List<EquipTaskEntity>();
                EquipTaskEntity tsk = null;
                if (updateequiptsks != null)
                    foreach (var item in updateequiptsks)
                    {
                        tsk = new EquipTaskEntity() { Length = item.Length, Created = item.Created, TaskGuid = item.TaskGuid, Deleted = item.Deleted, EquipContollerId = item.EquipContollerId, Id = item.Id, IsDeleted = item.IsDeleted, PlantNo = item.PlantNo, ProductType = item.ProductType, Source = item.Source, Status = item.Status, TaskLevel = item.TaskLevel, TaskType = item.TaskType, Updated = item.Updated, Supply1 = item.Supply1 };
                        equiptsklist.Add(tsk);
                    }
                List<MidStorageEntity> midlist = new List<MidStorageEntity>();
                foreach (var item in updatemids)
                {
                    midlist.Add(new MidStorageEntity() { Created = item.Created, Deleted = item.Deleted, Description = item.Description, HCoordinate = item.HCoordinate, Id = item.Id, IdsList = item.IdsList, IsDeleted = item.IsDeleted, IsEnable = item.IsEnable, IsOccupied = item.IsOccupied, IsVisible = item.IsVisible, SeqNo = item.SeqNo, StorageArea = item.StorageArea, Updated = item.Updated, VCoordinate = item.VCoordinate });
                }
                Update(session, equiptsklist);
                Update(session, midlist);
                Insert(session, insertagvtsk);
                Insert(session, insetarmtsks);
                if (outspools != null)
                    Insert(session, outspools);
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
                logger.ErrorMethod("创建直通线出库任务失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;
        }
        public bool EmptyAGVTask(List<EquipTaskViewEntity> equiptsks, AGVTasksEntity insertagvtsk, IStatelessSession session = null)
        {
            bool ret = false;
            if (session == null)
            {
                ret = BrokerDelegate(() => EmptyAGVTask(equiptsks, insertagvtsk, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();
                ret = true;
                var equiptsklist = new List<EquipTaskEntity>();
                EquipTaskEntity tsk = null;
                foreach (var item in equiptsks)
                {
                    tsk = new EquipTaskEntity() { Length = item.Length, Created = item.Created, TaskGuid = item.TaskGuid, Deleted = item.Deleted, EquipContollerId = item.EquipContollerId, Id = item.Id, IsDeleted = item.IsDeleted, PlantNo = item.PlantNo, ProductType = item.ProductType, Source = item.Source, Status = item.Status, TaskLevel = item.TaskLevel, TaskType = item.TaskType, Updated = item.Updated, Supply1 = item.Supply1 };
                    equiptsklist.Add(tsk);
                }
                Update(null, equiptsklist);
                Insert(session, insertagvtsk);
            }
            catch (Exception ex)
            {
                ret = false;
                logger.ErrorMethod("创建拉空轮任务", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;
        }
        #endregion
        /// <summary>
        /// 清空直通线入库任务
        /// </summary>
        /// <param name="armtsks"></param>
        /// <param name="mids"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool ClearInStoreageLine(List<RobotArmTaskSpoolEntity> armtsks, List<MidStorageEntity> mids, IStatelessSession session = null)
        {
            bool ret = false;
            if (session == null)
            {
                ret = BrokerDelegate(() => ClearInStoreageLine(armtsks, mids, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();
                List<RobotArmTaskEntity> list = new List<RobotArmTaskEntity>();
                foreach (var entity in armtsks)
                {
                    list.Add(new RobotArmTaskEntity() { WhoolBarCode = entity.WhoolBarCode, AGVSeqNo = entity.AGVSeqNo, CName = entity.CName, Completed = entity.Completed, Created = entity.Created, Deleted = entity.Deleted, EquipControllerId = entity.EquipControllerId, FromWhere = entity.FromWhere, Id = entity.Id, IsDeleted = entity.IsDeleted, PlantNo = entity.PlantNo, ProductType = entity.ProductType, RobotArmID = entity.RobotArmID, SeqNo = entity.SeqNo, SpoolStatus = entity.SpoolStatus, StorageArea = entity.StorageArea, TaskGroupGUID = entity.TaskGroupGUID, TaskLevel = entity.TaskLevel, TaskStatus = entity.TaskStatus, TaskType = entity.TaskType, ToWhere = entity.FromWhere, Updated = DateTime.Now, SpoolSeqNo = entity.SpoolSeqNo });
                }
                Update(session, list);
                Update(session, mids);
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
                logger.ErrorMethod("清除直通线功能失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;
        }

        /// <summary>
        /// 龙门异常处理,0重发,2完成,
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool ExceptionRobotArmTask(List<RobotArmTaskEntity> armtsks, List<MidStorageEntity> mids, IStatelessSession session = null)
        {
            bool ret = false;
            if (session == null)
            {
                ret = BrokerDelegate(() => ExceptionRobotArmTask(armtsks, mids, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();
                if (armtsks != null && armtsks.Count != 0)
                    Update(session, armtsks);
                if (mids != null && mids.Count != 0)
                    Update(session, mids);
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
                logger.ErrorMethod("ExceptionRobotArmTask", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;
        }

        public bool ClearInStoreToOutStoreLine(List<MidStorageEntity> updatemids, AGVTasksEntity updateagvtsk, List<RobotArmTaskEntity> updatearmtsks, List<InStoreToOutStoreSpoolEntity> updateoutspools, IStatelessSession session = null)
        {
            bool ret = false;
            if (session == null)
            {
                ret = BrokerDelegate(() => ClearInStoreToOutStoreLine(updatemids, updateagvtsk, updatearmtsks, updateoutspools, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();

                if (updateagvtsk != null)
                    Update(session, updateagvtsk);
                if (updateoutspools != null && updateoutspools.Count != 0)
                    Update(session, updateoutspools);
                if (updatearmtsks != null && updatearmtsks.Count != 0)
                    Update(session, updatearmtsks);
                if (updatemids != null && updatemids.Count != 0)
                    Update(session, updatemids);
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
                logger.ErrorMethod("清除直通线功能失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;
        }

        public bool InStoreToOutStoreLine(List<InStoreToOutStoreSpoolViewEntity> instoreoutstore, AGVTasksEntity agvtsk, List<EquipTaskViewEntity> updateequiptsks, IStatelessSession session = null)
        {
            bool ret = false;
            if (session == null)
            {
                ret = BrokerDelegate(() => InStoreToOutStoreLine(instoreoutstore, agvtsk, updateequiptsks, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();
                var intoout = new List<InStoreToOutStoreSpoolEntity>();
                var equiptsklist = new List<EquipTaskEntity>();
                EquipTaskEntity tsk = null;
                foreach (var item in updateequiptsks)
                {
                    tsk = new EquipTaskEntity() { Length = item.Length, Created = item.Created, TaskGuid = item.TaskGuid, Deleted = item.Deleted, EquipContollerId = item.EquipContollerId, Id = item.Id, IsDeleted = item.IsDeleted, PlantNo = item.PlantNo, ProductType = item.ProductType, Source = item.Source, Status = item.Status, TaskLevel = item.TaskLevel, TaskType = item.TaskType, Updated = item.Updated, Supply1 = item.Supply1 };
                    equiptsklist.Add(tsk);
                }
                foreach (var item in instoreoutstore)
                {
                    intoout.Add(new InStoreToOutStoreSpoolEntity() { AGVSeqNo = item.AGVSeqNo, Created = item.Created, Deleted = item.Deleted, Guid = item.Guid, Id = item.Id, InLineNo = item.InLineNo, IsDeleted = item.IsDeleted, PlantNo = item.PlantNo, SpoolId = item.SpoolId, Status = item.Status, StoreageNo = item.StoreageNo, Updated = item.Updated });
                }
                Update(session, intoout);
                Update(session, agvtsk);
                Update(session, equiptsklist);
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
                logger.ErrorMethod("创建直通口出库任务失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return ret;
        }
    }
}