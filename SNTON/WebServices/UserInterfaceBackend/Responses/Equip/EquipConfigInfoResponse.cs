using SNTON.WebServices.UserInterfaceBackend.Models.Equip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses
{
    public class EquipConfigInfoResponse : ResponseBase
    {
        public List<EquipInfoDataUI> data { get; set; } = new List<EquipInfoDataUI>();
    }
     
}
