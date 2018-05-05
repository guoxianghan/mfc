using NHibernate;
using SNTON.Entities.DBTables.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.AGV
{
    public interface IAGVBYSStatus
    {
        AGVBYSStatusEntity GetAGVBYSStatusEntityByID(long id, IStatelessSession session = null);
        List<AGVBYSStatusEntity> GetAllAGVBYSStatusEntity(IStatelessSession session = null);
        List<AGVBYSStatusEntity> _AGVBYSStatusCache { get; set; }
    }
}

