using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models
{
    public class MessageSearchCondition
    {
        /// <summary>
        /// Message search start time
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// Message search end time
        /// </summary>
        public DateTime? endTime { get; set; }

        /// <summary>
        /// Message source
        /// </summary>
        public string msgSource { get; set; }
        public string Key { get; set; }

        /// <summary>
        /// Message level
        /// </summary>
        public int msgLevel { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
    }
}
