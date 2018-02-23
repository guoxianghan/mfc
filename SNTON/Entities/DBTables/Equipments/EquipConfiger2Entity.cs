using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Equipments
{
    public class EquipConfiger2Entity : EntityBase
    {
        [DataMember]
        public virtual int EquipControllerId { get; set; }
        [DataMember]
        public virtual string EquipName { get; set; }
        [DataMember]
        public virtual string StorageArea { get; set; }
        [DataMember]
        public virtual string EquipFlag { get; set; }
        [DataMember]
        public virtual string LWCS { get; set; }
        [DataMember]
        public virtual string LineStatus { get; set; }
        [DataMember]
        public virtual string LStatus { get; set; }
        [DataMember]
        public virtual string Equip1Status { get; set; }
        [DataMember]
        public virtual string EStatus1 { get; set; }
        [DataMember]
        public virtual string Equip2Status { get; set; }
        [DataMember]
        public virtual string EStatus2 { get; set; }
        [DataMember]
        public virtual string TaskFlagDispatch { get; set; }
        [DataMember]
        public virtual string WAStatus { get; set; }
        [DataMember]
        public virtual string DispatchStatus { get; set; }
        [DataMember]
        public virtual string AGVDisStatus { get; set; }
        [DataMember]
        public virtual int PLCNo { get; set; }
        [DataMember]

        public virtual bool IsEnable { get; set; }
        [DataMember]
        public virtual string Location { get; set; }
        [DataMember]
        public virtual string AGVRoute { get; set; }
        [DataMember]
        public virtual int AStation { get; set; }
        [DataMember]
        public virtual int BStation { get; set; }
        [DataMember]
        public virtual int PlantNo { get; set; }
        [DataMember]

        public virtual int CommondID { get; set; }
        [DataMember]
        public virtual int ControlID { get; set; }
        [DataMember]
        public virtual int GroupID { get; set; }
    }
}
