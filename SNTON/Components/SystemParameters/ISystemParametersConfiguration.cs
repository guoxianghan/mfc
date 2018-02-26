using NHibernate;
using SNTON.Entities.DBTables.SystemParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.SystemParameters
{
    public interface ISystemParametersConfiguration
    {        
	    SystemParametersConfigurationEntity GetSystemParametersConfigurationEntityByID(long id, IStatelessSession session);
        
        List<SystemParametersConfigurationEntity> GetAllSystemParametersConfigurationEntity(IStatelessSession session);
    }
}

