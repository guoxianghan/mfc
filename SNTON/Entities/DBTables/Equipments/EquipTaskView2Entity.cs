using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Equipments
{
    public class EquipTaskView2Entity
    {
        [DataMember]
        public virtual long EquipTaskID { get; set; }
        [DataMember]
        public virtual long EquipControllerId { get; set; }
        [DataMember]
        public virtual string EquipName { get; set; }
        [DataMember]
        public virtual string Supply1 { get; set; }
        [DataMember]
        public virtual int PlantNo { get; set; }
        [DataMember]
        public virtual string GroupID { get; set; }
        /// <summary>
        /// 需要的单丝长度
        /// </summary>
        [DataMember]
        public virtual int Length { get; set; }
        [DataMember]
        public virtual string StorageArea { get; set; }
        [DataMember]
        public virtual string AGVRoute { get; set; }
        [DataMember]
        public virtual int TaskType { get; set; }
        [DataMember]
        public virtual int Status { get; set; }
        [DataMember]
        public virtual DateTime Created { get; set; }
        [DataMember]
        public virtual DateTime Updated { get; set; }
        [DataMember]
        public virtual string Source { get; set; }
        [DataMember]
        public virtual int TaskLevel { get; set; }
        [DataMember]
        public virtual string ProductType { get; set; }
        [DataMember]
        public virtual Guid TaskGuid { get; set; }
        [DataMember]
        public virtual int BStation { get; set; }
        [DataMember]
        public virtual int AStation { get; set; }
    }
}
