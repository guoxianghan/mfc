using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
    public interface IEquipCommand
    {
        List<EquipCommandEntity> _AllEquipCommandList { get; set; }
        List<EquipCommandEntity> GetAllEquipCommand(IStatelessSession session);
    }
}
