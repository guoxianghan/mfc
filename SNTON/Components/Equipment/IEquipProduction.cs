using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using SNTON.WebServices.UserInterfaceBackend.Models;
using SNTON.WebServices.UserInterfaceBackend.Models.Equip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
  public  interface IEquipProduction
    {
        /// <summary>
        /// Save equipment production
        /// </summary>
        /// <param name="equipProduction"></param>
        /// <param name="theSession"></param>
        void SaveEquipProduction(IList<EquipProductionEntity> equipProduction, IStatelessSession theSession);
        IList<EquipProductionEntity> GetEquipProductionsByEquipID(long equipid, IStatelessSession session);
        IList<EquipProductionEntity> GetEquipProductionsByGroupID(long groupid, IStatelessSession session);
        IList<EquipProductionEntity> GetEquipProductionsByProductType(string producttype, IStatelessSession session);
        IList<EquipProductionEntity> GetEquipProductionsByOperator(string oper, IStatelessSession session);
        IList<EquipProductionEntity> GetEquipProductionsSearch(EquipProductionSearchRequest search, IStatelessSession session);
        void SaveEquipProductionList(List<EquipProductionDataUI> list, string oper,IStatelessSession session=null);
        void UpdateEquipProduction(EquipProductionDataUI product , string oper,IStatelessSession session);
        List<EquipProductionEntity> GetAllEquipProductionEntity(IStatelessSession session = null);
        List<EquipProductionEntity> _AllEquipProductionEntityList { get; set; }
    }
}
