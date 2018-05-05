
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
using VI.MFC.Utils;

namespace SNTON.Components.AGV
{
    public class AGVBYSStatus : CleanUpBrokerBase, IAGVBYSStatus
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "AGVBYSStatusEntity";
        private const string DatabaseDbTable = "dbo.AGVBYSStatus";
        public List<AGVBYSStatusEntity> _AGVBYSStatusCache { get; set; }
        private VIThreadEx thread_AGVSystemRoute;
        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static AGVBYSStatus Create(XmlNode configNode)
        {
            var broker = new AGVBYSStatus();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public AGVBYSStatus()
        {
            thread_AGVSystemRoute = new VIThreadEx(ReadAGV_X_YStatus, null, "thread for reading AGV System AGVStatus", 2000);
        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public AGVBYSStatus(object dependency)
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
            thread_AGVSystemRoute.Start();
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


        void ReadAGV_X_YStatus()
        {
            _AGVBYSStatusCache = GetAllAGVBYSStatusEntity(null);
            if (_AGVBYSStatusCache == null)
                _AGVBYSStatusCache = new List<AGVBYSStatusEntity>();
        }

        public AGVBYSStatusEntity GetAGVBYSStatusEntityByID(long Id, IStatelessSession session)
        {
            AGVBYSStatusEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVBYSStatusEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<AGVBYSStatusEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<AGVBYSStatusEntity> GetAllAGVBYSStatusEntity(IStatelessSession session)
        {

            List<AGVBYSStatusEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllAGVBYSStatusEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<AGVBYSStatusEntity>(session, $"SELECT * FROM {DatabaseDbTable}");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVBYSStatusEntityList", e);
            }
            return ret;
        }
    }
}

