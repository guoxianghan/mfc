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
        MachineWarnningCodeEntity GetMachineWarnningCodeEntityByID(long id, IStatelessSession session = null);
        List<MachineWarnningCodeEntity> GetAllMachineWarnningCodeEntity(IStatelessSession session = null);
        List<MachineWarnningCodeEntity> MachineWarnningCache { get; set; }
        int UpdateWarning(List<MachineWarnningCodeEntity> list, IStatelessSession session = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="midstoreno">ÔÝ´æ¿âºÅ</param>
        /// <param name="machecode">1ÁúÃÅ£¬1ÏßÌå</param>
        /// <param name="session"></param>
        /// <returns></returns>
        bool ResetWarning(byte midstoreno,byte machecode, IStatelessSession session = null);
    }
}

