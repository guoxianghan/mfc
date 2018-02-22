using NHibernate;
using SNTON.Entities.DBTables.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.AGV
{
    public interface IAGVStatus
    {
        List<AGVStatusEntity> GetAllAGVStatus(IStatelessSession session);
        Dictionary<long, AGVStatusEntity> _DicAGVStatus { get; set; }
        void AddAGVStatus(List<AGVStatusEntity> list, IStatelessSession session = null);
        void SaveAGVStatus(IStatelessSession session = null, params AGVStatusEntity[] list);
    }
}
