using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VI.MFC.Logic;
using VI.MFC.Utils.ProcessQueue;
using VI.MFC.COM;
//using MITSUBISHI.Component;
using ActProgTypeLib;

namespace SNTON.Components.ACTLogic
{
    public class ACTLogicBase : VILogic
    {
        protected static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected ACTLogicBase(ProcessQueueHandler<WorkItem> handler) : base(handler)
        { 
        }
        //Set the value of 'LogicalStationNumber' to the property.
        public int ActLogicalStationNumber { get; set; }

        //Set the value of 'Password'.
        public string ActPassword { get; set; }
        public ACTLogicBase() : base(null)
        {
            actProgProvider = new ActProgTypeClass();
            actProgProvider.ActPassword = ActPassword;
            actProgProvider.ActStationNumber = ActLogicalStationNumber;

        }
        ActProgType actProgProvider;
         
        public ActProgType ActProgProvider
        {
            get
            {
                return actProgProvider;
            }

            set
            {
                actProgProvider = value;
            }
        }

        public override bool Send(Neutrino sendData)
        {
            string cmd = sendData.TheName.Trim();
            short lpsData = 0;
            int IStartIO = 0;//写入值的模块的 I/O 编号 
            int IAddress = 0;//缓冲存储器的地址
            int iRet = 0, lplData = 0, lSize = 0;
            string szDevice = "";
            string szDeviceList = "";
            //cmd 命令类型 
            /*
            int WriteDeviceBlock(string szDevice, int lSize, ref int lplData);       
            int WriteDeviceBlock2(string szDevice, int lSize, ref short lpsData);        
            int WriteDeviceRandom(string szDeviceList, int lSize, ref int lplData);       
            int WriteDeviceRandom2(string szDeviceList, int lSize, ref short lpsData);
            */
            if (sendData.FieldExists("lplData"))
                lplData = sendData.GetInt("lplData");
            if (sendData.FieldExists("lSize"))
                lSize = sendData.GetInt("lSize");
            if (sendData.FieldExists("IAddress"))
                IAddress = sendData.GetInt("IAddress");
            if (sendData.FieldExists("IStartIO"))
                IStartIO = sendData.GetInt("IStartIO");
            if (sendData.FieldExists("szDevice"))
                szDevice = sendData.GetField("szDevice");
            if (sendData.FieldExists("szDeviceList"))
                szDeviceList = sendData.GetField("szDeviceList");
            if (sendData.FieldExists("lpsData"))
            {
                string va = sendData.GetFieldOrDefault("lpsData", "0");
                lpsData = Convert.ToInt16(va);
            }
            switch (cmd)
            {
                case "WriteDeviceBlock":
                    //软元件的批量写入    
                    iRet = actProgProvider.WriteDeviceBlock(szDevice, lSize, ref lplData);
                    break;
                case "WriteDeviceBlock2":
                    //软元件的批量写入          
                    iRet = actProgProvider.WriteDeviceBlock2(szDevice, lSize, ref lpsData);
                    break;
                case "WriteDeviceRandom":
                    //软元件的随机写入
                    iRet = actProgProvider.WriteDeviceRandom(szDeviceList, lSize, ref lplData);
                    break;
                case "WriteDeviceRandom2":
                    //软元件的随机写入
                    iRet = actProgProvider.WriteDeviceRandom2(szDeviceList, lSize, ref lpsData);
                    break;
                case "WriteBuffer":
                    //缓冲存储器写入
                    iRet = actProgProvider.WriteBuffer(IStartIO, IAddress, lSize, ref lpsData);
                    break;
                default:
                    logger.Error("unknow ACT command:" + cmd);
                    break;
            }
            if (iRet != 0)
            {
                logger.Error("");
            }
            return base.Send(sendData);
        }
        protected override bool ProcessingCallHandler(WorkItem item)
        {
            return base.ProcessingCallHandler(item);
        }
        public override void OnConnect()
        {
            actProgProvider.Open();
            actProgProvider.Connect();
            base.OnConnect();
        }
        public override void OnDisconnect()
        {
            actProgProvider.Disconnect();
            base.OnDisconnect();
        }

    }
}
