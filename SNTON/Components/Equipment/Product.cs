
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
    public class Product : CleanUpBrokerBase, IProduct
    {


        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "ProductEntity";
        private const string DatabaseDbTable = "SNTON.Product";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();
        public List<ProductEntity> Products { get; set; }
        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static Product Create(XmlNode configNode)
        {
            var broker = new Product();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public Product()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public Product(object dependency)
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
            if (Products == null)
                Products = GetAllProductEntity(null);
        }
        #endregion



        public ProductEntity GetProductEntityByID(long Id, IStatelessSession session)
        {
            ProductEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetProductEntityByID(Id, session), ref session);
                return ret;
            }
            try
            {
                var tmp = ReadList<ProductEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} order by ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
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

        public List<ProductEntity> GetAllProductEntity(IStatelessSession session)
        {

            List<ProductEntity> ret = new List<ProductEntity>();

            if (session == null)
            {
                ret = BrokerDelegate(() => GetAllProductEntity(session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                var tmp = ReadSqlList<ProductEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get ProductEntityList", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public int UpdateEntity(IStatelessSession session, params ProductEntity[] products)
        {
            int i = 0;
            if (session == null)
            {
                i = BrokerDelegate(() => UpdateEntity(session, products), ref session);
                return i;
            }
            try
            {
                protData.EnterWriteLock();
                Update(session, products);
                i = products.Length;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("ProductEntity[]Ê§°Ü", ex);
            }
            finally
            {
                protData.ExitWriteLock();
            }
            return i;
        }
    }
}

