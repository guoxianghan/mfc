using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Components.CleanUp;
using log4net;
using System.Xml;
using VI.MFC.Logging;
using System.Reflection;
using NHibernate;

namespace SNTON.Components.Equipment
{
    public class EquipCommand : CleanUpBrokerBase, IEquipCommand
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipCommandEntity";
        private const string DatabaseDbTable = "SNTON.EquipCommand";
        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipCommand Create(XmlNode configNode)
        {
            var broker = new EquipCommand();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipCommand()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipCommand(object dependency)
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
                _AllEquipCommandList = GetAllEquipCommand(null);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to ReadBrokerData", e);
            }
        }
        public List<EquipCommandEntity> _AllEquipCommandList { get; set; }
        #endregion
        public List<EquipCommandEntity> GetAllEquipCommand(IStatelessSession session)
        {
            List<EquipCommandEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllEquipCommand(session), ref session);
                return ret;
            }
            try
            {
                if (_AllEquipCommandList != null)
                    return _AllEquipCommandList;
                var tmp = ReadList<EquipCommandEntity>(session, " FROM " + EntityDbTable + " where IsEnable=0 and isdeleted=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                    _AllEquipCommandList = ret;
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetAllEquipCommand", e);
            }
            return ret;
        }
    }
}
