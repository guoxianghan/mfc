﻿using SNTON.Entities.DBTables.AGV;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.RobotArmTask;
using SNTON.Entities.DBTables.Spools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC.COM;
using VI.MFC.Logging;
using VI.MFC.Logic;
using VI.MFC.Utils;
using VI.MFC.Utils.ConfigBinder;
using static SNTON.Constants.SNTONConstants;
using Newtonsoft.Json;
using SNTON.Entities.DBTables.MidStorage;
using System.IO;
using SNTON.Entities.DBTables.Message;
using System.Threading;
using SNTON.Entities.DBTables.PLCAddressCode;
using SNTON.Entities.DBTables.MES;
using SNTON.Constants;
using SNTON.Entities.DBTables.InStoreToOutStore;

namespace SNTON.Components.ComLogic
{
    /// <summary>
    /// 暂存库线体
    /// </summary>
    public class MidStoreLine4Logic : ComLogic
    {
        public MidStoreLine4Logic()
        {
            threadchecktask = new VIThreadEx(CheckTask, null, "CheckMidStoreLineTask", 3000);
            thread_ReadBarCode = new VIThreadEx(ReadBarCode, null, "read BarCode from PLC", 3000);
            thread_LineStatusCallAGV = new VIThreadEx(LineStatusCallAGV, null, "check line status to call agv", 3000);
            thread_heartbeat = new VIThreadEx(heartbeat, null, "heartbeat", 1000);
            thread_warninginfo = new VIThreadEx(ReadWarningInfo, null, "ReadMidStoreLineWarnning", 5000);
            //qrcode = $"./StorageArea{StorageArea}QrCode.json";
        }
        //Dictionary<string, bool> _DicWarnning = new Dictionary<string, bool>();
        public List<MachineWarnningCodeEntity> _WarnningCode = new List<MachineWarnningCodeEntity>();
        public new static MidStoreLine4Logic Create(XmlNode node)
        {
            MidStoreLine4Logic m = new MidStoreLine4Logic();
            m.Init(node);
            return m;
        }
        public override void Init(XmlNode configNode)
        {
            base.Init(configNode);
        }
        private VIThreadEx threadchecktask;
        private VIThreadEx thread_heartbeat;
        private VIThreadEx thread_ReadBarCode;
        private VIThreadEx thread_LineStatusCallAGV;
        private VIThreadEx thread_warninginfo;
        [ConfigBoundProperty("RobotArmID")]
        public string RobotArmID = "";
        /// <summary>
        /// 暂存库满
        /// </summary>
        public bool IsStoreageEnough = false;
        /// <summary>
        /// 线体报警
        /// </summary>
        public bool IsWarning = false;
        /// <summary>
        /// 扫马口满
        /// </summary>
        public bool IsScanEnough = false;
        public override bool Send(Neutrino sendData)
        {
            return base.Send(sendData);
        }
        public override void OnConnect()
        {
            base.OnConnect();
        }
        public override void OnDisconnect()
        {
            base.OnDisconnect();
        }
        public override Neutrino CreateSendNeutrino(string commModuleGlueId, string telegramId, string name = "")
        {
            return base.CreateSendNeutrino(commModuleGlueId, telegramId, name);
        }
        protected override void StartInternal()
        {
            thread_ReadBarCode.Start();
            thread_heartbeat.Start();
            thread_warninginfo.Start();
            thread_LineStatusCallAGV.Start();
            base.StartInternal();
        }
        int BACKUP3 = 0;
        void heartbeat()
        {
            BACKUP3 = BACKUP3 == 0 ? 1 : 0;
            Neutrino n = new Neutrino();
            n.TheName = "heartbeat";
            n.AddField("BACKUP3", BACKUP3.ToString());
            SendData(n);
        }
        void CheckTask()
        {
            //Neutrino neu = new Neutrino();
            //neu.TheName = "CheckMidStoreLineTask";
            //neu.AddField("SERVER_STATUS", "0");
            //neu.AddField("ONLINE_INSTORE_CALLAGV", "1");
            //Send(neu);

            //0从暂存库到出库线 出库
            int cmd = ReadLineStatus("OUTSTORE_ARM_SET");

            //1从暂存库到直通线 出库
            cmd = ReadLineStatus("ONLINE_OUTSTORE_ALLOW_SET");

            //2从直通线到暂存库(入库);
            cmd = ReadLineStatus("ONLINE_INSTORE_ALLOW_GET");

            //3直通线到异常口; 
            int i = ReadLineStatus("ONLINE_OUTSTORE_ALLOW_SET");
            cmd = ReadLineStatus("ExLine_ARM_SET");

            //4从暂存库到异常口 出库                 
            cmd = ReadLineStatus("ExLine_ARM_SET");
            //ReadLineStatus()
        }
        void ReadWarningInfo()
        {
            _WarnningCode = this.BusinessLogic.MachineWarnningCodeProvider.MachineWarnningCache.FindAll(x => x.MachineCode == 2 && x.MidStoreNo == this.StorageArea);
            if (_WarnningCode == null || _WarnningCode.Count == 0)
                return;
            Neutrino ne = new Neutrino();
            ne.TheName = "ReadMidStoreRobotArmWarnning";
            foreach (var item in _WarnningCode.GroupBy(x => x.AddressName.Trim()))
            {
                ne.AddField(item.Key.Trim(), "0");
            }
            ne.AddField("LINE_STATUS", "0");
            var n = this.MXParser.ReadData(ne, true);
            #region MyRegion
            foreach (var item in _WarnningCode.GroupBy(x => x.AddressName.Trim()))
            {
                int i = n.Item2.GetInt(item.Key.Trim());
                char[] binary = System.Convert.ToString(i, 2).ToArray();
                for (int c = 0; c < binary.Length; c++)
                {
                    var fi = _WarnningCode.FirstOrDefault(x => x.BIT == c && x.AddressName.Trim() == item.Key);
                    if (fi == null)
                        continue;
                    if (binary[c] != '0' && !fi.IsWarning)
                    {
                        fi.IsWarning = true;
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "号线体" + fi.Description.Trim(), Source = this.StorageArea + "号线体报警", MsgLevel = 7, MidStoreage = this.StorageArea });

                    }
                    else
                        fi.IsWarning = false;
                }
            }

            #endregion
            var realtimewarning = _WarnningCode.FindAll(x => x.LastWarning != x.IsWarning);
            if (realtimewarning != null && realtimewarning.Count != 0)
            {
                realtimewarning.ForEach(x => { x.LastWarning = x.IsWarning; x.Updated = DateTime.Now; });
                //_WarnningCode.ForEach(x=> { x.LastWarning});
                this.BusinessLogic.MachineWarnningCodeProvider.UpdateWarning(realtimewarning, null);
            }
            if (_WarnningCode.Exists(x => x.IsWarning))
                IsWarning = true;
            else IsWarning = false;
            int r = n.Item2.GetInt("LINE_STATUS");
            if (r != 1)
                IsWarning = true;
            else IsWarning = false;
        }


        public override void ReadDervice()
        {
            base.ReadDervice();
        }
        /// <summary>
        /// 直通线上待抓取位置的流水号,和将要下发的龙门指令做对比
        /// </summary>
        public int ONLINE_INSTORE_RobotArmSeqNo()
        {
            Neutrino ne = new Neutrino();
            ne.TheName = "ReaddingONLINE_INSTORE_RobotArmSeqNo";
            ne.AddField("ONLINE_INSTORE_RobotArmSeqNo", "0");
            var n = this.MXParser.ReadData(ne);
            if (!n.Item1)
            {
                return 0;
            }
            int i = n.Item2.GetInt("ONLINE_INSTORE_RobotArmSeqNo");
            return i;
        }
        public LinePLCCmd ReadMidStoreLineStatus()
        {
            LinePLCCmd line = new LinePLCCmd();
            Neutrino ne = ConvertToNeu(line);
            ne.TheName = "ReadMidStoreLineStatus";
            var n = this.MXParser.ReadData(ne, true);
            if (!n.Item1)
            {
                return null;
            }
            var l = ConvertToObj<LinePLCCmd>(n.Item2);
            return l;
        }

        /// <summary>
        /// 清空入库出库型号
        /// </summary>
        public void ClearLineProductType()
        {
            Neutrino ne = new Neutrino();
            ne.TheName = "ClearLineProductType";
            ne.AddField("INSTORE_ProductType", "0");
            ne.AddField("OUTSTORE_ProductType", "0");
            var n = SendData(ne);
            if (n)
            {
                logger.InfoMethod("Clear product type successfully");
            }
            else
            {
                logger.InfoMethod("Failed to clear product type");
            }
        }
        /// <summary>
        /// 给线体物料种类,用以判断数量,1(8个);2(12个)
        /// </summary>
        /// <param name="inorout">1入库:INSTORE_ProductType;2出库:OUTSTORE_ProductType</param>
        /// <param name="type">1(8个);2(12个)</param>
        public void SendProductType(Int32 inorout, string type)
        {
            Neutrino ne = new Neutrino();
            ne.TheName = "SendLineProductType";
            if (inorout == 1)
            {
                ne.AddField("INSTORE_ProductType", type);
            }
            else if (inorout == 2)
            {
                ne.AddField("OUTSTORE_ProductType", type);
            }
            else
            {
                logger.ErrorMethod("未知的入库出库类型:inorout:" + inorout);
            }
            SendData(ne);
        }
        /// <summary>
        /// 出库:OUTSTORE_ProductType,入库:INSTORE_ProductType,
        /// </summary>
        /// <param name="cmd">出库:OUTSTORE_ProductType,入库:INSTORE_ProductType,</param>
        /// <param name="type"></param>
        public void SendProductType(string cmd, string type)
        {
            Neutrino ne = new Neutrino();
            ne.TheName = "SendLineProductType";
            ne.AddField(cmd, type);
            SendData(ne);
        }
        /// <summary>
        /// 告诉线体任务流水号 直通口ONLINE_SeqNo_Write;正常出库口OUTSTORE_SeqNo_Write
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seqno"></param>
        public void WriteSeqNo(string key, int seqno)
        {
            Neutrino ne = new Neutrino();
            ne.TheName = "WriteSeqNo";
            ne.AddField(key, seqno.ToString());
            this.MXParser.SendData(ne);
        }
        /// <summary>
        /// 读取线体状态
        /// </summary>
        /// <param name="filed_key"></param>
        /// <returns></returns>
        public int ReadLineStatus(string filed_key)
        {
            Neutrino ne = new Neutrino();
            ne.TheName = "ReadLineStatus";
            ne.AddField(filed_key, "0");
            var n = this.MXParser.ReadData(ne);
            if (!n.Item1)
            {
                return 0;
            }
            int i = n.Item2.GetInt(filed_key);
            return i;
        }
        System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();

        /// <summary>
        /// 读线体状态,判断是否允许AGV来接,直通线,出库线
        /// </summary>
        void LineStatusCallAGV()
        {
            Neutrino ne = new Neutrino();
            ne.TheName = "LineCallAGV";
            ne.AddField("ONLINE_OUTSTORE_CALLAGV", "0");//直通口允许叫车1允许;0不允许
            ne.AddField("OUTSTORE_CALLAGV", "0");//出库口允许叫车1允许;0不允许
            ne.AddField("ONLINE_SeqNo_Read", "0");//直通出库反馈流水号
            ne.AddField("OUTSTORE_SeqNo_Read", "0");//单独出库反馈流水号
            var n = this.MXParser.ReadData(ne);
            if (!n.Item1)
            {
                return;
            }
            Neutrino neutrino = n.Item2;
            int online = neutrino.GetIntOrDefault("ONLINE_OUTSTORE_CALLAGV");
            int outline = neutrino.GetIntOrDefault("OUTSTORE_CALLAGV");
            int seqno = 0;
            if (online == 1)
            {//直通线
                seqno = neutrino.GetIntOrDefault("ONLINE_SeqNo_Read");
                if (seqno != 0)
                    LineCallAGV2(MidStoreLine.InStoreLine, seqno);
            }
            if (outline == 1)
            {//出库线
                seqno = neutrino.GetIntOrDefault("OUTSTORE_SeqNo_Read");
                if (seqno != 0)
                    LineCallAGV2(MidStoreLine.OutStoreLine, seqno);
            }
        }


        void LineCallAGV2(int midline, int seqno = 0)
        {
            string line = "";
            if (seqno == 0)
                return;
            //int ConveyorId = 0;
            if (midline == MidStoreLine.InStoreLine)
            {
                //ConveyorId = 4;
                line = "直通线";
            }
            else if (midline == MidStoreLine.OutStoreLine)
            {
                //ConveyorId = 1;
                line = "正常出库线";
            }

            var agvtsk = this.BusinessLogic.T_AGV_KJ_InterfaceProvider.GetT_AGV_KJ_InterfaceEntity($"Status=4 AND SeqNo ={seqno} AND ConveyorId={midline}");
            if (agvtsk == null)
                return;
            //var niagvtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTask($"Status=2 AND SeqNo ={seqno} AND ConveyorId={ConveyorId}");
            //if (niagvtsk != null)
            //{ niagvtsk.Status = 2; }
            var armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskGroupGUID='{agvtsk.TaskGuid.ToString()}'", null);
            if (armtsks != null && armtsks.Count != 0)
                if (armtsks.Exists(x => x.SpoolStatus == 1))
                {
                    return;
                }
            agvtsk.Status = 5;
            agvtsk.time_5 = DateTime.Now;
            //armtsks.ForEach(x => x.TaskStatus = 4);
            this.BusinessLogic.RobotArmTaskProvider.UpdateArmTaskStatus(agvtsk.TaskGuid, 4);
            bool r = this.BusinessLogic.T_AGV_KJ_InterfaceProvider.UpdateT_AGV_KJ_Interface(agvtsk, null);
            //将armtask改成4
            logger.InfoMethod($"{StorageArea}号库{line}线体收到接料请求,将AGV请求状态改为3,guid:" + agvtsk.TaskGuid.ToString());

        }

        public Queue<string> barcodeQueue { get; set; } = new Queue<string>();
        public Queue<MESSystemSpoolsEntity> barcodeLRQueue { get; set; } = new Queue<MESSystemSpoolsEntity>();
        //int _InStoreLineWhoolsCount = 0;

        int readbarcode = 0;
        /// <summary>
        /// 直通线读二维码功能,直通线出库功能
        /// </summary>
        void ReadBarCode()
        {

            //int seqno = ONLINE_INSTORE_RobotArmSeqNo();
            Neutrino ne = new Neutrino() { TheName = "ReadBarCode" };
            ne.AddField("BARCODE0", "0");
            ne.AddField("BARCODE1", "0");
            ne.AddField("BARCODE2", "0");
            ne.AddField("ONLINE_WHOOLCOUNT", "0");//直通线轮子数量
            ne.AddField("ONLINE_LOADSUCCESS", "0");//\直通线轮子数量
            ne.AddField("ExLine_SCANN_ISENOUGH", "0");//扫码异常口1.满  0.不满

            Neutrino neclear = new Neutrino() { TheName = "ClearBarCode" };
            neclear.AddField("BARCODE0", "0");
            neclear.AddField("BARCODE1", "0");
            neclear.AddField("BARCODE2", "0");

            var read = MXParser.ReadData(ne);
            if (!read.Item1)
            {
                return;
            }
            Neutrino neutrino = read.Item2;
            int ExLine_SCANN_ISENOUGH = neutrino.GetIntOrDefault("ExLine_SCANN_ISENOUGH");
            if (ExLine_SCANN_ISENOUGH == 1)
            {//扫码异常口满 创建Message
                if (!IsScanEnough)
                {
                    IsScanEnough = true;
                    this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "暂存库扫码异常口满", Source = this.StorageArea + "暂存库扫码异常口满", MsgLevel = 7, MidStoreage = this.StorageArea });
                }
                return;
            }
            IsScanEnough = false;
            int ONLINE_WHOOLCOUNT = ne.GetInt("ONLINE_WHOOLCOUNT");
            var b0 = BitConverter.GetBytes(neutrino.GetInt("BARCODE0"));
            var b1 = BitConverter.GetBytes(neutrino.GetInt("BARCODE1"));
            var b2 = BitConverter.GetBytes(neutrino.GetInt("BARCODE2"));
            int issuccess = neutrino.GetInt("ONLINE_LOADSUCCESS");//小车到直通线对接的完成标志 0未完成;1完成
            string barcode = (asciiEncoding.GetString(b0) + asciiEncoding.GetString(b1) + asciiEncoding.GetString(b2)).Replace("\0", "");

            //Move to the area when it is ready for usage
            //By Song@2018.01.24.
            barcodeQueue = GetbarcodeQueue();
            barcodeLRQueue = GetbarcodeLRQueue();
            if (issuccess == 1 && string.IsNullOrEmpty(barcode.Trim()))
            {
                //goto GOLOGIC;
                if (barcodeQueue.Count != 0)
                {
                    #region  0暂存库已满,1入库;2出库
                    //int agvseqno = getNextSeqNo();
                    //int outstoreagequeue = 0;//直通口出库时告诉线体替换或通过
                    int i = InLineRobotArmTask2();//1入库;2出库
                    if (i == 0)
                    {
                        if (!IsStoreageEnough)
                        {
                            IsStoreageEnough = true;
                            this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "号暂存库已满", Source = this.StorageArea + "号暂存库报警", MsgLevel = 6, MidStoreage = this.StorageArea });
                        }
                        return;
                    }
                    IsStoreageEnough = false;
                    i = i == 3 ? 2 : i;
                    if (i == 2)
                    {
                        //neclear.AddField("ONLINE_SeqNo_Write", agvseqno.ToString());//出库流水号 AGV任务流水号
                        //neclear.AddField("BACKUP6", outstoreagequeue.ToString());//出库队列
                    }

                    neclear.AddField("IN_OR_OUT", i.ToString());
                    neclear.AddField("ONLINE_LOADSUCCESS", "0");
                    barcodeQueue.Clear();
                    barcodeLRQueue.Clear();
                    //清空barcodeQueue序列化文件
                    SavebarcodeQueue();
                    SendData(neclear);
                    return;
                    #endregion
                }
                else
                {
                    neclear.AddField("ONLINE_LOADSUCCESS", "0");
                    SendData(neclear);
                    return;
                }
            }
            if (string.IsNullOrEmpty(barcode.Trim()))
                return;
            readbarcode++;
            //To trace exception barcode
            //By Song@2018.01.24
            string barcodeReadingInfo = string.Empty;
            if (barcode.Length < 6 && readbarcode <= 3)
            {
                barcodeReadingInfo = string.Format("Barcode({0}) is read at Storage {1} after {2} reading attempt",
                                                   barcode,
                                                   StorageArea,
                                                   readbarcode);
                File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcodeReadingInfo + "\r\n");
                return;
            }
            //Add log to trace the spool to exception
            //By Song@2018.01.24
            else
            {
                barcodeReadingInfo = barcodeReadingInfo + string.Format("Storage:{0}, Barcode:{1}, TryCount:{2}", StorageArea, barcode, readbarcode);
            }
            readbarcode = 0;

            if (barcodeQueue.ToList().Exists(x => x.Contains(barcode)))
            {
                SendData(neclear);
                return;
            }
            //barcodeQueue读取本地序列化文件
            int _BARCODE_SerialNumber = getNextSeqNo();//小于30000 
            //if (barcodeQueue.Count > 0)
            //{
            //    var id = barcodeQueue.Select(x => Convert.ToInt16(x.Split('_')[1]));
            //    _BARCODE_SerialNumber = id.OrderByDescending(x => x).FirstOrDefault();
            //}

            //if (_BARCODE_SerialNumber >= 30000)
            //    _BARCODE_SerialNumber = 1;
            //else _BARCODE_SerialNumber++;
            #region MesToSNTON
            //By Song@2018.01.25
            bool toExceptionFlow = false;
            try
            {
                int re = 0;
                if (barcode.Trim().Length == 6 && barcode.Trim() != "999999")
                {
                    string lr = "";
                    MESSystemSpoolsEntity messpool;
                    #region MyRegion
                    re = MesToSNTON(barcode, out lr, out messpool);
                    if (re == 1)
                    {
                        if (!barcodeQueue.ToList().Exists(x => x.Contains(barcode)))
                        {
                            barcodeQueue.Enqueue(barcode + "_" + _BARCODE_SerialNumber.ToString());
                            messpool.SeqNo = _BARCODE_SerialNumber;
                            barcodeLRQueue.Enqueue(messpool);
                            SavebarcodeQueue();
                            //序列化保存到本地
                        }
                    }
                    else if (re == -1)
                    {
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "错误的LR面信息:" + lr.ToString() + "," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 6, MidStoreage = this.StorageArea });
                        logger.WarnMethod("错误的LR面信息," + barcode + "," + lr.ToString());
                        toExceptionFlow = true;
                        barcodeReadingInfo = barcodeReadingInfo + "(Reason: Wrong L/R) => Exception route";
                    }
                    else if (re == 0)
                    {
                        logger.WarnMethod("MES系统不存在此二维码," + barcode);
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "MES系统不存在此二维码," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 6, MidStoreage = this.StorageArea });
                        toExceptionFlow = true;
                        barcodeReadingInfo = barcodeReadingInfo + "(Reason: No MES info) => Exception route";
                    }
                    else
                    {
                        logger.WarnMethod("添加到SNTON失败," + barcode);
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "添加到SNTON失败," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5, MidStoreage = this.StorageArea });
                        //By Song@2018.01.25
                        //neclear.AddField("ExceptionTag", "2");
                        //By Song@2018.01.25
                        toExceptionFlow = true;
                        //By Song@2018.01.24.
                        barcodeReadingInfo = barcodeReadingInfo + "(Reason: Error MES info) => Exception route";
                        //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcode + " 添加到SNTON失败\r\n");
                    }
                    #endregion
                }
                else
                {
                    if (barcode.Trim() == "999999")
                    {
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "扫码失败," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 6, MidStoreage = this.StorageArea });
                        logger.WarnMethod("扫码失败," + barcode);
                    }
                    else
                    {
                        //this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "错误的二维码," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5 });
                        logger.WarnMethod("错误的二维码," + barcode);
                    }
                    //By Song@2018.01.25
                    //neclear.AddField("ExceptionTag", "2");
                    //By Song@2018.01.25
                    toExceptionFlow = true;
                    //By Song@2018.01.24
                    barcodeReadingInfo = barcodeReadingInfo + "(Reason: Invalid barcode) => Exception route";
                    //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcode + "\r\n");
                }
            }
            catch (Exception ex)
            {
                this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "添加到SNTON失败," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5, MidStoreage = this.StorageArea });
                //By Song@2018.01.25
                //neclear.AddField("ExceptionTag", "2");
                //By Song@2018.01.25
                toExceptionFlow = true;
                logger.ErrorMethod(barcode + " MesToSNTON失败", ex);
                //By Song@2018.01.24
                barcodeReadingInfo = barcodeReadingInfo + "(Reason: Exception processing) => Exception route";
                //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcode + "\r\n");
            }
            #endregion
            //By Song@2018.01.24
            File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcodeReadingInfo + "\r\n");

            if (toExceptionFlow)
            {
                neclear.AddField("ExceptionTag", "2");
            }
            else
            {
                neclear.AddField("ONLINE_BARCODE_SerialNumber", _BARCODE_SerialNumber.ToString());
                neclear.AddField("ExceptionTag", "1");
            }
            SendData(neclear);
        }
        /// <summary>
        /// 1正常入库;2到异常口
        /// </summary>
        /// <param name="value"></param>
        public void SendToExceptionFlow(int value)
        {
            Neutrino neclear = new Neutrino() { TheName = "SendToExceptionFlow" };
            neclear.AddField("ExceptionTag", value.ToString());
            SendData(neclear);
        }
        /// <summary>
        /// 0暂存库已满,1入库;2出库
        /// </summary>
        /// <returns></returns>
        int InLineRobotArmTask2()
        {
            int cmd = 0;

            //barcodeQueue.Enqueue("7C2W8Q");
            string codeseqt = barcodeQueue.Peek();
            string codet = codeseqt.Substring(0, 6);
            var spool = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcode(codet, null);

            var midstores = this.BusinessLogic.MidStorageSpoolsProvider.GetMidStorages($"StorageArea ={StorageArea} AND IsOccupied IN (0)", null);//找出库里的空闲位置
            if (midstores == null || midstores.Count < barcodeQueue.Count)
            {
                return 0;
            }
            var tskconfig = TaskConfig.GetEnoughAGVEquipCount(spool.ProductType.Trim());//8 / 12  

            //创建AGVTask和RobotArmTask,并优先级最高
            var guid = Guid.NewGuid();
            //通过GUID标记一个龙门任务单元
            DateTime createtime = DateTime.Now;

            //全部入库 
            //没有需要该类型轮子的设备的任务,将所有轮子创建直通线入库任务 
            #region MyRegion

            List<RobotArmTaskEntity> instorearmtsks = new List<RobotArmTaskEntity>();
            var instore = midstores.FindAll(x => x.Spool == null && x.IsOccupied == 0).OrderBy(x => x.SeqNo).Take(barcodeQueue.Count).ToList();//取出空位置
            RobotArmTaskEntity armtsk = null;
            if (instore.Count < barcodeQueue.Count)
            {
                return 0;
            }
            cmd = 1;
            for (int i = 0; i <= instore.Count - 1; i++)
            {
                string codeseq = barcodeQueue.Dequeue();
                barcodeLRQueue.Dequeue();
                string code = codeseq.Substring(0, 6);
                #region 

                spool = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcode(code, null);
                armtsk = new RobotArmTaskEntity();
                // 全部入库
                armtsk.Created = createtime;
                armtsk.TaskGroupGUID = guid;
                armtsk.CName = spool.CName.Trim();
                armtsk.FromWhere = instore[i].SeqNo;
                armtsk.ProductType = spool.GetLineCode.ToString();
                armtsk.WhoolBarCode = code;
                armtsk.StorageArea = StorageArea;
                instore[i].IdsList = spool.Id.ToString();
                instore[i].IsOccupied = 5;
                instore[i].Updated = createtime;
                instore[i].FdTagNo = code;
                armtsk.PlantNo = PlantNo;
                armtsk.RobotArmID = RobotArmID;
                armtsk.TaskLevel = 7;
                armtsk.TaskType = 2;
                armtsk.ToWhere = MidStoreLine.InStoreLine;
                armtsk.TaskStatus = 0;
                armtsk.SpoolStatus = 0;
                armtsk.SpoolSeqNo = Convert.ToInt32(codeseq.Replace(code + "_", ""));
                armtsk.SeqNo = i;
                instorearmtsks.Add(armtsk);
                #endregion
            }
            this.BusinessLogic.MidStorageProvider.UpdateMidStore(null, instore.ToArray());
            logger.InfoMethod(" 没有需要该类型轮子的设备的任务,将所有轮子分配库位任务成功");
            this.BusinessLogic.RobotArmTaskProvider.InsertArmTask(instorearmtsks);
            logger.InfoMethod(" 没有需要该类型轮子的设备的任务,将所有轮子创建直通线入库任务成功");
            #endregion


            return cmd;
        }

        /// <summary>
        /// 直通线出库功能
        /// 0暂存库已满,1入库;2出库
        /// <param name="outstoreagequeue">直通口出库时告诉线体替换或通过</param>
        /// </summary>
        /// <returns></returns>
        int InLineRobotArmTask3(int agvseqno, out int outstoreagequeue)
        {
            int cmd = 1;
            outstoreagequeue = 0;
            //agvseqno = 0;
            //barcodeQueue.Enqueue("7C2W8Q");
            var codeseqt = barcodeLRQueue.Peek();
            string codet = codeseqt.FdTagNo.Trim();
            var spool = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcode(codet, null);
            var midstores = this.BusinessLogic.MidStorageSpoolsProvider.GetMidStorages($"StorageArea ={StorageArea} AND IsOccupied IN (0,1)", null);//找出库里的空闲位置和有单丝的位置
            var tskconfig = TaskConfig.GetEnoughAGVEquipCount(spool.ProductType.Trim());//8 / 12 

            //创建AGVTask和RobotArmTask,并优先级最高
            var guid = Guid.NewGuid();
            //通过GUID标记一个龙门任务单元
            DateTime createtime = DateTime.Now;

            bool islevel = true;//是否满足优先级
            List<MESSystemSpoolsEntity> list = barcodeLRQueue.ToList();
            var l = from i in list
                    group new MESSystemSpoolsEntity() { StructBarCode = i.StructBarCode.Trim(), Length = i.Length, BobbinNo = i.BobbinNo, BobbinNo2 = i.BobbinNo2, CName = i.CName, Const = i.Const, FdTagNo = i.FdTagNo.Trim(), GroupID = i.GroupID, InStoreToOutStoreage = i.InStoreToOutStoreage, SeqNo = i.SeqNo, SpoolId = i.SpoolId }
                    by new { i.StructBarCode, i.Length } into t
                    select t;

            if (l.Count() > 1)
            {
                cmd = 1;
                islevel = false;
            }
            //List<InStoreToOutStoreSpoolViewEntity> ttt = this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.GetInStoreToOutStoreSpool(this.StorageArea, this.PlantNo, null);
            var agvrunningtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTasks($"IsDeleted=0 and TaskType=2 AND [Status] IN(1,2,3,4,7,8,9) AND [StorageLineNo]=2 AND [StorageArea]={this.StorageArea} AND [PlantNo]={this.PlantNo}", null);
            if (agvrunningtsk == null) agvrunningtsk = new List<AGVTasksEntity>();
            //var ccc = ttt.FindAll(x => x.Status <= 3).GroupBy(x => x.Guid);
            if (islevel && tskconfig.Item4 < barcodeLRQueue.Count && agvrunningtsk != null && agvrunningtsk.Count() <= 2 && l.Count() == 1)
            {//满足优先级且大于半车,搭配出库
             //若需要调整,判断库里是否有能满足搭配出库的单丝
             //替换/补充 1,3 2,4
             //-2库位不足,-1入库,0出库,1替换,2补充,3补充L,4补充R
                int instore = 0;
                #region MyRegion
                switch (instore)
                {
                    case -2:
                        cmd = 0;
                        return 0;
                    case -1:
                        cmd = 1;
                        break;
                    case 0:
                        cmd = 2;
                        break;
                    default:
                        cmd = 1;
                        break;
                }
                #endregion
            }
            var res = this.MXParser.ReadData("ExLine_BACK0", "ZHITONGXIANMAN");
            //读直通线上是否已满 0允许出库;1满
            if (res.Item2 == 1)
                cmd = 1;
            if (cmd == 2 && res.Item2 == 0)
            {//创建出库任务

                //Guid guid = Guid.NewGuid();
                bool issuccesstooutstore = CreateInStoreToOutStore(agvseqno, guid, createtime, midstores, tskconfig, list, out outstoreagequeue);
                if (issuccesstooutstore)
                {
                    //将该批次保存下来
                    return 2;
                }
                else cmd = 1;
            }
            if (cmd == 1)
            {
                //全部入库
                cmd = 1;
                //没有需要该类型轮子的设备的任务,将所有轮子创建直通线入库任务
                logger.InfoMethod(" 没有需要该类型轮子的设备的任务,或者设备任务数量不够,或者设备排列不满足条件，将所有轮子创建直通线入库任务");
                #region MyRegion

                List<RobotArmTaskEntity> instorearmtsks = new List<RobotArmTaskEntity>();
                var instore = midstores.FindAll(x => x.Spool == null && x.IsOccupied == 0).OrderBy(x => x.SeqNo).Take(barcodeQueue.Count).ToList();//取出空位置
                RobotArmTaskEntity armtsk = null;
                if (instore.Count < barcodeLRQueue.Count)
                {
                    return 0;
                }
                for (int i = 0; i <= instore.Count - 1; i++)
                {
                    var codeseq = barcodeLRQueue.Dequeue();
                    //barcodeLRQueue.Dequeue();
                    barcodeQueue.Dequeue();
                    string code = codeseq.FdTagNo;
                    #region 

                    spool = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcode(code, null);
                    armtsk = new RobotArmTaskEntity();
                    //抓到直通线的轮子
                    armtsk.Created = createtime;
                    armtsk.TaskGroupGUID = guid;
                    armtsk.CName = spool.CName.Trim();
                    armtsk.FromWhere = instore[i].SeqNo;
                    armtsk.ProductType = spool.GetLineCode.ToString();
                    armtsk.WhoolBarCode = code;
                    armtsk.StorageArea = StorageArea;
                    instore[i].IdsList = spool.Id.ToString();
                    instore[i].IsOccupied = 5;
                    instore[i].Updated = createtime;
                    instore[i].FdTagNo = code;
                    armtsk.PlantNo = PlantNo;
                    armtsk.RobotArmID = RobotArmID;
                    armtsk.TaskLevel = 7;
                    armtsk.TaskType = 2;
                    armtsk.ToWhere = MidStoreLine.InStoreLine;
                    armtsk.TaskStatus = 0;
                    armtsk.SpoolStatus = 0;
                    armtsk.SpoolSeqNo = codeseq.SeqNo;
                    armtsk.SeqNo = i;
                    instorearmtsks.Add(armtsk);
                    #endregion
                }
                this.BusinessLogic.MidStorageProvider.UpdateMidStore(null, instore.ToArray());
                logger.InfoMethod(" 没有需要该类型轮子的设备的任务,将所有轮子分配库位任务成功");
                this.BusinessLogic.RobotArmTaskProvider.InsertArmTask(instorearmtsks);
                logger.InfoMethod(" 没有需要该类型轮子的设备的任务,将所有轮子创建直通线入库任务成功");
                #endregion
            }

            return cmd;
        }
        /// <summary>
        /// 校验单丝是否满足出库优先级
        /// </summary>
        /// <param name="spool"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        bool CheckSpoolIsLevel(SpoolsEntity spool, List<ProductEntity> list)
        {
            list = list.FindAll(x => x.SeqNo != 0).OrderByDescending(x => x.SeqNo).ToList();
            var product = list.FirstOrDefault(x => x.Const.Trim() == spool.Const.Trim() && x.Length == spool.Length);
            if (product != null)
                return true;
            else return false;
        }
        /// <summary>
        /// 直通口出库
        /// </summary>
        /// <param name="agvseqno"></param>
        /// <param name="midstores"></param>
        /// <param name="tskconfig"></param>
        /// <param name="createtime"></param>
        /// <param name="list"></param>
        /// <param name="guid"></param>
        /// <param name="inlineoutqueue"></param>
        /// <returns></returns>
        private bool CreateInStoreToOutStore(int agvseqno, Guid guid, DateTime createtime, List<MidStorageSpoolsEntity> midstores, Tuple<int, int, int, int, int> tskconfig, List<MESSystemSpoolsEntity> list, out int inlineoutqueue)
        {
            //logger.InfoMethod("已在直通线创建出库任务");
            //直通线0入,1出
            //创建龙门任务并提供缓存 
            var lr = from i in list select i.BobbinNo;
            inlineoutqueue = 0;
            StringBuilder inorout = new StringBuilder();
            //inorout.Append("1");
            int armtskseqno = 0;
            MESSystemSpoolsEntity spool = list[0];
            //midstores = midstores.FindAll(x => !string.IsNullOrEmpty(x.StructBarCode) && x.StructBarCode.Trim() == codeseqt.StructBarCode.Trim());
            List<RobotArmTaskEntity> instorearmtsks = new List<RobotArmTaskEntity>();
            //RobotArmTaskEntity armtsk = null;
            List<MidStorageSpoolsEntity> midsupdate = new List<MidStorageSpoolsEntity>();

            try
            {
                var spools = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcodes(null, (from i in list where !string.IsNullOrWhiteSpace(i.FdTagNo) select i.FdTagNo.Trim()).ToArray());
                foreach (var item in list)
                {//1替换,2补充,3补充L,4补充R
                    try
                    {
                        if (item.InStoreToOutStoreage != 3 && item.InStoreToOutStoreage != 4 && item.InStoreToOutStoreage != 2)
                            item.SpoolId = spools.FirstOrDefault(x => x.FdTagNo.Trim() == item.FdTagNo.Trim()).Id;
                        #region MyRegion
                        if (!string.IsNullOrEmpty(item.BobbinNo) && string.IsNullOrEmpty(item.BobbinNo2))
                        {//通过
                            inorout.Append("1");
                        }
                        else if (string.IsNullOrEmpty(item.BobbinNo))
                        {//补充
                            var mid = midstores.FindAll(x => x.IsOccupied == 1 && x.StructBarCode == spool.StructBarCode.Trim() && x.BobbinNo == item.BobbinNo2.ToArray()[0]).OrderByDescending(x => x.Updated).FirstOrDefault();
                            //inorout.Append("1");
                            instorearmtsks.Add(new RobotArmTaskEntity()
                            {
                                #region MyRegion
                                CName = mid.CName.Trim(),
                                Created = createtime,
                                StorageArea = this.StorageArea,
                                SeqNo = ++armtskseqno,
                                PlantNo = this.PlantNo,
                                RobotArmID = this.RobotArmID,
                                TaskType = 1,
                                ToWhere = 2,
                                TaskLevel = 7,
                                ProductType = tskconfig.Item3.ToString(),
                                AGVSeqNo = agvseqno,
                                FromWhere = mid.SeqNo,
                                SpoolSeqNo = item.SeqNo,
                                WhoolBarCode = mid.FdTagNo.Trim(),
                                TaskGroupGUID = guid
                                #endregion
                            });
                            mid.IsOccupied = 4;
                            item.SpoolId = mid.SpoolId;
                            midsupdate.Add(mid);
                        }
                        else
                        {//替换
                            inorout.Append("0");
                            //入库
                            var midin = midstores.FindAll(x => x.Spool == null && x.IsOccupied == 0).OrderByDescending(x => x.Updated).FirstOrDefault();
                            midin.IsOccupied = 5;
                            midin.IdsList = spools.FirstOrDefault(x => x.FdTagNo.Trim() == item.FdTagNo.Trim())?.Id.ToString();
                            midsupdate.Add(midin);
                            instorearmtsks.Add(new RobotArmTaskEntity()
                            {
                                #region MyRegion
                                CName = item.CName.Trim(),
                                Created = createtime,
                                StorageArea = this.StorageArea,
                                SeqNo = ++armtskseqno,
                                PlantNo = this.PlantNo,
                                RobotArmID = this.RobotArmID,
                                TaskType = 2,
                                ToWhere = 2,
                                TaskLevel = 7,
                                ProductType = tskconfig.Item3.ToString(),
                                AGVSeqNo = agvseqno,
                                FromWhere = midin.SeqNo,
                                SpoolSeqNo = item.SeqNo,
                                WhoolBarCode = item.FdTagNo.Trim(),
                                TaskGroupGUID = guid
                                #endregion
                            });
                            //出库
                            var midout = midstores.FindAll(x => x.IsOccupied == 1 && x.StructBarCode == spool.StructBarCode.Trim() && x.BobbinNo == item.BobbinNo2.ToArray()[0]).OrderByDescending(x => x.Updated).FirstOrDefault();
                            instorearmtsks.Add(new RobotArmTaskEntity()
                            {
                                #region MyRegion
                                CName = item.CName.Trim(),
                                Created = createtime,
                                StorageArea = this.StorageArea,
                                SeqNo = ++armtskseqno,
                                PlantNo = this.PlantNo,
                                RobotArmID = this.RobotArmID,
                                TaskType = 1,
                                ToWhere = 2,
                                TaskLevel = 7,
                                ProductType = tskconfig.Item3.ToString(),
                                AGVSeqNo = agvseqno,
                                FromWhere = midout.SeqNo,
                                SpoolSeqNo = item.SeqNo,
                                WhoolBarCode = midout.FdTagNo.Trim(),
                                TaskGroupGUID = guid
                                #endregion
                            });
                            midout.IsOccupied = 4;
                            //item.SpoolId = midout.SpoolId;
                            midsupdate.Add(midout);
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        logger.ErrorMethod("创建直通出库任务失败", ex);
                        return false;
                    }
                }
                char[] arr = inorout.ToString().ToCharArray();
                Array.Reverse(arr);
                string Reverse_inorout = new string(arr);
                inlineoutqueue = System.Convert.ToInt32(Reverse_inorout.ToString(), 2);
                List<InStoreToOutStoreSpoolEntity> ret = new List<InStoreToOutStoreSpoolEntity>();
                foreach (var item in list)
                {
                    ret.Add(new InStoreToOutStoreSpoolEntity() { AGVSeqNo = agvseqno, Created = createtime, Guid = guid, InLineNo = 2, SpoolId = item.SpoolId, Status = 0, PlantNo = this.PlantNo, StoreageNo = this.StorageArea });
                }
                AGVTasksEntity insertagvtsk = new AGVTasksEntity() { Created = createtime, TaskGuid = guid, PlantNo = this.PlantNo, ProductType = spool.CName, SeqNo = agvseqno, StorageArea = this.StorageArea, StorageLineNo = 2, TaskLevel = 6, TaskType = 2, EquipIdListTarget = TaskConfig.AGVStation(this.StorageArea, 2), Status = 1 };
                bool issuccess = this.BusinessLogic.SqlCommandProvider.OutStoreageTask(null, midsupdate, insertagvtsk, instorearmtsks, ret);
                logger.InfoMethod("出库队列为:" + Reverse_inorout.ToString() + ",guid:" + guid.ToString());
                return issuccess;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod(this.StorageArea + "号库直通口任务失败", ex);
                return false;
            }
        }


        int seq1 = 1;
        int lastseqno = 1;
        Random ran = new Random(DateTime.Now.Second);
        List<int> SeqNo3 = new List<int>();
        int getNextSeqNo()
        {
            while (true)
            {
                if (seq1 == 1 || seq1 == 30000)
                    seq1 = ran.Next(1, 30000);
                else seq1++;

                if (!SeqNo3.Exists(x => x == seq1))
                {
                    lastseqno = seq1;
                    if (SeqNo3.Count >= 12)
                        SeqNo3.RemoveAt(0);
                    SeqNo3.Add(lastseqno);
                    break;
                }
            }
            return lastseqno;
        }

        /// <summary>
        /// 直通线上扫到的条码,从MES查具体信息,然后插入到自己的数据库
        /// -1错误的LR面信息;0MES系统不存在此二维码;1添加成功;2添加失败
        /// </summary>
        /// <param name="barcode"></param>
        int MesToSNTON(string barcode, out string lr, out MESSystemSpoolsEntity messpool)
        {
            lr = "";
            SpoolsEntity spool = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcode(barcode, null);
            if (spool != null)
            {
                messpool = new MESSystemSpoolsEntity() { CName = spool.CName, BobbinNo = spool.BobbinNo.ToString(), Const = spool.Const, FdTagNo = spool.FdTagNo, GroupID = spool.GroupID, Length = spool.Length, SeqNo = 0, StructBarCode = spool.StructBarCode };
                lr = spool.BobbinNo.ToString();
                return 1;
            }
            messpool = this.BusinessLogic.MESSystemProvider.GetMESSpool(barcode, null);//从MES系统查轮子信息存到本地数据库
            if (messpool == null)
                messpool = this.BusinessLogic.MESSystemProvider.GetMESSpool(barcode, null);//从MES系统查轮子信息存到本地数据库
            if (messpool == null)
                messpool = this.BusinessLogic.MESSystemProvider.GetMESSpool(barcode, null);//从MES系统查轮子信息存到本地数据库
            if (messpool == null)
                return 0;
            if (messpool.BobbinNo.Trim() != "L" && messpool.BobbinNo.Trim() != "R")
            {
                if (!string.IsNullOrWhiteSpace(messpool.BobbinNo))
                    lr = messpool.BobbinNo.Trim();
                return -1;
            }
            spool = new SpoolsEntity();
            spool.FdTagNo = barcode;
            if (messpool != null)
            {
                spool.Created = DateTime.Now;
                spool.IsDeleted = 0;
                spool.Const = messpool.Const.Trim();
                spool.ProductType = messpool.CName;
                spool.CName = messpool.CName;
                spool.Length = messpool.Length;
                spool.FdTagNo = messpool.FdTagNo;
                spool.StructBarCode = messpool.StructBarCode;
                spool.GroupID = messpool.GroupID;
                spool.BobbinNo = messpool.BobbinNo.Trim().ToArray()[0];
                spool.CName = messpool.CName.Trim();
            }
            int result = this.BusinessLogic.SpoolsProvider.Add(spool, null);
            if (result != 1)
                return 2;
            return result;
        }
        /// <summary>
        /// 反馈回调
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool ProcessingCallHandler(WorkItem item)
        {
            bool processed = true;
            try
            {
                if (item.data.RawPacket != null)
                {
                    string theMessage = System.Text.UTF8Encoding.UTF8.GetString(item.data.RawPacket);
                    processed = ProcessMessage(item.data);
                }
                else
                {
                    logger.ErrorMethod("Neutrino to process did not contain any RawPacket Data.");
                }
                DateTime noDb = DateTime.UtcNow;

                item.data.TimeCallingTheDatabase = noDb;
                //int ret = 1;// Execute(item);
                item.data.TimeBackFromTheDatabase = noDb;
                item.data.TimeFinishedProcessing = DateTime.UtcNow;
                //logger.InfoMethod(string.Format("Processing took {0}", ret)); 
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Error", e);
                return true;
            }
            return true;
        }

        /// <summary>
        /// 处理接收的消息
        /// </summary>
        /// <param name="neutrino"></param>
        /// <returns></returns>
        protected override bool ProcessMessage(Neutrino neutrino)
        {
            return base.ProcessMessage(neutrino);
        }
        //string qrcode = "";
        void SavebarcodeQueue()
        {
            {
                string js = Newtonsoft.Json.JsonConvert.SerializeObject(barcodeQueue);
                var by = Encoding.UTF8.GetBytes(js);
                File.WriteAllBytes($".{SNTONConstants.FileTmpPath}/StorageArea{StorageArea}QrCode.json", by);
            }
            {
                string js = Newtonsoft.Json.JsonConvert.SerializeObject(barcodeLRQueue);
                var by = Encoding.UTF8.GetBytes(js);
                File.WriteAllBytes($".{SNTONConstants.FileTmpPath}/StorageArea{StorageArea}LRQrCode.json", by);
            }
        }
        public Queue<string> GetbarcodeQueue()
        {
            if (!File.Exists($".{SNTONConstants.FileTmpPath}/StorageArea{StorageArea}QrCode.json"))
                return new Queue<string>();
            Queue<string> obj = new Queue<string>();
            try
            {
                string json = File.ReadAllText($".{SNTONConstants.FileTmpPath}/StorageArea{StorageArea}QrCode.json");
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Queue<string>>(json);
            }
            catch (Exception ex)
            {
                logger.WarnMethod("反序列化barcodeQueue失败", ex);
            }
            return obj;
        }
        public Queue<MESSystemSpoolsEntity> GetbarcodeLRQueue()
        {
            if (!File.Exists($".{SNTONConstants.FileTmpPath}/StorageArea{StorageArea}LRQrCode.json"))
                return new Queue<MESSystemSpoolsEntity>();
            Queue<MESSystemSpoolsEntity> obj = new Queue<MESSystemSpoolsEntity>();
            try
            {
                string json = File.ReadAllText($".{SNTONConstants.FileTmpPath}/StorageArea{StorageArea}LRQrCode.json");
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Queue<MESSystemSpoolsEntity>>(json);
            }
            catch (Exception ex)
            {
                logger.WarnMethod("反序列化barcodeLRQueue失败", ex);
            }
            return obj;
        }
    }

}