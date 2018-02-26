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
    public class EquipControllerConfigEntity : EntityBase
    {

        /// <summary>
        /// PlantNo
        /// </summary>
        [DataMember]
        public virtual byte PlantNo { get; set; }

        /// <summary>
        /// EquipControllerName
        /// </summary>
        [DataMember]
        public virtual string EquipControllerName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public virtual string Description { get; set; }
        /// <summary>
        /// 该列设备需要在同一排上
        /// </summary>
        [DataMember]
        public virtual string AGVRoute { get; set; }
        /// <summary>
        /// 该设备对应那个暂存库
        /// </summary>
        [DataMember]
        public virtual string StorageArea { get; set; }

    }
}