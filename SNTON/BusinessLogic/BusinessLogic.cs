// Copyright (c) 2016 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Reflection;
using System.Xml;
using SNTON.Constants;
using log4net;
using NHibernate;
using VI.MFC;
using VI.MFC.Components;
using VI.MFC.Components.ConfigValues;
using VI.MFC.Components.Hibernate;
using VI.MFC.Components.Parser;
using VI.MFC.Logging;
using VI.MFC.Utils.ConfigBinder;

using VI.MFC.Components.Sequencer.PersistentSequencer;

using SNTON.Components.LockManager;

namespace SNTON.BusinessLogic
{
    /// <summary>
    /// This is the one and only business logic for FedEx. It will only be instantiated once on
    /// startup and it has to be used for any kind of transactional behaviour.
    /// Always use this class for any functionality requireing a transactional scope.
    /// </summary>
    public partial class BusinessLogic : VIRuntimeComponent
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Default constructor
        /// </summary>
        protected BusinessLogic()
        {

        }

        /// <summary>
        /// Constructor. Only use this for unittesting (Constructor Dependency Injection).
        /// Do not instantiate this class directly for any other purpose than unittesting using "new".
        /// </summary>
        //public BusinessLogic(IBarcodeAnalyzer barcodeAnalyzer) : base()
        //{
        //if (barcodeAnalyzer != null)    // we are not using the Glue for mocking...
        //{
        //BarcodeAnalyzer = barcodeAnalyzer;
        //}
        //}
        #region Dependencies
        
        



        /// <summary>
        /// The Hibernate Session Pool of the VI database to use as specified within the .XML configuration
        /// </summary>
        [ConfigBoundProperty("HibernateSessionPoolToUse")]
#pragma warning disable 649
        private string hibernateSessionPoolId;
#pragma warning restore 649
        protected IDbSessionPool sessionPool;

        public IDbSessionPool SessionPool
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref sessionPool, hibernateSessionPoolId);
                return sessionPool;
            }
            set { sessionPool = value; }
        }
        [ConfigBoundProperty("fileBasedSequencerId")]
#pragma warning disable 649
        private string fileBasedSequencerId;
        protected FileBasedSequencer sequencer;
        public VI.MFC.Components.Sequencer.PersistentSequencer.FileBasedSequencer Sequencer
        {
            get 
            {
                Kernel.Glue.RetrieveComponentInstance(ref sequencer, fileBasedSequencerId);
                return sequencer;
            }
            set { sequencer = value; }
        }
         
         




#pragma warning disable 649
        [ConfigBoundProperty("LockManagerId")]
        private string lockManagerId;
#pragma warning restore 649

        private ILockManager lockManagerIdProvider;

        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public ILockManager LockManagerProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref lockManagerIdProvider, lockManagerId, this);

                return lockManagerIdProvider;
            }
            set { lockManagerIdProvider = value; }
        }






        #endregion
        //#endregion Dependencies



        #region IVIRuntimeImplementation

        public override string GetInfo()
        {
            return "Business Logic V1.0, ready to serve.";
        }

        public static BusinessLogic Create(XmlNode configNode)
        {
            var module = new BusinessLogic();
            module.Init(configNode);
            return module;
        }

        protected override void ValidateParameters()
        {
            if (string.IsNullOrWhiteSpace(hibernateSessionPoolId))
            {
                throw new ArgumentException("Please specify a valid 'HibernateSessionPoolToUse'.");
            }

        }

        #endregion

        #region Example Templates
        /// <summary>
        /// This is an example of how to call or query components
        /// in a transactional scope. 
        /// Please use as a template for similar tasks.
        /// </summary>
        private void TransactionalExample()
        {
            IStatelessSession theSession = null;
            try
            {
                // Name of the transaction for statistics only
                theSession = SessionPool.GetStatelessSession("TransactionalExample");
                bool retry;
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        retry = false;
                        // Do whatever you like using 'theSession' here... 
                        // eg. call a sequence of different components
                        theSession.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(e, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                }
                while (retry);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Exception occured. ", e);
                throw;
            }
            finally
            {
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }
        }
        #endregion
    }
}
