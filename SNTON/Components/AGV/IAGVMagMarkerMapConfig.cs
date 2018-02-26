using NHibernate;
using SNTON.Entities.DBTables.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.AGV
{
    public interface IAGVMagMarkerMapConfig
    {        
	    AGVMagMarkerMapConfigEntity GetAGVMagMarkerMapConfigEntityByID(long id, IStatelessSession session);
        List<AGVMagMarkerMapConfigEntity> GetAllAGVMagMarkerMapConfigEntity(IStatelessSession session);
    }
}

