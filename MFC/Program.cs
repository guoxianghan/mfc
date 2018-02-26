// Copyright (c) 2009 - 2010 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.
//
// $Id: Program.cs,v 1.4 2011/10/05 11:44:40 deld Exp $

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using log4net;
using VI.MFC.Logging;

namespace MFC
{
    /// <summary>
    /// main class
    /// </summary>
    internal static class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">command line arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "VISION.MFC";
            bool normalStartup = false;

            // ReSharper disable once AssignNullToNotNullAttribute
            // In this case no error as an Assembly must allways have a location and therefore the is no null return possibly
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            if (Environment.UserInteractive)
            {
                if (args.Any())
                {
                    if (args[0] == "-install")
                    {
                        logger.DebugMethod("Install service");
                        MFCServiceInstaller.InstallService();
                        MessageBox.Show("VISION.MFC service was installed");
                    }
                    else
                    {
                        if (args[0] == "-uninstall")
                        {
                            logger.DebugMethod("UnInstall service");
                            MFCServiceInstaller.UninstallService();
                            MessageBox.Show("VISION.MFC service was uninstalled");
                        }
                        else
                        {
                            if (args[0] == "-start")
                            {
                                if (MFCServiceInstaller.SetServiceRunning(true))
                                {
                                    MessageBox.Show("VISION.MFC service was started");
                                }
                                else
                                {
                                    MessageBox.Show("Cannot find VISION.MFC Service. Please install with mfc.exe -install");
                                }
                            }
                            else
                            {
                                if (args[0] == "-stop")
                                {
                                    if (MFCServiceInstaller.SetServiceRunning(false))
                                    {
                                        MessageBox.Show("VISION.MFC service was stopped");
                                    }
                                    else
                                    {
                                        MessageBox.Show("Cannot find VISION.MFC Service. Please install with mfc.exe -install");
                                    }
                                }
                                else
                                {
                                    normalStartup = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    normalStartup = true;
                }

                if (normalStartup)
                {
                    logger.DebugMethod("Normal startup");
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm(args));
                }
            }
            else
            {
                logger.DebugMethod("Run service");
                ServiceBase.Run(new MFCServiceHandler());
            }
        }
    }
}