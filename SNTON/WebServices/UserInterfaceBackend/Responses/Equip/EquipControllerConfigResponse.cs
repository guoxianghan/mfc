using SNTON.WebServices.UserInterfaceBackend.Models.Equip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.Equip
{
   public class EquipControllerConfigResponse:ResponseBase
    {
        public List<EquipControllerConfigDataUI> data { get; set; } = new List<EquipControllerConfigDataUI>();
    }
}
