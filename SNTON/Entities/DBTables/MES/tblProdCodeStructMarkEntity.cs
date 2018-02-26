using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.MES
{
    /// <summary>
    /// 作业标准书对应的表
    /// </summary>
    public class tblProdCodeStructMarkEntity
    {
        /// <summary>
        /// （作业标准书编号）
        /// </summary>
        [DataMember]
        public virtual string StructBarCode { get; set; }
        /// <summary>
        /// （规格）
        /// </summary>
        [DataMember]
        public virtual string ProdCode { get; set; }
        /// <summary>
        /// （规格）
        /// </summary>
        [DataMember]
        public virtual string Const { get; set; }
        /// <summary>
        /// （规格）
        /// </summary>
        [DataMember]
        public virtual string CName { get; set; }
        /// <summary>
        /// （收线长度）
        /// </summary>
        [DataMember]
        public virtual int ProdLength { get; set; }
        /// <summary>
        /// (工字轮型号)
        /// </summary>
        [DataMember]
        public virtual string SpoolType { get; set; }
        /// <summary>
        /// （放线1）
        /// </summary>
        [DataMember]
        public virtual string Supply1 { get; set; }
        /// <summary>
        /// （放线1数量）
        /// </summary>
        [DataMember]
        public virtual int SupplyQty1 { get; set; }
        /// <summary>
        /// （放线2）
        /// </summary>
        [DataMember]
        public virtual string Supply2 { get; set; }
        /// <summary>
        /// （放线2数量）
        /// </summary>
        [DataMember]
        public virtual int SupplyQty2 { get; set; }

    }
}
