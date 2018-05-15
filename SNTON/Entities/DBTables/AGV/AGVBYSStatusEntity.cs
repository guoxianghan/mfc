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
    public class AGVBYSStatusEntity
    {

        /// <summary>
        /// AGVID
        /// </summary>
        [DataMember]
        public virtual short AGVID { get; set; }

        /// <summary>
        /// SystemID
        /// </summary>
        [DataMember]
        public virtual int SystemID { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public virtual string Description { get; set; }

        /// <summary>
        /// LastStatus
        /// </summary>
        [DataMember]
        public virtual int LastStatus { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember]
        public virtual int Status { get; set; }

        /// <summary>
        /// LastLandMark
        /// </summary>
        [DataMember]
        public virtual int LastLandMark { get; set; }

        /// <summary>
        /// LandMark
        /// </summary>
        [DataMember]
        public virtual int LandMark { get; set; }

        /// <summary>
        /// LocationX
        /// </summary>
        [DataMember]
        public virtual int LocationX { get; set; }

        /// <summary>
        /// LocationY
        /// </summary>
        [DataMember]
        public virtual int LocationY { get; set; }

        /// <summary>
        /// Connected
        /// </summary>
        [DataMember]
        public virtual bool Connected { get; set; }

        /// <summary>
        /// Velocity
        /// </summary>
        [DataMember]
        public virtual string Velocity { get; set; }

        /// <summary>
        /// PowerLow
        /// </summary>
        [DataMember]
        public virtual bool PowerLow { get; set; }

        /// <summary>
        /// Extend1
        /// </summary>
        [DataMember]
        public virtual int Extend1 { get; set; }

        /// <summary>
        /// Extend2
        /// </summary>
        [DataMember]
        public virtual int Extend2 { get; set; }

        /// <summary>
        /// Extend3
        /// </summary>
        [DataMember]
        public virtual int Extend3 { get; set; }

        /// <summary>
        /// Extend4
        /// </summary>
        [DataMember]
        public virtual int Extend4 { get; set; }

        /// <summary>
        /// LastStation
        /// </summary>
        [DataMember]
        public virtual int LastStation { get; set; }

        /// <summary>
        /// Station
        /// </summary>
        [DataMember]
        public virtual int Station { get; set; }

        /// <summary>
        /// BatteryPower
        /// </summary>
        [DataMember]
        public virtual int BatteryPower { get; set; }

        /// <summary>
        /// ConnectionLevel
        /// </summary>
        [DataMember]
        public virtual int ConnectionLevel { get; set; }

        /// <summary>
        /// Time
        /// </summary>
        [DataMember]
        public virtual DateTime Time { get; set; }

    }
}