using NHibernate;
using SNTON.Entities.DBTables.AGV_KJ_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.AGV_KJ_Interface
{
    public interface IT_AGV_KJ_Interface
    {
        T_AGV_KJ_InterfaceEntity GetT_AGV_KJ_InterfaceEntityByID(long id, IStatelessSession session = null);
        List<T_AGV_KJ_InterfaceEntity> GetAllT_AGV_KJ_InterfaceEntity(IStatelessSession session = null);
        List<T_AGV_KJ_InterfaceEntity> GetT_AGV_KJ_Interface(string sql, IStatelessSession session = null);
        T_AGV_KJ_InterfaceEntity GetT_AGV_KJ_InterfaceEntity(string sql, IStatelessSession session = null);
        bool UpdateT_AGV_KJ_Interface(T_AGV_KJ_InterfaceEntity entity, IStatelessSession session = null);
    }
}

