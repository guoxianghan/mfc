using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Spools
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class SpoolsEntity : EntityBase
    {

        /// <summary>
        /// 3#������ɹ����� GroupID='C06' 
        ///4#����ʪ�������� GroupID='C26
        /// </summary>
        [DataMember]
        public virtual string GroupID { get; set; }
        /// <summary>
        /// Barcode FdTagNo
        /// </summary>
        [DataMember]
        public virtual string FdTagNo { get; set; }

        /// <summary>
        /// ProductType
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; }
        /// <summary>
        /// 0.30HT ���
        /// </summary>
        [DataMember]
        public virtual string Const { get; set; }


        /// <summary>
        /// MidStorageId
        /// </summary>
        [DataMember]
        public virtual short MidStorageId { get; set; }

        /// <summary>
        /// EquipIdFrom
        /// </summary>
        [DataMember]
        public virtual short EquipIdFrom { get; set; }

        /// <summary>
        /// EquipIdTo
        /// </summary>
        [DataMember]
        public virtual short EquipIdTo { get; set; }

        /// <summary>
        /// AGVIdToMidStorage
        /// </summary>
        [DataMember]
        public virtual short AGVIdToMidStorage { get; set; }

        /// <summary>
        /// AGVIdFromMidStorage
        /// </summary>
        [DataMember]
        public virtual short AGVIdFromMidStorage { get; set; }

        /// <summary>
        /// TaskNo
        /// </summary>
        [DataMember]
        public virtual long TaskNo { get; set; }

        /// <summary>
        /// SeqNo
        /// </summary>
        [DataMember]
        public virtual int SeqNo { get; set; }

        /// <summary>
        /// Length
        /// </summary>
        [DataMember]
        public virtual int Length { get; set; }

        /// <summary>
        /// ץȡ��״̬ 0Ĭ��״̬,1����ץ,2ץ��
        /// </summary>
        [DataMember]
        public virtual int Status { get; set; }
        /// <summary>
        /// AGVTaskID
        /// </summary>
        [DataMember]
        public virtual long AGVTaskID { get; set; }

        public virtual MidStorage.MidStorageEntity MidStore { get; }
        /// <summary>
        /// ��ҵ��׼��
        /// </summary>
        [DataMember]
        public virtual string StructBarCode { get; set; }
        /// <summary>
        /// �����ֹ�� WR145/WS18(8��)/WS34/WS44(12��)
        /// </summary>
        [DataMember]
        public virtual string CName { get; set; }
        /// <summary>
        /// ��һ����Ҫ������
        /// </summary>
        public virtual int GetCount
        {
            get
            {
                int i = 1;
                switch (CName)
                {
                    case "WS18":
                    case "WS34":
                        i = 12;
                        break;
                    case "WS44":
                        i = 8;
                        break;
                    default:
                        break;
                }
                return i;
            }

        }
        [DataMember]
        public virtual char BobbinNo { get; set; }

        /// <summary>
        /// ��ȡ�����ͺŶ�Ӧ��������� 1(8��);2(12��)
        /// </summary>
        public virtual int GetLineCode
        {
            get
            {
                int i = 1;
                switch (CName)
                {
                    case "WS18":
                    case "WS34":
                        i = 2;
                        break;
                    case "WS44":
                        i = 3;
                        break;
                    default:
                        break;
                }
                return i;
            }
        }

    }
}