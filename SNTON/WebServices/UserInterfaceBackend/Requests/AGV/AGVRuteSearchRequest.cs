using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Requests.AGV
{
    /// <summary>
    /// 查询小车的历史轨迹
    /// </summary>
    public class AGVRuteSearchRequest
    {
        /// <summary>
        /// Message search start time
        /// </summary>
        public DateTime startTime;
        public DateTime endTime;
        /// <summary>
        /// 小车ID
        /// </summary>
        public int agvid;
        /// <summary>
        /// 总条数
        /// </summary>
        public int pageindex;
        /// <summary>
        /// 每页数量
        /// </summary>
        public int pagesize;
    }
}
