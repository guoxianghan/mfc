using NHibernate;
using SNTON.Entities.DBTables.MES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.MES
{
    public interface ItblProdCodeStructMark
    {
        /// <summary>
        /// 根据作业标准书编号获得具体规格工字轮型号,长度等详细信息
        /// </summary>
        /// <param name="StructBarCode"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        tblProdCodeStructMarkEntity GettblProdCodeStructMark(string StructBarCode, IStatelessSession session = null);
        /// <summary>
        /// 根据作业标准书编号获得具体规格工字轮型号,长度等详细信息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="StructBarCode"></param>
        /// <returns></returns>
        List<tblProdCodeStructMarkEntity> GettblProdCodeStructMarks(IStatelessSession session = null,params string[] StructBarCode);
    }
}
