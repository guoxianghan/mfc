using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.Equipments;

namespace SNTON.Components.Equipment
{
    public interface IEquipTask
    {
        void CreateEquipTask(int id, int TaskType, int PlantNo, IStatelessSession session);
        List<EquipTaskEntity> GetEquipTaskEntitySqlWhere(string sqlwhere, IStatelessSession session = null);
        List<EquipTaskEntity> GetEquipTaskEntityNotDeleted(string sqlwhere, IStatelessSession session = null);
        EquipTaskEntity GetEquipTaskEntityByID(long id, IStatelessSession session = null);
        bool UpdateEntity(EquipTaskEntity entity, IStatelessSession session = null);
        bool CreateEquipTask(EquipTaskEntity entity, IStatelessSession session = null);
        int UpdateEntity(List<EquipTaskEntity> entities, IStatelessSession session = null);
        bool UpdateStatus(int status, IStatelessSession session = null, params long[] ids);
    }
}
