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
    public class EquipTaskViewEntity : EntityBase
    {
        /// <summary>
        /// EquipTask
        /// </summary>
        //[DataMember]
        //public virtual long Id { get; set; }
        /// <summary>
        /// AGVId
        /// </summary>
        [DataMember]
        public virtual byte AGVId { get; set; }
        /// <summary>
        /// AGVStatus
        /// </summary>
        [DataMember]
        public virtual byte AGVStatus { get; set; }
        /// <summary>
        /// EquipContollerId
        /// </summary>
        [DataMember]
        public virtual long EquipContollerId { get; set; }
        /// <summary>
        /// ��Ҫ�ĵ�˿����
        /// </summary>
        [DataMember]
        public virtual int Length { get; set; }
        /// <summary>
        /// �豸��������:1������;2������
        /// </summary>
        [DataMember]
        public virtual byte TaskType { get; set; }

        /// <summary>
        ///   0��ʼ��EquipTask,1����AGVTask������Task,2����ץȡ,3,ץȡ���,4�ȴ�����AGV,5�ѵ���AGV,6AGV������,7�������(�����ۻ�����),8����ʧ��,9��֪ͨ�����Ͳ����������,10���ﵥ˿����
        /// </summary>
        [DataMember]
        public virtual byte Status { get; set; }
        /// <summary>
        /// Source
        /// </summary>
        [DataMember]
        public virtual byte Source { get; set; }
        /// <summary>
        /// AGV����
        /// </summary>
        [DataMember]
        public virtual byte TaskLevel { get; set; }
        /// <summary>
        /// ��� WS188/WS144��
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; }
        /// <summary>
        /// PlantNo
        /// </summary> 
        public virtual byte PlantNo { get; set; }
        /// <summary>
        /// PLCNo
        /// </summary> 
        public virtual byte PLCNo { get; set; }
        /// <summary>
        /// StorageArea
        /// </summary> 
        public virtual string StorageArea { get; set; }
        /// <summary>
        /// ��˿��ҵ��׼����
        /// </summary> 
        public virtual string Supply1 { get; set; }
        /// <summary>
        /// AGVRoute
        /// </summary> 
        public virtual string AGVRoute { get; set; }
        /// <summary>
        /// EquipFlag
        /// </summary> 
        public virtual string EquipFlag { get; set; }
        /// <summary>
        /// ��RobotArmTask��AGV������GUID
        /// </summary>
        [DataMember]
        public virtual Guid TaskGuid { get; set; }
        /// <summary>
        ///  
        /// </summary>
        [DataMember]
        public virtual int AStation { get; set; }
        /// <summary>
        ///  
        /// </summary>
        [DataMember]
        public virtual int BStation { get; set; }
    }
}