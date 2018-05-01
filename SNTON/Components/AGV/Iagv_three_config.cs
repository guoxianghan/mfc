using NHibernate;
using SNTON.Entities.DBTables.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.AGV
{
    public interface IAgv_three_config
    {
        agv_three_configEntity GetAgv_three_configEntityByID(long id, IStatelessSession session = null);
        List<agv_three_configEntity> GetAllAgv_three_configEntity(IStatelessSession session = null);
        List<agv_three_configEntity> _AllAgv_three_config { get; set; }
        /// <summary>
        /// ×ø±êÔ­µã
        /// </summary>
        agv_three_configEntity _originLocation { get; set; }
    }
}

