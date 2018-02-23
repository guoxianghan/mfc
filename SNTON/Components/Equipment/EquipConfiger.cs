using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Components.CleanUp;
using log4net;
using System.Reflection;
using System.Xml;
using VI.MFC.Logging;

namespace SNTON.Components.Equipment
{
    public class EquipConfiger : CleanUpBrokerBase, IEquipConfiger
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipConfigerEntity";
        private const string DatabaseDbTable = "EquipConfiger";
        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipConfiger Create(XmlNode configNode)
        {
            var broker = new EquipConfiger();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipConfiger()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipConfiger(object dependency)
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
                EquipConfigers = GetEquipConfigerEntities(null);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to ReadBrokerData", e);
            }
        }
        #endregion
        public List<EquipConfigerEntity> EquipConfigers { get; set; }

        public List<EquipConfigerEntity> GetEquipConfigerEntities(IStatelessSession session)
        {
            List<EquipConfigerEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipConfigerEntities(session), ref session);
                return ret;
            }
            try
            {
                if (EquipConfigers != null)
                    return EquipConfigers;
                var tmp = ReadSqlList<EquipConfigerEntity>(session, "SELECT * FROM " + DatabaseDbTable + "  where IsEnable=0 and isdeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                    EquipConfigers = ret;
                    var equips = ReadSqlList<EquipConfigEntity>(session, "SELECT * FROM [SNTON].EquipConfig WHERE ISDELETED='"
                       + Constants.SNTONConstants.DeletedTag.NotDeleted + "'");
                    foreach (var item in ret)
                    {
                        item.EquipList = equips.FindAll(x=>x.EquipControllerId==item.ControlID);
                    }
                }
            }

            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetEquipConfigerEntities", e);
            }
            return ret;
        }

    }
}