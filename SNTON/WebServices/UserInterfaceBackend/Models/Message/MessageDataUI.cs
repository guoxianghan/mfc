using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models
{
    public class MessageDataUI
    {
        /// <summary>
        /// Unique identification ID
        /// </summary>
        public long Id;

        /// <summary>
        /// Message sequence number
        /// </summary>
        public int SeqNo;

        /// <summary>
        /// Message source
        /// </summary>
        public string Source;

        /// <summary>
        /// Message display content
        /// </summary>
        public string Message;

        /// <summary>
        /// Message level
        /// </summary>
        public int MsgLevel;

        /// <summary>
        /// Time when message created
        /// </summary>
        public DateTime Created;
    }
}
