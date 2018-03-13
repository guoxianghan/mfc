using NHibernate;
using SNTON.Entities.DBTables.InStoreToOutStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.InStoreToOutStore
{
    public interface IInStoreToOutStoreSpool
    {        
	    InStoreToOutStoreSpoolEntity GetInStoreToOutStoreSpoolEntityByID(long id, IStatelessSession session=null);
        List<InStoreToOutStoreSpoolEntity> GetAllInStoreToOutStoreSpoolEntity(IStatelessSession session=null);
        List<InStoreToOutStoreSpoolEntity> GetInStoreToOutStoreSpoolEntity(string sql, IStatelessSession session = null);
        int UpdateEntity(IStatelessSession session, params InStoreToOutStoreSpoolEntity[] entity);
        int AddEntity(IStatelessSession session, params InStoreToOutStoreSpoolEntity[] entity);
    }
}

