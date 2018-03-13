using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.MES
{
    /// <summary>
    /// 机台号与作业标准书关联表
    /// </summary>
    public class tblProdCodeStructMachEntity
    {
        /// <summary>
        ///  3#车间捻股工字轮 GroupID='C06' 
        ///4#车间湿拉工字轮 GroupID='C26' 
        /// </summary>
        [DataMember]
        public virtual string GroupId { get; set; }
        /// <summary>
        ///  
        /// </summary>
        [DataMember]
        public virtual string FactoryId { get; set; }
        /// <summary>
        ///  机台号 ST-3B-08-18 
        /// </summary>
        [DataMember]
        public virtual string MachCode { get; set; }
        /// <summary>
        /// 作业标准书编号
        /// </summary>
        [DataMember]
        public virtual string StructBarCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public virtual DateTime? StartDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public virtual string StartUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public virtual string StartShift { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public virtual string StartShiftPart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public virtual string InsertUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public virtual DateTime InsertDate { get; set; }

        /// <summary>
        /// 机台对应的3#车间的作业标准书
        /// </summary>
        public virtual tblProdCodeStructMarkEntity ProdCodeStructMark3 { get; set; }
        /// <summary>
        /// 机台对应的4#车间的作业标准书
        /// </summary>
        public virtual tblProdCodeStructMarkEntity ProdCodeStructMark4 { get; set; }

        /// <summary>
        /// 一个三车间的作业标准书绑定多个作业标准书的(一般为1个或2个)
        /// </summary>
        public virtual List<tblProdCodeStructMarkEntity> ProdCodeStructMarks { get; set; } = new List<tblProdCodeStructMarkEntity>();
    }
}
