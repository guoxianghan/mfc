using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
    public interface IEquipConfiger2
    {
          List<EquipConfiger2Entity> AllEquipConfiger2 { get; set; }
          List<EquipConfiger2Entity> GetEquipConfiger2(IStatelessSession session);
    }
}
