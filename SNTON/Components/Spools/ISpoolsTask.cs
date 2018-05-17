using NHibernate;
using SNTON.Entities.DBTables.Spools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Spools
{
    public interface ISpoolsTask
    {        
	    SpoolsTaskEntity GetSpoolsTaskEntityByID(long id, IStatelessSession session=null);
        List<SpoolsTaskEntity> GetAllSpoolsTaskEntity(IStatelessSession session=null);
        List<SpoolsTaskEntity> GetSpoolsTask(string where,IStatelessSession session= null);
        List<SpoolsTaskEntity> GetSpoolsTasks(string where,IStatelessSession session= null);
    }
}

