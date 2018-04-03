using SNTON.Constants;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.RobotArmTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.AGV
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class AGVTasksEntity : EntityBase
    {

        /// <summary>
        /// TaskNo
        /// </summary>
        [DataMember]
        public virtual long TaskNo { get; set; }

        /// <summary>
        /// 0=δ֪��1������,2������
        ///1=3#��5#���䣬����AGV���м��ݴ�������Ʒ����������
        ///2=4#���䣬����AGV�Ͱ��Ʒ�����м��ݴ����
        ///3=4#���䣬����AGV���м��ݴ��������
        /// </summary>
        [DataMember]
        public virtual byte TaskType { get; set; }
        /// <summary>
        /// ����������ˮ��
        /// </summary>
        [DataMember]
        public virtual int SeqNo { get; set; }
        /// <summary>
        /// �ݴ��������,1������;2ֱͨ��
        /// </summary>
        [DataMember]
        public virtual int StorageLineNo { get; set; }
        /// <summary>
        /// �ݴ���1,2,3
        /// </summary>
        [DataMember]
        public virtual int StorageArea { get; set; }

        /// <summary>
        /// AGVId
        /// </summary>
        [DataMember]
        public virtual short AGVId { get; set; }

        /// <summary>
        /// ����ͣ����
        /// ���ϵ�,�ݴ������ͣ������,�ֺŷָ�
        /// </summary>
        [DataMember]
        public virtual string EquipIdListTarget { get; set; } = "";

        /// <summary>
        /// ʵ��ͣ����ͽ���
        /// �ֺŷָ���豸ID
        /// </summary>
        [DataMember]
        public virtual string EquipIdListActual { get; set; } = "";
        /// <summary>
        /// ProductType
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; } = "";

        /// <summary>
        /// 0,����;
        /// 1,����ִ��ץ����;
        /// 2,����׼���ú�,Ready ����AGVTasks��TaskNo;
        /// 3,С����������������Ӻ�״̬����Release
        /// 4,�ѷ���
        /// 7,ֱͨ�ڵ�˿�ѵ�λ
        /// 8,�յ��ظ�
        /// 16,��������������,����δ���
        /// 17,��֪ͨ�����Ͳ
        /// 32,��ȡ������ȷ��
        /// 64,����δ�����쳣��ԭ�����ɵ�����
        /// 128, �ѳɹ����ִ�е�����
        /// </summary>
        [DataMember]
        public virtual int Status { get; set; }
        /// <summary>
        /// AGV���񼶱�
        /// 0,���
        /// 2,�����
        /// 4,�쳣����
        /// 6ֱͨ��
        /// </summary>
        [DataMember]
        public virtual int TaskLevel { get; set; }
        [DataMember]
        public virtual int PLCNo { get; set; }
        /// <summary>
        /// PlantNo
        /// </summary>
        [DataMember]
        public virtual byte PlantNo { get; set; }

        /// <summary>
        /// ��RobotArmTask������GUID
        /// </summary>
        [DataMember]
        public virtual Guid TaskGuid { get; set; }
        public virtual bool IsHasSpools
        {
            get
            {
                SNTONConstants.AGVTaskStatus st = (SNTONConstants.AGVTaskStatus)Status;

                switch (st)
                {
                    case SNTONConstants.AGVTaskStatus.Canceled:
                    case SNTONConstants.AGVTaskStatus.Created:
                    case SNTONConstants.AGVTaskStatus.Faulted:
                    case SNTONConstants.AGVTaskStatus.Finished:
                    case SNTONConstants.AGVTaskStatus.Ready:
                        return false;
                    case SNTONConstants.AGVTaskStatus.Received:
                    case SNTONConstants.AGVTaskStatus.Running:
                    case SNTONConstants.AGVTaskStatus.Sent:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public virtual List<Spools.SpoolsEntity> Spools { get; set; } = new List<Spools.SpoolsEntity>();
        public virtual List<RobotArmTaskEntity> _RobotArmTasks { get; set; }
        public virtual List<EquipTaskEntity> _EquipTasks { get; set; }
        public virtual List<EquipTaskView2Entity> _EquipTasks2 { get; set; }
    }
}