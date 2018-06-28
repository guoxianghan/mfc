
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
using SNTON.Entities.DBTables.AGV_KJ_Interface;

namespace SNTON.Components.AGV_KJ_Interface
{
    public class T_AGV_KJ_Interface : CleanUpBrokerBase, IT_AGV_KJ_Interface
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "T_AGV_KJ_InterfaceEntity";
        private const string DatabaseDbTable = "T_AGV_KJ_Interface";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static T_AGV_KJ_Interface Create(XmlNode configNode)
        {
            var broker = new T_AGV_KJ_Interface();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public T_AGV_KJ_Interface()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public T_AGV_KJ_Interface(object dependency)
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



        public T_AGV_KJ_InterfaceEntity GetT_AGV_KJ_InterfaceEntityByID(long Id, IStatelessSession session)
        {
            T_AGV_KJ_InterfaceEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetT_AGV_KJ_InterfaceEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                var tmp = ReadList<T_AGV_KJ_InterfaceEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get " + EntityDbTable, e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public List<T_AGV_KJ_InterfaceEntity> GetAllT_AGV_KJ_InterfaceEntity(IStatelessSession session)
        {

            List<T_AGV_KJ_InterfaceEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllT_AGV_KJ_InterfaceEntity(session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                var tmp = ReadSqlList<T_AGV_KJ_InterfaceEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get T_AGV_KJ_InterfaceEntityList", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public List<T_AGV_KJ_InterfaceEntity> GetT_AGV_KJ_Interface(string sql, IStatelessSession session = null)
        {
            List<T_AGV_KJ_InterfaceEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetT_AGV_KJ_Interface(sql, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                var tmp = ReadSqlList<T_AGV_KJ_InterfaceEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE " + sql);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get T_AGV_KJ_InterfaceEntityList", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public bool UpdateT_AGV_KJ_Interface(T_AGV_KJ_InterfaceEntity entity = null, IStatelessSession session = null)
        {
            bool r = false;
            if (session == null)
            {
                r = BrokerDelegate(() => UpdateT_AGV_KJ_Interface(entity, session), ref session);
                return r;
            }
            try
            {
                protData.EnterWriteLock();
                Update(session, entity);
                r = true;
            }
            catch (Exception ex)
            {
                r = false;
                logger.ErrorMethod("filed to update T_AGV_KJ_Interface", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return r;
        }

        public T_AGV_KJ_InterfaceEntity GetT_AGV_KJ_InterfaceEntity(string sql, IStatelessSession session = null)
        {
            T_AGV_KJ_InterfaceEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetT_AGV_KJ_InterfaceEntity(sql, session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                var tmp = ReadSqlList<T_AGV_KJ_InterfaceEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE " + sql);
                if (tmp.Any())
                {
                    ret = tmp.ToList().FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get T_AGV_KJ_InterfaceEntity", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }
    }
}

