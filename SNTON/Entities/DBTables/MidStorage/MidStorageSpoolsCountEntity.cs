using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.MidStorage
{
    public class MidStorageSpoolsCountEntity
    {
        [DataMember]
        public virtual int StorageArea { get; set; }
        [DataMember]
        public virtual string Length { get; set; }
        [DataMember]
        public virtual string CName { get; set; }
        [DataMember]
        public virtual string StructBarCode { get; set; }
        [DataMember]
        public virtual string Const { get; set; }
        [DataMember]
        public virtual string BobbinNo { get; set; } = "";
        [DataMember]
        public virtual int Count { get; set; }
    }
}
