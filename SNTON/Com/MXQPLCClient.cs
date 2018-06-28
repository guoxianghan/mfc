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
    public class MXQPLCClient : MXPLCClient
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //Add new virtual method to set connection parameters
        //By Song@2018.01.15.
        protected override void SetConnectionParameters()
        {
            if (actProgProvider != null)
            {
                actProgProvider.ActUnitType = 0x002C;
                actProgProvider.ActProtocolType = 0x0005;
                actProgProvider.ActNetworkNumber = 0;
                actProgProvider.ActUnitNumber = 0;
                actProgProvider.ActConnectUnitNumber = 0;
                actProgProvider.ActIONumber = 0x03FF;
                actProgProvider.ActHostAddress = this.HostAddress;
                actProgProvider.ActCpuType = 0x90;
                #region Other property setting
                //actProgProvider.ActStationNumber = 255;
                //actProgProvider.ActTimeOut = ComTimeout;
                //actProgProvider.ActPortNumber = 0;
                //actProgProvider.ActBaudRate = 0;
                //actProgProvider.ActDataBits = 0x0000;
                //actProgProvider.ActParity = 0x0000;
                //actProgProvider.ActStopBits = 0x0000;
                //actProgProvider.ActControl = 0x0000;
                //actProgProvider.ActCpuTimeOut = 0;
                //actProgProvider.ActTimeOut = 60000;
                //actProgProvider.ActSumCheck = 0x0000;
                //actProgProvider.ActSourceNetworkNumber = 0;
                //actProgProvider.ActSourceStationNumber = 0;
                //actProgProvider.ActDestinationPortNumber = 0x138F;
                //actProgProvider.ActDestinationIONumber = 0x0000;
                //actProgProvider.ActMultiDropChannelNumber = 0;
                //actProgProvider.ActThroughNetworkType = 0x0000;
                //actProgProvider.ActIntelligentPreferenceBit = 0x0000;
                //actProgProvider.ActDidPropertyBit = 0x0001;
                //actProgProvider.ActDsidPropertyBit = 0x0001;
                //actProgProvider.ActPacketType = 0x0001;
                //actProgProvider.ActConnectWay = 0;
                //actProgProvider.ActDialNumber = "";
                //actProgProvider.ActOutsideLineNumber = "";
                //actProgProvider.ActCallbackNumber = "";
                //actProgProvider.ActLineType = 0;
                //actProgProvider.ActConnectionCDWaitTime = 0;
                //actProgProvider.ActConnectionModemReportWaitTime = 0;
                //actProgProvider.ActDisconnectionCDWaitTime = 0;
                //actProgProvider.ActDisconnectionDelayTime = 0;
                //actProgProvider.ActTransmissionDelayTime = 0;
                //actProgProvider.ActATCommandResponseWaitTime = 0;
                //actProgProvider.ActPasswordCancelResponseWaitTime = 0;
                //actProgProvider.ActATCommandPasswordCancelRetryTimes = 0;
                //actProgProvider.ActCallbackCancelWaitTime = 0;
                //actProgProvider.ActCallbackDelayTime = 0;
                //actProgProvider.ActCallbackReceptionWaitingTimeOut = 0;
                //actProgProvider.ActTargetSimulator = 0;
                #endregion
            }
        }
    }
}