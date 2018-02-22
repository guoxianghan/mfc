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
        /// 库位号
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
        /// 库位状态 -1无效;0空;1有轮子;2已预约
        /// </summary>
        [DataMember]
        public virtual int IsOccupied { get; set; }

         
        /// <summary>
        /// 用逗号分隔的工字轮ID列表,
        /// </summary>
        [DataMember]
        public virtual string IdsList { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public virtual string Description { get; set; }

        /// <summary>
        /// 获取该库位所有的轮子实体
        /// </summary>
        public virtual List<Spools.SpoolsEntity> Spools
        {
            get; set;
        } = new List<DBTables.Spools.SpoolsEntity>();
        /// <summary>
        /// 获取该库位的第一个轮子
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