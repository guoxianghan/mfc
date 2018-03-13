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
    public class ProductEntity : EntityBase
    {

        /// <summary>
        /// 单丝 
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; }

        /// <summary>
        /// M1
        /// </summary>
        [DataMember]
        public virtual string ProductNo { get; set; }

        /// <summary>
        /// ProductionType
        /// </summary>
        [DataMember]
        public virtual string Const { get; set; }

        /// <summary>
        /// WS18/WS44
        /// </summary>
        [DataMember]
        public virtual string CName { get; set; }

        /// <summary>
        /// 电镀类型
        /// </summary>
        [DataMember]
        public virtual string PlatingType { get; set; }
        /// <summary>
        /// LR配比
        /// </summary>
        [DataMember]
        public virtual string LRRatio { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        [DataMember]
        public virtual int Length { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        [DataMember]
        public virtual byte SeqNo { get; set; }

    }
}