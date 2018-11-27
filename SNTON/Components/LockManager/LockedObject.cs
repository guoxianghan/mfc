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
using System.Threading;

namespace SNTON.Components.LockManager
{
    public class LockedObject
    {
        /// <summary>
        /// Name of the lock object as specified in the .XML configuration file
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The lock object itself
        /// </summary>
        public ReaderWriterLockSlim LockSlim
        {
            get;
            set;
        }

        /// <summary>
        /// What type of locking shall be performed?
        /// </summary>
        public LockManager.LockType Locktype
        {
            get;
            set;
        }

        public LockedObject(string name, LockManager.LockType lockType)
        {
            Name = name;
            Locktype = lockType;
        }
    }
}
