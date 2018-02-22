// Copyright (c) 2009 - 2011 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.
//
// $Id: ShutdownForm.cs,v 1.5 2011/10/27 07:33:20 demba Exp $

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace MFC
{
    /// <summary>
    /// A form that gives the user a feedback while the MFC is shutting
    /// down all threads.
    /// In the constructor the number of threads will be defined that will terminate.
    /// Each thread must call the function ThreadFinished after it was stopped.
    /// A progressbar will show the user the progress of the shutdown process.
    /// </summary>
    public partial class ShutdownForm : Form
    {
        #region members

        private readonly int numberOfThreads;
        private readonly ReaderWriterLockSlim progressLock = new ReaderWriterLockSlim();
        private readonly List<string> shutdownList = new List<string>();

        private bool closeForm;
        private int threadsFinished;

        private delegate void ParameterlessDelegate();

        private delegate void ParameterStringDelegate(string stringParam);

        #endregion

        #region constructor

        /// <summary>
        /// Constructor of the ShutdownForm.
        /// </summary>
        /// <param name="numberOfThreads">The number of threads that are going to be stopped.</param>
        public ShutdownForm(int numberOfThreads)
        {
            InitializeComponent();

            this.numberOfThreads = numberOfThreads;
            progressBar.Maximum = numberOfThreads;
            if (numberOfThreads <= 0)
            {
                closeForm = true;
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Will be called after a thread was finished. The progressbar
        /// will be updated and the for will be closed, if all threads
        /// were closed.
        /// </summary>
        /// <param name="threadName">The name of the thread that was stopped.</param>
        public void ThreadFinished(string threadName)
        {
            try
            {
                progressLock.EnterWriteLock();

                threadsFinished += 1;

                if (this.Visible)
                {
                    progressBar.Invoke(new ParameterlessDelegate(this.UpdateProgressBar));
                    threadNameLabel.Invoke(new ParameterStringDelegate(SetThreadNameLabel), threadName);
                }
                else
                {
                    shutdownList.Add(threadName);
                }

                if (threadsFinished >= numberOfThreads)
                {
                    closeForm = true;

                    if (this.Visible)
                    {
                        this.Invoke(new ParameterlessDelegate(this.CloseForm));
                    }
                }
            }
            finally
            {
                progressLock.ExitWriteLock();
            }
        }

        private void UpdateProgressBar()
        {
            progressBar.Value = threadsFinished;
        }

        private void SetThreadNameLabel(string label)
        {
            if (shutdownList.Count > 0)
            {
                foreach (string finishedThread in shutdownList)
                {
                    finishedThreads.Items.Add(label + " stopped.");
                }
                shutdownList.Clear();
            }

            finishedThreads.Items.Add(label + " stopped.");
        }

        private void CloseForm()
        {
            this.Close();
        }

        private void ShutdownFormActivated(object sender, EventArgs e)
        {
            if (closeForm)
            {
                this.Close();
            }
        }

        #endregion
    }
}