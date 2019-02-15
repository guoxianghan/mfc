using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.AGV
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class AGVConfigEntity: EntityBase
    {

        /// <summary>
        /// PlantNo
        /// </summary>
        [DataMember]
        public virtual int PlantNo { get; set; }

        /// <summary>
        /// SeqNo
        /// </summary>
        [DataMember]
        public virtual byte SeqNo { get; set; }

        /// <summary>
        /// AGVName
        /// </summary>
        [DataMember]
        public virtual string AGVName { get; set; }

        /// <summary>
        /// MainUsage
        /// </summary>
        [DataMember]
        public virtual byte MainUsage { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public virtual string Description { get; set; }

    }	
}