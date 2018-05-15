using SNTON.Entities.DBTables.MES;
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
    public class EquipTask5Entity : EntityBase
    {

        /// <summary>
        /// 设备编号，科捷通过二维码系统获取此设备需要原料
        /// </summary>
        [DataMember]
        public virtual string DeviceID { get; set; }

        /// <summary>
        /// 输送线出口编号。0-不指定出口，由系统自动分配。1/2/3/4-指定出口
        /// </summary>
        [DataMember]
        public virtual byte ConveyorId { get; set; }

        /// <summary>
        /// -1：（AGV）预备任务0：（AGV）新任务1：(科捷)输送线已接收（有库存,准备出库）2：（AGV）接收确认3：(科捷)正在出库4：(科捷)出库完成5：（AGV）出库完成确认6：（科捷）缓存到位7：（AGV）正在取货8：(科捷)任务完成9：（AGV）完成确认（删除）
        /// </summary>
        [DataMember]
        public virtual byte Status { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        [DataMember]
        public virtual DateTime issuetime { get; set; }

        /// <summary>
        /// time_0
        /// </summary>
        [DataMember]
        public virtual DateTime time_0 { get; set; }

        /// <summary>
        /// time_1
        /// </summary>
        [DataMember]
        public virtual DateTime time_1 { get; set; }

        /// <summary>
        /// time_2
        /// </summary>
        [DataMember]
        public virtual DateTime time_2 { get; set; }

        /// <summary>
        /// time_3
        /// </summary>
        [DataMember]
        public virtual DateTime time_3 { get; set; }

        /// <summary>
        /// time_4
        /// </summary>
        [DataMember]
        public virtual DateTime time_4 { get; set; }

        /// <summary>
        /// time_5
        /// </summary>
        [DataMember]
        public virtual DateTime time_5 { get; set; }

        /// <summary>
        /// time_6
        /// </summary>
        [DataMember]
        public virtual DateTime time_6 { get; set; }

        /// <summary>
        /// time_7
        /// </summary>
        [DataMember]
        public virtual DateTime time_7 { get; set; }

        /// <summary>
        /// time_8
        /// </summary>
        [DataMember]
        public virtual DateTime time_8 { get; set; }
        /// <summary>
        /// time_8
        /// </summary>
        [DataMember]
        public virtual DateTime time_9 { get; set; }

        /// <summary>
        /// PlatNo
        /// </summary>
        [DataMember]
        public virtual byte PlatNo { get; set; }
        /// <summary>
        /// outOfStock 是否有库存默认0, 没库存1
        /// </summary>
        [DataMember]
        public virtual byte outOfStock { get; set; }

        /// <summary>
        /// StorageArea
        /// </summary>
        [DataMember]
        public virtual byte StorageArea { get; set; }

        /// <summary>
        /// SeqNo
        /// </summary>
        [DataMember]
        public virtual byte SeqNo { get; set; }

        /// <summary>
        /// TaskGuid
        /// </summary>
        [DataMember]
        public virtual Guid TaskGuid { get; set; }

        /// <summary>
        /// 单丝作业标准书数量
        /// </summary> 
        public virtual byte SupplyQty1 { get; set; }
        /// <summary>
        /// 单丝作业标准书编号
        /// </summary> 
        public virtual string Supply2 { get; set; }
        /// <summary>
        /// 单丝作业标准书编号
        /// </summary> 
        public virtual string TitleProdName { get; set; }
        /// <summary>
        /// 单丝作业标准书数量
        /// </summary> 
        public virtual byte SupplyQty2 { get; set; }
        /// <summary>
        /// 需要的单丝长度
        /// </summary> 
        public virtual int Length2 { get; set; }

        /// <summary>
        /// 作业标准书及长度相关
        /// </summary>
        public virtual tblProdCodeStructMachEntity tblProdCodeStructMach { get; set; }

    }
}