
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
using SNTON.Components.PLCAddressCode;
using SNTON.Entities.DBTables.PLCAddressCode;

namespace SNTON.Components.PLCAddressCode
{
    public class MachineWarnningCode : CleanUpBrokerBase, IMachineWarnningCode
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "MachineWarnningCodeEntity";
        private const string DatabaseDbTable = "SNTON.MachineWarnningCode";

        public List<MachineWarnningCodeEntity> MachineWarnningCodes { get; set; }

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static MachineWarnningCode Create(XmlNode configNode)
        {
            var broker = new MachineWarnningCode();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public MachineWarnningCode()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public MachineWarnningCode(object dependency)
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
            if (MachineWarnningCodes == null)
                MachineWarnningCodes = GetAllMachineWarnningCodeEntity(null);
        }
        #endregion



        public MachineWarnningCodeEntity GetMachineWarnningCodeEntityByID(long Id, IStatelessSession session)
        {
            MachineWarnningCodeEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetMachineWarnningCodeEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<MachineWarnningCodeEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<MachineWarnningCodeEntity> GetAllMachineWarnningCodeEntity(IStatelessSession session)
        {

            List<MachineWarnningCodeEntity> ret = new List<MachineWarnningCodeEntity>();
            if (MachineWarnningCodes != null)
                return MachineWarnningCodes;
            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllMachineWarnningCodeEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<MachineWarnningCodeEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
                MachineWarnningCodes = ret;
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get MachineWarnningCodeEntityList", e);
            }
            return ret;
        }
    }
}

