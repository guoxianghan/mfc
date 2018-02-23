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
using VI.MFC.Logging;
using System.Xml;

namespace SNTON.Components.Equipment
{
    public class EquipControllerConfig : CleanUpBrokerBase, IEquipControllerConfig
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipControllerConfigEntity";
        private const string DatabaseDbTable = "SNTON.EquipControllerConfig";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipControllerConfig Create(XmlNode configNode)
        {
            var broker = new EquipControllerConfig();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipControllerConfig()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipControllerConfig(object dependency)
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

        public List<EquipControllerConfigEntity> GetEquipControllerConfigByCtlName(string controllername, IStatelessSession session)
        {
            List<EquipControllerConfigEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipControllerConfigByCtlName(controllername, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipControllerConfigEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND EquipControllerName='" + controllername+"'");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetEquipControllerConfigByCtlName", e);
            }
            return ret;
        }

        public EquipControllerConfigEntity GetEquipControllerConfigById(long id, IStatelessSession session)
        {
            EquipControllerConfigEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipControllerConfigById(id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<EquipControllerConfigEntity>(session, string.Format("FROM {0} where  ID = {1} AND IsDeleted={2} order by ID desc", EntityDbTable, id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<EquipControllerConfigEntity> GetEquipControllerConfigByPlantNo(string plantno, IStatelessSession session)
        {
            List<EquipControllerConfigEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipControllerConfigByPlantNo(plantno, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipControllerConfigEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND plantno='" + plantno + "'");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetEquipControllerConfigByPlantNo", e);
            }
            return ret;
        }

        public List<EquipControllerConfigEntity> GetAllEquipControllerConfig(IStatelessSession session)
        {
            List<EquipControllerConfigEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllEquipControllerConfig(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipControllerConfigEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted );
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetAllEquipControllerConfig", e);
            }
            return ret;
        }
    }
}
