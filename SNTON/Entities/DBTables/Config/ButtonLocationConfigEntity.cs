using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Config
{
    /// <summary>
    /// �����ͲID
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
        /// �����豸��Ҫ��ͬһ����
        /// </summary>
        [DataMember]
        public virtual string AGVRoute { get; set; }
        /// <summary>
        /// ���豸��Ӧ�Ǹ��ݴ��
        /// </summary>
        [DataMember]
        public virtual string StorageArea { get; set; }

    }	
}