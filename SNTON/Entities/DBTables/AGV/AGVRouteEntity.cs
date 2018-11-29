using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.AGV
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class AGVRouteEntity : EntityBase
    {

        /// <summary>
        /// AGVId
        /// </summary>
        [DataMember]
        public virtual short AGVId { get; set; }


        public virtual int agv_id { get; set; }
        ///<summary> 
        /// fac_x
        /// </summary>
        [DataMember]
        public virtual float fac_x { get; set; }
        ///<summary>
        /// fac_y
        /// </summary>
        [DataMember]
        public virtual float fac_y { get; set; }
        /// <summary>
        /// 实时坐标 毫米
        /// </summary>
        [DataMember]
        public virtual float X { get; set; }
        /// <summary>
        /// 小车状态
        /// 每一位对应一个小车的通讯状态值
        /// 每位上的值含义如下：
        /// 0=AGV处于待命状态
        /// 1=AGV处于空闲状态
        /// 2=AGV处于任务执行状态
        /// 3=AGV处于故障状态
        /// 彭天提供
        /// 未知 = 0,
        ///行驶中 = 1,
        ///故障已清 = 2,
        ///故障中 = 3,
        ///待机 = 4,
        ///排队中 = 5,
        ///手动运行中 = 6,
        ///矫正位姿中 = 7,
        ///准备 = 8,
        ///关机 = 9,
        ///暂停 = 10,
        ///开机 = 11,
        ///接收中 = 12,
        /// </summary>
        [DataMember]
        public virtual byte Status { get; set; }

        /// <summary>
        /// 实时坐标 毫米
        /// </summary>
        [DataMember]
        public virtual float Y { get; set; }

        /// <summary>
        /// Speed
        /// </summary>
        [DataMember]
        public virtual decimal Speed { get; set; }

    }
}