using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.Business
{
    /// <summary>
    /// this used by sortation logic to return the chute list back to sortation logic.
    /// then roundrobin componment can use this list to re-order the chutes which return to PLC.
    /// </summary>
    public class SortationChuteTransient
    {
        /// <summary>
        /// The barcode of this parcel
        /// </summary>
        public string Brcode { get; set; }

        /// <summary>
        /// The sortingplanSessionId
        /// </summary>
        public long SortingPlanSessionId { get; set; }

        /// <summary>
        /// The sorter Id
        /// </summary>
        public long SorterId { get; set; }

        /// <summary>
        /// The iLoc location
        /// </summary>
        public long iLoc { get; set; }

        /// <summary>
        /// The chute id list of this parcel
        /// </summary>
        public List<long> ChuteId { get; set; }
    }
}
