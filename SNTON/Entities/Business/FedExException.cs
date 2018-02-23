using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace FedEx.Entities.Business
{
    /// <summary>
    /// FedEx custom exception
    /// </summary>
    public class FedExException: Exception
    {
        /// <summary>
        /// The Exception type of a chuteException
        /// </summary>
        public string ExceptionType { get; private set; }

        /// <summary>
        /// Method to get the execeptiontype
        /// </summary>
        /// <param name="message"></param>
        public FedExException(string message)
            : base(message)
        {
            ExceptionType = message;
        }
    }
}
