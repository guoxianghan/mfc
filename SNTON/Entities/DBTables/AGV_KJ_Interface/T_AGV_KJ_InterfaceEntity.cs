using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SNTON.Entities.DBTables.MES;

namespace SNTON.Entities.DBTables.AGV_KJ_Interface
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class T_AGV_KJ_InterfaceEntity//: EntityBase
    {
        [DataMember]
        public virtual long ID
        {
            get;
            set;
        }

        /// <summary>
        /// DeviceID
        /// </summary>
        [DataMember]
        public virtual string DeviceID { get; set; }

        /// <summary>
        /// ConveyorID
        /// </summary>
        [DataMember]
        public virtual string ConveyorID { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember]
        public virtual int Status { get; set; }

        /// <summary>
        /// outOfStock
        /// </summary>
        [DataMember]
        public virtual int outOfStock { get; set; }

        /// <summary>
        /// issuetime
        /// </summary>
        [DataMember]
        public virtual DateTime issuetime { get; set; }

        /// <summary>
        /// time_0
        /// </summary>
        [DataMember]
        public virtual DateTime time_0 { get; set; }

        /// <summary>
        /// time_1
        /// </summary>
        [DataMember]
        public virtual DateTime time_1 { get; set; }

        /// <summary>
        /// time_2
        /// </summary>
        [DataMember]
        public virtual DateTime time_2 { get; set; }

        /// <summary>
        /// time_3
        /// </summary>
        [DataMember]
        public virtual DateTime time_3 { get; set; }

        /// <summary>
        /// time_4
        /// </summary>
        [DataMember]
        public virtual DateTime time_4 { get; set; }

        /// <summary>
        /// time_5
        /// </summary>
        [DataMember]
        public virtual DateTime time_5 { get; set; }

        /// <summary>
        /// time_6
        /// </summary>
        [DataMember]
        public virtual DateTime time_6 { get; set; }

        /// <summary>
        /// time_7
        /// </summary>
        [DataMember]
        public virtual DateTime time_7 { get; set; }

        /// <summary>
        /// time_8
        /// </summary>
        [DataMember]
        public virtual DateTime time_8 { get; set; }

        /// <summary>
        /// StorageArea
        /// </summary>
        [DataMember]
        public virtual int StorageArea { get; set; }

        /// <summary>
        /// TaskGuid
        /// </summary>
        [DataMember]
        public virtual Guid TaskGuid { get; set; }

        /// <summary>
        /// PlatNo
        /// </summary>
        [DataMember]
        public virtual byte PlatNo { get; set; }

        /// <summary>
        /// SeqNo
        /// </summary>
        [DataMember]
        public virtual int SeqNo { get; set; }

        /// <summary>
        /// Count
        /// </summary>
        [DataMember]
        public virtual int Count { get; set; }
        public tblProdCodeStructMachEntity tblProdCodeStructMach { get; internal set; }
    }	
}