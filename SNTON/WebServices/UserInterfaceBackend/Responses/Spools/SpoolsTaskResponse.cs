using SNTON.WebServices.UserInterfaceBackend.Models.Spool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.Spools
{
    public class SpoolsTaskResponse : ResponseBase
    {  
        /// <summary>
       /// 消息总共的页数
       /// </summary>
        public int pageCount { get; set; }
        /// <summary>
        /// 总条数
        /// </summary>
        public int CountNumber { get; set; }
        public IList<SpoolTaskDataUI> data { get; set; } = new List<SpoolTaskDataUI>();
    }
}
