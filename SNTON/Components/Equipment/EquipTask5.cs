
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
using SNTON.Entities.DBTables.Equipments;

namespace SNTON.Components.Equipment
{
    public class EquipTask5 : CleanUpBrokerBase, IEquipTask5
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipTask5Entity";
        private const string DatabaseDbTable = "SNTON.EquipTask5";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipTask5 Create(XmlNode configNode)
        {
            var broker = new EquipTask5();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipTask5()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipTask5(object dependency)
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



        public EquipTask5Entity GetEquipTask5EntityByID(long Id, IStatelessSession session)
        {
            EquipTask5Entity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTask5EntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<EquipTask5Entity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<EquipTask5Entity> GetAllEquipTask5Entity(IStatelessSession session)
        {

            List<EquipTask5Entity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllEquipTask5Entity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipTask5Entity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTask5EntityList", e);
            }
            return ret;
        }

        public List<EquipTask5Entity> GetEquipTask5(string sqlwhere, IStatelessSession session = null)
        {
            List<EquipTask5Entity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipTask5(sqlwhere, session), ref session);
                return ret;
            }
            try
            {
                if (!string.IsNullOrEmpty(sqlwhere))
                {
                    sqlwhere = " and " + sqlwhere;
                }
                var tmp = ReadList<EquipTask5Entity>(session, $"FROM EquipTask5 where IsDeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted + sqlwhere + " order by ID desc");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get EquipTask5Entity list", e);
            }
            return ret;
        }

        public  int UpdateEquipTask5(IStatelessSession session = null, params EquipTask5Entity[] tasks)
        {
            if (tasks == null || tasks.Length == 0)
                return 0;
            int r = 0;
            try
            {
                protData.EnterWriteLock();
                Update(session, tasks.ToList());
                r = tasks.Length;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("¸üÐÂEquipTask5EntityÊ§°Ü", ex);
                r = 0;
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return r;
        }
    }
}

