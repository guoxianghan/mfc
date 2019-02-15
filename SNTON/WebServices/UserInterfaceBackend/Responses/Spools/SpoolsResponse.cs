using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.WebServices.UserInterfaceBackend.Models;

namespace SNTON.WebServices.UserInterfaceBackend.Responses
{
    public class SpoolsResponse: ResponseBase
    {
        /// <summary>
        /// 工字轮数据的列表
        /// </summary>
        public IList<SpoolDataUI> data;

        public SpoolsResponse()
        {
            data = new List<SpoolDataUI>();
        }
    }
}
