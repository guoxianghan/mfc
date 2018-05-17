using NHibernate;
using SNTON.Entities.DBTables.AGV;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.InStoreToOutStore;
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
        bool OutStoreageTask(List<EquipTaskViewEntity> updateequiptsks, List<MidStorageSpoolsEntity> updatemids, AGVTasksEntity insertagvtsk, List<RobotArmTaskEntity> insetarmtsks, List<InStoreToOutStoreSpoolEntity> outspools, IStatelessSession session = null);
        /// <summary>
        /// 清空直通线入库任务
        /// </summary>
        /// <param name="armtsks"></param>
        /// <param name="mids"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        bool ClearInStoreageLine(List<RobotArmTaskSpoolEntity> armtsks, List<MidStorageEntity> mids, IStatelessSession session = null);

        bool ExceptionRobotArmTask(List<RobotArmTaskEntity> armtsks, List<MidStorageEntity> mids, IStatelessSession session = null);
        bool ClearInStoreToOutStoreLine(List<MidStorageEntity> updatemids, AGVTasksEntity updateagvtsk, List<RobotArmTaskEntity> updatearmtsks, List<InStoreToOutStoreSpoolEntity> updateoutspools, IStatelessSession session = null);

        /// <summary>
        /// 创建直通口出库任务,并关联相应的机台
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="agvtsk"></param>
        /// <param name="updateequiptsks"></param>
        /// <returns></returns>
        bool InStoreToOutStoreLine(List<InStoreToOutStoreSpoolViewEntity> instoreoutstore, AGVTasksEntity agvtsk, List<EquipTaskViewEntity> updateequiptsks, IStatelessSession session = null);
        bool CreateEquipTask5(EquipTask5Entity task, List<RobotArmTaskEntity> armtsks, List<MidStorageSpoolsEntity> mids, IStatelessSession session = null);
    }
}
