using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
    public interface IEquipTask5
    {
        EquipTask5Entity GetEquipTask5EntityByID(long id, IStatelessSession session = null);
        List<EquipTask5Entity> GetAllEquipTask5Entity(IStatelessSession session = null);
        List<EquipTask5Entity> GetEquipTask5(string sqlwhere, IStatelessSession session = null);
        int UpdateEquipTask5(IStatelessSession session = null, params EquipTask5Entity[] tasks);
    }
}

