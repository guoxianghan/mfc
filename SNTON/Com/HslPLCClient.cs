using HslCommunication;
using HslCommunication.Profinet;
using log4net;
using SNTON.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC.COM;
using VI.MFC.Components.Packetizer;
using VI.MFC.Components.Sequencer.Sequencehandler;
using VI.MFC.Logging;
using VI.MFC.Logic;

namespace SNTON.Com
{
    public class HslPLCClient : MXCom, IMXCommModule
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private object connectLocker = new object();
        public static HslPLCClient Create(XmlNode configNode)
        {
            HslPLCClient comModule = new HslPLCClient();
            comModule.Init(configNode);
            return comModule;
        }

        protected MelsecNet actProgProvider = null;
        //public DotUtlType MXDotUtlType = new DotUtlType();
        public HslPLCClient()
        {
            //_MXAxActUtlType = new AxActUtlType();
            //_axForm.Controls.Add(_MXAxActUtlType);
            //int i = _MXAxActUtlType.Open();
            //_MXAxActUtlType.ActLogicalStationNumber = 1;
            //_MXAxActUtlType.ActPassword = "";
            //actProgProvider = new ActProgTypeClass();

        }
        #region Beginog ICommModule Interface implementation
        public ISequenceHandler InboundSequencer { get; }
        public ISequenceHandler OutboundSequencer { get; }
        public IPacketizer Packetizer { get; }

        //Add new virtual method to set connection parameters
        //By Song@2018.01.15.
        protected virtual void SetConnectionParameters()
        {
            if (actProgProvider != null)
            {
                System.Net.IPAddress address;
                if (!System.Net.IPAddress.TryParse(HostAddress, out address))
                {
                    logger.ErrorMethod("HostAddress格式有误:" + HostAddress);
                }
                actProgProvider.PLCIpAddress = address;
                actProgProvider.PortRead = Port;
                if (WritePort == 0)
                    actProgProvider.PortWrite = Port;
                else
                    actProgProvider.PortWrite = WritePort;
            }
        }
        public void Connect()
        {
            try
            {
                if (actProgProvider == null)
                {
                    actProgProvider = new MelsecNet();
                }
                SetConnectionParameters();
                var isopen = actProgProvider.ConnectServer();
                if (isopen.IsSuccess)
                {
                    //Console.WriteLine("通信线路打开成功," + this.HostAddress);
                    OnConnect();
                    logger.ErrorMethod("通信线路打开成功," + this.HostAddress);
                }
                else
                {
                    IsComConnected = false;
                    Console.WriteLine("通信线路打开失败," + this.HostAddress + ",errcode:" + isopen.Message);
                    logger.ErrorMethod("通信线路打开失败," + this.HostAddress + ",errcode:" + isopen.Message);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorMethod(string.Format("Error while opening the connection {0}", GetGlueId()), ex);
            }
        }
        public void Disconnect()
        {
            //Use close() method to close the communication
            //actProgProvider.Disconnect();
            if (actProgProvider == null)
            {
                return;
            }
            var disconnected = actProgProvider.ConnectClose();
            if (disconnected.IsSuccess)
            {
                OnDisconnect();
            }
            else
            {
                logger.ErrorMethod(string.Format("Failed to close the connection {0}", GetGlueId()));
            }
        }
        public void GetBytesInSendAndReceiveBuffer(out int bytesInSendBuffer, out int bytesInReceiveBuffer)
        {
            bytesInSendBuffer = 0;
            bytesInReceiveBuffer = 0;
        }
        public string GetConnectionName()
        {
            return connectionName;
        }
        public string GetGlueId()
        {
            return GetId();
        }
        public IList<ILogic> GetLogicList()
        {
            return logicList;
        }
        public string GetResourceType()
        {
            return string.Empty;
        }
        public void GetSendAndReceiveTime(out DateTime lastSendTime, out DateTime lastReceiveTime)
        {
            lastSendTime = DateTime.Now;
            lastReceiveTime = DateTime.Now;
        }
        public void ReleaseSend()
        {

        }
        public bool Send(byte[] dataToSend, int dataLen)
        {
            return true;
        }
        #endregion End og ICommModule Interface implementation
        #region Begin of IMXCommModule Interface implementaion
        protected override void OnConnect()
        {
            //Set com connected status to true
            //By Song@2018.01.15
            IsComConnected = true;
            //Do the logging
            //By Song@2018.01.15
            logger.InfoMethod(string.Format("{0} is Connected", GetGlueId()));

            base.OnConnect();
        }
        protected override void OnDisconnect()
        {
            //Set com connected status to false
            //By Song@2018.01.15
            IsComConnected = false;
            //Do the logging
            //By Song@2018.01.15
            logger.InfoMethod(string.Format("{0} is Disconnected", GetGlueId()));

            base.OnDisconnect();
        }
        //主动写PLC设备的数据的方法  启动Client时候,启动一个线程检测变量,如果有变化,则进行处理,
        //
        [Obsolete("Please use Try2SendData method to write to data block", true)]
        public bool SendData(List<MXDataBlock> data)
        {
            //string szdevice = "D1004";
            //int length = 1;
            //short[] value = { 2 };
            //int result = actProgProvider.WriteDeviceBlock2(szdevice, length, ref value[0]);
            //调用三菱发送的方法,发送失败的处理方式
            foreach (var item in data)
            {
                string szlabel = item.DBName;
                if (item.DBDataInIn4Bytes != null)
                {
                    item.Result = 0;// actProgProvider.WriteDeviceBlock(szlabel, item.DBLength, ref item.DBDataInIn4Bytes[0]);
                    if (item.Result == 0x01010002)
                    {
                        //this.actProgProvider.Close();
                        this.Connect();
                    }
                }
                else if (item.DBDataIn2Bytes != null)
                {
                    //item.Result = actProgProvider.WriteDeviceBlock2(szlabel, item.DBLength, ref item.DBDataIn2Bytes[0]);
                    if (item.Result == 0x01010002)
                    {
                        //this.actProgProvider.Close();
                        this.Connect();
                    }
                }
                //重连
                //if (item.Result != 0)
                //    break; 
            }
            //若发送失败则重连
            //OnConnect();
            return true;
        }
        public bool Try2SendData(List<MXDataBlock> dataBlockList, short maxSendCount = 1)
        {
            foreach (var item in dataBlockList)
            {
                //By Song@2018.01.20
                if (!WriteDataBlock(item, maxSendCount))
                {
                    return false;
                }
            }
            return true;
        }
        public Neutrino Try2ReadData(List<MXDataBlock> dataBlockList, short maxReadCount = 1)
        {
            Neutrino nue = new Neutrino();
            try
            {
                foreach (var item in dataBlockList)
                {
                    //int db4; short db2;
                    switch (item.Type)
                    {
                        case "Int":
                            var readInt = ReadDataBlock(item, maxReadCount);
                            if (readInt.Item1)
                            {
                                nue.AddField(item.Name, readInt.Item2.ToString());
                            }
                            else
                            {
                                nue.AddField(item.Name, string.Empty);
                            }
                            break;
                        case "String":
                            var readString = ReadDataBlock(item, maxReadCount);
                            if (readString.Item1)
                            {
                                nue.AddField(item.Name, readString.Item2.ToString());
                            }
                            else
                            {
                                nue.AddField(item.Name, string.Empty);
                            }
                            break;
                        case "Short":
                            var readShort = ReadDataBlock(item, maxReadCount);
                            if (readShort.Item1)
                            {
                                nue.AddField(item.Name, readShort.Item2.ToString());
                            }
                            else
                            {
                                nue.AddField(item.Name, string.Empty);
                            }
                            break;
                        default:
                            break;
                    }
                }
                return nue;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("Error while try to read data", ex);
                return nue;
            }
        }
        public Tuple<bool, Neutrino> Try2ReadDataWithSign(List<MXDataBlock> dataBlockList, short maxReadCount = 1)
        {
            Neutrino nue = new Neutrino();
            //调用三菱发送的方法,发送失败的处理方式
            //nue.TheName = dataBlockList[0].
            foreach (var item in dataBlockList)
            {
                //int db4; short db2;
                switch (item.Type)
                {
                    case "Int":
                        var readInt = ReadDataBlock(item, maxReadCount);
                        if (readInt.Item1)
                        {
                            nue.AddField(item.Name, readInt.Item2.ToString());
                        }
                        else
                        {
                            return new Tuple<bool, Neutrino>(false, nue);
                        }
                        break;
                    case "String":
                        var readString = ReadDataBlock(item, maxReadCount);
                        if (readString.Item1)
                        {
                            nue.AddField(item.Name, readString.Item2.ToString());
                        }
                        else
                        {
                            return new Tuple<bool, Neutrino>(false, nue);
                        }
                        break;
                    case "Short":
                        var readShort = ReadDataBlock(item, maxReadCount);
                        if (readShort.Item1)
                        {
                            nue.AddField(item.Name, readShort.Item2.ToString());
                        }
                        else
                        {
                            return new Tuple<bool, Neutrino>(false, nue);
                        }
                        break;
                    default:
                        break;
                }
            }
            return new Tuple<bool, Neutrino>(true, nue);
        }
        private Tuple<bool, short> ReadDataBlock(MXDataBlock db, short tryReadCount)
        {

            short shortDb = 0;
            try
            {
                bool readSuccess = true;
                //Reconnect while connetion is broken
                if (!IsComConnected)
                {
                    Reconnect();
                }
                for (int i = 0; i < tryReadCount || !readSuccess; i++)
                {
                    var result = actProgProvider.ReadShortFromPLC(db.DBName);
                    if (result.IsSuccess)
                    {
                        shortDb = result.Content;
                        readSuccess = true;
                    }
                    else
                    {
                        //Try to reconnect
                        IsComConnected = false;
                        Reconnect();
                        readSuccess = false;
                    }
                }
                return new Tuple<bool, short>(readSuccess, shortDb);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod(string.Format("Error while reading DataBlock(Name:{0})", db.DBName), ex);
                return new Tuple<bool, short>(false, shortDb);
            }
        }
        private bool WriteDataBlock(MXDataBlock db, short maxSendCount = 1)
        {
            try
            {
                short shortDb = 0;
                bool sendSuccess = true;
                //Try to connect while connection is broken
                //By Song@2018.01.20
                if (!IsComConnected)
                {
                    Reconnect();
                }
                if (db.DBDataIn2Bytes != null)
                {
                    shortDb = db.DBDataIn2Bytes[0];
                }
                else if (db.DBDataInIn4Bytes != null)
                {
                    shortDb = Convert.ToInt16(db.DBDataInIn4Bytes[0]);
                }
                for (int i = 0; i < maxSendCount || !sendSuccess; i++)
                {
                    OperateResult result = null;
                    lock (connectLocker)
                    {
                        result = actProgProvider.WriteIntoPLC(db.DBName, shortDb);
                    }

                    if (result.IsSuccess)
                    {
                        sendSuccess = true;
                    }
                    else
                    {
                        IsComConnected = false;
                        Reconnect();
                        sendSuccess = false;
                    }
                }
                return sendSuccess;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod(string.Format("Error while sending DataBlock(Name:{0})", db.DBName), ex);
                return false;
            }
        }

        private void Reconnect()
        {
            try
            {
                lock (connectLocker)
                {
                    if (!IsComConnected)
                    {
                        //First disconnecting
                        Disconnect();
                        //Then connecting
                        Connect();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorMethod(string.Format("Error while reconnecting {0}", GetGlueId()), ex);
            }
        }
        #endregion End of IMXCommModule Interaface implementation
        #region Begin of Override method
        public override void Init(XmlNode configNode)
        {
            base.Init(configNode);
            //Instant MX communication component
            //By Song@2018.01.20.
            actProgProvider = new MelsecNet();

        }
        protected override void ReadParameters(XmlNode configNode)
        {
            base.ReadParameters(configNode);
            //读取组件对应的配置信息参数
        }
        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            //检验读取到的配置参数的有效性
            //当关键参数非法时，抛出异常退出程序的执行
        }

        protected override void StartInternal()
        {
            base.StartInternal();
            //
        }
        public bool IsConnected()
        {
            return IsComConnected;
        }
        #region Comment part
        //public bool TrySendData(List<MXDataBlock> data, short maxTrySendCount = 1)
        //{
        //    bool result = true;
        //    foreach (var item in data)
        //    {
        //        var r = WriteDataBlock(item, maxTrySendCount);
        //        if (!r)
        //            result = r;
        //    }
        //    return result;
        //}
        #endregion
        #endregion end of override method
    }
}
