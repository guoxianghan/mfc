using log4net;
using NHibernate;
using SNTON.Components.CleanUp;
using SNTON.Constants;
using SNTON.Entities.DBTables.AGV;
using SNTON.WebServices.UserInterfaceBackend.Requests.AGV;
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
    public class AGVRouteArchive : CleanUpBrokerBase, IAGVRouteArchive
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "AGVRouteArchiveEntity";
        private const string DatabaseDbTable = "SNTON.AGVRouteArchive";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static AGVRouteArchive Create(XmlNode configNode)
        {
            var broker = new AGVRouteArchive();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public AGVRouteArchive()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public AGVRouteArchive(object dependency)
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



        public List<AGVRouteArchiveEntity> GetAGVRouteArchive(long agvId, DateTime startTime, DateTime endTime, IStatelessSession session = null)
        {
            List<AGVRouteArchiveEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAGVRouteArchive(agvId, startTime, endTime, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<AGVRouteArchiveEntity>(session, $"SELECT  * FROM {EntityDbTable} WHERE IsDeleted={SNTONConstants.DeletedTag.NotDeleted} and  AGVID= {agvId} AND Created>='" + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and Created<='" + endTime.ToString("yyyy-MM-dd HH:mm:ss") + "' order by id desc");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get AGVRouteArchive  list", e);
            }
            return ret;
        }


        public Tuple<List<AGVRouteArchiveEntity>, int, int> GetHistoryAGVRoute(AGVRuteSearchRequest search, IStatelessSession session = null)
        {
            string starttime = search.startTime.ToString("yyyy-MM-dd HH:mm:ss");
            string endtime = search.endTime.ToString("yyyy-MM-dd HH:mm:ss");
            string sqllist = @"SELECT T.* FROM (
SELECT row_number() over (order by Id) as rownum, [Id]      ,[AGVId]      ,[X]      ,[Y]      ,[Speed]      ,[Created]      ,[Updated]      ,[Deleted]      ,[IsDeleted]
  FROM [SNTON].[SNTON].[AGVRouteArchive] WHERE IsDeleted= :deletedTag and [Created] between '" + starttime + "' and '" + endtime + "' AND  [AGVId]= '" + search.agvid + @"' )T
  WHERE  T.rownum BETWEEN :startindex AND :endindex";
            string sqlcount = @"SELECT COUNT(1) FROM  [SNTON].[SNTON].[AGVRouteArchive] WHERE IsDeleted= :deletedTag and [Created] between '" + starttime + "' and '" + endtime + "'  AND  [AGVId]= '" + search.agvid + "'";
            int count = ReadSql<int>(null, sqlcount, new
            {
                deletedTag = SNTONConstants.DeletedTag.NotDeleted
            });
            List<AGVRouteArchiveEntity> dataList = null;
            try
            {
                dataList = ReadSqlList<AGVRouteArchiveEntity>(session, sqllist,
                                                    new
                                                    {
                                                        deletedTag = SNTONConstants.DeletedTag.NotDeleted,
                                                        startindex = search.pagesize * search.pageindex,
                                                        endindex = search.pagesize * (search.pageindex + 1)
                                                    });
            }
            catch (Exception e)
            {
                e.ToString();
            }
            double pagesize = Math.Ceiling((double)((double)count / (double)search.pagesize));
            return new Tuple<List<AGVRouteArchiveEntity>, int, int>(dataList, count, (int)pagesize);
        }
        public Tuple<List<AGVRouteArchiveEntity>, int, int> GetAllHistoryAGVRoute(AGVRuteSearchRequest search, IStatelessSession session = null)
        {
            string sqllist = @"SELECT T.rownum FROM (
SELECT row_number() over (order by Id) as rownum, [Id]      ,[AGVId]      ,[X]      ,[Y]      ,[TurnToPos]      ,[Speed]      ,[Created]      ,[Updated]      ,[Deleted]      ,[IsDeleted]
  FROM [SNTON].[SNTON].[AGVRouteArchive] WHERE IsDeleted= :deletedTag and [Created] between ':starttime' and ':endtime' order by id desc)T
  WHERE  T.rownum BETWEEN :startindex AND :endindex";
            string sqlcount = @"SELECT COUNT(1) FROM  [SNTON].[SNTON].[AGVRouteArchive] WHERE IsDeleted= :deletedTag and [Created] between ':starttime' and ':endtime' order by id desc";
            int count = ReadSql<int>(null, sqlcount, new
            {
                starttime = search.startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                endtime = search.endTime.ToString("yyyy-MM-dd HH:mm:ss"),
                deletedTag = SNTONConstants.DeletedTag.NotDeleted
            });
            var dataList = ReadSqlList<AGVRouteArchiveEntity>(session, sqllist,
                                                new
                                                {
                                                    starttime = search.startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                    endtime = search.endTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                    deletedTag = SNTONConstants.DeletedTag.NotDeleted,
                                                    startindex = search.pagesize * search.pageindex,
                                                    endindex = search.pagesize * (search.pageindex + 1)
                                                });
            double pagesize = Math.Ceiling((double)(count / search.pagesize));
            return new Tuple<List<AGVRouteArchiveEntity>, int, int>(dataList, count, (int)pagesize);
        }

        public void SaveAGVRoute(List<AGVRouteArchiveEntity> list, IStatelessSession session = null)
        {
            if (session == null)
            {
                BrokerDelegate(() => SaveAGVRoute(list, session), ref session);
                return;
            }
            try
            {
                protData.EnterWriteLock();
                if (list.Any())
                {
                    try
                    {
                        Insert(session, list);
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

        public void AddAGVRoute(IStatelessSession session = null, params AGVRouteArchiveEntity[] routes)
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
    }
}
