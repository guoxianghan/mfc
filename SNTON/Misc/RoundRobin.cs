// Copyright (c) 2009, 2014 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.
//
// $Id: $

using System;
using System.Collections.Generic;
using System.Linq;

namespace FedEx.Misc
{
    /// <summary>
    /// Class for easily achieving a quick round robin distribution
    ///
    /// How To Use:
    ///
    /// 1.  Instantiate the class ;-)
    /// 2.  Throw a key on it specifying the unique id for doing a round robin
    ///     sortation, eg. for Schuh project this was the name of a "Store" which
    ///     was assigned one or more chutes.
    /// 2a. Feed it with multiple possible destinations (eg. "chute names",
    ///     "FSC coordinates",...). Whatever you want.
    /// 3.  This class will return the last recently used destination back.
    ///
    /// Note:   Older, formerly used destinations will be removed automatically in case
    ///         they do not occur in the latest list of possible destinations anymore.
    ///         If they do, they will be kept and reused once they become the last
    ///         recently used one.
    ///
    /// Housekeeping:
    ///
    ///         Due to the fact that all keys will be stored "forever" although they
    ///         may not be used anymore, we have an "updated" timestamp where we
    ///         could "guess" when a key has become obsolete. However, it is
    ///         easier to use the feature that we can simply reinstantiate this component
    ///         whenever we like to really free any memory it previous instance reserved, eg. once a
    ///         month in the class, which is using this RoundRobin functionality.
    ///
    /// </summary>
    public class RoundRobin
    {
        /// <summary>
        /// A destination
        /// </summary>
        protected class Destination
        {
            /// <summary>
            /// The round robin sortation criteria. There may later be more...
            /// </summary>
            public long criteria;

            /// <summary>
            /// Created timestamp
            /// </summary>
            public DateTime created;

            /// <summary>
            /// Updated timestamp
            /// </summary>
            public DateTime updated;

            /// <summary>
            /// Name of the destination (eg. ChuteName, FSC Name...)
            /// </summary>
            public string name;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name">name information</param>
            public Destination(string name)
            {
                criteria = 0;
                created = DateTime.UtcNow;
                updated = created;
                this.name = name;
            }
        }

        /// <summary>
        /// Key, name of destination (possible destinations)
        /// </summary>
        protected Dictionary<string, Dictionary<string, Destination>> destinationTree = new Dictionary<string, Dictionary<string, Destination>>(5000);

        /// <summary>
        /// Returns the last recently used destination out of the list of possible destinations.
        /// </summary>
        /// <param name="theKey">Unique round robin sortation key, eg. a "StoreId" or something.</param>
        /// <param name="possibleDestinations">A list of possible destinations.</param>
        /// <returns>The last recently used destination or NULL in case of an error.</returns>
        public string GetLastRecentlyUsedDestination(string theKey, List<string> possibleDestinations)
        {
            string ret = null;

            if (!string.IsNullOrWhiteSpace(theKey))
            {
                if (destinationTree.ContainsKey(theKey))
                {
                    // More difficult: Check if one of the possible destinations has
                    // already been added. If not, add it. If yes, update the timestamp.

                    Dictionary<string, Destination> destCheck = destinationTree[theKey];
                    long offset=1;
                    foreach (string dest in possibleDestinations)
                    {
                        if (destCheck.ContainsKey(dest)) // formerly known
                        {
                            destCheck[dest].updated = DateTime.UtcNow;
                        }
                        else
                        {
                            var destRecord = new Destination(dest);
                            destRecord.criteria = DateTime.UtcNow.Ticks+offset++;
                            destCheck.Add(dest, destRecord);
                        }
                    }

                    // Now we have to get rid of unused destination records, that is,
                    // records which are no longer available because they are no longer
                    // in the list of possible destinations

                    var toKeep = (from p in destCheck
                        join s in possibleDestinations on p.Key equals s
                        select p.Value).ToList();

                    destCheck.Clear();

                    if (toKeep.Any())
                    {
                        foreach (Destination destRec in toKeep)
                        {
                            if (!destCheck.ContainsKey(destRec.name))
                            {
                                destCheck.Add(destRec.name, destRec);
                            }
                        }
                    }

                    // Now determine the oldest entry...
                    var theDestination = destCheck.Values.OrderBy(p => p.criteria).FirstOrDefault();
                    if (theDestination != null)
                    {
                        theDestination.criteria = DateTime.UtcNow.Ticks;
                        ret = theDestination.name;
                    }
                }
                else
                {
                    // Create a completely new destination tree entry
                    var theDestination = new Dictionary<string, Destination>();
                    long offset = 1;
                    foreach (string s in possibleDestinations)
                    {
                        if (!theDestination.ContainsKey(s))
                        {
                            var dest = new Destination(s);
                            dest.criteria = DateTime.UtcNow.Ticks + offset++;
                            theDestination.Add(s, dest);
                        }
                    }
                    if (theDestination.Count > 0)
                    {
                        destinationTree.Add(theKey, theDestination);

                        // just return the first entry because all of the are
                        // of the same age...
                        theDestination.Values.First().criteria = DateTime.UtcNow.Ticks+offset++;
                        ret = theDestination.ElementAt(0).Key;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Returns the list of the last recently used destination out of the list of possible destinations in order.
        /// First [0] element means "highest prio", last element means least priority.
        /// </summary>
        /// <param name="theKey">Unique round robin sortation key, eg. a "StoreId" or something.</param>
        /// <param name="possibleDestinations">A list of possible destinations.</param>
        /// <returns>The last recently used list of destinations or NULL in case of an error.</returns>
        public List<string> GetLastRecentlyUsedDestinations(string theKey, List<string> possibleDestinations)
        {
            List<string> ret = null;

            if (!string.IsNullOrWhiteSpace(theKey))
            {
                if (destinationTree.ContainsKey(theKey))
                {
                    // More difficult: Check if one of the possible destinations has
                    // already been added. If not, add it. If yes, update the timestamp.

                    Dictionary<string, Destination> destCheck = destinationTree[theKey];
                    long offset = 1;
                    foreach (string dest in possibleDestinations)
                    {
                        if (destCheck.ContainsKey(dest)) // formerly known
                        {
                            destCheck[dest].updated = DateTime.UtcNow;
                        }
                        else
                        {
                            var destRecord = new Destination(dest);
                            destRecord.criteria = DateTime.UtcNow.Ticks+offset++;
                            destCheck.Add(dest, destRecord);
                        }
                    }

                    // Now we have to get rid of unused destination records, that is,
                    // records which are no longer available because they are no longer
                    // in the list of possible destinations

                    var toKeep = (from p in destCheck
                                  join s in possibleDestinations on p.Key equals s
                                  select p.Value).ToList();

                    destCheck.Clear();

                    if (toKeep.Any())
                    {
                        foreach (Destination destRec in toKeep)
                        {
                            if (!destCheck.ContainsKey(destRec.name))
                            {
                                destCheck.Add(destRec.name, destRec);
                            }
                        }
                    }

                    // Now determine the oldest entry...
                    var theDestination = destCheck.Values.OrderBy(p => p.criteria).ToList();
                    if (theDestination.Any())
                    {
                        //Destination des = theDestination.ElementAt(0);
                        //destCheck[des.name].criteria = DateTime.UtcNow.Ticks + offset++;
                        theDestination.ElementAt(0).criteria = DateTime.UtcNow.Ticks + offset++;
                        ret= theDestination.Select(dst => dst.name).ToList();
                    }
                }
                else
                {
                    // Create a completely new destination tree entry
                    var theDestination = new Dictionary<string, Destination>();
                    long offset = 1;
                    foreach (string s in possibleDestinations)
                    {
                        if (!theDestination.ContainsKey(s))
                        {
                            var dest = new Destination(s);
                            dest.criteria = DateTime.UtcNow.Ticks + offset++;
                            theDestination.Add(s, dest);
                        }
                    }
                    if (theDestination.Count > 0)
                    {
                        destinationTree.Add(theKey, theDestination);

                        // just return the same order because all of the are
                        // of the same age...
                        theDestination.Values.First().criteria = DateTime.UtcNow.Ticks+offset++;
                        ret = theDestination.Values.Select(dst => dst.name).ToList();
                    }
                }
            }
            return ret;
        }
    }
}