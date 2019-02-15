using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.MES;
using log4net;
using System.Reflection;
using VI.MFC.Logging;
using SNTON.Components.CleanUp;
using System.Xml;
namespace SNTON.Components.MES
{
    public class MESSystemSpools : CleanUpBrokerBase, IMESSystemSpools
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "MESSystemSpoolsEntity";
        private const string DatabaseDbTable = "dbo.tblFdProd";
        /// <summary>
        /// {0}C06
        /// </summary>
        const string QUERYSQL = @"select A.FdTagNo,A.StructBarcode,A.Length,A.BobbinNo,C.CName,P.Const,C.GroupID from tblFdProd A Inner Join tblProdCodeStructMark B on A.StructBarCode=B.StructBarCode
Inner Join tblCommon C on C.CodeID=B.SpoolType 
LEFT JOIN [dbo].[tblProdCode] P ON B.ProdCode=P.ProdCode where C.GroupID='{0}' AND FDTAGNO='{1}'";
        public MESSystemSpoolsEntity GetMESSpool(string barcode, IStatelessSession session = null)
        {
            MESSystemSpoolsEntity ret = null;

            if (session == null)
            {
                try
                {
                    ret = BrokerDelegate(() => GetMESSpool(barcode, session), ref session);
                }
                catch (Exception ex)
                {
                    logger.ErrorMethod("查询单丝失败", ex);
                }
                return ret;
            }
            try
            {
                string sql = string.Format(QUERYSQL, "C26", barcode);
                //sql = "SELECT StructBarcode, FdTagNo,[Length] FROM tblFdProd WHERE FDTAGNO='64002S'";
                //var tmp = ReadList<MESSystemWhoolsEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} orderby ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
                var tmp = ReadSqlList<MESSystemSpoolsEntity>(session, sql, null);
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

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static MESSystemSpools Create(XmlNode configNode)
        {
            var broker = new MESSystemSpools();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public MESSystemSpools()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public MESSystemSpools(object dependency)
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
        public List<MESSystemSpoolsEntity> GetMESSpools(string barcode, IStatelessSession session = null)
        {
            List<MESSystemSpoolsEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetMESSpools(barcode, session), ref session);
                return ret;
            }
            try
            {
                string sql = string.Format(QUERYSQL, "C06", barcode);
                //var tmp = ReadList<MESSystemWhoolsEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} orderby ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
                var tmp = ReadSqlList<MESSystemSpoolsEntity>(session, sql, null);
                if (tmp.Any())
                {
                    return ret;
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get " + EntityDbTable, e);
            }
            return ret;
        }
    }
}
