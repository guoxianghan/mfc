using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Components.CleanUp;
using SNTON.Entities.DBTables.AGV;
using log4net;
using System.Reflection;
using VI.MFC.Logging;
using NHibernate;
using System.Xml;
using SNTON.Constants;

namespace SNTON.Components.AGV
{
    public class AGVRoute : CleanUpBrokerBase, IAGVRoute
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "AGVRouteEntity";
        private const string DatabaseDbTable = "SNTON.AGVRoute";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static AGVRoute Create(XmlNode configNode)
        {
            var broker = new AGVRoute();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public AGVRoute()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public AGVRoute(object dependency)
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
            logger.InfoMethod("AGVRoute broker started.");
        }

        /// <summary>
        /// Here we may load data from the database during startup in case we were
        /// a caching broker. Non-caching broker (preferred) may not do anything here.
        /// </summary>
        public override void ReadBrokerData()
        {
            for (byte i = 1; i <= 30; i++)
            {
                RealTimeAGVRute.Add(i, new AGVRouteEntity { AGVId = i, Created = DateTime.Now, X = "0", Y = "0" });
            }
        }
        #endregion
        public Dictionary<short, AGVRouteEntity> RealTimeAGVRute { get; set; } = new Dictionary<short, AGVRouteEntity>();
        public List<AGVRouteEntity> GetAGVRoute(long agvId, IStatelessSession session = null)
        {
            List<AGVRouteEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVRoute(agvId, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<AGVRouteEntity>(session, string.Format("FROM {0} where IsDeleted={2} and AGVId = {1} order by ID", EntityDbTable, agvId, Constants.SNTONConstants.DeletedTag.NotDeleted));
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVRouteEntity", e);
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
                var dataList = ReadSqlList<AGVRouteArchiveEntity>(theSession, string.Format(queryString, DatabaseDbTable),
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

        public void SaveAGVRoute(List<AGVRouteEntity> agvRoutesList, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => SaveAGVRoute(agvRoutesList, session), ref session);
                return;
            }
            try
            {
                protData.EnterWriteLock();
                if (agvRoutesList.Any())
                {
                    try
                    {
                        Insert(session, agvRoutesList);
                    }
                    catch (Exception e)
                    {
                        logger.ErrorMethod("Failed to save AGVRouteEntity", e);

                    }
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to save AGVRouteEntity", e);
            }
            finally
            {
                protData.ExitWriteLock();
            }

        }

        public List<AGVRouteEntity> GetAllAGVRute(IStatelessSession session = null)
        {
            List<AGVRouteEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllAGVRute(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<AGVRouteEntity>(session, "SELECT * FROM [SNTON].[SNTON].[AGVRoute] WHERE ID IN (SELECT MAX(ID)FROM [SNTON].[SNTON].[AGVRoute] GROUP BY [AGVId])");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get  AGVRouteEntity list", e);
            }
            return ret;
        }

        public void AddAGVRoute(IStatelessSession session = null, params AGVRouteEntity[] routes)
        {
            if (session == null)
            {
                BrokerDelegate(() => AddAGVRoute(session, routes), ref session);
                return;
            }
            try
            {
                protData.EnterWriteLock();
                if (routes.Any())
                {
                    try
                    {
                        Insert(session, routes.ToList());
                    }
                    catch (Exception e)
                    {
                        logger.ErrorMethod("Failed to save AGVRouteEntity", e);
                    }
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to save AGVRouteEntity", e);
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }

        public int DeleteAGVRoute(long agvId, IStatelessSession session = null)
        {
            int i = 0;
            if (session == null)
            {
                i = BrokerDelegate(() => DeleteAGVRoute(agvId, session), ref session);
                return i;
            }
            try
            {
                protData.EnterWriteLock();
                i = RunSqlStatement(session, "DELETE " + DatabaseDbTable + " WHERE AGVId=" + agvId);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("failed to delete agvroute", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }
    }
}
