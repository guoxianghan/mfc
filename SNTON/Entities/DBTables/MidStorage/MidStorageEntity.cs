using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.MidStorage
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class MidStorageEntity : EntityBase
    {

        //SNTON..Components.Spools.Spools n = new
        /// <summary>
        /// StorageArea
        /// </summary>
        [DataMember]
        public virtual int StorageArea { get; set; }

        /// <summary>
        /// ��λ��
        /// </summary>
        [DataMember]
        public virtual int SeqNo { get; set; }

        /// <summary>
        /// HCoordinate
        /// </summary>
        [DataMember]
        public virtual int HCoordinate { get; set; }

        /// <summary>
        /// VCoordinate
        /// </summary>
        [DataMember]
        public virtual int VCoordinate { get; set; }

        /// <summary>
        /// IsVisible
        /// </summary>
        [DataMember]
        public virtual int IsVisible { get; set; }

        /// <summary>
        /// IsEnable
        /// </summary>
        [DataMember]
        public virtual int IsEnable { get; set; }

        /// <summary>
        /// ��λ״̬ -1��Ч;0��;1������;2��ԤԼ
        /// </summary>
        [DataMember]
        public virtual int IsOccupied { get; set; }

         
        /// <summary>
        /// �ö��ŷָ��Ĺ�����ID�б�,
        /// </summary>
        [DataMember]
        public virtual string IdsList { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public virtual string Description { get; set; }

        /// <summary>
        /// ��ȡ�ÿ�λ���е�����ʵ��
        /// </summary>
        public virtual List<Spools.SpoolsEntity> Spools
        {
            get; set;
        } = new List<DBTables.Spools.SpoolsEntity>();
        /// <summary>
        /// ��ȡ�ÿ�λ�ĵ�һ������
        /// </summary>
        public virtual Spools.SpoolsEntity Spool
        {
            get
            {
                if (Spools != null && Spools.Count != 0)
                    return Spools[0];
                else return null;
            }
        }
    }
}