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

namespace SNTON.Components.Equipment
{
    public class EquipTaskView2 : CleanUpBrokerBase, IEquipTaskView2
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipTaskView2Entity";
        private const string DatabaseDbTable = "EquipTaskView2";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipTaskView2 Create(XmlNode configNode)
        {
            var broker = new EquipTaskView2();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipTaskView2()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipTaskView2(object dependency)
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
            var tmp = ReadSqlList<EquipTaskView2Entity>(null, $"SELECT top 1 * FROM {DatabaseDbTable}  order by EquipTaskID desc");
        }

        public List<EquipTaskView2Entity> GetEquipTaskView2(string sqlwhere, IStatelessSession session = null)
        {
            List<EquipTaskView2Entity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTaskView2(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    //sqlwhere = " and " + sqlwhere;
                }
                var tmp = ReadSqlList<EquipTaskView2Entity>(session, $"SELECT * FROM EquipTaskView2 where " + sqlwhere + " order by EquipTaskID desc");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTaskView2 list", e);
            }
            return ret;
        }
        #endregion
    }
}
