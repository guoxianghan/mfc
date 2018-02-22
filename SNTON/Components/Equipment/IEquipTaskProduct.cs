using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
   public interface IEquipTaskProduct
    {
        List<EquipTaskProductEntity> GetEquipTaskProductEntityList(string sqlwhere, IStatelessSession theSession);
        EquipTaskProductEntity GetEquipTaskProductEntity(string sqlwhere, IStatelessSession theSession);
    }
}
