using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.AGV
{
    public class AGVRouteArchiveUI
    {
        public string x;
        public string y;
        public int agvid;
        public int index;
    }
    public class AGVRouteDataUI
    {
        /// <summary>
        /// 实时坐标,毫米
        /// </summary>
        public float x;
        public float y;
        /// <summary>
        /// 1拉空轮;2拉满轮
        /// </summary>
        public int TaskType;
        /// <summary>
        /// 
        /// </summary>
        public int agvid;
        public long id;
        public DateTime CreateTime;
        public byte Status { get; set; }
        public int agv_id { get; set; }

        ///<summary> 
        /// fac_x
        /// </summary> 
        public float fac_x { get; set; }
        ///<summary>
        /// fac_y
        /// </summary> 
        public float fac_y { get; set; }
    }
}
