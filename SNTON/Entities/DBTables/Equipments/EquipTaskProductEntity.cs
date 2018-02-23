using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Equipments
{
    public class EquipTaskProductEntity
    {
        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual long TaskId { get; set; }
        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual long EquipContollerId { get; set; }
        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual short TaskType { get; set; }
        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual int EquipId { get; set; }
        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; }
        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual string EmptySpoolType { get; set; }
        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual short GroupId { get; set; }
        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual short Status { get; set; }
    }
}
