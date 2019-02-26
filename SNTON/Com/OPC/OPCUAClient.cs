using Hylasoft.Opc.Ua;
using log4net;
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
    public class OPCUAClient : OPCCom, IOPCUACommModule
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private object connectLocker = new object();
        public static OPCUAClient Create(XmlNode configNode)
        {
            OPCUAClient comModule = new OPCUAClient();
            comModule.Init(configNode);
            comModule._UaClient = new UaClient(new Uri(comModule.HostUrl));
            return comModule;
        }
        public UaClient _UaClient = null;
        public OPCUAClient()
        {
        }
        #region Beginog ICommModule Interface implementation
        public ISequenceHandler InboundSequencer { get; }
        public ISequenceHandler OutboundSequencer { get; }
        public IPacketizer Packetizer { get; }
        //Add new virtual method to set connection parameters
        //By Song@2018.01.15.
        protected virtual void SetConnectionParameters()
        {

        }
        public void Connect()
        {
            try
            {
                if (_UaClient == null)
                {
                    _UaClient = new UaClient(new Uri(this.HostUrl));
                }
                _UaClient.Connect();
                if (_UaClient.Status == Hylasoft.Opc.Common.OpcStatus.Connected)
                {
                    //IsComConnected = true;
                    OnConnect();
                    logger.InfoMethod("通信线路打开成功," + this.HostUrl);
                    Console.WriteLine("通信线路打开成功," + this.HostUrl);
                }
                else
                {
                    IsComConnected = false;
                    Console.WriteLine("通信线路打开失败," + this.HostUrl);
                    logger.InfoMethod("通信线路打开失败," + this.HostUrl);
                }

            }
            catch (Exception ex)
            {
                IsComConnected = false;
                logger.ErrorMethod(string.Format("Error while opening the connection {0}", GetGlueId()), ex);
                Console.WriteLine("通信线路打开失败," + this.HostUrl + "," + ex.Message);
                logger.ErrorMethod("通信线路打开失败," + this.HostUrl + "," + ex.Message);
            }
        }
        public void Disconnect()
        {
            //Use close() method to close the communication 
            if (_UaClient == null)
            {
                return;
            }
            //int disconnected = 0;
            try
            {
                if (_UaClient.Status == Hylasoft.Opc.Common.OpcStatus.Connected)
                {
                    _UaClient.Dispose();
                }
                OnDisconnect();
            }
            catch (Exception e)
            {
                logger.ErrorMethod(string.Format("Failed to close the connection {0}", GetGlueId()), e);
            }
        }
        public void GetBytesInSendAndReceiveBuffer(out int bytesInSendBuffer, out int bytesInReceiveBuffer)
        {
            bytesInSendBuffer = 0;
            bytesInReceiveBuffer = 0;
        }
        public string GetConnectionName()
        {
            return "OPCUAClient:opc:tcp//" + this.HostUrl;
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


        public bool Try2SendData(List<OPCUADataBlock> dataBlockList, short maxSendCount = 1)
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
        public Neutrino Try2ReadData(List<OPCUADataBlock> dataBlockList, short maxReadCount = 1)
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
        private Tuple<bool, short> ReadDataBlock(OPCUADataBlock db, short tryReadCount)
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
                    try
                    {
                        var result = this._UaClient.Read<dynamic>(db.DBName);
                        shortDb = result.Value;
                        if (result.Quality == Hylasoft.Opc.Common.Quality.Good)
                            readSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        //Try to reconnect
                        IsComConnected = false;
                        Reconnect();
                        readSuccess = false;
                        ex.ToString();
                        logger.ErrorMethod(this.HostUrl + "通信失败；" + ex.ToString(), ex);
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
        private bool WriteDataBlock(OPCUADataBlock db, short maxSendCount = 1)
        {
            try
            {
                bool sendSuccess = true;
                //Try to connect while connection is broken
                //By Song@2018.01.20
                if (!IsComConnected)
                {
                    Reconnect();
                }

                for (int i = 0; i < maxSendCount || !sendSuccess; i++)
                {
                    int result = 0;
                    lock (connectLocker)
                    {
                        _UaClient.Write<dynamic>(db.DBName, db.Value);
                        result = 0;
                    }

                    if (result == 0)
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

        public bool Try2ReadData2(List<OPCUADataBlock> dataBlockList, short maxReadCount = 1)
        {
            foreach (var item in dataBlockList)
            {
                if (!IsComConnected)
                {
                    Reconnect();
                    return false;
                }
                var r = this._UaClient.Read<dynamic>(item.DBName);
                if (r.Quality == Hylasoft.Opc.Common.Quality.Good)
                    item.Value = r.Value;
                else return false;
            }
            return true;
        }

        public Tuple<bool, Neutrino> Try2ReadDataWithSign(List<OPCUADataBlock> dataBlockList, short maxReadCount = 1)
        {
            throw new NotImplementedException();
        }
        #region Comment part
        //public bool TrySendData(List<OPCUADataBlock> data, short maxTrySendCount = 1)
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

        /// <summary>
        /// 订阅标签值的变化
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        public void SubscribeEvent(string tag, Action<bool, dynamic> action)
        {
            _UaClient.Monitor<dynamic>(tag, (x, y) =>
            {
                bool r = x.Quality == Hylasoft.Opc.Common.Quality.Good ? true : false;
                if (action != null)
                    action.Invoke(r, x.Value);
            });
        }
        #region Test
        
        #endregion
    }
}
