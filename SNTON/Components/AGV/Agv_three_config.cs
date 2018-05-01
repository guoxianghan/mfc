
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
using SNTON.Entities.DBTables.AGV;

namespace SNTON.Components.AGV
{
    public class Agv_three_config : CleanUpBrokerBase, IAgv_three_config
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "agv_three_configEntity";
        private const string DatabaseDbTable = "SNTON.Agv_three_config";

        public agv_three_configEntity _originLocation { get; set; }
        public List<agv_three_configEntity> _AllAgv_three_config { get; set; }

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static Agv_three_config Create(XmlNode configNode)
        {
            var broker = new Agv_three_config();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public Agv_three_config()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public Agv_three_config(object dependency)
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
            if (_AllAgv_three_config == null)
                _AllAgv_three_config = GetAllAgv_three_configEntity(null);
            _originLocation = _AllAgv_three_config?.FirstOrDefault(x => x.fac_x == 0 && x.fac_y == 0);
        }
        #endregion



        public agv_three_configEntity GetAgv_three_configEntityByID(long Id, IStatelessSession session)
        {
            agv_three_configEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAgv_three_configEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<agv_three_configEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<agv_three_configEntity> GetAllAgv_three_configEntity(IStatelessSession session)
        {

            List<agv_three_configEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllAgv_three_configEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<agv_three_configEntity>(session, $"SELECT * FROM {DatabaseDbTable}");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get Agv_three_configEntityList", e);
            }
            return ret;
        }
    }
}

