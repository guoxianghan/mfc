using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.WebServices.UserInterfaceBackend.Models;

namespace SNTON.WebServices.UserInterfaceBackend.Responses
{
    public class SystemParametersResponse: ResponseBase
    {
        /// <summary>
        /// 系统参数列表信息
        /// </summary>
        public IList<SystemParametersUI> data;

        public SystemParametersResponse()
        {
            data = new List<SystemParametersUI>();
        }
    }
}
