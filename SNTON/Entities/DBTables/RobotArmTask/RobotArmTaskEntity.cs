using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.RobotArmTask
{
    /// <summary>
    /// DB table entity class
    /// </summary> 
    public class RobotArmTaskEntity : EntityBase
    {

        /// <summary>
        /// ��������:0���ݴ�⵽������;1���ݴ浽ֱͨ��;2��ֱͨ�ߵ��ݴ��(���);3��ֱͨ�ߵ��쳣��;4���ݴ�⵽�쳣��
        /// </summary>
        [DataMember]
        public virtual int TaskType { get; set; }
        /// <summary>
        /// ����������ˮ��
        /// </summary>
        [DataMember]
        public virtual int AGVSeqNo { get; set; }

        /// <summary>
        /// �������ʱ��
        /// </summary>
        [DataMember]
        public virtual DateTime? Completed { get; set; }

        /// <summary>
        /// ��������Ԫ�Ĺ���״̬:-1ʧЧ;0����;1����׼����,׼��ץȡ;2����ץȡ;3ץȡ���;4�ȴ�AGV����;5AGV�������;6ץȡʧ��;7����ʧ��;8����ʧ��;9�ȴ�������Ӧ����С������
        /// </summary>
        [DataMember]
        public virtual int TaskStatus { get; set; }

        /// <summary>
        /// �ݴ��λ�� ��λ��
        /// </summary>
        [DataMember]
        public virtual int FromWhere { get; set; }

        /// <summary>
        /// ����״̬:-1ʧЧ;0����;1����ץȡ;2ץȡ���;3�ȴ�AGV����;4AGV�������;5ץȡʧ��;6����ʧ��;7����ʧ��
        /// </summary>
        [DataMember]
        public virtual int SpoolStatus { get; set; }
        /// <summary>
        /// ץ���ĸ������� 1����������;2ֱͨ��
        /// ����ֵ:MidStoreLine
        /// </summary>
        [DataMember]
        public virtual int ToWhere { get; set; }

        /// <summary>
        /// �������ȼ�1,2,3,4,5,6ץ��ֱͨ��,7���,8�쳣�ڳ���
        /// </summary>
        [DataMember]
        public virtual int TaskLevel { get; set; }

        /// <summary>
        /// ����id
        /// </summary>
        [DataMember]
        public virtual string RobotArmID { get; set; }

        /// <summary>
        /// �������ʶ
        /// </summary>
        [DataMember]
        public virtual Guid TaskGroupGUID { get; set; }

        /// <summary>
        /// ���ϵİ�ť��ID(����λ��)
        /// </summary>
        [DataMember]
        public virtual string EquipControllerId { get; set; }
        [DataMember]
        public virtual byte PlantNo { get; set; }
        /// <summary>
        ///  ��ȡ�����ͺŶ�Ӧ��������� 1(8��);2(12��)
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; }
        [DataMember]
        public virtual string WhoolBarCode { get; set; }
        [DataMember]
        public virtual string CName { get; set; }
        /// <summary>
        /// ��������˳��
        /// </summary>
        [DataMember]
        public virtual int SeqNo { get; set; }
        [DataMember]
        public virtual int StorageArea { get; set; }
        /// <summary>
        /// ��˿��ˮ��
        /// </summary>
        [DataMember]
        public virtual int SpoolSeqNo { get; set; }
    }
}