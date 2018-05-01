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
        /// <summary>
        /// X
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
        /// </summary>
        [DataMember]
        public virtual byte Status { get; set; }

        /// <summary>
        /// Y
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