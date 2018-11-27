// Copyright (c) 2016 Vanderlande Industries
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Vanderlande Industries. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.LockManager
{
    public class LockRequests
    {
        /// <summary>
        /// Will contain sorted list of all object names to aquire lock for / free lock
        /// </summary>
        public List<LockedObject> requestedLocks;

        /// <summary>
        /// Name of the story
        /// </summary>
        public string story;

        /// <summary>
        /// Id of the thread requesting the lock
        /// </summary>
#pragma warning disable 169
        public int requestThreadId;
        public string requestThreadName;
#pragma warning restore 169
        
        /// <summary>
        /// Date and time when lock has been requested
        /// </summary>
#pragma warning disable 169
        public DateTime? requestDate;
#pragma warning restore 169

        public LockRequests()
        {
            requestedLocks = new List<LockedObject>();
        }
    }
}
