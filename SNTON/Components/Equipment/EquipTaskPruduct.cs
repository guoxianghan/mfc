using log4net;
using NHibernate;
using SNTON.Components.CleanUp;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VI.MFC.Logging;
using System.Xml;
namespace SNTON.Components.Equipment
{
    public class EquipTaskPruduct : CleanUpBrokerBase, IEquipTaskProduct
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipTaskProductEntity";
        private const string DatabaseDbTable = "SNTON.EquipTaskProduct";
        public EquipTaskProductEntity GetEquipTaskProductEntity(string sqlwhere, IStatelessSession session)
        {
            EquipTaskProductEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTaskProductEntity(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = " and " + sqlwhere;
                }
                var tmp = ReadList<EquipTaskProductEntity>(session, $"FROM EquipTaskProduct where IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sqlwhere + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTaskEntity list", e);
            }
            return ret;
        }

        public List<EquipTaskProductEntity> GetEquipTaskProductEntityList(string sqlwhere, IStatelessSession session)
        {
            List<EquipTaskProductEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTaskProductEntityList(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = " and " + sqlwhere;
                }
                var tmp = ReadList<EquipTaskProductEntity>(session, $"FROM EquipTaskProduct where IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sqlwhere + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTaskEntity list", e);
            }
            return ret;
        }

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipTaskPruduct Create(XmlNode configNode)
        {
            var broker = new EquipTaskPruduct();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipTaskPruduct()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipTaskPruduct(object dependency)
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
    }
}
