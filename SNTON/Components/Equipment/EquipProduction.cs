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
using SNTON.WebServices.UserInterfaceBackend.Models;
using SNTON.WebServices.UserInterfaceBackend.Models.Equip;

namespace SNTON.Components.Equipment
{
    public class EquipProduction : CleanUpBrokerBase, IEquipProduction
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "EquipProductionEntity";
        private const string DatabaseDbTable = "SNTON.EquipProduction";
        public List<EquipProductionEntity> _AllEquipProductionEntityList { get; set; } = new List<EquipProductionEntity>();
        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static EquipProduction Create(XmlNode configNode)
        {
            var broker = new EquipProduction();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public EquipProduction()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public EquipProduction(object dependency)
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
            _AllEquipProductionEntityList = GetAllEquipProductionEntity(null);
        }
        #endregion


        public IList<EquipProductionEntity> GetEquipProductionsByEquipID(long equipid, IStatelessSession session)
        {
            IList<EquipProductionEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipProductionsByEquipID(equipid, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipProductionEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND equipid='" + equipid + "'");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetEquipProductionsByEquipID", e);
            }
            return ret;
        }

        public IList<EquipProductionEntity> GetEquipProductionsByGroupID(long groupid, IStatelessSession session)
        {
            IList<EquipProductionEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipProductionsByGroupID(groupid, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipProductionEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND GroupId='" + groupid + "'");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetEquipProductionsByGroupID", e);
            }
            return ret;
        }

        public IList<EquipProductionEntity> GetEquipProductionsByOperator(string oper, IStatelessSession session)
        {
            IList<EquipProductionEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipProductionsByOperator(oper, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipProductionEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND Operator='" + oper + "'");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetEquipProductionsByOperator", e);
            }
            return ret;
        }

        public IList<EquipProductionEntity> GetEquipProductionsByProductType(string producttype, IStatelessSession session)
        {
            IList<EquipProductionEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetEquipProductionsByProductType(producttype, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipProductionEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND ProductType='" + producttype + "'");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetEquipProductionsByProductType", e);
            }
            return ret;
        }

        public IList<EquipProductionEntity> GetEquipProductionsSearch(EquipProductionSearchRequest search, IStatelessSession session)
        {
            IList<EquipProductionEntity> list = null;
            try
            {
                StringBuilder sql = new StringBuilder(" WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (!string.IsNullOrEmpty(search.producttype))
                { sql.Append($" and ProductType='{ search.producttype}'"); }
                if (search.equipid != 0)
                { sql.Append($" and equipid={search.equipid}"); }
                if (search.groupid != 0)
                { sql.Append($" and groupid={search.groupid}"); }
                if (search.endTime.HasValue)
                    sql.Append(" AND Created >='" + search.endTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
                if (search.startTime.HasValue)
                    sql.Append(" AND Created <='" + search.startTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
                if (search.endIndex != 0 && search.startIndex != 0)
                {
                    list = ReadSqlList<EquipProductionEntity>(session, "SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY ID desc) AS RowNumber,* FROM " + DatabaseDbTable + sql.ToString() + ")T WHERE T.RowNumber BETWEEN " + search.startIndex + " AND " + search.endIndex);
                }
                else
                {
                    list = ReadSqlList<EquipProductionEntity>(session, "SELECT * FROM " + sql.ToString());
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetEquipProductionsSearch", e);
            }
            return list;
        }

        public void SaveEquipProductionList(List<EquipProductionDataUI> list, string oper, IStatelessSession session)
        {

            if (session == null)
            {
                BrokerDelegate(() => SaveEquipProductionList(list, oper, session), ref session);
                return;
            }
            try
            {
                List<EquipProductionEntity> ret = new List<EquipProductionEntity>();
                List<EquipConfigEntity> config = ReadList<EquipConfigEntity>(session, " FROM " + EntityDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " ORDER BY ID DESC");
                if (list != null && list.Count != 0)
                {
                    var dt = DateTime.Now;
                    foreach (var item in list)
                    {
                        var pro = new EquipProductionEntity() { Created = dt, Description = item.Description, EquipId = item.EquipId, EmptySpoolType = item.EmptySpoolType, GroupId = item.GroupId, Operator = oper, ProductType = item.ProductType };
                        if (config != null)
                        {
                            pro.Description = config.FirstOrDefault(x => x.Id.ToString() == pro.EquipId)?.Description;
                        }
                        ret.Add(pro);
                    }
                    protData.EnterUpgradeableReadLock();
                    try
                    {
                        this.protData.EnterWriteLock();
                        Insert(session, ret);
                        RunSqlStatement(session, "UPDATE " + DatabaseDbTable + " ISDELETED=" + Constants.SNTONConstants.DeletedTag.Deleted);
                        this.protData.ExitWriteLock();
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorMethod("Failed to  SaveEquipProductionList", ex);
                    }
                    finally
                    {
                        this.protData.ExitUpgradeableReadLock();
                    }
                }

            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to  SaveEquipProductionList", e);
            }
        }

        public void UpdateEquipProduction(EquipProductionDataUI product, string oper, IStatelessSession session)
        {
            if (product != null)
            {
                EquipProductionEntity ent = Read<EquipProductionEntity>(session, " FROM " + EntityDbTable + " WHERE ID =" + product.Id + " ORDER BY ID DESC");
                if (ent != null)
                {
                    ent.ProductType = product.ProductType;
                    ent.Updated = DateTime.Now;
                    ent.EquipId = product.EquipId;
                    ent.GroupId = product.GroupId;
                    ent.EmptySpoolType = product.EmptySpoolType;
                    try
                    {
                        Update(session, ent);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorMethod("Failed to  UpdateEquipProduction", ex);
                    }
                }
                else
                {
                    throw new ArgumentNullException("错误的EquipProductionID");
                }
            }
            else throw new ArgumentNullException("product 是空的");

        }

        public List<EquipProductionEntity> GetAllEquipProductionEntity(IStatelessSession session = null)
        {
            List<EquipProductionEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllEquipProductionEntity(session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadSqlList<EquipProductionEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED="
                    + Constants.SNTONConstants.DeletedTag.NotDeleted + " ORDER BY ID DESC");
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetAllEquipProductionEntity", e);
            }
            return ret;
        }
        public void SaveEquipProduction(IList<EquipProductionEntity> equipProduction, IStatelessSession session)
        {
            if (session == null)
            {
                BrokerDelegate(() => SaveEquipProduction(equipProduction, session), ref session);
                return;
            }
            try
            {
                if (equipProduction.Any())
                {
                    Insert(session, equipProduction);
                    logger.InfoMethod(string.Format("Set {0} records exception EquipProductionEntity lists to DB successfully", equipProduction.Count));
                }
                else
                {
                    //logger.ErrorMethod("Argument chutes is empty, action will be ignored");
                    throw new ArgumentException("Argument EquipProductionEntity is NULL");
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to save EquipProductionEntity", e);
            }

        }

    }
}
