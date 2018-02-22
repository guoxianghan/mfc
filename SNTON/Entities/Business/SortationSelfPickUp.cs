using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.Business
{
    /// <summary>
    /// this used by sortation logic to return the self pick up chute list back to sortation logic.
    /// this will combine SelfPickupList, selfPickUpParcel and SelfPickupChutes
    /// </summary>
    public class SortationSelfPickUp
    {

        /// <summary>
        /// Relation to FX_SORTER table. Defines which sorter is responsible for this pickup list.
        /// </summary>
        public virtual long SorterID { get; set; }

        /// <summary>
        /// The name (order) of the list for this sorter. Example: '1' up to '10'. Pre-created during setup.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// User editable description for this pickup list.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Relation to FX_SELFPICKUPLIST table.
        /// </summary>
        public virtual long SelfPickupListID { get; set; }

        /// <summary>
        /// AWB barcode (and related MPS barcodes) to send to self-pickup chutes (from .CSV file).
        /// </summary>
        public virtual string Barcode { get; set; }

        /// <summary>
        /// Start date of self-pickup: Send to pickup chute only when this date time is reached but before END(date) (from .CSV file).
        /// </summary>
        public virtual DateTime StartTime { get; set; }

        /// <summary>
        /// When to stop self-pickup of this barcode. Send to pickup chute only when current date time is between START(date) and END(date) (from .CSV file).
        /// </summary>
        public virtual DateTime EndTime { get; set; }

        /// <summary>
        /// Relation to FX_SELFPICKUP table.
        /// </summary>
        public virtual long SelfPickupID { get; set; }

        /// <summary>
        /// Relation to SFX_CHUTE table defining which chute to use.
        /// </summary>
        public virtual long ChuteID { get; set; }

    }
}
