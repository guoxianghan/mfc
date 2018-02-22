using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.MES;
using log4net;
using System.Reflection;
using SNTON.Components.CleanUp;
using System.Xml;
using VI.MFC.Logging;

namespace SNTON.Components.MES
{
    /// <summary>
    /// 作业标准书对应的表
    /// </summary>
    public class tblProdCodeStructMark : CleanUpBrokerBase, ItblProdCodeStructMark
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "tblProdCodeStructMarkEntity";
        private const string DatabaseDbTable = "dbo.tblProdCodeStructMark";
        /// <summary>
        /// 根据作业标准书查到作业标准详情以及工字轮类型
        /// </summary>
        const string QUERYSQL_StructMark = @"select A.FdTagNo,A.StructBarcode,A.Length,C.CName,P.Const,C.GroupID from tblFdProd A Inner Join tblProdCodeStructMark B on A.StructBarCode=B.StructBarCode
Inner Join tblCommon C on C.CodeID=B.SpoolType 
LEFT JOIN [dbo].[tblProdCode] P ON B.ProdCode=P.ProdCode where C.GroupID='C06' AND B.StructBarCode  in ({0})";


        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static tblProdCodeStructMach Create(XmlNode configNode)
        {
            var broker = new tblProdCodeStructMach();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public tblProdCodeStructMark()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public tblProdCodeStructMark(object dependency)
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

        /// <summary>
        ///  根据作业标准书编号获得具体规格工字轮型号,长度等详细信息
        /// </summary>
        /// <param name="StructBarCode"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public tblProdCodeStructMarkEntity GettblProdCodeStructMark(string StructBarCode, IStatelessSession session = null)
        {
            tblProdCodeStructMarkEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GettblProdCodeStructMark(StructBarCode, session), ref session);
                return ret;
            }
            try
            {
                string sql = string.Format(QUERYSQL_StructMark, "'" + StructBarCode + "'");
                //var tmp = ReadList<MESSystemWhoolsEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} orderby ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
                ret = ReadSql<tblProdCodeStructMarkEntity>(session, sql, null);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get " + EntityDbTable, e);
            }
            return ret;
        }

        public List<tblProdCodeStructMarkEntity> GettblProdCodeStructMarks(IStatelessSession session = null, params string[] StructBarCode)
        {
            List<tblProdCodeStructMarkEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GettblProdCodeStructMarks(null, StructBarCode), ref session);
                return ret;
            }
            try
            {
                StringBuilder codes = new StringBuilder();
                foreach (var item in StructBarCode)
                {
                    codes.Append("'" + item + "',");
                }
                string sql = string.Format(QUERYSQL_StructMark, codes.ToString().TrimEnd(','));
                //var tmp = ReadList<MESSystemWhoolsEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} orderby ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
                ret = ReadSqlList<tblProdCodeStructMarkEntity>(session, sql, null);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get " + EntityDbTable, e);
            }
            return ret;
        }
        #endregion
    }
}
