using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.RobotArmTask
{
    /// <summary>
    /// DB table entity class
    /// </summary> 
    public class RobotArmTaskEntity : EntityBase
    {

        /// <summary>
        /// 任务类型:0从暂存库到出库线;1从暂存到直通线;2从直通线到暂存库(入库);3从直通线到异常口;4从暂存库到异常口
        /// </summary>
        [DataMember]
        public virtual int TaskType { get; set; }
        /// <summary>
        /// 线体任务流水号
        /// </summary>
        [DataMember]
        public virtual int AGVSeqNo { get; set; }

        /// <summary>
        /// 任务完成时间
        /// </summary>
        [DataMember]
        public virtual DateTime? Completed { get; set; }

        /// <summary>
        /// 龙门任务单元的工作状态:-1失效;0创建;1线体准备好,准备抓取;2正在抓取;3抓取完毕;4等待AGV接收;5AGV接收完毕;6抓取失败;7接收失败;8任务失败;9等待线体响应允许小车来接
        /// </summary>
        [DataMember]
        public virtual int TaskStatus { get; set; }

        /// <summary>
        /// 暂存库位置 库位号
        /// </summary>
        [DataMember]
        public virtual int FromWhere { get; set; }

        /// <summary>
        /// 任务状态:-1失效;0创建;1正在抓取;2抓取完毕;3等待AGV接收;4AGV接收完毕;5抓取失败;6接收失败;7任务失败
        /// </summary>
        [DataMember]
        public virtual int SpoolStatus { get; set; }
        /// <summary>
        /// 抓到哪个线体上 1正常出库线;2直通口
        /// 常量值:MidStoreLine
        /// </summary>
        [DataMember]
        public virtual int ToWhere { get; set; }

        /// <summary>
        /// 任务优先级1,2,3,4,5,6抓到直通线,7入库,8异常口出库
        /// </summary>
        [DataMember]
        public virtual int TaskLevel { get; set; }

        /// <summary>
        /// 龙门id
        /// </summary>
        [DataMember]
        public virtual string RobotArmID { get; set; }

        /// <summary>
        /// 任务组标识
        /// </summary>
        [DataMember]
        public virtual Guid TaskGroupGUID { get; set; }

        /// <summary>
        /// 叫料的按钮的ID(线体位置)
        /// </summary>
        [DataMember]
        public virtual string EquipControllerId { get; set; }
        [DataMember]
        public virtual byte PlantNo { get; set; }
        /// <summary>
        ///  获取轮子型号对应的线体编码 1(8个);2(12个)
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; }
        [DataMember]
        public virtual string WhoolBarCode { get; set; }
        [DataMember]
        public virtual string CName { get; set; }
        /// <summary>
        /// 龙门任务顺序
        /// </summary>
        [DataMember]
        public virtual int SeqNo { get; set; }
        [DataMember]
        public virtual int StorageArea { get; set; }
        /// <summary>
        /// 单丝流水号
        /// </summary>
        [DataMember]
        public virtual int SpoolSeqNo { get; set; }
    }
}