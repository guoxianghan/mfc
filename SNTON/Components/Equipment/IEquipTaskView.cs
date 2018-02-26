using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
    public interface IEquipTaskView
    {
        List<EquipTaskViewEntity> GetEquipTaskViewEntities(string sql, IStatelessSession session);
        int Update(IStatelessSession session, params EquipTaskViewEntity[] equiptsks);
        int UpdateStatus(IStatelessSession session, byte status, long[] ids);
        List<EquipTaskViewEntity> GetEquipTaskViewNotDeleted(string sql, IStatelessSession session = null);
    }
}
