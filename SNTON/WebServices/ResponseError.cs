// Copyright (c) 2016 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of 
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Runtime.Serialization;

namespace SNTON.WebServices
{
    public class ResponseError
    {
        /// <summary>
        /// The string resource constant (language text constant) to be used
        /// </summary>
        [DataMember]
        public string Message { get; set; }
        
        /// <summary>
        /// Arguments to dynamically put in to the resource constant
        /// </summary>
        [DataMember]
        public string[] Arguments { get; set; }

    }
}
