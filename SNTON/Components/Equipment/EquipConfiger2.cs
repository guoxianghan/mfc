using System;
using System.Collections.Generic;
using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using VI.MFC.Logging;
using SNTON.Components.CleanUp;
using System.Xml;
using System.Reflection;
using log4net;

namespace SNTON.Components.Equipment
{
    public class EquipConfiger2 : CleanUpBrokerBase, IEquipConfiger2
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipConfiger2Entity";
        private const string DatabaseDbTable = "EquipConfiger2";
        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipConfiger2 Create(XmlNode configNode)
        {
            var broker = new EquipConfiger2();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipConfiger2()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipConfiger2(object dependency)
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
            try
            {
                AllEquipConfiger2 = GetEquipConfiger2(null);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to ReadBrokerData", e);
            }
        }
        #endregion
        public List<EquipConfiger2Entity> AllEquipConfiger2 { get; set; }

        public List<EquipConfiger2Entity> GetEquipConfiger2(IStatelessSession session)
        {
            //List<EquipConfiger2Entity> list = null;
            if (AllEquipConfiger2 != null)
                return AllEquipConfiger2;
            if (session == null)
            {
                if (session == null)
                {
                    AllEquipConfiger2 = BrokerDelegate(() => GetEquipConfiger2(session), ref session);
                    return AllEquipConfiger2;
                }
            }
            try
            {
                protData.EnterReadLock();
                AllEquipConfiger2= ReadSqlList<EquipConfiger2Entity>(session, "SELECT * FROM EquipConfiger2");
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("读取GetEquipConfiger2失败", ex);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return AllEquipConfiger2;
        }
    }
}
