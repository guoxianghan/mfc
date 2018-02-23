using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.Message
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class MessageEntity: EntityBase
    {

        /// <summary>
        /// MsgLevel
        /// </summary>
        [DataMember]
        public virtual int MsgLevel { get; set; }

        /// <summary>
        /// Source
        /// </summary>
        [DataMember]
        public virtual string Source { get; set; }

        /// <summary>
        /// MsgContent
        /// </summary>
        [DataMember]
        public virtual string MsgContent { get; set; }

    }	
}