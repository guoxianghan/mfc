using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VI.MFC.COM;
using System.Xml;
using VI.MFC.Components.Packetizer;
using VI.MFC.Components.Parser;
using VI.MFC.Components.Sequencer.Sequencehandler;
using VI.MFC.Logic;
using log4net;
using System.Reflection;
//using MITSUBISHI.Component;
using VI.MFC.Logging;
using VI.MFC.Utils;
using System.Windows.Forms;
//using AxActUtlTypeLib;
using ActProgTypeLib;
using SNTON.Constants;

namespace SNTON.Com
{
    public class MXTPLCClient : MXPLCClient, IMXCommModule
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //Add new virtual method to set connection parameters
        //By Song@2018.01.15.
        protected override void SetConnectionParameters()
        {
            if (actProgProvider != null)
            {
                actProgProvider.ActHostAddress = HostAddress;
                //actProgProvider.ActPortNumber = this.Port;
                actProgProvider.ActCpuType = CpuType;
                actProgProvider.ActUnitType = UnitType;
                actProgProvider.ActProtocolType = ProtocolType;
                //actProgProvider.ActTimeOut = ComTimeout;
            }
        }
    }
}