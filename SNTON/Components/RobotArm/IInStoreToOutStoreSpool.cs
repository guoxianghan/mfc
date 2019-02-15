using NHibernate;
using SNTON.Entities.DBTables.RobotArmTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.RobotArm
{
    public interface IInStoreToOutStoreSpool
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeageno">暂存库号</param>
        /// <param name="plantno">车间号</param>
        /// <returns></returns>
        List<InStoreToOutStoreSpoolEntity> GetInStoreToOutStoreSpool(int storeageno, int plantno, IStatelessSession session);
        List<InStoreToOutStoreSpoolEntity> GetAllInStoreToOutStoreSpool(IStatelessSession session);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeageno">暂存库号</param>
        /// <param name="plantno">车间号</param>
        /// <returns></returns>
        void SaveInStoreToOutStoreSpool(int storeageno, int plantno, List<InStoreToOutStoreSpoolEntity> list, IStatelessSession session);
    }
}
