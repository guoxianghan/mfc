// Copyright (c) 2010 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.
//
// $Id: MFCServiceInstaller.cs,v 1.3 2011/10/05 11:44:39 deld Exp $

using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using log4net;
using VI.MFC.Logging;

namespace MFC
{
    /// <summary>
    /// This class is used for the installation of the MFC service.
    /// The constructor will be automatically called by a corresponding installer.
    /// Use InstallService or UninstallService to call the installer.
    /// </summary>
    [RunInstaller(true)]
    public class MFCServiceInstaller : Installer
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default constructor. Will be called by an installer.
        /// </summary>
        public MFCServiceInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller =
                    new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            // Installation properties
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            // Service properties
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.DisplayName = "VISION.MFC";
            serviceInstaller.ServiceName = "VISION.MFC";
            serviceInstaller.Description = "Ni Technology Genuine Material Flow Controller";

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }

        /// <summary>
        /// Installs the MFC as a windows service.
        /// </summary>
        public static void InstallService()
        {
            string[] args = new string[1];
            args[0] = "";
            AssemblyInstaller installer = new AssemblyInstaller(typeof(Program).Assembly, args);
            IDictionary state = new Hashtable();
            installer.UseNewContext = true;

            try
            {
                installer.Install(state);
                installer.Commit(state);
            }
            catch (Exception e)
            {
                installer.Rollback(state);
                logger.ErrorMethod("InstallService exception", e);
            }
        }

        /// <summary>
        /// Uninstalls the MFC windows service.
        /// </summary>
        public static void UninstallService()
        {
            string[] args = new string[1];
            args[0] = "";
            AssemblyInstaller installer = new AssemblyInstaller(typeof(Program).Assembly, args);

            IDictionary state = new Hashtable();
            installer.UseNewContext = true;

            try
            {
                installer.Uninstall(state);
            }
            catch (Exception e)
            {
                installer.Rollback(state);
                logger.ErrorMethod("UninstallService exception", e);
            }
        }

        /// <summary>
        /// Starts or stops the MFC service.
        /// </summary>
        /// <param name="start">If true the service will be started, otherwise it will be stopped.</param>
        /// <returns>True if the service was successfully started or stopped.</returns>
        public static bool SetServiceRunning(bool start)
        {
            bool success = false;
            try
            {
                ServiceController service = new ServiceController("VISION.MFC");

                if ((service.Status == ServiceControllerStatus.Stopped && start) ||
                    (service.Status == ServiceControllerStatus.Running && !start))
                {
                    if (start)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 10));
                    }
                    else
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
                    }
                    success = true;
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SetServiceRunning exception", e);
            }

            return success;
        }
    }
}