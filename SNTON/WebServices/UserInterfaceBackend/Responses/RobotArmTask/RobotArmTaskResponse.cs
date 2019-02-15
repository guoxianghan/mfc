using SNTON.WebServices.UserInterfaceBackend.Models.RobotArmTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.RobotArmTask
{
    public class RobotArmTaskResponse : ResponseBase
    {
        public List<RobotArmTaskDataUI> data { get; set; } = new List<RobotArmTaskDataUI>();
    }
}
