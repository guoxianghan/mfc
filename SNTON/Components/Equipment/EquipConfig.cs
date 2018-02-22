using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Components.CleanUp;
using SNTON.Entities.DBTables.Equipments;
using System.Reflection;
using log4net;
using System.Xml;
using VI.MFC.Logging;

namespace SNTON.Components.Equipment
{
    public class EquipConfig : CleanUpBrokerBase, IEquipConfig
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipConfigEntity";
        private const string DatabaseDbTable = "SNTON.EquipConfig";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipConfig Create(XmlNode configNode)
        {
            var broker = new EquipConfig();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipConfig()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipConfig(object dependency)
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
            try
            {

                _AllEquipConfigList = GetAllEquipConfig(null);
                if (_AllEquipConfigList != null)
                    _AllEquipConfigList.ForEach(x =>
                    {
                        _AllEquipConfigDic.Add(x.Id, x);
                    });
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to ReadBrokerData", e);
            }
        }
        public List<EquipConfigEntity> _AllEquipConfigList { get; set; } = new List<EquipConfigEntity>();
        public Dictionary<long, EquipConfigEntity> _AllEquipConfigDic { get; set; } = new Dictionary<long, EquipConfigEntity>();
        #endregion
        public List<EquipConfigEntity> GetEquipConfigByPlantNo(short plantNo, IStatelessSession session)
        {
            List<EquipConfigEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipConfigByPlantNo(plantNo, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipConfigEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND plantNo='" + plantNo + "'");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetEquipConfigByPlantNo", e);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<EquipConfigEntity> GetAllEquipConfig(IStatelessSession session)
        {
            List<EquipConfigEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllEquipConfig(session), ref session);
                return ret;
            }
            try
            {
                //var tmp = ReadSqlList<EquipConfigEntity>(session, "SELECT * FROM " + DatabaseDbTable);
                var tmp = ReadList<EquipConfigEntity>(session, " FROM " + EntityDbTable + " where isdeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetAllEquipConfig", e);
            }
            return ret;
        }

        public EquipConfigEntity GetEquipConfigById(long id, IStatelessSession session)
        {
            EquipConfigEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipConfigById(id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<EquipConfigEntity>(session, string.Format("FROM {0} where  ID = {1} AND IsDeleted={2} order by ID desc", EntityDbTable, id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<EquipConfigEntity> GetEquipConfigBySqlWhere(string sqlwhere, IStatelessSession session)
        {
            List<EquipConfigEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipConfigBySqlWhere(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = " and " + sqlwhere;
                }
                //var tmp = ReadSqlList<EquipConfigEntity>(session, "SELECT * FROM " + DatabaseDbTable);
                var tmp = ReadList<EquipConfigEntity>(session, " FROM " + EntityDbTable + " where isdeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sqlwhere);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetAllEquipConfig", e);
            }
            return ret;
        }

        public int UpdateEquipConfig(IStatelessSession session, params EquipConfigEntity[] entities)
        {
            int ret = 0;
            if (session == null)
            {
                ret = BrokerDelegate(() => UpdateEquipConfig(session, entities), ref session);
                return ret;
            }
            try
            {
                protData.EnterWriteLock();
                Update(session, entities.ToList());
                return entities.Length;
            }
            catch (Exception ex)
            {
                logger.WarnMethod("failed to update EquipConfig ", ex);
                return 0;
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }
    }
}
