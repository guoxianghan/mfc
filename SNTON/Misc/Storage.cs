// Copyright (c) 2016 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of 
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Mono.CSharp;
using NHibernate;

namespace FedEx.Misc
{
    /// <summary>
    /// All generally useable methods go here.
    /// </summary>
    public class Storage
    {
        protected ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The storage capacity for system logs is limited 
        /// When system logs exceed the disk quaota given in maxByteSize then oldest directory containing files will be deleted
        /// <param name="directoryToCheck">Path of the directory</param>
        /// <param name="directoryToCheck">max size in Byte</param>
        /// <param name="theSession">the session</param>
        /// </summary>
        public bool CheckDirectorySizeAndDelete(string directoryToCheck, long maxByteSize, IStatelessSession theSession = null)
        {
            long directorySize = DirectorySize(new DirectoryInfo(directoryToCheck));
            bool ret = true;

            // If size >= maxByteSize then delete the oldest month      
            while (directorySize >= maxByteSize)
            {
                try
                {
                    // Get all folders in the archiving directory which are containing files
                    // if just selecting the oldest folder, it can lead to deleting an empty folder or only the year folder if this one is already empty
                    //List<string> folders = GetAllFolders(directoryToCheck);
                    List<string> topFolders = Directory.GetDirectories(directoryToCheck, "*.*", SearchOption.TopDirectoryOnly).ToList();
                    if (topFolders.Count > 0)
                    {
                        List<string> folders = Directory.GetDirectories(topFolders[0], "*.*", SearchOption.AllDirectories).ToList();
                        if ((folders.Count > 0 && topFolders.Count > 1) || folders.Count > 1)
                        {
                            Directory.Delete(folders[0], true);
                        }
                        else
                            if (folders.Count == 0 && topFolders.Count > 1)
                            {
                                Directory.Delete(topFolders[0], true);
                            }
                            // we are in the oldest folder and that we dont want to delete
                            else
                            {
                                ret = false;
                                break;
                            }
                    }
                    directorySize = DirectorySize(new DirectoryInfo(directoryToCheck));
                }
                catch (Exception e)
                {
                    logger.Error("Can't delete logs, size exceeded", e);
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Calculate the size of all files in given folder
        /// </summary>
        /// <param name="dInfo">Archive directory</param>
        /// <returns>Size in byte</returns>
        static long DirectorySize(DirectoryInfo dInfo)
        {
            // Enumerate all the files
            long totalSize = dInfo.EnumerateFiles().Sum(file => file.Length);

            // Enumerate all sub-directories
            totalSize += dInfo.EnumerateDirectories().Sum(dir => DirectorySize(dir));
            return totalSize;
        }

        /// <summary>
        /// Puts all filenames with path from a given folder into a list
        /// the list will be sorted ascending
        /// </summary>
        /// <param name="directory">archiving directory</param>
        /// <returns>Path of all files</returns>
        static List<String> GetAllFiles(String directory)
        {
            return Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).ToList();
        }

        static List<String> GetAllFolders(String directory)
        {
            return Directory.GetDirectories(directory, "*.*", SearchOption.AllDirectories).ToList();
        }
    }
}
