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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CSScriptLibrary;
//using SNTON.Components.RoundRobinDistribution;
using log4net;
using VI.MFC;
using VI.MFC.Components;
using VI.MFC.Components.ParcelHistory;
using VI.MFC.Components.Xml;
using VI.MFC.Logging;

namespace SNTON.Components.LockManager
{
    /// <summary>
    /// Data synchronization access made easy. This component will lock/unlock named objects required
    /// for a story in a global.
    /// Please note that locking same lock object within the same thread is not allowed.
    /// We may later on add features like "who is locking what" as well as a "deadlock detection" 
    /// after xx-seconds.
    /// </summary>
    public class LockManager : VIRuntimeComponent, IViSupportingComponent, ILockManager
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Type of lock to hold
        /// </summary>
        public enum LockType
        {
            ReadLock = 1,
            WriteLock = 2
        }

        /// <summary>
        /// The global pool of all defined lock objects for any story
        /// indexed by name
        /// </summary>
        Dictionary<string, LockedObject> lockObjects;

        /// <summary>
        /// Stories to locked objects relationship. LockedObject is a reference here, so same object(s).
        /// </summary>
#pragma warning disable 169
        private Dictionary<string, List<LockedObject>> stories;
#pragma warning restore 169


        /// <summary>
        /// We have to protect the lockRequests list.
        /// </summary>
        private ReaderWriterLockSlim protData;
        /// <summary>
        /// List of currently issued lock requests... Index will be used as "handle" towards caller 
        /// of "Lock" method
        /// </summary>
#pragma warning disable 169
        private Dictionary<Guid,LockRequests> lockRequests;
#pragma warning restore 169

        /// <summary>
        ///  Standard constructor
        /// </summary>
        public LockManager()
            : base()
        {
            protData = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            lockObjects = new Dictionary<string, LockedObject>();
            stories=new Dictionary<string, List<LockedObject>>();
            lockRequests=new Dictionary<Guid, LockRequests>();
        }

        /// <summary>
        /// Read in configuration and create all lockedObjects
        /// </summary>
        /// <param name="configNode"></param>
        protected override void ReadParameters(XmlNode configNode)
        {
            base.ReadParameters(configNode);

            foreach (XmlNode a in configNode)
            {
                if (a.Name.ToLower() == "config")
                {
                    foreach (XmlNode b in a)
                    {
                        if (b.NodeType == XmlNodeType.Element && b.Name.ToLower() == "story" && b.Attributes != null)
                        {
                            string storyName;
                            var story = ReadStory(b, out storyName);
                            if (!stories.ContainsKey(storyName))
                            {
                                var lst=new List<LockedObject>();
                                foreach (LockedObject l in story)
                                {
                                    if (!string.IsNullOrWhiteSpace(l.Name))
                                    {
                                        if (!lockObjects.ContainsKey(l.Name))
                                        {
                                            l.LockSlim = new ReaderWriterLockSlim();
                                            lockObjects.Add(l.Name, l);
                                        }
                                        lst.Add(lockObjects[l.Name]);
                                    }
                                }
                                if (lst.Any())
                                {
                                    lst = lst.OrderBy(o => o.Name).ToList();
                                    stories.Add(storyName,lst);
                                }
                            }
                            else
                            {
                                string errorMsg=string.Format("Duplicate storyname {0}. Please do not use same story multiple times.",storyName);
                                logger.ErrorMethod(errorMsg);
                                throw new ArgumentException(errorMsg);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns all lockobject names for this story.
        /// </summary>
        /// <param name="configNode"></param>
        /// <param name="storyName">Name of the story</param>
        /// <returns>empty list if no objects found, else objects.</returns>
        protected List<LockedObject> ReadStory(XmlNode configNode, out string storyName)
        {
            storyName = String.Empty;
            List<LockedObject> ret = new List<LockedObject>();

            if (configNode.Attributes != null)
            {
                foreach (XmlAttribute a in configNode.Attributes)
                {
                    if (a.Name.ToLower() == "name")
                    {
                        storyName = a.Value;
                    }
                }
            }
            foreach (XmlNode a in configNode)
            {
                if (a.NodeType == XmlNodeType.Element)
                {

                    if (a.Name.ToLower() == "read")
                    {
                        LockedObject lo = new LockedObject(a.InnerText, LockType.ReadLock);
                        ret.Add(lo);
                    }
                    else if (a.Name.ToLower() == "write")
                    {
                        LockedObject lo = new LockedObject(a.InnerText, LockType.WriteLock);
                        ret.Add(lo);
                    }
                    else
                    {
                        string errorMsg = string.Format("Unknown locking type '{0}' detected during configuration. Please only use either 'Read' or 'Write'.", a.Name);
                        logger.ErrorMethod(errorMsg);
                        throw new ArgumentException(errorMsg);
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// Starts locking a business story.
        /// The story must be configured in the .XML configuration file including a list
        /// of unique lock object names.
        /// Note: Same lock object names mean same "physical" lock object. 
        /// Throws Argument exception in case story is not configured/known.
        /// Example (locking the same object, here "Parcel"):
        ///              <CONFIG>
        ///                  <Story Name="EditingSortingPlan">
        ///                     <ReadLock>ExceptionChute</ReadLock>
        ///                     <WriteLock>SortingPlan</WriteLock>
        ///                     <ReadLock>Parcel</ReadLock>
        ///                  </Story>
        ///                  <Story Name="SortReportArchive">
        ///                     <ReadLock>Parcel</ReadLock>
        ///                     <ReadLock>SortReports</ReadLock>
        ///                  </Story>
        ///              </CONFIG>
        /// </summary>
        /// <param name="story">Name of the story according to .XML</param>
        /// <returns>(Story-)Handle to the lock object. Has to be returned in the call to "Unlock" later on.</returns>
        /// 
        public Guid Lock(string story)
        {
            Guid ret;
            if (!stories.ContainsKey(story))
            {
                throw new ArgumentException(string.Format("Unknown story {0}.",story));
            }
            ret = AcquireLock(story);
            return ret;
        }

        /// <summary>
        /// Get the lock in the correct order...
        /// </summary>
        /// <param name="story"></param>
        /// <returns></returns>
        private Guid AcquireLock(string story)
        {
            Guid ret = Guid.NewGuid();
            try
            {
                LockRequests lr=new LockRequests();
                lr.story = story;
                lr.requestedLocks = stories[story];
                lr.requestDate = DateTime.UtcNow;
                lr.requestThreadName=Thread.CurrentThread.Name;
                lr.requestThreadId = Thread.CurrentThread.ManagedThreadId;
                DoLock(lr);
                try
                {
                    protData.EnterWriteLock();
                    lockRequests.Add(ret, lr);
                }
                finally
                {
                    protData.ExitWriteLock();
                }
                
            }
            catch (Exception e)
            {
                logger.ErrorMethod(e);
                throw;
            }   
            return ret;
        }


        /// <summary>
        /// Physically execute lock
        /// </summary>
        /// <param name="lr">LockRequest to execute</param>
        private void DoLock(LockRequests lr)
        {
            if (lr != null && lr.requestedLocks!=null)
            {
                logger.Info(string.Format("Trying to lock objects for story '{0}' on thread '{1}' [{2}].", lr.story,lr.requestThreadName,lr.requestThreadId));
                foreach (LockedObject l in lr.requestedLocks)
                {
                    
                    if (l.Locktype == LockType.ReadLock)
                    {
                        logger.Info(string.Format("Trying to enter read lock {0} for story '{1}' on thread '{2}' [{3}].",l.Name,lr.story, lr.requestThreadName, lr.requestThreadId));
                        l.LockSlim.EnterReadLock();
                        logger.Info(string.Format("Read lock {0} for story '{1}' on thread '{2}' [{3}] aquired.", l.Name, lr.story, lr.requestThreadName, lr.requestThreadId));
                    }
                    else
                    {
                        logger.Info(string.Format("Trying to enter write lock {0} for story '{1}' on thread '{2}' [{3}].", l.Name, lr.story, lr.requestThreadName, lr.requestThreadId));
                        l.LockSlim.EnterWriteLock();
                        logger.Info(string.Format("Write lock {0} for story '{1}' on thread '{2}' [{3}] aquired.", l.Name, lr.story, lr.requestThreadName, lr.requestThreadId));

                    }
                }
                logger.Info(string.Format("All locks for story '{0}' have been fully aquired for thread '{1}' [{2}].", lr.story, lr.requestThreadName, lr.requestThreadId));
            }
        }

        /// <summary>
        /// Unlocks (releases, frees) the locked objects. Has to be called at the end of the story.
        /// Throws Argument exception in case lock was not held.
        /// Note: The storyHandle will become invalid after calling this method.
        /// </summary>
        /// <param name="storyHandle"></param>
        public void Unlock(Guid storyHandle)
        {
            try
            {
                protData.EnterWriteLock();
                if (!lockRequests.ContainsKey(storyHandle))
                {
                    throw new ArgumentException(string.Format("Unknown or already released lock request {0}.", storyHandle));    
                }
                DoUnlock(lockRequests[storyHandle]);
                lockRequests.Remove(storyHandle);
            }
            finally
            {
                protData.ExitWriteLock();
            }
        }

        /// <summary>
        /// Unlock the locks (in reverse order)
        /// </summary>
        /// <param name="lr"></param>
        private void DoUnlock(LockRequests lr)
        {
            
            if (lr != null && lr.requestedLocks != null)
            {
                logger.Info(string.Format("Trying to unlock objects for story '{0}' on thread '{1}' [{2}].", lr.story, lr.requestThreadName, lr.requestThreadId));
                for (int lockNo=lr.requestedLocks.Count-1;lockNo>=0;lockNo--)
                {
                    try
                    {

                        if (lr.requestedLocks[lockNo].Locktype == LockType.ReadLock)
                        {
                            logger.Info(string.Format("Trying to exit read lock {0} for story '{1}' on thread '{2}' [{3}].",
                                                      lr.requestedLocks[lockNo].Name,
                                                      lr.story,
                                                      lr.requestThreadName,
                                                      lr.requestThreadId));
                            lr.requestedLocks[lockNo].LockSlim.ExitReadLock();
                            logger.Info(string.Format("Read lock {0} for story '{1}' on thread '{2}' [{3}] removed.",
                                                      lr.requestedLocks[lockNo].Name,
                                                      lr.story,
                                                      lr.requestThreadName,
                                                      lr.requestThreadId));
                        }
                        else
                        {
                            logger.Info(string.Format("Trying to exit write lock {0} for story '{1}' on thread '{2}' [{3}].",
                                                      lr.requestedLocks[lockNo].Name,
                                                      lr.story,
                                                      lr.requestThreadName,
                                                      lr.requestThreadId));
                            lr.requestedLocks[lockNo].LockSlim.ExitWriteLock();
                            logger.Info(string.Format("Write lock {0} for story '{1}' on thread '{2}' [{3}] removed.",
                                                      lr.requestedLocks[lockNo].Name,
                                                      lr.story,
                                                      lr.requestThreadName,
                                                      lr.requestThreadId));

                        }
                    }
                    catch (Exception e)
                    {
                        logger.ErrorMethod(string.Format("Exception occured during unlock of lock object {0} of story {1}.",lr.requestedLocks[lockNo].Name,lr.story),e);
                    }
                }
                logger.Info(string.Format("All locks for story '{0}' have been removed on thread '{1}' [{2}].", lr.story, lr.requestThreadName, lr.requestThreadId));
            }

        }

        /// <summary>
        /// Component Info
        /// </summary>
        /// <returns></returns>
        public override string GetInfo()
        {
            return "LockManager V1.0, story based locking made easy...";
        }
    }
}
