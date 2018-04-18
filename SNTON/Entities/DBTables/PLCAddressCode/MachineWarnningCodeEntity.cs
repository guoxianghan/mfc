using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.PLCAddressCode
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class MachineWarnningCodeEntity : EntityBase
    {

        /// <summary>
        /// 1龙门,2线体
        /// </summary>
        [DataMember]
        public virtual byte MachineCode { get; set; }

        /// <summary>
        /// PLCAddress
        /// </summary>
        [DataMember]
        public virtual string AddressName { get; set; }

        /// <summary>
        /// PLCAddress
        /// </summary>
        [DataMember]
        public virtual string PLCAddress { get; set; }
        /// <summary>
        /// Warning0
        /// </summary>
        [DataMember]
        public virtual string Warning { get; set; }
        /// <summary>
        /// PLCAddress
        /// </summary>
        [DataMember]
        public virtual int BIT { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public virtual string Description { get; set; }
        /// <summary>
        /// 是否触发报警
        /// </summary>
        public virtual bool Value { get; set; }

    }
}