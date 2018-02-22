using SNTON.WebServices.UserInterfaceBackend.Models.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.AGV
{
    public class AGVTaskResponse : ResponseBase
    {
        public List<AGVTaskDataUI> data { get; set; } = new List<AGVTaskDataUI>();
    }
}
