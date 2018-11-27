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
    /// <summary>
    /// Data synchronization access made easy. This component will lock/unlock named objects required
    /// for a story in a global.
    /// Please note that locking same lock object within the same(!) thread is not allowed.
    /// </summary>
    public interface ILockManager
    {
        /// <summary>
        /// Starts locking a business story.
        /// The story must be configured in the .XML configuration file including a list
        /// of unique lock object names.
        /// Note: Same lock object names mean same "physical" lock object.
        /// Example:
        ///              <CONFIG>
        ///                  <Story Name="EditingSortingPlan">
        ///                     <ReadLock>ExceptionChute</ReadLock>
        ///                     <WriteLock>SortingPlan</WriteLock>
        ///                  </Story>
        ///                  <Story Name="SystemLogQuery">
        ///                     <ReadLock>SystemLog</ReadLock>
        ///                  </Story>
        ///              </CONFIG>
        /// </summary>
        /// <param name="story">Name of the story according to .XML</param>
        /// <returns>(Story-)Handle to the lock object. Has to be returned in the call to "Unlock" later on.</returns>
        Guid Lock(string story);
        
        /// <summary>
        /// Unlocks (releases, frees) the locked objects. Has to be called at the end of the story.
        /// Note: The storyHandle will become invalid after calling this method.
        /// </summary>
        /// <param name="storyHandle"></param>
        void Unlock(Guid storyHandle);
    }
}
