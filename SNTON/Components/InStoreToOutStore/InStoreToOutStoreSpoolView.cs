
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
    public class InStoreToOutStoreSpoolView : CleanUpBrokerBase, IInStoreToOutStoreSpoolView
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "InStoreToOutStoreSpoolViewEntity";
        private const string DatabaseDbTable = "dbo.InStoreToOutStoreSpoolView";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static InStoreToOutStoreSpoolView Create(XmlNode configNode)
        {
            var broker = new InStoreToOutStoreSpoolView();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public InStoreToOutStoreSpoolView()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public InStoreToOutStoreSpoolView(object dependency)
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


        public InStoreToOutStoreSpoolViewEntity GetInStoreToOutStoreSpoolEntityByID(long Id, IStatelessSession session)
        {
            InStoreToOutStoreSpoolViewEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetInStoreToOutStoreSpoolEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<InStoreToOutStoreSpoolViewEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} orderby ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<InStoreToOutStoreSpoolViewEntity> GetAllInStoreToOutStoreSpoolEntity(IStatelessSession session)
        {

            List<InStoreToOutStoreSpoolViewEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllInStoreToOutStoreSpoolEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<InStoreToOutStoreSpoolViewEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
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



        public List<InStoreToOutStoreSpoolViewEntity> GetInStoreToOutStoreSpoolEntity(string sqlwhere, IStatelessSession session = null)
        {
            List<InStoreToOutStoreSpoolViewEntity> ret = null;

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
                var tmp = ReadSqlList<InStoreToOutStoreSpoolViewEntity>(session, sql);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get InStoreToOutStoreSpoolViewEntityList", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public int UpdateEntity(IStatelessSession session, params InStoreToOutStoreSpoolViewEntity[] entity)
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
                List<InStoreToOutStoreSpoolEntity> list = new List<InStoreToOutStoreSpoolEntity>();
                foreach (var item in entity)
                {
                    list.Add(new InStoreToOutStoreSpoolEntity() { Created = item.Created, Deleted = item.Deleted, Id = item.Id, IsDeleted = item.IsDeleted, Updated = item.Updated, AGVSeqNo = item.AGVSeqNo, Guid = item.Guid, InLineNo = item.InLineNo, PlantNo = item.PlantNo, SpoolId = item.SpoolId, Status = item.Status, StoreageNo = item.StoreageNo });
                }
                Update(session, list);
                i = entity.Length;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("InStoreToOutStoreSpoolViewEntity[]Ê§°Ü", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }

        public List<InStoreToOutStoreSpoolViewEntity> GetInStoreToOutStoreSpool(int storeageno, int plantno, IStatelessSession session)
        {
            List<InStoreToOutStoreSpoolViewEntity> ret = new List<InStoreToOutStoreSpoolViewEntity>();

            if (session == null)
            {
                ret = BrokerDelegate(() => GetInStoreToOutStoreSpool(storeageno, plantno, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                string sql = $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + $" AND StoreageNo={storeageno} AND PlantNo={plantno}";

                var tmp = ReadSqlList<InStoreToOutStoreSpoolViewEntity>(session, sql);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get InStoreToOutStoreSpoolViewEntityList", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
    }
}

