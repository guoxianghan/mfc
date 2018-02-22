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
    public class AGVMagMarkerMapConfigEntity: EntityBase
    {

        /// <summary>
        /// PlantNo
        /// </summary>
        [DataMember]
        public virtual int PlantNo { get; set; }

        /// <summary>
        /// MagMarkerId
        /// </summary>
        [DataMember]
        public virtual string MagMarkerId { get; set; }

        /// <summary>
        /// IsColumnPos
        /// </summary>
        [DataMember]
        public virtual byte IsColumnPos { get; set; }

    }	
}