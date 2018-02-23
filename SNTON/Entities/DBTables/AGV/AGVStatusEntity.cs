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
    public class AGVStatusEntity: EntityBase
    {

        /// <summary>
        /// AGVId
        /// </summary>
        [DataMember]
        public virtual short AGVId { get; set; }

        /// <summary>
        /// 0=AGV���ڴ���״̬;
        ///1=AGV���ڿ���״̬;
        ///2=AGV��������ִ��״̬;
        ///3=AGV���ڹ���״̬;
        /// </summary>
        [DataMember]
        public virtual byte Status { get; set; }

        /// <summary>
        /// MinPower
        /// </summary>
        [DataMember]
        public virtual byte MinPower { get; set; }

        /// <summary>
        /// MsgNo
        /// </summary>
        [DataMember]
        public virtual byte MsgNo { get; set; }

    }	
}