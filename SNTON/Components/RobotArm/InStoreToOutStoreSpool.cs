using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Entities.DBTables.RobotArmTask;
using NHibernate;
using SNTON.Components.CleanUp;
using log4net;
using System.Reflection;
using System.Xml;
using VI.MFC.Logging;
using System.IO;
using SNTON.Constants;

namespace SNTON.Components.RobotArm
{
    public class InStoreToOutStoreSpool : CleanUpBrokerBase, IInStoreToOutStoreSpool
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "InStoreToOutStoreSpool";
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


        public List<InStoreToOutStoreSpoolEntity> GetInStoreToOutStoreSpool(int storeageno, int plantno, IStatelessSession session)
        {
            List<InStoreToOutStoreSpoolEntity> list = new List<InStoreToOutStoreSpoolEntity>();
            if (session == null)
            {
                list = BrokerDelegate(() => GetInStoreToOutStoreSpool(storeageno, plantno, session), ref session);
                return list;
            }
            if (!File.Exists($".{SNTONConstants.FileTmpPath}/{plantno}#OutStoreSpool{storeageno}.json"))
                return list;
            try
            {
                protData.EnterReadLock();
                string json = File.ReadAllText($".{SNTONConstants.FileTmpPath}/{plantno}#OutStoreSpool{storeageno}.json");
                list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InStoreToOutStoreSpoolEntity>>(json);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("读取GetInStoreToOutStoreSpool失败", ex);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return list;
        }

        public void SaveInStoreToOutStoreSpool(int storeageno, int plantno, List<InStoreToOutStoreSpoolEntity> list, IStatelessSession session)
        {
            if (session == null)
            {
                BrokerDelegate(() => SaveInStoreToOutStoreSpool(storeageno, plantno, list, session), ref session);
                return;
            }
            try
            {
                protData.EnterWriteLock();
                string js = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                var by = Encoding.UTF8.GetBytes(js);
                File.WriteAllBytes($".{SNTONConstants.FileTmpPath}/{plantno}#OutStoreSpool{storeageno}.json", by);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("保存GetInStoreToOutStoreSpool失败", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }

        public List<InStoreToOutStoreSpoolEntity> GetAllInStoreToOutStoreSpool(IStatelessSession session = null)
        {
            List<InStoreToOutStoreSpoolEntity> list = new List<InStoreToOutStoreSpoolEntity>();
            if (session == null)
            {
                list = BrokerDelegate(() => GetAllInStoreToOutStoreSpool(session), ref session);
                return list;
            }
            //if (!File.Exists($".{SNTONConstants.FileTmpPath}/3#OutStoreSpool1.json"))
            //    return list;
            try
            {
                protData.EnterReadLock();
                for (int i = 1; i <= 3; i++)
                {
                    try
                    {
                        if (!File.Exists($".{SNTONConstants.FileTmpPath}/3#OutStoreSpool{i}.json"))
                            continue;
                        string json = File.ReadAllText($".{SNTONConstants.FileTmpPath}/3#OutStoreSpool{i}.json");
                        var tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InStoreToOutStoreSpoolEntity>>(json);
                        list.AddRange(tmp);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorMethod("读取GetInStoreToOutStoreSpool失败", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("读取GetInStoreToOutStoreSpool失败", ex);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return list;
        }
    }
}
