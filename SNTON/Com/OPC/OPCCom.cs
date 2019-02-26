using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC;
using VI.MFC.Components;
using VI.MFC.Components.Parser;
using VI.MFC.Logic;
using VI.MFC.Utils.ConfigBinder;

namespace SNTON.Com
{
    public class OPCCom : VIRuntimeComponent, IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly IList<ILogic> logicList = new List<ILogic>();

        [ConfigBoundProperty("HostUrl")]
        protected string HostUrl { get; set; }
        [ConfigBoundProperty("Port")]
        protected int Port { get; set; }
        [ConfigBoundProperty("Parser")]
        private string parserId = null;
        private IParser parserInstance;//HostUrl
        #region Property defination

        protected string connectionName;
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
