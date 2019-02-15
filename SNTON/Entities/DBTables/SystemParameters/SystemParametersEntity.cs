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
    public class SystemParametersEntity : EntityBase
    {

        /// <summary>
        /// ParameterName
        /// </summary>
        [DataMember]
        public virtual string ParameterName { get; set; }

        /// <summary>
        /// ParameterValue
        /// </summary>
        [DataMember]
        public virtual string ParameterValue { get; set; }

        /// <summary>
        /// DisplayFormat
        /// </summary>
        [DataMember]
        public virtual byte DisplayFormat { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public virtual string Description { get; set; }

        /// <summary>
        /// SeqNo
        /// </summary>
        [DataMember]
        public virtual int SeqNo { get; set; }

        public virtual List<KeyValuePair<string, string>> SelectValue { get; set; } = new List<KeyValuePair<string, string>>();
    }
}