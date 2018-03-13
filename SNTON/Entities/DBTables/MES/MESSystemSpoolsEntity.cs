using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.MES
{
    /// <summary>
    /// MES工字轮信息
    /// </summary>
    public class MESSystemSpoolsEntity
    {
        /// <summary>
        /// 3#车间捻股工字轮 GroupID='C06' 
        ///4#车间湿拉工字轮 GroupID='C26
        /// </summary>
        [DataMember]
        public virtual string GroupID { get; set; }
        /// <summary>
        /// 轮子二维码
        /// </summary>
        [DataMember]
        public virtual string FdTagNo { get; set; }
        /// <summary>
        /// 规格 0.30HT
        /// </summary>
        [DataMember]
        public virtual string Const { get; set; }
        /// <summary>
        /// 作业标准书编号
        /// </summary>
        [DataMember]
        public virtual string StructBarCode { get; set; }
        /// <summary>
        /// 收线米长
        /// </summary>
        [DataMember]
        public virtual int Length { get; set; }

        /// <summary>
        /// 工字轮规格 WR145/WS18(8个)/WS34/WS44(12个)
        /// </summary> 
        [DataMember]
        public virtual string CName { get; set; }
        /// <summary>
        /// L R
        /// </summary>
        [DataMember]
        public virtual string BobbinNo { get; set; }
        /// <summary>
        /// -1入库,0放过,1替换,2补充,3补充L,4补充R
        /// </summary
        public virtual int InStoreToOutStoreage { get; set; }
        /// <summary>
        /// 单丝扫码流水号
        /// </summary>
        public virtual int SeqNo { get; set; }
        public virtual long SpoolId { get; set; }
    }
}
