using SNTON.WebServices.UserInterfaceBackend.Models.Equip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses
{
    public class EquipConfigStatusResponse : ResponseBase
    {
        public List<EquipStatusDataUI> data { get; set; } = new List<EquipStatusDataUI>();
    }
    public class EquipTaskStatusResponse : ResponseBase
    {
        public List<EquipTaskStatusDataUI> data { get; set; } = new List<EquipTaskStatusDataUI>();
    }
    public class EquipCallInfoResponse : ResponseBase
    {
        public List<EquipCallDataUI> data { get; set; } = new List<EquipCallDataUI>();

    }
}
