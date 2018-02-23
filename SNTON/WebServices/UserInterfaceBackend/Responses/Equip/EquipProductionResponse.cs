using SNTON.WebServices.UserInterfaceBackend.Models.Equip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.Equip
{
   public class EquipProductionResponse:ResponseBase
    {
        public List<EquipProductionDataUI> data { get; set; } = new List<EquipProductionDataUI>();
    }
}
