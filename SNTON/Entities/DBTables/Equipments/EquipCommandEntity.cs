using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Equipments
{
    public class EquipCommandEntity : EntityBase
    {
        /// <summary>
        ///  设备PLC命令
        /// </summary>
        [DataMember]
        public virtual int ControlID { get; set; }

        [DataMember]
        public virtual string EquipFlag { get; set; }
        [DataMember]
        public virtual string LineStatus { get; set; }
        [DataMember]
        public virtual string Equip1Status { get; set; }
        [DataMember]
        public virtual int PLCNo { get; set; }
        [DataMember]
        public virtual string Equip2Status { get; set; }
        [DataMember]
        public virtual string TaskFlagDispatch { get; set; }
        [DataMember]
        public virtual string DispatchStatus { get; set; }
        /// <summary>
        /// 滚筒>WCS请求调度AGV(读)
        /// </summary>
        [DataMember]
        public virtual string LWCS { get; set; }
        [DataMember]
        public virtual string LStatus { get; set; }
         
        [DataMember]
        public virtual string EStatus1 { get; set; }
        [DataMember]
        public virtual string EStatus2 { get; set; }
        /// <summary>
        /// WCS已调度AGV(写)
        /// </summary>
        [DataMember]
        public virtual string WAStatus { get; set; }
        /// <summary>
        /// 调度状态
        /// </summary>
        [DataMember]
        public virtual string AGVDisStatus { get; set; }
        /// <summary>
        ///  
        /// </summary>
        [DataMember]
        public virtual int IsEnable { get; set; }
        /// <summary>
        ///  
        /// </summary>
        [DataMember]
        public virtual int AStation { get; set; }
        /// <summary>
        ///  
        /// </summary>
        [DataMember]
        public virtual int BStation { get; set; }
        public virtual string EquipName { get; set; } = "";
    }
}
