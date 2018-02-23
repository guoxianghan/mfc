using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
    public interface IEquipStatus
    {
        EquipStatusEntity GetEquipStatusEntityByID(long id, IStatelessSession session);
        EquipStatusEntity GetEquipStatusEntityByEquipID(long equipid, IStatelessSession session);
        List<EquipStatusEntity> GetEquipStatusEntityByStatus(byte status, IStatelessSession session);
        List<EquipStatusEntity> GetAllEquipStatusEntity(IStatelessSession session);

    }
}
