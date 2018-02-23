// Copyright (c) 2009 - 2015 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of 
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Collections.Generic;
using System.Reflection;
using SNTON.Constants;
using SNTON.Entities.DBTables;
using log4net;
using NHibernate;
using NHibernate.Mapping;
using VI.MFC.Broker;
using VI.MFC.Logging;
using VI.MFC.Utils;
using VI.MFC.Utils.ConfigBinder;
// ReSharper disable InconsistentNaming

namespace SNTON.Components.CleanUp
{
    /// <summary>
    /// Broker for general cleanup/housekeeping
    /// </summary>
    public abstract class CleanUpBrokerBase : BrokerBaseEx
    {
        #region Properties

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

#pragma warning disable 649

        [ConfigBoundProperty("HousekeepingCleanupAfterDays")]
        private string housekeepingCleanupAfterDays;

        [ConfigBoundProperty("HousekeepingCleanupAfterMins")]
        private string housekeepingCleanupAfterMins;

        [ConfigBoundProperty("HousekeepingFrequencyInMs")]
        private string housekeepingFrequencyStr;

        [ConfigBoundProperty("HousekeepingCleanupMaxRecords")]
        private string housekeepingCleanupMaxRecordsStr;

        [ConfigBoundProperty("HousekeepingEnabled")]
        private bool housekeepingEnabled;

        [ConfigBoundProperty("CachingEnabled")]
        protected bool cachingEnabled;

#pragma warning restore 649

        private VIThreadEx cleanupThread;

        private bool useCleanupAfterMins;

        private const int ThreadShutdownTimeout = 5000; // 5 seconds
        private int cleaningThreadTimeout;
        private int cleaningThreadDeleteOlderThan;
        private int cleaningThreadDeleteMaxRecords;

        protected virtual string EntityTableName {get; }

        #endregion Properties

        #region Helper classes

        /// <summary>
        /// Helper class which logs error and throws new ArgumentException
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentException">Throw the message</exception>
        protected void ThrowArgumenException(String message)
        {
            logger.ErrorMethod(message);
            throw new ArgumentException(message);
        }

        protected virtual void MarkDataForDeletion(DateTime olderThan, int threadDeleteMaxRecords, IStatelessSession theSession = null)
        {
            logger.InfoMethod("Nothing to be marked deleted.");
        }
        /// <summary>
        /// Executes delete statement for entity class
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="olderThan"></param>
        /// <param name="numRows"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        protected virtual int DeleteDataMarkedDeleted(IStatelessSession session = null)
        {
            var query = string.Format("Delete {0} where IsDeleted = :tag", EntityTableName);

            return RunHibernateStatement(session, query, new { tag = SNTONConstants.DeletedTag.Deleted });
        }
        #endregion Helper classes

        #region Base class overrides

        /// <summary>
        /// Validate the parameters we just read in.
        /// </summary>
        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            if (!housekeepingEnabled)
            {
                housekeepingEnabled = false;
                logger.WarnMethod("Housekeeping disabled!");
            }
            else
            {
                if (String.IsNullOrEmpty(housekeepingCleanupAfterDays) &&
                    String.IsNullOrEmpty(housekeepingCleanupAfterMins))
                {
                    ThrowArgumenException("Please specify a valid HousekeepingCleanupAfterDays or HousekeepingCleanupAfterMins");
                }

                if (!String.IsNullOrEmpty(housekeepingCleanupAfterDays) &&
                    !String.IsNullOrEmpty(housekeepingCleanupAfterMins))
                {
                    ThrowArgumenException("Please specify only one from HousekeepingCleanupAfterDays or HousekeepingCleanupAfterMins");
                }

                try
                {
                    cleaningThreadDeleteMaxRecords = Convert.ToInt32(housekeepingCleanupMaxRecordsStr);
                }
                catch (Exception)
                {
                    ThrowArgumenException("Please specify a valid HousekeepingCleanupMaxRecordsStr");
                }
                if (cleaningThreadDeleteMaxRecords < 0)
                {
                    ThrowArgumenException("Please specify a valid HousekeepingFrequencyInMs");
                }

                try
                {
                    cleaningThreadTimeout = Convert.ToInt32(housekeepingFrequencyStr);
                }
                catch (Exception)
                {
                    ThrowArgumenException("Please specify a valid HousekeepingFrequencyInMs");
                }
                if (cleaningThreadTimeout <= 0)
                {
                    ThrowArgumenException("Please specify a valid HousekeepingFrequencyInMs");
                }

                var strValue = housekeepingCleanupAfterDays;
                if (String.IsNullOrEmpty(strValue))
                {
                    strValue = housekeepingCleanupAfterMins;
                    useCleanupAfterMins = true;
                }
                try
                {
                    cleaningThreadDeleteOlderThan = Convert.ToInt32(strValue);
                }
                catch (Exception)
                {
                    ThrowArgumenException("Please specify a valid HousekeepingCleanupAfterDays or HousekeepingCleanupAfterMins. Value must be a number");
                }
                if (cleaningThreadDeleteOlderThan < 0)
                {
                    ThrowArgumenException("Please specify a valid HousekeepingCleanupAfterDays or HousekeepingCleanupAfterMins. Value must be a number");
                }
            }
        }

        protected override void StartInternal()
        {
            if (housekeepingEnabled)
            {
                base.StartInternal();
                StartCleanupThread();
            }
        }

        /// <summary>
        /// Exiting this broker
        /// </summary>
        public override void Exit()
        {
            if (cleanupThread != null)
            {
                cleanupThread.Stop(ThreadShutdownTimeout);
            }

            base.Exit();
        }

        #endregion Base class overrides

        #region Cleanup thread

        void StartCleanupThread()
        {
            // Create and start a new cleanup thread, responsible for cleaning deleted records from the table.
            cleanupThread = new VIThreadEx(CleanupRun, null, "Housekeeping thread (Id: " + GetId() + ")", cleaningThreadTimeout, 0);
            cleanupThread.Start();
        }

        private void CleanupRun()
        {
            var date = useCleanupAfterMins ? DateTime.UtcNow.AddMinutes(-cleaningThreadDeleteOlderThan)
                                        : DateTime.UtcNow.AddDays(-cleaningThreadDeleteOlderThan);
            logger.InfoMethod(
                String.Format("Housekeeping started. Cleanup records older than {0} (UTC).", date));
            try
            {
                MarkDataForDeletion(date, cleaningThreadDeleteMaxRecords);
                DeleteDataMarkedDeleted();
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Exception in the Cleanup thread.", e);
            }
        }
        #endregion Cleanup thread
    }
}
