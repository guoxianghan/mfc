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
using SNTON.Components.MessageInfo;
using SNTON.Components.SystemParameters;
using SNTON.Components.Spools;
using SNTON.Components.AGV;
using SNTON.Components.Equipment;
using SNTON.Components.MidStorage;
using VI.MFC.Components.Sequencer.PersistentSequencer;
using SNTON.Components.Config;
using SNTON.Components.MES;
using SNTON.Components.RobotArm;
using SNTON.Components.ComLogic;
using SNTON.Components.SQLCommand;
using SNTON.Components.PLCAddressCode;
using SNTON.Components.InStoreToOutStore;

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
        [ConfigBoundProperty("ConfigValueProviderId")]
        private string configValueProviderId;
#pragma warning restore 649

        private IConfigValues configValuesProvider;

        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IConfigValues ConfigValuesProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref configValuesProvider, configValueProviderId, this);

                return configValuesProvider;
            }
            set { configValuesProvider = value; }
        }

        [ConfigBoundProperty("tblProdCodeStructMachProviderId")]
#pragma warning disable 649
        private string tblProdCodeStructMachId;
#pragma warning restore 649
        private ItblProdCodeStructMach tblProdCodeStructMach;
        public ItblProdCodeStructMach tblProdCodeStructMachProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref tblProdCodeStructMach, tblProdCodeStructMachId, this);
                return tblProdCodeStructMach;
            }
            set { tblProdCodeStructMach = value; }
        }
        [ConfigBoundProperty("tblProdCodeStructMarkProviderId")]
#pragma warning disable 649
        private string tblProdCodeStructMarkId;
#pragma warning restore 649
        private ItblProdCodeStructMark tblProdCodeStructMark;
        public ItblProdCodeStructMark ItblProdCodeStructMarkProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref tblProdCodeStructMark, tblProdCodeStructMarkId, this);
                return tblProdCodeStructMark;
            }
            set { tblProdCodeStructMark = value; }
        }

        [ConfigBoundProperty("MESSystemProviderId")]
#pragma warning disable 649
        private string MESSystemProviderId;
#pragma warning restore 649
        private IMESSystemSpools mesProvider;
        public IMESSystemSpools MESSystemProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref mesProvider, MESSystemProviderId, this);
                return mesProvider;
            }
            set { mesProvider = value; }
        }
        #region EquipConfiger2
        [ConfigBoundProperty("EquipConfiger2ProviderId")]
#pragma warning disable 649
        private string EquipConfiger2ProviderId;
#pragma warning restore 649
        private IEquipConfiger2 equipConfiger2Provider;
        public IEquipConfiger2 EquipConfiger2Provider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipConfiger2Provider, EquipConfiger2ProviderId, this);
                return equipConfiger2Provider;
            }
            set { equipConfiger2Provider = value; }
        }

        #endregion
        [ConfigBoundProperty("RobotArmTaskSpoolProviderId")]
#pragma warning disable 649
        private string RobotArmTaskSpoolProviderId;
#pragma warning restore 649
        private IRobotArmTaskSpool armSpoolProvider;
        public IRobotArmTaskSpool RobotArmTaskSpoolProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref armSpoolProvider, RobotArmTaskSpoolProviderId, this);
                return armSpoolProvider;
            }
            set { armSpoolProvider = value; }
        }

        #region guo
        #region AGVConfig
#pragma warning disable 649
        [ConfigBoundProperty("AGVConfigProviderId")]
        private string aGVConfigProviderId;
#pragma warning restore 649

        private IAGVConfig aGVConfigProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IAGVConfig AGVConfigProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref aGVConfigProvider, aGVConfigProviderId, this);

                return aGVConfigProvider;
            }
            set { aGVConfigProvider = value; }
        }
        #endregion

        [ConfigBoundProperty("MidStoreLineLogicId1")]
#pragma warning disable 649
        private string _MidStoreLineLogic1;
#pragma warning restore 649
        private MidStoreLineLogic midStoreLineLogic1;
        public MidStoreLineLogic MidStoreLineLogic1
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref midStoreLineLogic1, _MidStoreLineLogic1, this);
                return midStoreLineLogic1;
            }
        }


        #region 龙门
        [ConfigBoundProperty("PLCRobotArm1Logic")]
#pragma warning disable 649
        private string _RobotArmLogic1;
#pragma warning restore 649
        private MidStoreRobotArmLogic robotArmLogic1;
        public MidStoreRobotArmLogic RobotArmLogic1
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref robotArmLogic1, _RobotArmLogic1, this);
                return robotArmLogic1;
            }
        }
        [ConfigBoundProperty("PLCRobotArm2Logic")]
#pragma warning disable 649
        private string _RobotArmLogic2;
#pragma warning restore 649
        private MidStoreRobotArmLogic robotArmLogic2;
        public MidStoreRobotArmLogic RobotArmLogic2
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref robotArmLogic2, _RobotArmLogic2, this);
                return robotArmLogic2;
            }
        }

        [ConfigBoundProperty("PLCRobotArm3Logic")]
#pragma warning disable 649
        private string _RobotArmLogic3;
#pragma warning restore 649
        private MidStoreRobotArmLogic robotArmLogic3;
        public MidStoreRobotArmLogic RobotArmLogic3
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref robotArmLogic3, _RobotArmLogic3, this);
                return robotArmLogic3;
            }
        }


        #endregion
        [ConfigBoundProperty("EquipTaskLogicProviderid")]
#pragma warning disable 649
        private string EquipTaskLogicProviderid;
#pragma warning restore 649
        private EquipTaskLogic _EquipTaskLogic;
        public EquipTaskLogic EquipTaskLogic
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref _EquipTaskLogic, EquipTaskLogicProviderid, this);
                return _EquipTaskLogic;
            }
        }

        [ConfigBoundProperty("MidStoreLineLogicId2")]
#pragma warning disable 649
        private string _MidStoreLineLogic2;
#pragma warning restore 649
        private MidStoreLineLogic midStoreLineLogic2;
        public MidStoreLineLogic MidStoreLineLogic2
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref midStoreLineLogic2, _MidStoreLineLogic2, this);
                return midStoreLineLogic2;
            }
        }
        [ConfigBoundProperty("MidStoreLineLogicId3")]
#pragma warning disable 649
        private string _MidStoreLineLogic3;
#pragma warning restore 649
        private MidStoreLineLogic midStoreLineLogic3;
        public MidStoreLineLogic MidStoreLineLogic3
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref midStoreLineLogic3, _MidStoreLineLogic3, this);
                return midStoreLineLogic3;
            }
        }
        public MidStoreLineLogic GetMidStoreLineLogic(int storeageno)
        {
            switch (storeageno)
            {
                case 1:
                    return MidStoreLineLogic1;
                case 2:
                    return MidStoreLineLogic2;
                case 3:
                    return MidStoreLineLogic3;
                default:
                    throw new ArgumentNullException("未知的lineno,没有找到对应的MidStoreLineLogic");
            }
        }
        public MidStoreRobotArmLogic GetMidStoreRobotArmLogic(int storeageno)
        {
            switch (storeageno)
            {
                case 1:
                    return RobotArmLogic1;
                case 2:
                    return RobotArmLogic2;
                case 3:
                    return RobotArmLogic3;
                default:
                    throw new ArgumentNullException("未知的lineno,MidStoreRobotArmLogic");
            }
        }

        #region MidStorage

        #endregion
        #region AGVMagMarkerMapConfig
#pragma warning disable 649
        [ConfigBoundProperty("AGVMagMarkerMapConfigProviderId")]
        private string aGVMagMarkerMapConfigProviderId;
#pragma warning restore 649

        private IAGVMagMarkerMapConfig aGVMagMarkerMapConfigProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IAGVMagMarkerMapConfig AGVMagMarkerMapConfigProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref aGVMagMarkerMapConfigProvider, aGVMagMarkerMapConfigProviderId, this);

                return aGVMagMarkerMapConfigProvider;
            }
            set { aGVMagMarkerMapConfigProvider = value; }
        }
        #endregion

        #region AGVRoute
#pragma warning disable 649
        [ConfigBoundProperty("AGVRouteProviderId")]
        private string aGVRouteProviderId;
#pragma warning restore 649

        private IAGVRoute aGVRouteProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IAGVRoute AGVRouteProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref aGVRouteProvider, aGVRouteProviderId, this);

                return aGVRouteProvider;
            }
            set { aGVRouteProvider = value; }
        }
        #endregion

        #region AGVRouteArchive
#pragma warning disable 649
        [ConfigBoundProperty("AGVRouteArchiveProviderId")]
        private string aGVRouteArchiveProviderId;
#pragma warning restore 649

        private IAGVRouteArchive aGVRouteArchiveProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IAGVRouteArchive AGVRouteArchiveProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref aGVRouteArchiveProvider, aGVRouteArchiveProviderId, this);

                return aGVRouteArchiveProvider;
            }
            set { aGVRouteArchiveProvider = value; }
        }
        #endregion

        #region AGVStatus
#pragma warning disable 649
        [ConfigBoundProperty("AGVStatusProviderId")]
        private string aGVStatusProviderId;
#pragma warning restore 649

        private IAGVStatus aGVStatusProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IAGVStatus AGVStatusProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref aGVStatusProvider, aGVStatusProviderId, this);

                return aGVStatusProvider;
            }
            set { aGVStatusProvider = value; }
        }
        #endregion
        #region ButtonLocationConfig
#pragma warning disable 649
        [ConfigBoundProperty("ButtonLocationConfigProviderId")]
        private string buttonLocationConfigProviderId;
#pragma warning restore 649

        private IButtonLocationConfig buttonLocationConfigProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IButtonLocationConfig ButtonLocationConfigProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref buttonLocationConfigProvider, buttonLocationConfigProviderId, this);

                return buttonLocationConfigProvider;
            }
            set { buttonLocationConfigProvider = value; }
        }
        #endregion
        #region Product
#pragma warning disable 649
        [ConfigBoundProperty("ProductProviderId")]
        private string productProviderId;
#pragma warning restore 649

        private IProduct productProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IProduct ProductProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref productProvider, productProviderId, this);

                return productProvider;
            }
            set { productProvider = value; }
        }
        #endregion


        #region AGVTasks
#pragma warning disable 649
        [ConfigBoundProperty("AGVTasksProviderId")]
        private string aGVTasksProviderId;
#pragma warning restore 649

        private IAGVTasks aGVTasksProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IAGVTasks AGVTasksProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref aGVTasksProvider, aGVTasksProviderId, this);

                return aGVTasksProvider;
            }
            set { aGVTasksProvider = value; }
        }
        #endregion

        #region EquipCommand

        [ConfigBoundProperty("EquipCommandProviderId")]
#pragma warning disable 649
        private string equipCommandProviderId;
#pragma warning restore 649
        private IEquipCommand equipCommandProvider;
        public IEquipCommand EquipCommandProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipCommandProvider, equipCommandProviderId, this);

                return equipCommandProvider;
            }
            set { equipCommandProvider = value; }
        }
        #endregion

        #region EquipConfig
#pragma warning disable 649
        [ConfigBoundProperty("EquipConfigProviderId")]
        private string equipConfigProviderId;
#pragma warning restore 649

        private IEquipConfig equipConfigProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IEquipConfig EquipConfigProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipConfigProvider, equipConfigProviderId, this);

                return equipConfigProvider;
            }
            set { equipConfigProvider = value; }
        }
        #endregion



        #region product
        #region InStoreToOutStoreSpool
#pragma warning disable 649
        [ConfigBoundProperty("InStoreToOutStoreSpoolViewProviderId")]
        private string inStoreToOutStoreSpoolViewProviderId;
#pragma warning restore 649

        private IInStoreToOutStoreSpoolView inStoreToOutStoreSpoolViewProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IInStoreToOutStoreSpoolView InStoreToOutStoreSpoolViewProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref inStoreToOutStoreSpoolViewProvider, inStoreToOutStoreSpoolViewProviderId, this);

                return inStoreToOutStoreSpoolViewProvider;
            }
            set { inStoreToOutStoreSpoolViewProvider = value; }
        }
        #endregion


        #endregion



        #region EquipControllerConfig
#pragma warning disable 649
        [ConfigBoundProperty("EquipControllerConfigProviderId")]
        private string equipControllerConfigProviderId;
#pragma warning restore 649

        private IEquipControllerConfig equipControllerConfigProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IEquipControllerConfig EquipControllerConfigProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipControllerConfigProvider, equipControllerConfigProviderId, this);

                return equipControllerConfigProvider;
            }
            set { equipControllerConfigProvider = value; }
        }
        #endregion

        #region EquipProduction
#pragma warning disable 649
        [ConfigBoundProperty("EquipProductionProviderId")]
        private string equipProductionProviderId;
#pragma warning restore 649

        private IEquipProduction equipProductionProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IEquipProduction EquipProductionProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipProductionProvider, equipProductionProviderId, this);

                return equipProductionProvider;
            }
            set { equipProductionProvider = value; }
        }
        #endregion

        #region EquipStatus
#pragma warning disable 649
        [ConfigBoundProperty("EquipStatusProviderId")]
        private string equipStatusProviderId;
#pragma warning restore 649

        private IEquipStatus equipStatusProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IEquipStatus EquipStatusProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipStatusProvider, equipStatusProviderId, this);

                return equipStatusProvider;
            }
            set { equipStatusProvider = value; }
        }
        #endregion

        [ConfigBoundProperty("EquipTaskView2ProviderId")]
#pragma warning disable 649
        private string equipTaskView2ProviderId;
#pragma warning restore 649
        private IEquipTaskView2 equipTaskView2Provider;
        public IEquipTaskView2 EquipTaskView2Provider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipTaskView2Provider, equipTaskView2ProviderId, this);

                return equipTaskView2Provider;
            }
            set { equipTaskView2Provider = value; }
        }

        #region EquipTask
#pragma warning disable 649
        [ConfigBoundProperty("EquipTaskProviderId")]
        private string equipTaskProviderId;
#pragma warning restore 649

        private IEquipTask equipTaskProvider;

        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IEquipTask EquipTaskProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipTaskProvider, equipTaskProviderId, this);

                return equipTaskProvider;
            }
            set { equipTaskProvider = value; }
        }
        #endregion
        #region EquipTask
#pragma warning disable 649
        [ConfigBoundProperty("MachineWarnningCodeProviderId")]
        private string machineWarnningCodeProviderId;
#pragma warning restore 649

        private IMachineWarnningCode machineWarnningCodeProvider;

        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IMachineWarnningCode MachineWarnningCodeProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref machineWarnningCodeProvider, machineWarnningCodeProviderId, this);

                return machineWarnningCodeProvider;
            }
            set { machineWarnningCodeProvider = value; }
        }
        #endregion

        #region SQLCommandProviderid
        [ConfigBoundProperty("SQLCommandProviderid")]
#pragma warning disable 649
        private string sqlCommandProviderid;
#pragma warning restore 649
        private ISQLCommandBroker sqlcommandProvider;
        public ISQLCommandBroker SqlCommandProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref sqlcommandProvider, sqlCommandProviderid, this);
                return sqlcommandProvider;
            }
            set { sqlcommandProvider = value; }
        }
        #endregion

        [ConfigBoundProperty("RobotArmTaskProviderid")]
#pragma warning disable 649
        private string robotArmTaskProviderid;
#pragma warning restore 649
        private IRobotArmTask robotarmTaskProvider;
        public IRobotArmTask RobotArmTaskProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref robotarmTaskProvider, robotArmTaskProviderid, this);
                return robotarmTaskProvider;
            }
            set { robotarmTaskProvider = value; }
        }
        [ConfigBoundProperty("EquipTaskViewProviderid")]
#pragma warning disable 649
        private string equipTaskViewProviderid;
#pragma warning restore 649
        private IEquipTaskView equipTaskViewProvider;
        public IEquipTaskView EquipTaskViewProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipTaskViewProvider, equipTaskViewProviderid, this);
                return equipTaskViewProvider;
            }
            set { equipTaskViewProvider = value; }
        }
        [ConfigBoundProperty("EquipConfigerProviderid")]
#pragma warning disable 649
        private string equipConfigerProviderid;
#pragma warning restore 649
        private IEquipConfiger equipConfigerProvider;
        public IEquipConfiger EquipConfigerProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipConfigerProvider, equipConfigerProviderid, this);
                return equipConfigerProvider;
            }
            set { equipConfigerProvider = value; }
        }
        [ConfigBoundProperty("EquipTaskproductProviderid")]
#pragma warning disable 649
        private string equiptaskproductProviderid;
#pragma warning restore 649
        private IEquipTaskProduct equipTaskProductProvider;
        public IEquipTaskProduct EquipTaskProductProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref equipTaskProductProvider, equiptaskproductProviderid, this);
                return equipTaskProductProvider;
            }
            set { equipTaskProductProvider = value; }
        }
        #region Message
#pragma warning disable 649
        [ConfigBoundProperty("MessageProviderId")]
        private string messageProviderId;
#pragma warning restore 649

        private IMessageInfo messageProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IMessageInfo MessageInfoProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref messageProvider, messageProviderId, this);

                return messageProvider;
            }
            set { messageProvider = value; }
        }
        #endregion

        #region MidStorage
#pragma warning disable 649
        [ConfigBoundProperty("MidStorageProviderId")]
        private string midStorageProviderId;
#pragma warning restore 649

        private IMidStorage midStorageProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IMidStorage MidStorageProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref midStorageProvider, midStorageProviderId, this);

                return midStorageProvider;
            }
            set { midStorageProvider = value; }
        }
        #endregion

        #region IInStoreToOutStoreSpool
#pragma warning disable 649
        [ConfigBoundProperty("InStoreToOutStoreSpoolProviderId")]
        private string inStoreToOutStoreSpoolProviderId;
#pragma warning restore 649

        private IInStoreToOutStoreSpool inStoreToOutStoreSpoolProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IInStoreToOutStoreSpool InStoreToOutStoreSpoolProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref inStoreToOutStoreSpoolProvider, inStoreToOutStoreSpoolProviderId, this);

                return inStoreToOutStoreSpoolProvider;
            }
            set { inStoreToOutStoreSpoolProvider = value; }
        }
        #endregion

        #region Spools 
        [ConfigBoundProperty("SpoolsProviderId")]
#pragma warning disable 649
        private string spoolsProviderId;
#pragma warning restore 649
        private ISpools spoolsProvider;

        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public ISpools SpoolsProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref spoolsProvider, spoolsProviderId, this);

                return spoolsProvider;
            }
            set { spoolsProvider = value; }
        }
        #endregion
        #region IMidStorageSpools 
        [ConfigBoundProperty("MidStorageSpoolsProviderId")]
#pragma warning disable 649
        private string MidStorageSpoolsProviderId;
#pragma warning restore 649
        private IMidStorageSpools midStorageSpoolsProvider;

        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public IMidStorageSpools MidStorageSpoolsProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref midStorageSpoolsProvider, MidStorageSpoolsProviderId, this);
                return midStorageSpoolsProvider;
            }
            set { midStorageSpoolsProvider = value; }
        }
        #endregion

        #region SystemParameters
#pragma warning disable 649
        [ConfigBoundProperty("SystemParametersProviderId")]
        private string systemParametersProviderId;
#pragma warning restore 649

        private ISystemParameters systemParametersProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public ISystemParameters SystemParametersProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref systemParametersProvider, systemParametersProviderId, this);

                return systemParametersProvider;
            }
            set { systemParametersProvider = value; }
        }
        #endregion

        #region SystemParametersConfiguration
#pragma warning disable 649
        [ConfigBoundProperty("SystemParametersConfigurationProviderId")]
        private string systemParametersConfigurationProviderId;
#pragma warning restore 649

        private ISystemParametersConfiguration systemParametersConfigurationProvider;
        /* brokerNode

		*/
        /// <summary>
        /// Instance of the configValues
        /// </summary>
        public ISystemParametersConfiguration SystemParametersConfigurationProvider
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref systemParametersConfigurationProvider, systemParametersConfigurationProviderId, this);

                return systemParametersConfigurationProvider;
            }
            set { systemParametersConfigurationProvider = value; }
        }
        #endregion


        #endregion
        #endregion Dependencies



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
