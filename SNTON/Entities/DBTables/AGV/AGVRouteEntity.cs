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
    public class AGVRouteEntity : EntityBase
    {

        /// <summary>
        /// AGVId
        /// </summary>
        [DataMember]
        public virtual short AGVId { get; set; }


        public virtual int agv_id { get; set; }
        ///<summary> 
        /// fac_x
        /// </summary>
        [DataMember]
        public virtual float fac_x { get; set; }
        ///<summary>
        /// fac_y
        /// </summary>
        [DataMember]
        public virtual float fac_y { get; set; }
        /// <summary>
        /// ʵʱ���� ����
        /// </summary>
        [DataMember]
        public virtual float X { get; set; }
        /// <summary>
        /// С��״̬
        /// ÿһλ��Ӧһ��С����ͨѶ״ֵ̬
        /// ÿλ�ϵ�ֵ�������£�
        /// 0=AGV���ڴ���״̬
        /// 1=AGV���ڿ���״̬
        /// 2=AGV��������ִ��״̬
        /// 3=AGV���ڹ���״̬
        /// �����ṩ
        /// δ֪ = 0,
        ///��ʻ�� = 1,
        ///�������� = 2,
        ///������ = 3,
        ///���� = 4,
        ///�Ŷ��� = 5,
        ///�ֶ������� = 6,
        ///����λ���� = 7,
        ///׼�� = 8,
        ///�ػ� = 9,
        ///��ͣ = 10,
        ///���� = 11,
        ///������ = 12,
        /// </summary>
        [DataMember]
        public virtual byte Status { get; set; }

        /// <summary>
        /// ʵʱ���� ����
        /// </summary>
        [DataMember]
        public virtual float Y { get; set; }

        /// <summary>
        /// Speed
        /// </summary>
        [DataMember]
        public virtual decimal Speed { get; set; }

    }
}