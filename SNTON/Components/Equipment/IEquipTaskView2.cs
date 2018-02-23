using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
   public interface IEquipTaskView2
    {
        List<EquipTaskView2Entity> GetEquipTaskView2(string sql, IStatelessSession session);
    }
}
