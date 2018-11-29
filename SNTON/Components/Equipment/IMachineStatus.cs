using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
    public interface IMachineStatus
    {        
	    MachineStatusEntity GetMachineStatusEntityByID(long id, IStatelessSession session=null);
        List<MachineStatusEntity> GetAllMachineStatusEntity(IStatelessSession session=null);
    }
}

