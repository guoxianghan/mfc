using NHibernate;
using SNTON.Entities.DBTables.AGV;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.MidStorage;
using SNTON.Entities.DBTables.RobotArmTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.SQLCommand
{
    public interface ISQLCommandBroker
    {
        bool RunSqlCommand(string[] sqlcmdlist, IStatelessSession session = null);
        /// <summary>
        /// 创建拉空轮任务
        /// </summary>
        /// <param name="equiptsks"></param>
        /// <param name="insertagvtsk"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        bool EmptyAGVTask(List<EquipTaskViewEntity> equiptsks, AGVTasksEntity insertagvtsk, IStatelessSession session = null);
        /// <summary>
        /// 创建拉满轮任务
        /// </summary>
        /// <param name="updateequiptsks"></param>
        /// <param name="updatemids"></param>
        /// <param name="insertagvtsk"></param>
        /// <param name="insetarmtsks"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        bool OutStoreageTask(List<EquipTaskViewEntity> updateequiptsks, List<MidStorageSpoolsEntity> updatemids, AGVTasksEntity insertagvtsk, List<RobotArmTaskEntity> insetarmtsks, IStatelessSession session = null);
        /// <summary>
        /// 清空直通线入库任务
        /// </summary>
        /// <param name="armtsks"></param>
        /// <param name="mids"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        bool ClearInStoreageLine(List<RobotArmTaskSpoolEntity> armtsks, List<MidStorageEntity> mids, IStatelessSession session = null);

        bool ExceptionRobotArmTask(List<RobotArmTaskEntity> armtsks, List<MidStorageEntity> mids, IStatelessSession session = null);
    }
}
