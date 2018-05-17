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
    public class SpoolsTaskEntity: EntityBase
    {

        /// <summary>
        /// FdTagNo
        /// </summary>
        [DataMember]
        public virtual string FdTagNo { get; set; }

        /// <summary>
        /// ProductType
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; }

        /// <summary>
        /// CName
        /// </summary>
        [DataMember]
        public virtual string CName { get; set; }

        /// <summary>
        /// Const
        /// </summary>
        [DataMember]
        public virtual string Const { get; set; }

        /// <summary>
        /// Length
        /// </summary>
        [DataMember]
        public virtual int Length { get; set; }
        /// <summary>
        /// Length
        /// </summary>
        [DataMember]
        public virtual int StorageArea { get; set; }

        /// <summary>
        /// BobbinNo
        /// </summary>
        [DataMember]
        public virtual string BobbinNo { get; set; }

        /// <summary>
        /// TaskGroupGUID
        /// </summary>
        [DataMember]
        public virtual Guid TaskGroupGUID { get; set; }

    }	
}