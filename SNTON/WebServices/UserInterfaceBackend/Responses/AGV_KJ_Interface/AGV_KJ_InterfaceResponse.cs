using SNTON.WebServices.UserInterfaceBackend.Models.AGV_KJ_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.AGV_KJ_Interface
{
    public class AGV_KJ_InterfaceResponse : ResponseBase
    {
        public List<AGV_KJ_InterfaceDataUI> data { get; set; } = new List<AGV_KJ_InterfaceDataUI>();
    }
}
