using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Components.CleanUp;
using SNTON.Entities.DBTables.Spools;
using log4net;
using System.Reflection;
using System.Xml;
using VI.MFC.Logging;
using static SNTON.Constants.SNTONConstants;

namespace SNTON.Components.Spools
{
    public class Spools : CleanUpBrokerBase, ISpools
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "SpoolsEntity";
        private const string DatabaseDbTable = "SNTON.Spools";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static Spools Create(XmlNode configNode)
        {
            var broker = new Spools();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public Spools()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public Spools(object dependency)
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
        public SpoolsEntity GetSpoolByBarcode(string barcode, IStatelessSession session = null)
        {
            SpoolsEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSpoolByBarcode(barcode, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<SpoolsEntity>(session, string.Format(" FROM {0} where  FdTagNo = '{1}' AND IsDeleted={2} order by ID desc", EntityDbTable, barcode, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public SpoolsEntity GetSpoolByMidStorageId(int midStorageId, IStatelessSession session = null)
        {
            SpoolsEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSpoolByMidStorageId(midStorageId, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<SpoolsEntity>(session, string.Format(" FROM {0} where  MidStorageId = {1} AND IsDeleted={2} order by ID desc", EntityDbTable, midStorageId, Constants.SNTONConstants.DeletedTag.NotDeleted));
                //var tmp = ReadSqlList<SpoolsEntity>(session, "SELECT *"+string.Format(" FROM {0} where  MidStorageId = {1} AND IsDeleted={2} order by ID desc", DatabaseDbTable, midStorageId, Constants.SNTONConstants.DeletedTag.NotDeleted));

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

        public IList<SpoolsEntity> GetSpoolsByAGVId(int agvId, IStatelessSession session = null)
        {
            IList<SpoolsEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSpoolsByAGVId(agvId, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<SpoolsEntity>(session, $"SELECT * FROM {EntityDbTable} where AGVIdFromMidStorage={agvId}");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetSpoolsByAGVId", e);
            }
            return ret;
        }

        public List<SpoolsEntity> GetSpoolsByProudctType(string proudctType, IStatelessSession session)
        {
            List<SpoolsEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSpoolsByProudctType(proudctType, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<SpoolsEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND ProductType LIKE '%" + proudctType + "%'");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetSpoolsByProudctType", e);
            }
            return ret;
        }
        public void UpdateSpools(IStatelessSession session, params SpoolsEntity[] SpoolsID)
        {
            if (session == null)
            {
                BrokerDelegate(() => UpdateSpools(session, SpoolsID), ref session);
                return;
            }
            try
            {
                this.Update<SpoolsEntity>(session, SpoolsID);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to UpdateSpools", e);
            }
            return;
        }
        public List<SpoolsEntity> GetSpoolsByTaskNo(string taskno, IStatelessSession session)
        {
            List<SpoolsEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSpoolsByTaskNo(taskno, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<SpoolsEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND taskno = '" + taskno + "'");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetSpoolsByProudctType", e);
            }
            return ret;
        }

        public SpoolsEntity GetSpoolBysqlwhere(string sql, IStatelessSession session)
        {
            SpoolsEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSpoolBysqlwhere(sql, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<SpoolsEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND " + sql + "");
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetSpoolsEntityBysqlwhere", e);
            }
            return ret;
        }

        public List<SpoolsEntity> GetSpoolsBysqlwhere(string sql, IStatelessSession session)
        {
            List<SpoolsEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSpoolsBysqlwhere(sql, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<SpoolsEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND " + sql + "");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetSpoolsBysqlwhere", e);
            }
            return ret;
        }

        public int FinishedSpool(IStatelessSession session = null)
        {
            var d = GetSpoolBysqlwhere(" STATUS=" + (int)SpoolsStatus.Grab, null);
            if (d != null)
            {
                d.Status = (int)SpoolsStatus.Finished;
                UpdateSpools(null, d);
                return 1;
            }
            else return 0;
        }

        public int Add(SpoolsEntity entity, IStatelessSession session = null)
        {

            try
            {
                Insert<SpoolsEntity>(session, entity);
                return 1;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("Spool插入失败", ex);
                return 0;
            }
        }

        public int AddRange(List<SpoolsEntity> entities, IStatelessSession session = null)
        {
            try
            {
                Insert<SpoolsEntity>(session, entities);
                return entities.Count;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("Spools插入失败", ex);
                return 0;
            }
        }

        public long GetSpoolID(string barcode, IStatelessSession theSession)
        {
            //long id = 0;
            //try
            //{
            //    //id = ReadScalar<long>(theSession,"" );
            //}
            //catch (Exception)
            //{

            //    throw;
            //}
            throw new NotImplementedException("尚未实现该方法");
        }
    }
}
