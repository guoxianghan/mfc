using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Entities.DBTables.MidStorage;
using NHibernate;

namespace SNTON.Components.MidStorage
{
    public interface IMidStorage
    {
        /// <summary>
        /// Get Mid storage By Area
        /// </summary>
        /// <param name="area">0 = all, 1, 2, 3, 4</param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        List<MidStorageEntity> GetMidStorageByArea(short area, IStatelessSession theSession);
        MidStorageEntity GetMidStorageById(int id, IStatelessSession session);
        List<MidStorageEntity> GetMidStorages(string sql, IStatelessSession session);
        int UpdateMidStore(IStatelessSession session, params MidStorageSpoolsEntity[] mids);
        /// <summary>
        /// 获取暂存库有轮子的位置
        /// </summary>
        /// <param name="area"></param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        List<MidStorageEntity> GetMidStoragePosOccupiedByArea(short area, IStatelessSession theSession);

        /// <summary>
        /// Get the Spool Id in the Mid Storage location which is occupied
        /// </summary>
        /// <param name="midStorageId"></param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        IList<long> GetSpoolIdListInMidStoragePosOccuiped(int midStorageId, IStatelessSession theSession);
        MidStorageEntity GetMidStorage(string sql, IStatelessSession session);
        int ClearMidStoreNo(IStatelessSession session, int storeageareaid, params int[] ids);
        int UpdateMidStore(IStatelessSession session, params MidStorageEntity[] mids);
        List<MidStorageSpoolsCountEntity> GetMidStorageAccount(IStatelessSession session);
        List<MidStorageSpoolsCountEntity> MidStorageAccountCache { get; set; }
    }
}
