using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.SystemParameters
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class SystemParametersConfigurationEntity: EntityBase
    {

        /// <summary>
        /// SysParamId
        /// </summary>
        [DataMember]
        public virtual short SysParamId { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [DataMember]
        public virtual string Value { get; set; }

        /// <summary>
        /// DisplayValue
        /// </summary>
        [DataMember]
        public virtual string DisplayValue { get; set; }

    }	
}