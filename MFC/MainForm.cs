// Copyright (c) 2009, 2010 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.
//
// $Id: MainForm.cs,v 1.4 2011/12/14 16:37:36 debkr Exp $

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using VI.MFC;
using VI.MFC.COM;
using VI.MFC.Utils;
using VI.MFC.Logging;

namespace MFC
{
    /// <summary>
    /// Class implementing the main window
    /// </summary>
    public partial class MainForm : Form
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Emulation Plugin Directory as specified in the .XML (if any)
        /// </summary>
        public string pluginDirectory;

        /// <summary>
        /// Config file
        /// </summary>
        public string configFileName;

        /// <summary>
        /// Bootloader Reference
        /// </summary>
        public BootLoader BootLoader
        {
            get;
            private set;
        }

        private readonly bool isService;

        private TaskbarNotifier taskbarNotifier;

        /// <summary>
        /// The application's main form.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public MainForm(string[] args)
        {
            InitializeComponent();
            if (args.Length > 0)
            {
                configFileName = args[0];
            }
            else
            {
                configFileName = "MFC.XML";
            }
            BootLoader = new BootLoader(configFileName);
        }

        /// <summary>
        /// New alternative constructor of the main form
        /// allowing the MFC to run as a service.
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <param name="isService">Flag if application is started as a windows service-</param>
        public MainForm(string[] args, bool isService)
        {
            this.isService = isService;
            InitializeComponent();
            // First argument is XML configuration filename
            if (args.Length > 0)
            {
                configFileName = args[0];
            }
            else
            {
                configFileName = "MFC.XML";
            }
            BootLoader = new BootLoader(configFileName);
        }

        /// <summary>
        /// The main entrance to the MFC.
        /// </summary>
        /// <param name="sender">Opject which triggered the call.</param>
        /// <param name="e">Arguments for this event handler</param>
        private void OnLoad(object sender, EventArgs e)
        {
            Kernel.Glue.SetModuleInstance("MainForm", this);
            // Set plugin Directory
            pluginDirectory = GetPluginDirectory(configFileName);

            try
            {
                BootLoader.HandleStartup();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            Text = BootLoader.ApplicationName;
            if (BootLoader.UseTray)
            {
                if (!isService)
                {
                    InitTrayIcon();
                    InitTaskbar();

                    taskbarNotifier.Show(BootLoader.ApplicationName,
                                         BootLoader.ApplicationVersion + "\n" + Convert.ToString(Kernel.Glue.GetModulesByType<ICommModule>().Count) +
                                         " connections created.\n\n\n",
                                         1500,
                                         2000,
                                         2500);
                }
            }
        }

        private string GetPluginDirectory(string xmlConfigFilename)
        {
            string ret = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlConfigFilename);
                XmlNode emuConfigNode = doc.SelectSingleNode("//MFC.NET/SUPPORTING_COMPONENTS/EMULATION");
                if (emuConfigNode != null)
                {
                    ret = ReadPluginDirectory(emuConfigNode);
                    ValidatePluginDirectory(ret);
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod(string.Format("Error in configuration XML file '{0}' detected: ", xmlConfigFilename), e);
                throw;
            }
            return ret;
        }

        /// <summary>
        /// Check if parameters specified in the .XML are valid.
        /// </summary>
        /// <param name="directory">path to directory to check</param>
        protected void ValidatePluginDirectory(string directory)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    pluginDirectory = Path.GetFullPath(directory);

                    if (!Directory.Exists(pluginDirectory))
                    {
                        logger.InfoMethod(string.Format("Create plugin directory which is not existent: {0}", pluginDirectory));
                        Directory.CreateDirectory(pluginDirectory);
                    }

                    if (!Utility.IsDirectoryAccessible(directory))
                    {
                        throw new ArgumentException(string.Format("Cannot access specified plugin directory '{0}'. Please ensure directory exists and has read/write acess rights.", directory));
                    }
                }
                else
                {
                    logger.InfoMethod("No 'PlugInDirectory' specified. Will not be loading any plugins.");
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Please check 'PlugInDirectory' config parameter.", e);
                throw;
            }
        }

        /// <summary>
        /// Read emulator plugin directory to be used.
        /// </summary>
        /// <param name="localNode">Component's root node</param>
        /// <returns>Plugin directory or empty string</returns>
        protected string ReadPluginDirectory(XmlNode localNode)
        {
            string ret = "";
            // XPATH expression for navigation to the first <CLIENT> element
            XmlNode configNode = localNode.SelectSingleNode("./CONFIG");
            // Check if we have a config node specified at all.
            if (configNode != null && configNode.Attributes != null)
            {
                foreach (XmlAttribute b in configNode.Attributes)
                {
                    switch (b.Name.ToLower())
                    {
                        case "plugindirectory":
                            ret = b.Value;
                            break;
                    }
                }
            }
            return ret;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            BootLoader.CloseHandler();
        }

        private void CloseClick(object obj, EventArgs ea)
        {
            if (MessageBox.Show("Do you really want to shutdown?", BootLoader.ApplicationName, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Close();
            }
        }

        private void TitleClick(object obj, EventArgs ea)
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            SystemSounds.Asterisk.Play();
            MessageBox.Show(assem.GetName().Name + " - Path:" + assem.CodeBase, BootLoader.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ContentClick(object obj, EventArgs ea)
        {
            string assemblyVersion = Assembly.GetExecutingAssembly().FullName;
            SystemSounds.Asterisk.Play();
            MessageBox.Show(assemblyVersion, BootLoader.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
            }
        }

        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (AboutBox aboutBox = new AboutBox())
            {
                aboutBox.ShowDialog();
            }
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void OnTrayDoubleClick(object sender, EventArgs e)
        {
            int connected = Kernel.Glue.GetModulesByType<ICommModule>().Count(m => m.IsConnected());
            if (BootLoader.UseTray)
            {
                taskbarNotifier.Show(BootLoader.ApplicationName,
                                     BootLoader.ApplicationVersion + "\n" + Convert.ToString(Kernel.Glue.GetModulesByType<ICommModule>().Count) +
                                     " connections created, " + connected.ToString(CultureInfo.InvariantCulture) + " connected.\n\n\n",
                                     1500,
                                     2000,
                                     2500);
            }
        }

        private void OnShow(object sender, EventArgs e)
        {
            Hide();
        }

        private void InitTrayIcon()
        {
            trayIcon.BalloonTipText = BootLoader.ApplicationName + " (" + Application.ProductName + ", " + Application.ProductVersion + " (" +
                                      Application.CurrentInputLanguage.LayoutName + ")).";
            string help = BootLoader.ApplicationName + " (" + Application.ProductName + ", " + Application.ProductVersion + " (" +
                          Application.CurrentInputLanguage.LayoutName + ")).";
            trayIcon.Text = help.Substring(0, Math.Min(help.Length, 63));
        }

        private void InitTaskbar()
        {
            taskbarNotifier = new TaskbarNotifier();
            taskbarNotifier.SetBackgroundBitmap(new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("VI.Images.mfcstartupskin.bmp")),
                                                Color.FromArgb(255, 255, 255));
            taskbarNotifier.SetCloseBitmap(new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("VI.Images.close.bmp")),
                                           Color.FromArgb(0, 0, 0),
                                           new Point(187, 60));
            taskbarNotifier.titleRectangle = new Rectangle(70, 50, 155, 35);
            taskbarNotifier.contentRectangle = new Rectangle(60, 50, 100, 150);
            taskbarNotifier.OnTitleClick += TitleClick;
            taskbarNotifier.OnContentClick += ContentClick;
            taskbarNotifier.OnCloseClick += CloseClick;

            taskbarNotifier.closeClickable = true;
            taskbarNotifier.titleClickable = true;
            taskbarNotifier.contentClickable = true;
            taskbarNotifier.enableSelectionRectangle = false;
            taskbarNotifier.KeepVisibleOnMousOver = true;
            taskbarNotifier.ReShowOnMouseOver = true;
        }
    }
}