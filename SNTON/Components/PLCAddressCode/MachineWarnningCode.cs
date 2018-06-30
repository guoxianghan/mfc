
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
using VI.MFC.Utils;

namespace SNTON.Components.PLCAddressCode
{
    public class MachineWarnningCode : CleanUpBrokerBase, IMachineWarnningCode
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "MachineWarnningCodeEntity";
        private const string DatabaseDbTable = "SNTON.MachineWarnningCode";
        private VIThreadEx thread_realtimeWarningCache;
        public List<MachineWarnningCodeEntity> MachineWarnningCache { get; set; }
        void GetWarnningCache()
        {
            MachineWarnningCache = GetAllMachineWarnningCodeEntity(null);
        }
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
            thread_realtimeWarningCache = new VIThreadEx(GetWarnningCache, null, "thread readding for warninfo", 4000);
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
            thread_realtimeWarningCache.Start();
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
            //if (MachineWarnningCodes != null)
            //    return MachineWarnningCodes;
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
                MachineWarnningCache = ret;
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get MachineWarnningCodeEntityList", e);
            }
            return ret;
        }

        public int UpdateWarning(List<MachineWarnningCodeEntity> list, IStatelessSession session = null)
        {
            if (list == null || list.Count == 0)
                return 0;
            int i = 0;
            if (session == null)
            {
                i = BrokerDelegate(() => UpdateWarning(list, session), ref session);
                return i;
            }
            try
            {
                protData.EnterWriteLock();

                Update(session, list);
                i = list.Count;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("更新MachineWarnningCodeEntity list 失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }

        public bool ResetWarning(byte midstoreno, byte machecode, IStatelessSession session = null)
        {
            bool i = false;
            if (session == null)
            {
                i = BrokerDelegate(() => ResetWarning(midstoreno, machecode, session), ref session);
                return i;
            }
            try
            {
                protData.EnterWriteLock();
                string sql = $"UPDATE SNTON.MachineWarnningCode SET IsWarning=1 WHERE [MidStoreNo]={midstoreno} AND [MachineCode]={machecode}";
                RunSqlStatement(null, sql);

            }
            catch (Exception ex)
            {
                logger.ErrorMethod("重置报警信息失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }
    }
}

