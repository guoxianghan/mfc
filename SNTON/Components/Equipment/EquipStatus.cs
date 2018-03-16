using SNTON.Entities.DBTables.Equipments;
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

namespace SNTON.Components.Equipment
{
    public class EquipStatus : CleanUpBrokerBase, IEquipStatus
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipStatusEntity";
        private const string DatabaseDbTable = "SNTON.EquipStatus";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipStatus Create(XmlNode configNode)
        {
            var broker = new EquipStatus();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipStatus()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipStatus(object dependency)
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

        public EquipStatusEntity GetEquipStatusEntityByEquipID(long equipid, IStatelessSession session)
        {
            EquipStatusEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipStatusEntityByEquipID(equipid, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<EquipStatusEntity>(session, string.Format("FROM {0} WHERE  equipid = {1}  AND ISDELETED={2} order by ID desc", EntityDbTable, equipid, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public EquipStatusEntity GetEquipStatusEntityByID(long Id, IStatelessSession session)
        {
            EquipStatusEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipStatusEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<EquipStatusEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<EquipStatusEntity> GetEquipStatusEntityByStatus(byte status, IStatelessSession session)
        {
            List<EquipStatusEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipStatusEntityByStatus(status, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipStatusEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE Status={status} AND ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipStatusEntityList", e);
            }
            return ret;
        }

        public List<EquipStatusEntity> GetAllEquipStatusEntity(IStatelessSession session)
        {

            List<EquipStatusEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllEquipStatusEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipStatusEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipStatusEntityList", e);
            }
            return ret;
        }
    }
}
