using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
    public interface IEquipControllerConfig
    {

        List<EquipControllerConfigEntity> GetAllEquipControllerConfig(IStatelessSession session);
        List<EquipControllerConfigEntity> GetEquipControllerConfigByPlantNo(string plantno, IStatelessSession session);
        List<EquipControllerConfigEntity> GetEquipControllerConfigByCtlName(string controllername, IStatelessSession session);
        EquipControllerConfigEntity GetEquipControllerConfigById(long id, IStatelessSession session);
    }
}
