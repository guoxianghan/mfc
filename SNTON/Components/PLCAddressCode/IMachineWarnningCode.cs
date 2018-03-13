using NHibernate;
using SNTON.Entities.DBTables.PLCAddressCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.PLCAddressCode
{
    public interface IMachineWarnningCode
    {        
	    MachineWarnningCodeEntity GetMachineWarnningCodeEntityByID(long id, IStatelessSession session=null);
        List<MachineWarnningCodeEntity> GetAllMachineWarnningCodeEntity(IStatelessSession session=null);
        List<MachineWarnningCodeEntity> MachineWarnningCodes { get; set; }
    }
}

