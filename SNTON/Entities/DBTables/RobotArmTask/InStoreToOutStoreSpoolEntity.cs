using SNTON.Entities.DBTables.MES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.RobotArmTask
{
    public class InStoreToOutStoreSpoolEntity : EntityBase
    {
        [DataMember]
        public virtual int AGVSeqNo { get; set; }
        //[DataMember]
        //public virtual DateTime CreateTime { get; set; }
        [DataMember]
        public virtual int StoreageNo { get; set; }
        [DataMember]
        public virtual int PlantNo { get; set; }
        [DataMember]
        /// <summary>
        /// 1出库线,2直通线
        /// </summary>
        public virtual int InLineNo { get; set; }
        [DataMember]
        public virtual Guid Guid { get; set; }
        /// <summary>
        /// 注释:-1删除,0创建,1正在抓取,2抓取完毕,等待缓存,3缓存完成,等待分配机台,4申请调度AGV,8等待调度AGV,16已调度AGV,128任务完成,129删除
        /// </summary>  
        [DataMember]
        public virtual int Status { get; set; }
        [DataMember]
        public virtual string StatusDescripton { get; set; } = "Status注释:0创建,1正在抓取,2抓取完毕,等待缓存,3缓存完成,等待调度AGV";
        /// <summary>
        /// 直通线上的单丝
        /// </summary>
        public virtual List<MESSystemSpoolsEntity> Spools { get; set; } = new List<MESSystemSpoolsEntity>();
        /// <summary>
        /// 轮子二维码
        /// </summary>
        [DataMember]
        public virtual string FdTagNo { get; set; }
    }
}
