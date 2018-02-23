using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
    public interface IEquipConfiger
    {
        List<EquipConfigerEntity> EquipConfigers { get; set; }
        List<EquipConfigerEntity> GetEquipConfigerEntities(IStatelessSession session);
    }
}
