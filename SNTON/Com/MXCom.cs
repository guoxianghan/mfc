using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using VI.MFC.Components;
using VI.MFC.Components.Parser;
using VI.MFC.Utils.ConfigBinder;
using VI.MFC;
using SNTON.Components.Parser;
using VI.MFC.Logic;
//using MITSUBISHI.Component;
using VI.MFC.Utils;
using System.Xml;

namespace SNTON.Com
{
    public abstract class MXCom : VIRuntimeComponent, IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly IList<ILogic> logicList = new List<ILogic>();

        [ConfigBoundProperty("CpuType")]
        protected int CpuType { get; set; }
        [ConfigBoundProperty("HostAddress")]
        protected string HostAddress { get; set; }
        [ConfigBoundProperty("Port")]
        protected int Port { get; set; }
        [ConfigBoundProperty("WritePort")]
        protected int WritePort { get; set; }

        [ConfigBoundProperty("ProtocolType")]
        protected int ProtocolType { get; set; }
        [ConfigBoundProperty("ComTimeout")]
        protected int ComTimeout { get; set; }
        [ConfigBoundProperty("UnitType")]
        protected int UnitType { get; set; }

        [ConfigBoundProperty("ActLogicalStationNumber")]
        protected int ActLogicalStationNumber { get; set; }
        [ConfigBoundProperty("ActPassword")]
        protected string ActPassword { get; set; }
        //private VIThreadEx checkFlagBitThread;
        protected string connectionName;


        [ConfigBoundProperty("Parser")]
        private string parserId = null;
        private IParser parserInstance;
        //private int ThreadShutdownTimeout = 5000;
        //private int tcheckFlagBitThreadTimeout = 5000;
        #region Property defination
        protected bool IsComConnected
        {
            get;
            set;
        } = false;
        #endregion
        protected override void ReadParameters(XmlNode configNode)
        {
            base.ReadParameters(configNode);
        }
        public IParser Parser
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref parserInstance, parserId);
                return parserInstance;
            }
        }
        #region IDisposable Interface implementation
        public void Dispose()
        {
            Dispose(true);
        }
        //public override void Exit()
        //{
        //    //if (checkFlagBitThread != null)
        //    //{
        //    //    checkFlagBitThread.Stop(ThreadShutdownTimeout);
        //    //}
        //    base.Exit();
        //}
        public virtual void Dispose(bool doDispose)
        {
            if (doDispose)
            {
                //Do the dispose work
                //By Song@2018.01.15
            }
        }
        #endregion
        protected virtual void OnDisconnect()
        {
            if (logicList.Count == 0)
            {
                logger.ErrorFormat("No logic for Com {0}", GetId());
                return;
            }
            foreach (var logic in logicList)
            {
                //Do disconnection action in the logic
                //By Song@2018.01.15
                logic.OnDisconnect();
            }
        }
        protected override void StartInternal()
        {
            base.StartInternal();
        }
        protected virtual void OnConnect()
        {

            if (logicList.Count == 0)
            {
                logger.ErrorFormat("No logic for Com {0}", GetId());
                return;
            }
            foreach (var logic in logicList)
            {
                //Do connection action in the logic
                logic.OnConnect();
            }
        }
    }
}
