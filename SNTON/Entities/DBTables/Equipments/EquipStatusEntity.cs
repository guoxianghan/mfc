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
    public class EquipStatusEntity: EntityBase
    {

        /// <summary>
        /// EquipId
        /// </summary>
        [DataMember]
        public virtual short EquipId { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember]
        public virtual byte Status { get; set; }

    }	
}