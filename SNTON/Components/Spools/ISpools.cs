using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Entities.DBTables.Spools;
using NHibernate;

namespace SNTON.Components.Spools
{
    public interface ISpools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        SpoolsEntity GetSpoolByBarcode(string barcode, IStatelessSession theSession);

        /// <summary>
        /// Get Spools By Mid storage Id
        /// </summary>
        /// <param name="midStorageId"></param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        SpoolsEntity GetSpoolByMidStorageId(int midStorageId, IStatelessSession theSession);
        List<SpoolsEntity> GetSpoolByBarcodes(IStatelessSession theSession, params string[] barcode);
        /// <summary>
        /// Get Spools by AGV Id
        /// </summary>
        /// <param name="agvId"></param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        IList<SpoolsEntity> GetSpoolsByAGVId(int agvId, IStatelessSession theSession);
        List<SpoolsEntity> GetSpoolsByProudctType(string proudctType, IStatelessSession theSession);
        List<SpoolsEntity> GetSpoolsByTaskNo(string taskno, IStatelessSession session);
        void UpdateSpools(IStatelessSession session, params SpoolsEntity[] SpoolsID);
        SpoolsEntity GetSpoolBysqlwhere(string sql, IStatelessSession session);
        List<SpoolsEntity> GetSpoolsBysqlwhere(string sql, IStatelessSession session);
        /// <summary>
        /// 设置轮子的完成抓取的状态
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        int FinishedSpool(IStatelessSession session = null);
        int Add(SpoolsEntity entity, IStatelessSession session = null);
        int AddRange(List<SpoolsEntity> entities, IStatelessSession session = null);
        long GetSpoolID(string barcode, IStatelessSession theSession);

    }
}
