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

namespace SNTON.Components.AGV
{
    public class AGVStatus : CleanUpBrokerBase, IAGVStatus
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "AGVStatusEntity";
        private const string DatabaseDbTable = "SNTON.AGVStatus";

        public Dictionary<long, AGVStatusEntity> _DicAGVStatus { get; set; } = new Dictionary<long, AGVStatusEntity>();
        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static AGVStatus Create(XmlNode configNode)
        {
            var broker = new AGVStatus();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public AGVStatus()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public AGVStatus(object dependency)
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
            var status = this.GetAllAGVStatus();
            if (status != null)
            {
                foreach (var item in status)
                {
                    if (_DicAGVStatus.Keys.Contains((item.AGVId)))
                        _DicAGVStatus[item.AGVId] = item;
                    else
                        _DicAGVStatus.Add(item.AGVId, item);
                }
            }
        }
        #endregion

        public List<AGVStatusEntity> GetAllAGVStatus(IStatelessSession session = null)
        {
            List<AGVStatusEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllAGVStatus(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<AGVStatusEntity>(session, $"SELECT * FROM [SNTON].[SNTON].[AGVStatus] WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get  AGVStatusEntitylist", e);
            }
            return ret;
        }

        public void AddAGVStatus(List<AGVStatusEntity> list, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => AddAGVStatus(list, session), ref session);
                return;
            }
            try
            {
                if (list.Any())
                {
                    Insert(session, list);
                    logger.InfoMethod(string.Format("Set {0} records exception AGVStatusEntity lists to DB successfully", list.Count));
                }
                else
                {
                    logger.ErrorMethod("AGV status list is empty, action will be ignored");
                    //throw new ArgumentException("Argument list is NULL");
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to save AGVStatusEntity", e);
            }

        }

        public void SaveAGVStatus(IStatelessSession session = null, params AGVStatusEntity[] list)
        {
            if (session == null)
            {
                BrokerDelegate(() => SaveAGVStatus(session, list), ref session);
                return;
            }
            try
            {
                if (list.Any())
                {
                    Update(session, list.ToList());
                    logger.InfoMethod(string.Format("Set {0} records AGVStatusEntity lists to DB successfully", list.Length));
                }
                else
                {
                    logger.ErrorMethod("Argument AGVStatusEntitylist is empty, action will be ignored");
                    //throw new ArgumentException("Argument list is NULL");
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to save AGVStatusEntity", e);
            }
        }

    }
}
