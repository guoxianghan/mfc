using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.MES;
using SNTON.Components.CleanUp;
using log4net;
using System.Reflection;
using System.Xml;
using VI.MFC.Logging;

namespace SNTON.Components.MES
{
    /// <summary>
    /// 车间机台号与作业标准书关联表
    /// </summary>
    public class tblProdCodeStructMach : CleanUpBrokerBase, ItblProdCodeStructMach
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "tblProdCodeStructMachEntity";
        private const string DatabaseDbTable = "dbo.tblProdCodeStructMach";
        /// <summary>
        /// {0}3A0103 机台号ST-3B-05-18
        /// </summary>
        const string QUERYSQL = @"SELECT * FROM tblProdCodeStructMach where MachCode={0} and StartDate=(SELECT max(StartDate) FROM tblProdCodeStructMach where  MachCode  IN ({0}))";
        const string QUERYSQL_MULITI = @"SELECT * FROM tblProdCodeStructMach where MachCode in({0})";
        /// <summary>
        /// 根据作业标准书查到作业标准详情以及工字轮类型
        /// </summary>
        const string QUERYSQL_StructMark = @"select B.StructBarCode,B.ProdCode,B.ProdLength,P.Const,C.CName,B.[TitleProdName],B.Supply1,B.SupplyQty1,B.Supply2,B.SupplyQty2,B.SpoolType from tblProdCodeStructMark B 
Inner Join tblCommon C on C.CodeID=B.SpoolType 
LEFT JOIN [tblProdCode] P ON B.ProdCode=P.ProdCode where C.GroupID='{1}' AND B.StructBarCode IN ({0})";


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
        public tblProdCodeStructMach()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public tblProdCodeStructMach(object dependency)
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

        [Obsolete("该方法尚未实现")]
        public tblProdCodeStructMachEntity GettblProdCodeStructMach(string sql, IStatelessSession session = null)
        {
            throw new NotImplementedException("该方法尚未实现");
        }

        public tblProdCodeStructMachEntity GettblProdCodeStructMachMachCode(string MachCode, IStatelessSession session = null)
        {
            tblProdCodeStructMachEntity ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GettblProdCodeStructMachMachCode(MachCode, session), ref session);
                return ret;
            }
            try
            {
                string sql = string.Format(QUERYSQL, "'" + MachCode + "'");
                //var tmp = ReadList<MESSystemWhoolsEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} orderby ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
                var tmp = ReadSqlList<tblProdCodeStructMachEntity>(session, sql, null);
                if (tmp.Any())
                {
                    ret = tmp.FirstOrDefault();
                    ret.ProdCodeStructMark3 = ReadSql<tblProdCodeStructMarkEntity>(null, string.Format(QUERYSQL_StructMark, ret.StructBarCode.Trim(), "C06"));
                    ret.ProdCodeStructMark4 = ReadSql<tblProdCodeStructMarkEntity>(null, string.Format(QUERYSQL_StructMark, ret.ProdCodeStructMark3.Supply1.Trim(), "C26"));
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get " + EntityDbTable, e);
            }
            return ret;
        }


        public List<tblProdCodeStructMachEntity> GettblProdCodeStructMachs(IStatelessSession session = null, params string[] machcodes)
        {
            List<tblProdCodeStructMachEntity> ret = new List<tblProdCodeStructMachEntity>();

            if (session == null)
            {
                ret = BrokerDelegate(() => GettblProdCodeStructMachs(session, machcodes), ref session);
                return ret;
            }
            try
            {
                StringBuilder sbcode = new StringBuilder();
                foreach (var item in machcodes)
                {
                    sbcode.Append("'" + item.Trim() + "',");
                    //var n = GettblProdCodeStructMachMachCode(item.Trim(), session);
                    //if (n != null)
                    //    ret.Add(n);
                }
                string sql = string.Format(QUERYSQL_MULITI, sbcode.ToString().TrimEnd(','));
                //var tmp = ReadList<MESSystemWhoolsEntity>(session, string.Format("FROM {0} where  ID = {1} AND ISDELETED={2} orderby ID desc", EntityDbTable, Id, Constants.SNTONConstants.DeletedTag.NotDeleted));
                var tmp = ReadSqlList<tblProdCodeStructMachEntity>(session, sql, null);

                if (tmp != null || tmp.Count != 0)
                {
                    foreach (var item in machcodes)
                    {
                        var t = tmp.FindAll(x => x.MachCode == item)?.OrderByDescending(x => x.StartDate).FirstOrDefault();
                        if (t != null)
                            ret.Add(t);
                    }
                }
                if (ret.Any())
                {
                    StringBuilder s = new StringBuilder();
                    foreach (var item in ret)
                    {
                        if (!s.ToString().Contains(item.StructBarCode.Trim()))
                            s.Append("'" + item.StructBarCode.Trim() + "',");
                    }
                    var list = ReadSqlList<tblProdCodeStructMarkEntity>(null, string.Format(QUERYSQL_StructMark, s.ToString().TrimEnd(','), "C06"));
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in list)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(item.Supply1))
                                continue;
                            if (!sb.ToString().Trim(',').Contains(item.Supply1.Trim()))
                                sb.Append("'" + item.Supply1.Trim() + "',");
                            if (!string.IsNullOrWhiteSpace(item.Supply2) && !sb.ToString().Trim(',').Contains(item.Supply2.Trim()))
                                sb.Append("'" + item.Supply2.Trim() + "',");
                        }
                        catch (Exception ex)
                        {
                            logger.ErrorMethod("单丝作业标准书未绑定", ex);
                        }
                    }
                    string sqlc06 = @"select B.StructBarCode,B.ProdCode,B.ProdLength,P.Const,C.CName,B.[TitleProdName],B.Supply1,B.SupplyQty1,B.Supply2,B.SupplyQty2,B.SpoolType from tblProdCodeStructMark B 
Inner Join tblCommon C on C.CodeID = B.SpoolType
LEFT JOIN[tblProdCode] P ON B.ProdCode = P.ProdCode where C.GroupID IN ({1}) AND B.StructBarCode IN ({0})";
                    var list4 = ReadSqlList<tblProdCodeStructMarkEntity>(null, string.Format(sqlc06, sb.ToString().TrimEnd(','), "'C26','C06'"));
                    foreach (var item in ret)
                    {
                        item.ProdCodeStructMark3 = list.FirstOrDefault(x => x.StructBarCode.Trim() == item.StructBarCode.Trim());
                        if (item.ProdCodeStructMark3 != null)
                        {
                            try
                            {
                                if (item.ProdCodeStructMark3.Supply1 == null)
                                    continue;
                                item.ProdCodeStructMark4 = list4?.FirstOrDefault(x => x.StructBarCode.Trim() == item.ProdCodeStructMark3.Supply1.Trim());
                            }
                            catch (Exception ex)
                            {
                                logger.ErrorMethod("单丝作业标准书未绑定", ex);
                                continue;
                            }
                            if (item.ProdCodeStructMark4 != null)
                            {
                                item.ProdCodeStructMark4.Count = item.ProdCodeStructMark3.SupplyQty1;
                                item.ProdCodeStructMarks.Add(item.ProdCodeStructMark4);
                            }
                            if (!string.IsNullOrWhiteSpace(item.ProdCodeStructMark3.Supply2))
                            {
                                var ex = list4?.FirstOrDefault(x => x.StructBarCode.Trim() == item.ProdCodeStructMark3.Supply2.Trim());
                                if (ex != null)
                                {
                                    ex.Count = item.ProdCodeStructMark3.SupplyQty2;
                                    item.ProdCodeStructMarks.Add(ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get " + EntityDbTable, e);
            }
            return ret;
        }

        public tblProdCodeStructMarkEntity GettblProdCodeStructMachStructBarCode(string StructBarCode, IStatelessSession session = null)
        {
            var list = ReadSql<tblProdCodeStructMarkEntity>(null, string.Format(QUERYSQL_StructMark, "'" + StructBarCode.Trim() + "'", "C06"));
            return list;
        }
    }
}
