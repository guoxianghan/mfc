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
    public class EquipProductionEntity: EntityBase
    {

        /// <summary>
        /// EquipId
        /// </summary>
        [DataMember]
        public virtual string EquipId { get; set; }

        /// <summary>
        /// GroupId
        /// </summary>
        [DataMember]
        public virtual short GroupId { get; set; }

        /// <summary>
        /// ProductType
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; }

        /// <summary>
        /// EmptySpoolType
        /// </summary>
        [DataMember]
        public virtual string EmptySpoolType { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public virtual string Description { get; set; }

        /// <summary>
        /// Operator
        /// </summary>
        [DataMember]
        public virtual string Operator { get; set; }

    }	
}