using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.InStoreToOutStore
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class InStoreToOutStoreSpoolEntity : EntityBase
    {

        /// <summary>
        /// SpoolId
        /// </summary>
        [DataMember]
        public virtual long SpoolId { get; set; }

        /// <summary>
        /// AGVSeqNo
        /// </summary>
        [DataMember]
        public virtual int AGVSeqNo { get; set; }

        /// <summary>
        /// StoreageNo
        /// </summary>
        [DataMember]
        public virtual int StoreageNo { get; set; }

        /// <summary>
        /// PlantNo
        /// </summary>
        [DataMember]
        public virtual int PlantNo { get; set; }

        /// <summary>
        /// InLineNo
        /// </summary>
        [DataMember]
        public virtual int InLineNo { get; set; }

        /// <summary>
        /// Guid
        /// </summary>
        [DataMember]
        public virtual Guid Guid { get; set; }

        /// <summary>
        /// 注释:-1删除,0创建,1正在抓取,2抓取完毕,等待缓存,3缓存完成,等待分配机台,4申请调度AGV,8等待调度AGV,16已调度AGV,128任务完成,129删除
        /// </summary>
        [DataMember]
        public virtual int Status { get; set; }

    }	
}