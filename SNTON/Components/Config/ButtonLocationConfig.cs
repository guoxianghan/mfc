
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
using SNTON.Entities.DBTables.Config;

namespace SNTON.Components.Config
{
    public class ButtonLocationConfig : CleanUpBrokerBase, IButtonLocationConfig
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "ButtonLocationConfigEntity";
        private const string DatabaseDbTable = "SNTON.ButtonLocationConfig";

        public List<ButtonLocationConfigEntity> _ButtonLocationConfigList { get; set; } = new List<ButtonLocationConfigEntity>();

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static ButtonLocationConfig Create(XmlNode configNode)
        {
            var broker = new ButtonLocationConfig();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public ButtonLocationConfig()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public ButtonLocationConfig(object dependency)
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
            _ButtonLocationConfigList = this.GetAllButtonLocationConfigEntity(null);
        }
        #endregion



        public ButtonLocationConfigEntity GetButtonLocationConfigEntityByID(long Id, IStatelessSession session)
        {
            ButtonLocationConfigEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetButtonLocationConfigEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<ButtonLocationConfigEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} orderby ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<ButtonLocationConfigEntity> GetAllButtonLocationConfigEntity(IStatelessSession session)
        {

            List<ButtonLocationConfigEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllButtonLocationConfigEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<ButtonLocationConfigEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get ButtonLocationConfigEntityList", e);
            }
            return ret;
        }
    }
}

