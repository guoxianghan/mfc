using NHibernate;
using SNTON.Entities.DBTables.RobotArmTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.RobotArm
{
    public interface IRobotArmTaskSpool
    {
        List<RobotArmTaskSpoolEntity> GetRobotArmTaskSpools(string sqlwhere, IStatelessSession session = null);
        byte UpdateArmTask(RobotArmTaskSpoolEntity entity, IStatelessSession session = null);
        int UpdateArmTask(List<RobotArmTaskSpoolEntity> entity, IStatelessSession session = null);
        RobotArmTaskSpoolEntity GetRobotArmTaskSpool(long id, IStatelessSession session = null);

    }
}
