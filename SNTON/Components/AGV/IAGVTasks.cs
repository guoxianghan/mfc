using NHibernate;
using SNTON.Entities.DBTables.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SNTON.Constants.SNTONConstants;

namespace SNTON.Components.AGV
{
    public interface IAGVTasks
    {
        AGVTasksEntity GetAGVTaskEntityById(long id, IStatelessSession session = null);
        AGVTasksEntity GetAGVTaskEntityByTaskNo(long taskno, IStatelessSession session = null);
        AGVTasksEntity GetAGVTaskEntityByTaskNo(long taskno,int equiptskstatus, IStatelessSession session = null);
        List<AGVTasksEntity> GetAllAGVTaskEntity(IStatelessSession session = null);
        void CreateAGVTask(AGVTasksEntity entity, IStatelessSession session = null);
        void SaveAGVTaskStatus(long taskno, byte status, IStatelessSession session = null);
        void UpdateEntity(AGVTasksEntity entity, IStatelessSession session = null);
        int UpdateEntity(IStatelessSession session, params AGVTasksEntity[] entities);
        List<AGVTasksEntity> GetAGVTasks(string sqlwhere, IStatelessSession session = null);
        List<AGVTasksEntity> GetAGVTasks(int top ,string sqlwhere, IStatelessSession session = null);
        AGVTasksEntity GetAGVTask(string sqlwhere, IStatelessSession session = null);
        bool UpdateStatus(long id, int status, IStatelessSession session = null);
        int Insert(IStatelessSession session, params AGVTasksEntity[] agvs);
    }
}

