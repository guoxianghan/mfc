using NHibernate;
using SNTON.Entities.DBTables.RobotArmTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.RobotArm
{
    public interface IRobotArmTask
    {
        RobotArmTaskEntity GetRobotArmTaskEntityByID(long id, IStatelessSession session = null);
        List<RobotArmTaskEntity> GetAllRobotArmTaskEntity(IStatelessSession session = null);
        List<RobotArmTaskEntity> GetRobotArmTasks(string sqlwhere, IStatelessSession session = null);
        RobotArmTaskEntity GetRobotArmTask(string sqlwhere, IStatelessSession session = null);
        void CreateArmTask(RobotArmTaskEntity entity, IStatelessSession session = null);
        void InsertArmTask(List<RobotArmTaskEntity> lst, IStatelessSession session = null);
        bool UpdateArmTask(RobotArmTaskEntity entity, IStatelessSession session = null);
        void UpdateArmTasks(List<RobotArmTaskEntity> lst, IStatelessSession session = null);
        void UpdateArmTaskStatus(Guid guid, int status, IStatelessSession session = null);
        int SetArmTaskDelete(IStatelessSession session = null, params long[] ids);
        bool SetArmTasksUnitStatus(Guid guid, IStatelessSession session = null);
    }
}

