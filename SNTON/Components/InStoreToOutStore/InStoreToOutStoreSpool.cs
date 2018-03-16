
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
using SNTON.Entities.DBTables.InStoreToOutStore;

namespace SNTON.Components.InStoreToOutStore
{
    public class InStoreToOutStoreSpool : CleanUpBrokerBase, IInStoreToOutStoreSpool
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "InStoreToOutStoreSpoolEntity";
        private const string DatabaseDbTable = "SNTON.InStoreToOutStoreSpool";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static InStoreToOutStoreSpool Create(XmlNode configNode)
        {
            var broker = new InStoreToOutStoreSpool();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public InStoreToOutStoreSpool()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public InStoreToOutStoreSpool(object dependency)
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



        public InStoreToOutStoreSpoolEntity GetInStoreToOutStoreSpoolEntityByID(long Id, IStatelessSession session)
        {
            InStoreToOutStoreSpoolEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetInStoreToOutStoreSpoolEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<InStoreToOutStoreSpoolEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<InStoreToOutStoreSpoolEntity> GetAllInStoreToOutStoreSpoolEntity(IStatelessSession session)
        {

            List<InStoreToOutStoreSpoolEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllInStoreToOutStoreSpoolEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<InStoreToOutStoreSpoolEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get InStoreToOutStoreSpoolEntityList", e);
            }
            return ret;
        }



        public List<InStoreToOutStoreSpoolEntity> GetInStoreToOutStoreSpoolEntity(string sqlwhere, IStatelessSession session = null)
        {
            List<InStoreToOutStoreSpoolEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetInStoreToOutStoreSpoolEntity(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                string sql = $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted;
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sql = sql + " and " + sqlwhere;
                }
                var tmp = ReadSqlList<InStoreToOutStoreSpoolEntity>(session, sql);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetInStoreToOutStoreSpoolEntityList", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public int UpdateEntity(IStatelessSession session, params InStoreToOutStoreSpoolEntity[] entity)
        {
            int i = 0;
            if (session == null)
            {
                i = BrokerDelegate(() => UpdateEntity(session, entity), ref session);
                return i;
            }
            try
            {
                protData.EnterWriteLock();
                Update(session, entity);
                i = entity.Length;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("InStoreToOutStoreSpoolEntity[]Ê§°Ü", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }

        public int AddEntity(IStatelessSession session, params InStoreToOutStoreSpoolEntity[] entity)
        {
            int i = 0;
            if (session == null)
            {
                i = BrokerDelegate(() => AddEntity(session, entity), ref session);
                return i;
            }
            try
            {
                protData.EnterWriteLock();
                Insert(session, entity);
                i = entity.Length;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("InStoreToOutStoreSpoolEntity[]Ê§°Ü", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }
    }
}

