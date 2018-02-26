using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Equipments
{
    /// <summary>
    /// 设备信息
    /// </summary>
    public class EquipConfigEntity : EntityBase
    {

        /// <summary>
        /// PlantNo
        /// </summary>
        [DataMember]
        public virtual byte PlantNo { get; set; }

        /// <summary>
        /// 界面上的叫料按钮ID
        /// </summary>
        [DataMember]
        public virtual int ShowID { get; set; }
        /// <summary>
        /// EquipName
        /// </summary>
        [DataMember]
        public virtual string EquipName { get; set; }

        /// <summary>
        /// EquipControllerId
        /// </summary>
        [DataMember]
        public virtual int EquipControllerId { get; set; }

        /// <summary>
        /// X
        /// </summary>
        [DataMember]
        public virtual byte X { get; set; }

        /// <summary>
        /// Y
        /// </summary>
        [DataMember]
        public virtual byte Y { get; set; }

        /// <summary>
        /// 作业标准书
        /// </summary>
        public virtual string StructBarCode { get; set; }
        /// <summary>
        /// ControllerName
        /// </summary>
        [DataMember]
        public virtual string ControllerName { get; set; }

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

        /// <summary>
        /// GroupID
        /// </summary>
        [DataMember]
        public virtual int GroupID { get; set; }

    }
}