using SNTON.WebServices.UserInterfaceBackend.Models.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.AGV
{
    public class AGVConfigResponse : ResponseBase
    {
        public List<AGVConfigUI> data { get; set; } = new List<AGVConfigUI>();
    }
    public class AGVDetailResponse : ResponseBase
    {
       public AGVDetailUI data { get; set; }
    }
    public class AGVConfigInfoResponse : ResponseBase
    {
        public List<AGVConfigInfoUI> data { get; set; } = new List<AGVConfigInfoUI>();
    }
}
