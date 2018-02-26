// Copyright (c) 2010 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.
//
// $Id: MFCServiceHandler.cs,v 1.3 2011/10/05 11:44:39 deld Exp $

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using log4net;
using VI.MFC.Logging;

namespace MFC
{
    /// <summary>
    /// MFC service handler. It is used to start or stop the MFc service.
    /// </summary>
    public class MFCServiceHandler : ServiceBase
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MFCServiceHandler()
        {
            CanPauseAndContinue = false;
            CanHandlePowerEvent = true;

            InitializeComponent();
        }

        private MainForm mainFormReference;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            ServiceName = "VISION.MFC";
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be handled</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Power Management strikes again
        /// </summary>
        /// <param name="powerStatus"> power status information</param>
        /// <returns>true on power on</returns>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            if (powerStatus == PowerBroadcastStatus.Suspend)
            {
                logger.WarnMethod("MFC detected that the system is about to go into suspend power mode.");
            }
            else if (powerStatus == PowerBroadcastStatus.ResumeSuspend)
            {
                logger.WarnMethod("MFC detected that the system has been resumed.");
            }
            return base.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// Start the service.
        /// </summary>
        /// <param name="args">List with command line options</param>
        protected override void OnStart(string[] args)
        {
            string applicationDir = Path.GetDirectoryName(Application.ExecutablePath);
            string[] envArguments = Environment.GetCommandLineArgs();
            string[] arguments = new string[1];
            if (envArguments.Length == 2)
            {
                arguments[0] = Path.Combine(applicationDir, envArguments[1]);
                logger.InfoMethod(string.Format("Service starts with following configured xml file: {0}", arguments[0]));
            }
            else
            {
                arguments[0] = applicationDir + "\\mfc.xml";
                logger.InfoMethod("Service starts with default config: mfc.xml");
            }
            Thread starterThread = new Thread(MfcServiceExecute) { IsBackground = true };
            starterThread.Start(arguments);
        }

        /// <summary>
        /// Stop the service.
        /// </summary>
        protected override void OnStop()
        {
            if (mainFormReference != null)
            {
                mainFormReference.BootLoader.CloseHandler();
            }
        }

        /// <summary>
        /// Method used to start up the MFC.
        /// </summary>
        /// <param name="arguments">class to be run</param>
        private void MfcServiceExecute(object arguments)
        {
            string[] theArguments = (string[])arguments;

            logger.DebugMethod("Start MFC as service ");
            Application.SetCompatibleTextRenderingDefault(false);
            mainFormReference = new MainForm(theArguments, true);
            Application.Run(mainFormReference);
        }
    }
}