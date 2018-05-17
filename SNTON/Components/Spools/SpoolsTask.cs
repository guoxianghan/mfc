
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
using SNTON.Entities.DBTables.Spools;

namespace SNTON.Components.Spools
{
    public class SpoolsTask : CleanUpBrokerBase, ISpoolsTask
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "SpoolsTaskEntity";
        private const string DatabaseDbTable = "SNTON.SpoolsTask";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static SpoolsTask Create(XmlNode configNode)
        {
            var broker = new SpoolsTask();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public SpoolsTask()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public SpoolsTask(object dependency)
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



        public SpoolsTaskEntity GetSpoolsTaskEntityByID(long Id, IStatelessSession session)
        {
            SpoolsTaskEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSpoolsTaskEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<SpoolsTaskEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<SpoolsTaskEntity> GetAllSpoolsTaskEntity(IStatelessSession session)
        {

            List<SpoolsTaskEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllSpoolsTaskEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<SpoolsTaskEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get SpoolsTaskEntityList", e);
            }
            return ret;
        }

        public List<SpoolsTaskEntity> GetSpoolsTask(string where, IStatelessSession session = null)
        {
            List<SpoolsTaskEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllSpoolsTaskEntity(session), ref session);
                return ret;
            }
            try
            {
                where = where + " ORDER BY ID DESC";
                var tmp = ReadSqlList<SpoolsTaskEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + where);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get SpoolsTaskEntityList", e);
            }
            return ret;
        }

        public List<SpoolsTaskEntity> GetSpoolsTasks(string sqlwhere, IStatelessSession session = null)
        {
            List<SpoolsTaskEntity> ret = new List<SpoolsTaskEntity>();

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSpoolsTasks(sqlwhere,session), ref session);
                return ret;
            }
            try
            { 
                var tmp = ReadSqlList<SpoolsTaskEntity>(session, sqlwhere);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get SpoolsTaskEntityList", e);
            }
            return ret;
        }
    }
}
