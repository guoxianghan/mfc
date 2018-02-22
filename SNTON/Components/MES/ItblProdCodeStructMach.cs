using NHibernate;
using SNTON.Entities.DBTables.MES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.MES
{
    /// <summary>
    /// 车间机台号对应作业标准书的表
    /// </summary>
    public interface ItblProdCodeStructMach
    {
        List<tblProdCodeStructMachEntity> GettblProdCodeStructMachs(IStatelessSession session = null, params string[] machcodes);
        tblProdCodeStructMachEntity GettblProdCodeStructMach(string sql, IStatelessSession session = null);
        /// <summary>
        /// 条码
        /// </summary>
        /// <param name="FdTagNo"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        tblProdCodeStructMachEntity GettblProdCodeStructMachMachCode(string FdTagNo, IStatelessSession session = null);
    }
}
