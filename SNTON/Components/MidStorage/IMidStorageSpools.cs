using NHibernate;
using SNTON.Entities.DBTables.MidStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.MidStorage
{
    public interface IMidStorageSpools
    {
        /// <summary>
        /// Get Mid storage By Area
        /// </summary>
        /// <param name="area">0 = all, 1, 2, 3, 4</param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        List<MidStorageSpoolsEntity> GetMidStorageByArea(short area, IStatelessSession theSession);
        MidStorageSpoolsEntity GetMidStorageById(int storageid, int id, IStatelessSession session);
        List<MidStorageSpoolsEntity> GetMidStorages(string sql, IStatelessSession session);
        int UpdateMidStore(IStatelessSession session, params MidStorageSpoolsEntity[] mids);
        //void UpdateMidStore(IStatelessSession session, MidStorageSpoolsEntity[] midStorageSpoolsEntity);
    }
}
