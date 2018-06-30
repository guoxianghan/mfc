using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SNTON.Entities.DBTables.MES;

namespace SNTON.Entities.DBTables.AGV_KJ_Interface
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class T_AGV_KJ_InterfaceEntity//: EntityBase
    {
        [DataMember]
        public virtual long ID
        {
            get;
            set;
        }

        /// <summary>
        /// DeviceID
        /// </summary>
        [DataMember]
        public virtual string DeviceID { get; set; }

        /// <summary>
        /// 输送线出口编号。0-不指定出口，由系统自动分配。1/2/3/4-指定出口。
        /// </summary>
        [DataMember]
        public virtual string ConveyorID { get; set; }

        /// <summary>
        /// -1：（AGV）预备任务0：（AGV）新任务1：(科捷)输送线已接收（有库存,准备出库）2：（AGV）接收确认3：(科捷)出库完成4：（AGV）出库完成确认5：（科捷）缓存到位6：（AGV）正在取货//7：(科捷)任务完成8：（AGV）完成确认（删除） 删除：松动
        /// </summary>
        [DataMember]
        public virtual int Status { get; set; }

        /// <summary>
        /// outOfStock
        /// </summary>
        [DataMember]
        public virtual int outOfStock { get; set; }

        /// <summary>
        /// issuetime
        /// </summary>
        [DataMember]
        public virtual DateTime? issuetime { get; set; }

        /// <summary>
        /// time_0
        /// </summary>
        [DataMember]
        public virtual DateTime? time_0 { get; set; }

        /// <summary>
        /// time_1
        /// </summary>
        [DataMember]
        public virtual DateTime? time_1 { get; set; }

        /// <summary>
        /// time_2
        /// </summary>
        [DataMember]
        public virtual DateTime? time_2 { get; set; }

        /// <summary>
        /// time_3
        /// </summary>
        [DataMember]
        public virtual DateTime? time_3 { get; set; }

        /// <summary>
        /// time_4
        /// </summary>
        [DataMember]
        public virtual DateTime? time_4 { get; set; }

        /// <summary>
        /// time_5
        /// </summary>
        [DataMember]
        public virtual DateTime? time_5 { get; set; }

        /// <summary>
        /// time_6
        /// </summary>
        [DataMember]
        public virtual DateTime? time_6 { get; set; }

        /// <summary>
        /// time_7
        /// </summary>
        [DataMember]
        public virtual DateTime? time_7 { get; set; }

        /// <summary>
        /// time_8
        /// </summary>
        [DataMember]
        public virtual DateTime? time_8 { get; set; }
        /// <summary>
        /// time_8
        /// </summary>
        [DataMember]
        public virtual DateTime? Created { get; set; }

        /// <summary>
        /// StorageArea
        /// </summary>
        [DataMember]
        public virtual int StorageArea { get; set; }

        /// <summary>
        /// TaskGuid
        /// </summary>
        [DataMember]
        public virtual Guid TaskGuid { get; set; }

        /// <summary>
        /// PlatNo
        /// </summary>
        [DataMember]
        public virtual byte PlatNo { get; set; }

        /// <summary>
        /// SeqNo
        /// </summary>
        [DataMember]
        public virtual int SeqNo { get; set; }

        /// <summary>
        /// Count
        /// </summary>
        [DataMember]
        public virtual int Count { get; set; }
        [DataMember]
        /// <summary>
        /// 长度
        /// </summary>
        public virtual int Length { get; set; }
        public virtual tblProdCodeStructMachEntity tblProdCodeStructMach { get; set; }
    }
}