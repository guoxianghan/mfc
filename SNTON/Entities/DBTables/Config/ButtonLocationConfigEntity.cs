using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Config
{
    /// <summary>
    /// 地面滚筒ID
    /// </summary>
    public class ButtonLocationConfigEntity: EntityBase
    {

        /// <summary>
        /// EquipControllerID
        /// </summary>
        [DataMember]
        public virtual long EquipControllerID { get; set; }

        /// <summary>
        /// Location
        /// </summary>
        [DataMember]
        public virtual string Location { get; set; }
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