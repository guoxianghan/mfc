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