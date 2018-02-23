using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Equipments
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class EquipTaskEntity : EntityBase
    {

        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual long EquipContollerId { get; set; }

        /// <summary>
        /// 设备任务类型:1拉空轮;2拉满轮
        /// </summary>
        [DataMember]
        public virtual byte TaskType { get; set; }

        /// <summary>
        /// 需要的单丝长度
        /// </summary>
        [DataMember]
        public virtual int Length { get; set; }
        /// <summary>
        /// 0初始化EquipTask,1创建AGVTask和龙门Task,2正在抓取,3,抓取完毕,4等待调度AGV,5已调度AGV,6AGV运行中(等待送料或拉空轮),7任务完成(拉空论或满轮),8任务失败,9已通知地面滚筒创建任务,10库里单丝不够
        /// </summary>
        [DataMember]
        public virtual byte Status { get; set; }
        /// <summary>
        /// Source
        /// </summary>
        [DataMember]
        public virtual byte Source { get; set; }
        /// <summary>
        /// AGV级别
        /// </summary>
        [DataMember]
        public virtual byte TaskLevel { get; set; }
        /// <summary>
        /// 规格 WS188/WS144等
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; }
        /// <summary>
        /// 单丝作业标准书编号
        /// </summary>
        [DataMember]
        public virtual string Supply1 { get; set; }
        /// <summary>
        /// PlantNo
        /// </summary> 
        public virtual byte PlantNo { get; set; }
        /// <summary>
        /// 与RobotArmTask和AGV关联的GUID
        /// </summary>
        [DataMember]
        public virtual Guid TaskGuid { get; set; }
    }
}