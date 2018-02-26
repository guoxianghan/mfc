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

        /// <summary>
        /// X
        /// </summary>
        [DataMember]
        public virtual string X { get; set; }
        /// <summary>
        /// Ð¡³µ×´Ì¬
        /// </summary>
        [DataMember]
        public virtual byte Status { get; set; }

        /// <summary>
        /// Y
        /// </summary>
        [DataMember]
        public virtual string Y { get; set; }

        /// <summary>
        /// Speed
        /// </summary>
        [DataMember]
        public virtual decimal Speed { get; set; }

    }
}