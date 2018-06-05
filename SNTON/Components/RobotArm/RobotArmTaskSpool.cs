using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.RobotArmTask;
using SNTON.Components.CleanUp;
using log4net;
using System.Reflection;
using System.Xml;
using VI.MFC.Logging;

namespace SNTON.Components.RobotArm
{
    public class RobotArmTaskSpool : CleanUpBrokerBase, IRobotArmTaskSpool
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "RobotArmTaskSpoolEntity";
        private const string DatabaseDbTable = "RobotArmTaskSpool";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static RobotArmTaskSpool Create(XmlNode configNode)
        {
            var broker = new RobotArmTaskSpool();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public RobotArmTaskSpool()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public RobotArmTaskSpool(object dependency)
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

        public List<RobotArmTaskSpoolEntity> GetRobotArmTaskSpools(string sqlwhere, IStatelessSession session = null)
        {
            List<RobotArmTaskSpoolEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetRobotArmTaskSpools(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                string sql = $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted;
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sql = sql + " AND " + sqlwhere;
                }
                var tmp = ReadSqlList<RobotArmTaskSpoolEntity>(session, sql);

                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get RobotArmTaskSpoolsList", e);
            }
            return ret;
        }


        public int UpdateArmTask(List<RobotArmTaskSpoolEntity> armtsks, IStatelessSession session = null)
        {
            int ret = 0;
            if (session == null)
            {
                ret = BrokerDelegate(() => UpdateArmTask(armtsks, session), ref session);
                return ret;
            }
            try
            {
                List<RobotArmTaskEntity> list = new List<RobotArmTaskEntity>();
                foreach (var entity in armtsks)
                {
                    list.Add(new RobotArmTaskEntity() { WhoolBarCode = entity.WhoolBarCode, AGVSeqNo = entity.AGVSeqNo, CName = entity.CName, Completed = entity.Completed, Created = entity.Created, Deleted = entity.Deleted, EquipControllerId = entity.EquipControllerId, FromWhere = entity.FromWhere, Id = entity.Id, IsDeleted = entity.IsDeleted, PlantNo = entity.PlantNo, ProductType = entity.ProductType, RobotArmID = entity.RobotArmID, SeqNo = entity.SeqNo, SpoolStatus = entity.SpoolStatus, StorageArea = entity.StorageArea, TaskGroupGUID = entity.TaskGroupGUID, TaskLevel = entity.TaskLevel, TaskStatus = entity.TaskStatus, TaskType = entity.TaskType, ToWhere = entity.FromWhere, Updated = DateTime.Now });
                }
                Update(session, list);
                return armtsks.Count;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("failed to update RobotArmTaskSpoolEntity", ex);
                return 0;
            }
        }
        public byte UpdateArmTask(RobotArmTaskSpoolEntity entity, IStatelessSession session = null)
        {
            byte ret = 0;
            if (session == null)
            {
                ret = BrokerDelegate(() => UpdateArmTask(entity, session), ref session);
                return ret;
            }
            try
            {
                RobotArmTaskEntity armtsk = new RobotArmTaskEntity() { WhoolBarCode = entity.WhoolBarCode, AGVSeqNo = entity.AGVSeqNo, CName = entity.CName, Completed = entity.Completed, Created = entity.Created, Deleted = entity.Deleted, EquipControllerId = entity.EquipControllerId, FromWhere = entity.FromWhere, Id = entity.Id, IsDeleted = entity.IsDeleted, PlantNo = entity.PlantNo, ProductType = entity.ProductType, RobotArmID = entity.RobotArmID, SeqNo = entity.SeqNo, SpoolStatus = entity.SpoolStatus, StorageArea = entity.StorageArea, TaskGroupGUID = entity.TaskGroupGUID, TaskLevel = entity.TaskLevel, TaskStatus = entity.TaskStatus, TaskType = entity.TaskType, ToWhere = entity.FromWhere, Updated = DateTime.Now };
                Update(session, armtsk);
                return 1;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("failed to update RobotArmTaskSpoolEntity", ex);
                return 0;
            }
        }
        public RobotArmTaskSpoolEntity GetRobotArmTaskSpool(long id, IStatelessSession session = null)
        {
            RobotArmTaskSpoolEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetRobotArmTaskSpool(id, session), ref session);
                return ret;
            }
            try
            {
                string sql = $"SELECT * FROM {DatabaseDbTable} WHERE id=" + id;

                var tmp = ReadSqlList<RobotArmTaskSpoolEntity>(session, sql);

                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get RobotArmTaskSpool", e);
            }
            return ret;
        }
    }
}
