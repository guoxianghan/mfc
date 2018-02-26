using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.Equipments;

namespace SNTON.Components.Equipment
{
    public interface IEquipConfig
    {

        /// <summary>
        /// Get equipment congfiguration by plant no
        /// </summary>
        /// <param name="plantNo"></param>
        /// <param name="theSession"></param>
        /// <returns></returns>
        List<EquipConfigEntity> GetEquipConfigByPlantNo(short plantNo, IStatelessSession theSession);

        EquipConfigEntity GetEquipConfigById(long id, IStatelessSession session);

        List<EquipConfigEntity> GetEquipConfigBySqlWhere(string sqlwhere, IStatelessSession session);
        List<EquipConfigEntity> _AllEquipConfigList { get; set; }
        Dictionary<long, EquipConfigEntity> _AllEquipConfigDic { get; set; }
        int UpdateEquipConfig( IStatelessSession session,params EquipConfigEntity[] entities);
    }
}
