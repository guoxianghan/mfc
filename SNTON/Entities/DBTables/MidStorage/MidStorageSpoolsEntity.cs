using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.MidStorage
{
    public class MidStorageSpoolsEntity : EntityBase
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
        /// SpoolId
        /// </summary>
        [DataMember]
        public virtual int SpoolId { get; set; }

        /// <summary>
        /// 库位状态 -1无效;0空;1有轮子;2已预约,4待抓取,5待放置
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
        /// 0.30HT
        /// </summary>
        [DataMember]
        public virtual string Const { get; set; }


        /// <summary>
        /// 获取该库位的第一个轮子
        /// </summary>
        public virtual Spools.SpoolsEntity Spool { get; set; }

        [DataMember]
        public virtual int Length { get; set; }

        [DataMember]
        public virtual string CName { get; set; }

        [DataMember]
        public virtual string FdTagNo { get; set; }

        [DataMember]
        public virtual string StructBarCode { get; set; }
        /// <summary>
        /// L R
        /// </summary>
        [DataMember]
        public virtual char BobbinNo { get; set; }
        //,S.CName
        //,S.FdTagNo
        //,S.StructBarCode
    }
}
