using NHibernate;
using SNTON.Entities.DBTables.InStoreToOutStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.InStoreToOutStore
{
    public interface IInStoreToOutStoreSpoolView
    {
        InStoreToOutStoreSpoolViewEntity GetInStoreToOutStoreSpoolEntityByID(long id, IStatelessSession session = null);
        List<InStoreToOutStoreSpoolViewEntity> GetAllInStoreToOutStoreSpoolEntity(IStatelessSession session = null);
        List<InStoreToOutStoreSpoolViewEntity> GetInStoreToOutStoreSpoolEntity(string sql, IStatelessSession session = null);
        int UpdateEntity(IStatelessSession session, params InStoreToOutStoreSpoolViewEntity[] entity);
        int AddEntity(IStatelessSession session, params InStoreToOutStoreSpoolEntity[] entity);
        List<InStoreToOutStoreSpoolViewEntity> GetInStoreToOutStoreSpool(int storeageno, int plantno, IStatelessSession session);
    }
}

