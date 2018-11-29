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
    public class MachineStatusEntity: EntityBase
    {

        /// <summary>
        /// MachineID
        /// </summary>
        [DataMember]
        public virtual string MachineID { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember]
        public virtual string Status { get; set; }

        /// <summary>
        /// Pruduct
        /// </summary>
        [DataMember]
        public virtual string Pruduct { get; set; }

    }	
}