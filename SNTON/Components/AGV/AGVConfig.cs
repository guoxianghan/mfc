using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Components.CleanUp;
using SNTON.Entities.DBTables.AGV;
using NHibernate;
using log4net;
using System.Reflection;
using VI.MFC.Logging;
using System.Xml;
using SNTON.Constants;
using SNTON.WebServices.UserInterfaceBackend.Requests.AGV;

namespace SNTON.Components.AGV
{
    public class AGVConfig : CleanUpBrokerBase, IAGVConfig
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "AGVConfigEntity";
        private const string DatabaseDbTable = "SNTON.AGVConfig";


        public Dictionary<long, AGVConfigEntity> _DicAGVConfig { get; set; } = new Dictionary<long, AGVConfigEntity>();


        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static AGVConfig Create(XmlNode configNode)
        {
            var broker = new AGVConfig();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public AGVConfig()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public AGVConfig(object dependency)
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
            var config = this.GetAllAGVConfig();
            if (config != null)
                config.ForEach(x => _DicAGVConfig.Add(x.Id, x));
        }
        #endregion
        #region Methods related to the thread

        #endregion
        public AGVConfigEntity GetAGVById(long Id, IStatelessSession session = null)
        {
            AGVConfigEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVById(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<AGVConfigEntity>(session, string.Format("FROM {0} where  ID = {1} AND IsDeleted={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public AGVConfigEntity GetAGVByName(string agvName, IStatelessSession session = null)
        {
            AGVConfigEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVByName(agvName, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<AGVConfigEntity>(session, string.Format("SELECT * FROM {0} where  [SeqNo] = {1} AND IsDeleted=0 order by ID desc", DatabaseDbTable, agvName));
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

        protected override void MarkDataForDeletion(DateTime olderThan, int threadDeleteMaxRecords, IStatelessSession theSession = null)
        {
            if (theSession == null)
            {
                BrokerDelegate(() => MarkDataForDeletion(olderThan, threadDeleteMaxRecords, theSession), ref theSession);
                return;
            }
            try
            {
                string queryString = @"select * from 
                                (select row_number() over (order by Id) as rownum, t.* from {0} t 
                                 where created <= :created 
                                 and created is not null
                                 and IsDeleted = :deletedtag)
                                 where rownum <= :deletedRows";
                var dataList = ReadSqlList<AGVConfigEntity>(theSession, string.Format(queryString, DatabaseDbTable),
                                                     new
                                                     {
                                                         created = olderThan,
                                                         deletedTag = SNTONConstants.DeletedTag.NotDeleted,
                                                         deletedRows = threadDeleteMaxRecords
                                                     });
                if (dataList.Any())
                {
                    dataList.ForEach(r => r.IsDeleted = SNTONConstants.DeletedTag.Deleted);
                    Update(theSession, dataList);
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to mark data for the deletion", e);
                throw;
            }
        }

        public List<AGVConfigEntity> GetAllAGVConfig(IStatelessSession session = null)
        {
            List<AGVConfigEntity> ret = null;
            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllAGVConfig(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<AGVConfigEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get  AGVConfigEntitylist", e);
            }
            return ret;
        }

        public void AddAGVConfig(List<AGVConfigEntity> list, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => AddAGVConfig(list, session), ref session);
                return;
            }
            try
            {
                if (list.Any())
                {
                    Insert(session, list);
                    logger.InfoMethod(string.Format("Set {0} records exception AddAGVConfig lists to DB successfully", list.Count));
                }
                else
                {
                    logger.ErrorMethod("Argument AGVConfigEntity list is empty, action will be ignored");
                    //throw new ArgumentException("Argument list is NULL");
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to save AddAGVConfig", e);
            }
        }



        public void SaveAGVConfig(IStatelessSession session = null, params AGVConfigEntity[] agvs)
        {
            if (session == null)
            {
                BrokerDelegate(() => SaveAGVConfig(session, agvs), ref session);
                return;
            }
            try
            {
                if (agvs.Any())
                {
                    Update(session, agvs.ToArray());
                    logger.InfoMethod(string.Format("Set {0} records exception AGVStatusEntity lists to DB successfully", agvs.Length));
                }
                else
                {
                    logger.ErrorMethod("Argument AGVConfigEntity list is empty, action will be ignored");
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
