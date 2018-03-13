using SNTON.Entities.DBTables.AGV;
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
    public class MidStoreLineLogic : ComLogic
    {
        public MidStoreLineLogic()
        {
            threadchecktask = new VIThreadEx(CheckTask, null, "CheckMidStoreLineTask", 3000);
            thread_ReadBarCode = new VIThreadEx(ReadBarCode, null, "read BarCode from PLC", 3000);
            thread_LineStatusCallAGV = new VIThreadEx(LineStatusCallAGV, null, "check line status to call agv", 3000);
            thread_heartbeat = new VIThreadEx(heartbeat, null, "heartbeat", 1000);
            thread_warninginfo = new VIThreadEx(ReadWarningInfo, null, "ReadMidStoreLineWarnning", 5000);
            //qrcode = $"./StorageArea{StorageArea}QrCode.json";
        }
        //Dictionary<string, bool> _DicWarnning = new Dictionary<string, bool>();
        List<MachineWarnningCodeEntity> _WarnningCode = new List<MachineWarnningCodeEntity>();
        public new static MidStoreLineLogic Create(XmlNode node)
        {
            MidStoreLineLogic m = new MidStoreLineLogic();
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
            thread_LineStatusCallAGV.Start();
            thread_warninginfo.Start();
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
            _WarnningCode = this.BusinessLogic.MachineWarnningCodeProvider.MachineWarnningCodes.FindAll(x => x.MachineCode == 2);
            if (_WarnningCode == null || _WarnningCode.Count == 0)
                return;
            Neutrino ne = new Neutrino();
            ne.TheName = "ReadMidStoreRobotArmWarnning";
            foreach (var item in _WarnningCode.GroupBy(x => x.AddressName.Trim()))
            {
                ne.AddField(item.Key.Trim(), "0");
            }
            var n = this.MXParser.ReadData(ne, true);
            foreach (var item in _WarnningCode.GroupBy(x => x.AddressName.Trim()))
            {
                int i = n.Item2.GetInt(item.Key.Trim());
                char[] binary = System.Convert.ToString(i, 2).ToArray();
                for (int c = 0; c < binary.Length; c++)
                {
                    var fi = _WarnningCode.FirstOrDefault(x => x.BIT == c && x.AddressName.Trim() == item.Key);
                    if (fi == null)
                        continue;
                    if (binary[c] != '0' && !fi.Value)
                    {
                        fi.Value = true;
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "号线体" + fi.Description.Trim(), Source = this.StorageArea + "号线体报警", MsgLevel = 7 });

                    }
                    else
                        fi.Value = false;
                }
            }
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
                LineCallAGV2(MidStoreLine.InStoreLine, seqno);
            }
            if (outline == 1)
            {//出库线
                seqno = neutrino.GetIntOrDefault("OUTSTORE_SeqNo_Read");
                LineCallAGV2(MidStoreLine.OutStoreLine, seqno);
            }
        }

        [Obsolete("用LineCallAGV2代替")]
        void LineCallAGV(int midline)
        {
            string line = "";
            if (midline == MidStoreLine.InStoreLine)
            {
                line = "直通线";
            }
            else if (midline == MidStoreLine.OutStoreLine)
            { line = "正常出库线"; }
            List<RobotArmTaskEntity> list = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskStatus=9 AND ToWhere IN (1,2) AND RobotArmID='{RobotArmID}' AND [StorageArea]={StorageArea}", null);//找到该任务单元 1出库口,2直通线 常量值:MidStoreLine
            if (list == null || list.Count == 0)
                return;
            var agvtsks = this.BusinessLogic.AGVTasksProvider.GetAGVTasks($"Status IN (16,17)  AND StorageLineNo={midline} AND StorageArea={StorageArea}", null);
            {//
                var online_armtsks = list.FindAll(x => x.ToWhere == midline);
                var group_armtsks = online_armtsks.GroupBy(x => x.TaskGroupGUID);
                var guids = from i in online_armtsks
                            select i.TaskGroupGUID;
                if (group_armtsks.Count() == 0)
                {
                    logger.ErrorMethod(line + "出库口等待AGV接收,但是没发现对应的龙门及AGV任务");
                }
                else if (group_armtsks.Count() > 1)
                {
                    logger.ErrorMethod(line + "出库口等待AGV接收,发现多个对应的龙门及AGV任务");
                    logger.ErrorMethod(JsonConvert.SerializeObject(guids));
                }
                if (group_armtsks.Count() != 0)
                {
                    var guid = guids.First();
                    var agvtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTask($"TaskGuid in ('{ guid.ToString()}')");
                    var armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskGroupGUID='{guid.ToString()}'", null);
                    armtsks.ForEach(x => x.TaskStatus = 4);
                    this.BusinessLogic.RobotArmTaskProvider.UpdateArmTaskStatus(guid, 4);
                    //将armtask改成4
                    if (agvtsk != null)
                    {
                        agvtsk.Status = 2;
                        this.BusinessLogic.AGVTasksProvider.UpdateEntity(agvtsk);
                    }
                }

            }
        }
        void LineCallAGV2(int midline, int seqno = 0)
        {
            string line = "";
            if (midline == MidStoreLine.InStoreLine)
            {
                line = "直通线";
            }
            else if (midline == MidStoreLine.OutStoreLine)
            {
                line = "正常出库线";
            }
            var agvtsks = this.BusinessLogic.AGVTasksProvider.GetAGVTasks($" Status IN (1)  AND SeqNo={seqno} AND StorageLineNo={midline} AND StorageArea={StorageArea}", null);
            //agvtsks = this.BusinessLogic.AGVTasksProvider.GetAGVTasks($" Status IN (1)  AND StorageLineNo={midline} AND StorageArea={StorageArea}", null);//AND SeqNo={seqno} 
            if (agvtsks == null || agvtsks.Count == 0)
            {
                logger.WarnMethod($"{StorageArea}号暂存库{line}线体找不到对应的AGV接料任务,SeqNo={seqno} !");
                return;
            }
            else if (agvtsks.Count > 1)
            {
                logger.WarnMethod($"{StorageArea}号暂存库{line}线体找到多个对应的AGV接料任务,SeqNo={seqno} !");
            }
            var agvtsk = agvtsks.OrderBy(x => x.Created).ToList().FirstOrDefault();
            var armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskGroupGUID='{agvtsk.TaskGuid.ToString()}'", null);
            if (armtsks != null && armtsks.Count != 0)
                if (armtsks.Exists(x => x.SpoolStatus == 1))
                {
                    return;
                }
            //armtsks.ForEach(x => x.TaskStatus = 4);
            this.BusinessLogic.RobotArmTaskProvider.UpdateArmTaskStatus(agvtsk.TaskGuid, 4);
            //将armtask改成4
            if (agvtsk != null)
            {
                agvtsk.Status = 2;
                if (midline == MidStoreLine.InStoreLine)
                {//直通口单丝已到位
                    agvtsk.Status = 7;
                    var t = this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.GetInStoreToOutStoreSpoolEntity($"GUID='{agvtsk.TaskGuid.ToString()}'", null);
                    if (t != null)
                    {
                        t.ForEach(x => x.Status = 3);
                        this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.UpdateEntity(null, t.ToArray());
                    }
                }
                agvtsk.Updated = DateTime.Now;
                this.BusinessLogic.AGVTasksProvider.UpdateEntity(agvtsk);
                logger.InfoMethod($"{StorageArea}号库{line}线体收到接料请求,将AGV请求状态改为2,guid:" + agvtsk.TaskGuid.ToString());
            }
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
                    this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "暂存库扫码异常口满", Source = this.StorageArea + "暂存库扫码异常口满", MsgLevel = 7 });
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
            //Add condition when barcode and load signal both reach at the same time
            //By Song@2018.01.25
            //if (issuccess == 1)
            if (issuccess == 1 && string.IsNullOrEmpty(barcode.Trim()))
            {
                //goto GOLOGIC;
                if (barcodeQueue.Count != 0)
                {
                    #region  0暂存库已满,1入库;2出库
                    int agvseqno = 0;
                    int outstoreagequeue = 0;//直通口出库时告诉线体替换或通过
                    int i = InLineRobotArmTask3(out agvseqno, out outstoreagequeue);//1入库;2出库
                    if (i == 0)
                    {
                        if (!IsStoreageEnough)
                        {
                            IsStoreageEnough = true;
                            this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "号暂存库已满", Source = this.StorageArea + "号暂存库已满", MsgLevel = 6 });
                        }
                        return;
                    }
                    IsStoreageEnough = false;
                    i = i == 3 ? 2 : i;
                    if (i == 2)
                    {
                        neclear.AddField("ONLINE_SeqNo_Write", agvseqno.ToString());//出库流水号 AGV任务流水号
                        neclear.AddField("BACKUP6", outstoreagequeue.ToString());//出库流水号 AGV任务流水号
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
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "错误的LR面信息:" + lr.ToString() + "," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 6 });
                        logger.WarnMethod("错误的LR面信息," + barcode + "," + lr.ToString());
                        toExceptionFlow = true;
                        barcodeReadingInfo = barcodeReadingInfo + "(Reason: Wrong L/R) => Exception route";
                    }
                    else if (re == 0)
                    {
                        logger.WarnMethod("MES系统不存在此二维码," + barcode);
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "MES系统不存在此二维码," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 6 });
                        toExceptionFlow = true;
                        barcodeReadingInfo = barcodeReadingInfo + "(Reason: No MES info) => Exception route";
                    }
                    else
                    {
                        logger.WarnMethod("添加到SNTON失败," + barcode);
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "添加到SNTON失败," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5 });
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
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "扫码失败," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 6 });
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
                this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "添加到SNTON失败," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5 });
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
            List<EquipTaskViewEntity> equplinetsks;
            if (StorageArea == 3)
                equplinetsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities($"Status IN (0) AND PlantNo=3 AND StorageArea={StorageArea} AND Supply1='{spool.StructBarCode.Trim()}'", null);
            else
                equplinetsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities($"Status IN (0) AND PlantNo=3 AND StorageArea=12 AND Supply1='{spool.StructBarCode.Trim()}'", null);
            var midstores = this.BusinessLogic.MidStorageSpoolsProvider.GetMidStorages($"StorageArea ={StorageArea} AND IsOccupied IN (0)", null);//找出库里的空闲位置
            if (midstores == null || midstores.Count < barcodeQueue.Count)
            {
                return 0;
            }
            var tskconfig = TaskConfig.GetEnoughAGVEquipCount(spool.ProductType.Trim());//8 / 12 
            if (equplinetsks == null)
                equplinetsks = new List<EquipTaskViewEntity>();

            //创建AGVTask和RobotArmTask,并优先级最高
            var guid = Guid.NewGuid();
            //通过GUID标记一个龙门任务单元
            DateTime createtime = DateTime.Now;
            int iscreate = 0;
            //iscreate = CreateRobotAGVTask((short)StorageArea, equplinetsks, barcodeQueue.Count, 2);
            //if (StorageArea == 3)
            //    iscreate = CreateRobotAGVTask((short)StorageArea, equplinetsks, barcodeQueue.Count, 2);
            //else
            //    iscreate = CreateRobotAGVTask(12, equplinetsks, barcodeQueue.Count, 2);
            if (iscreate == 1)
            {
                logger.InfoMethod("已在直通线创建出库任务");
                cmd = 2;
                return cmd;
            }
            else
            {
                //全部入库
                cmd = 1;
                //没有需要该类型轮子的设备的任务,将所有轮子创建直通线入库任务
                logger.InfoMethod(" 没有需要该类型轮子的设备的任务,或者设备任务数量不够,或者设备排列不满足条件，将所有轮子创建直通线入库任务");
                #region MyRegion

                List<RobotArmTaskEntity> instorearmtsks = new List<RobotArmTaskEntity>();
                var instore = midstores.FindAll(x => x.Spool == null && x.IsOccupied == 0).OrderBy(x => x.SeqNo).Take(barcodeQueue.Count).ToList();//取出空位置
                RobotArmTaskEntity armtsk = null;
                if (instore.Count < barcodeQueue.Count)
                {
                    return 0;
                }
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
            }

            return cmd;
        }

        /// <summary>
        /// 直通线出库功能
        /// 0暂存库已满,1入库;2出库
        /// <param name="outstoreagequeue">直通口出库时告诉线体替换或通过</param>
        /// </summary>
        /// <returns></returns>
        int InLineRobotArmTask3(out int agvseqno, out int outstoreagequeue)
        {
            int cmd = 1;
            outstoreagequeue = 0;
            agvseqno = 0;
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
            var levellist = this.BusinessLogic.ProductProvider.GetAllProductEntity(null);
            islevel = CheckSpoolIsLevel(spool, levellist);
            List<InStoreToOutStoreSpoolViewEntity> ttt = this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.GetInStoreToOutStoreSpool(this.StorageArea, this.PlantNo, null);
            if (islevel && tskconfig.Item4 < barcodeLRQueue.Count && ttt != null && ttt.Count < 2)
            {//满足优先级且大于半车,搭配出库
             //若需要调整,判断库里是否有能满足搭配出库的单丝
             //替换/补充 1,3 2,4
             //-2库位不足,-1入库,0出库,1替换,2补充,3补充L,4补充R
                int instore = InStoreToOutStore(tskconfig, list);
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
            }
            if (cmd == 2)
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
            int armtskseqno = 0;
            MESSystemSpoolsEntity spool = list[0];
            //midstores = midstores.FindAll(x => !string.IsNullOrEmpty(x.StructBarCode) && x.StructBarCode.Trim() == codeseqt.StructBarCode.Trim());
            List<RobotArmTaskEntity> instorearmtsks = new List<RobotArmTaskEntity>();
            //RobotArmTaskEntity armtsk = null;
            List<MidStorageSpoolsEntity> midsupdate = new List<MidStorageSpoolsEntity>();

            try
            {

                var spools = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcodes(null, (from i in list select i.FdTagNo.Trim()).ToArray());
                foreach (var item in list)
                {//1替换,2补充,3补充L,4补充R
                    try
                    {
                        item.SpoolId = spools.FirstOrDefault(x => x.FdTagNo.Trim() == item.FdTagNo.Trim()).Id;
                        #region MyRegion
                        if (item.InStoreToOutStoreage == 1)
                        {//出 替换
                            inorout.Append("1");

                            #region 替换
                            //入库
                            var midin = midstores.FindAll(x => x.Spool == null && x.IsOccupied == 0).OrderBy(x => x.SeqNo).FirstOrDefault();
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

                            MidStorageSpoolsEntity midout = null;
                            if (item.BobbinNo.Trim() == "L")
                            {//把L替换成R
                                midout = midstores.FindAll(x => x.IsOccupied == 1 && x.StructBarCode == spool.StructBarCode.Trim() && x.BobbinNo == 'R').OrderBy(x => x.Updated).FirstOrDefault();
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
                                item.SpoolId = midout.SpoolId;
                                midsupdate.Add(midout);
                            }
                            else
                            {//把R替换成L
                                midout = midstores.FindAll(x => x.IsOccupied == 1 && x.StructBarCode == spool.StructBarCode.Trim() && x.BobbinNo == 'L').OrderBy(x => x.Updated).FirstOrDefault();
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
                                item.SpoolId = midout.SpoolId;
                                midsupdate.Add(midout);
                            }
                            #endregion
                        }
                        else if (item.InStoreToOutStoreage == 3)
                        {//补充L
                            var midout = midstores.FindAll(x => x.IsOccupied == 1 && x.StructBarCode == spool.StructBarCode.Trim() && x.BobbinNo == 'L').OrderBy(x => x.Updated).FirstOrDefault();
                            instorearmtsks.Add(new RobotArmTaskEntity()
                            {
                                #region MyRegion
                                CName = midout.CName.Trim(),
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
                            item.SpoolId = midout.SpoolId;
                            midsupdate.Add(midout);
                        }
                        else if (item.InStoreToOutStoreage == 4)
                        {//补充R
                            var midout = midstores.FindAll(x => x.IsOccupied == 1 && x.StructBarCode == spool.StructBarCode.Trim() && x.BobbinNo == 'R').OrderBy(x => x.Updated).FirstOrDefault();
                            instorearmtsks.Add(new RobotArmTaskEntity()
                            {
                                #region MyRegion
                                CName = midout.CName.Trim(),
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
                            item.SpoolId = midout.SpoolId;
                            midsupdate.Add(midout);
                        }
                        else
                        {
                            inorout.Append("0");
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
                inlineoutqueue = System.Convert.ToInt32(inorout.ToString(), 2);
                List<InStoreToOutStoreSpoolEntity> ret = new List<InStoreToOutStoreSpoolEntity>();
                foreach (var item in list)
                {
                    ret.Add(new InStoreToOutStoreSpoolEntity() { AGVSeqNo = agvseqno, Created = createtime, Guid = guid, InLineNo = 2, SpoolId = item.SpoolId, Status = 0, PlantNo = this.PlantNo, StoreageNo = this.StorageArea });
                }
                AGVTasksEntity insertagvtsk = new AGVTasksEntity() { Created = createtime, TaskGuid = guid, PlantNo = this.PlantNo, ProductType = spool.CName, SeqNo = agvseqno, StorageArea = this.StorageArea, StorageLineNo = 2, TaskLevel = 6, TaskType = 2, EquipIdListTarget = TaskConfig.AGVStation(this.StorageArea, 2) };
                bool issuccess = this.BusinessLogic.SqlCommandProvider.OutStoreageTask(null, midsupdate, insertagvtsk, instorearmtsks, null);
                return issuccess;
            }
            catch (Exception ex)
            {
                logger.ErrorMethod(this.StorageArea + "号库直通口任务失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 直通线出库
        /// -2库位不足,-1入库,0出库,1替换,2补充,3补充L,4补充R
        /// </summary>
        /// <param name="codeseqt"></param>
        /// <param name="tskconfig"></param>
        /// <param name="list">直通线上的单丝</param>
        private int InStoreToOutStore(Tuple<int, int, int, int, int> tskconfig, List<MESSystemSpoolsEntity> list)
        {
            var lr = from i in list select i.BobbinNo;
            MESSystemSpoolsEntity codeseqt = list[0];
            while (list.Count < tskconfig.Item1)
            {
                list.Add(new MESSystemSpoolsEntity() { InStoreToOutStoreage = 2, StructBarCode = codeseqt.StructBarCode.Trim() });
            }
            var midlr = this.BusinessLogic.MidStorageSpoolsProvider.GetMidStorages($"StorageArea ={StorageArea} AND IsOccupied IN (1,0)", null);//找出库里的空闲位置
            if (midlr == null) midlr = new List<MidStorageSpoolsEntity>();
            if (midlr.FindAll(x => x.BobbinNo == 'L' && x.StructBarCode.Trim() == codeseqt.StructBarCode.Trim()).Count < (tskconfig.Item4 - list.FindAll(x => x.BobbinNo == "L").Count))
                return -1;//L不够补足
            if (midlr.FindAll(x => x.BobbinNo == 'R' && x.StructBarCode.Trim() == codeseqt.StructBarCode.Trim()).Count < (tskconfig.Item4 - list.FindAll(x => x.BobbinNo == "R").Count))
                return -1;//R不够补足
            int emptymids = 0;
            if (tskconfig.Item4 - list.FindAll(x => x.BobbinNo == "L").Count < 0)
                emptymids += (tskconfig.Item4 - list.FindAll(x => x.BobbinNo == "L").Count);//取出几个L
            if (tskconfig.Item4 - list.FindAll(x => x.BobbinNo == "R").Count < 0)
                emptymids += (tskconfig.Item4 - list.FindAll(x => x.BobbinNo == "R").Count);//取出几个R

            if (midlr.FindAll(x => x.IsOccupied == 0).Count + emptymids < 0)
                return -2;//剩余库位不足,库存已满

            var p1 = list.Take(tskconfig.Item4 / 2).ToList();
            var p2 = list.Skip(tskconfig.Item4 / 2).Take(tskconfig.Item4 / 2).ToList();
            var p3 = list.Skip((tskconfig.Item4 / 2) * 2).Take(tskconfig.Item4 / 2).ToList();
            var p4 = list.Skip((tskconfig.Item4 / 2) * 3).Take(tskconfig.Item4 / 2).ToList();


            List<MESSystemSpoolsEntity> p13 = new List<MESSystemSpoolsEntity>();
            p13.AddRange(p1);
            p13.AddRange(p3);
            List<MESSystemSpoolsEntity> p24 = new List<MESSystemSpoolsEntity>();
            p24.AddRange(p2);
            p24.AddRange(p4);
            var l13 = p13.FindAll(x => x.BobbinNo == "L");
            var r13 = p13.FindAll(x => x.BobbinNo == "R");
            var l24 = p24.FindAll(x => x.BobbinNo == "L");
            var r24 = p24.FindAll(x => x.BobbinNo == "R");
            //L-R
            int cl13 = tskconfig.Item4 / 2 - l13.Count;//13段需要补充几个L 大于0需要补充,小于0需要取出
            int cr13 = tskconfig.Item4 / 2 - r13.Count;//13段需要补充几个R
            int cl24 = tskconfig.Item4 / 2 - l24.Count;//24段需要补充几个L
            int cr24 = tskconfig.Item4 / 2 - r24.Count;//24段需要补充几个R
                                                       //0放过,1替换,2补充,3补充L,4补充R
            if (cl13 <= 0)
            {//需要将L替换成R
                cr13 = cl13 + cr13;//需要补充R
                p13.FindAll(x => x.BobbinNo == "L").Take(-cl13).ToList().ForEach(x => x.InStoreToOutStoreage = 1);
                p13.FindAll(x => x.InStoreToOutStoreage == 2).ForEach(x => x.InStoreToOutStoreage = 4);
            }
            else if (cr13 <= 0)
            {//需要将R替换成L
                cl13 = cl13 + cr13;//需要补充L
                p13.FindAll(x => x.BobbinNo == "R").Take(-cr13).ToList().ForEach(x => x.InStoreToOutStoreage = 1);
                p13.FindAll(x => x.InStoreToOutStoreage == 2).ForEach(x => x.InStoreToOutStoreage = 3);
            }
            if (cl24 <= 0)
            {//需要将L替换成R
                cr24 = cl24 + cr24;//需要补充R
                p24.FindAll(x => x.BobbinNo == "L").Take(-cl24).ToList().ForEach(x => x.InStoreToOutStoreage = 1);
                p24.FindAll(x => x.InStoreToOutStoreage == 2).ForEach(x => x.InStoreToOutStoreage = 4);
            }
            else if (cr24 <= 0)
            {//需要将R替换成L
                cl24 = cl24 + cr24;//需要补充L
                p24.FindAll(x => x.BobbinNo == "R").Take(-cr24).ToList().ForEach(x => x.InStoreToOutStoreage = 1);
                p24.FindAll(x => x.InStoreToOutStoreage == 2).ForEach(x => x.InStoreToOutStoreage = 3);
            }

            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageno">暂存库编号,1,2,3</param>
        /// <param name="list">所有未分配任务的送料EquipTask</param>
        /// 
        /// <param name="inlinecount">直通线上的单丝数量</param>
        /// <param name="lineno">线体编号</param>
        ///<returns></returns>
        public int CreateRobotAGVTask(short storageno, List<EquipTaskViewEntity> list, int inlinecount, int lineno = 1)
        {//所有同种规格的单丝
            int result = 0;
            var Storage = list.FindAll(x => x.StorageArea.Trim() == storageno.ToString());
            var agvroutes = Storage.GroupBy(x => x.AGVRoute.Trim());
            foreach (var item in agvroutes)
            {
                result = NewMethod(storageno, item, inlinecount, lineno);
                if (result == 1)
                    return 1;
            }
            return 0;

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
        /// 检测是否有能够创建AGV任务的EquipTask
        /// </summary>
        /// <param name="storageno"></param>
        /// <param name="supply1">按照单丝标准书分组的单组设备任务</param>
        /// <returns>1创建任务;0没有创建任务</returns>
        private int NewMethod(short storageno, IGrouping<string, EquipTaskViewEntity> supply1, int inlinecount, int lineno = 1)
        {
            int seq = getNextSeqNo();
            EquipTaskViewEntity exequiptsk = supply1.FirstOrDefault();
            var tskconfig = TaskConfig.GetEnoughAGVEquipCount(exequiptsk.ProductType);//8 / 12
            if (tskconfig.Item1 == 0)
            {
                logger.WarnMethod("未知的单丝型号");
                return 0;
            }
            int needstoeageno = tskconfig.Item1 - inlinecount;
            //if (needstoeageno == 0)
            //{
            //    logger.InfoMethod("直通线上的单丝刚好够一车");
            //    return 1;
            //}
            var mids = this.BusinessLogic.MidStorageProvider.GetMidStorageByArea(storageno, null);
            mids = mids.FindAll(x => x.IsOccupied == 1 && x.Spool.StructBarCode.Trim() == exequiptsk.Supply1.Trim());
            if (needstoeageno > mids.Count)
            {
                StringBuilder sb = new StringBuilder();
                supply1.ToList().ForEach(x => sb.Append(x.EquipContollerId + ","));
                //logger.InfoMethod(storageno + "号暂存库中的单丝不满足出库数量,单丝作业标准书:" + supply1.Key.Trim() + ",地面滚筒id:" + sb.ToString().Trim(','));
                return 0;
            }
            if (needstoeageno <= mids.Count())
            {
                #region MyRegion
                //暂存库单丝轮数量足够创建一车任务并且需要该类型单丝轮的设备也满一车
                //创建龙门Task和AGVTask
                //更新EquipTask状态
                //更新库位状态
                var guid = Guid.NewGuid();
                //通过GUID标记一个龙门任务单元
                DateTime createtime = DateTime.Now;
                RobotArmTaskEntity armtsk = null;
                List<RobotArmTaskEntity> listarmtsk = new List<RobotArmTaskEntity>();
                var agvtsk = new AGVTasksEntity() { Created = createtime, SeqNo = seq, TaskGuid = guid, PlantNo = PlantNo, ProductType = exequiptsk.ProductType, Status = 0, TaskLevel = 5 };

                var tskspools = mids.OrderBy(x => x.Updated).Take(needstoeageno).ToList();//先取出入库时间早的单丝轮 
                foreach (var spool in tskspools)
                {
                    #region 将库里的单丝分配龙门出库任务
                    armtsk = new RobotArmTaskEntity();
                    armtsk.Created = createtime;
                    armtsk.TaskGroupGUID = guid;
                    armtsk.FromWhere = spool.SeqNo;
                    armtsk.PlantNo = PlantNo;
                    armtsk.RobotArmID = storageno.ToString();
                    armtsk.TaskLevel = 5;
                    armtsk.TaskType = 0;
                    armtsk.AGVSeqNo = seq;
                    armtsk.ToWhere = lineno;
                    armtsk.TaskStatus = 0;
                    if (lineno == 1)
                        armtsk.TaskType = 0;
                    else
                        armtsk.TaskType = 1;
                    armtsk.SpoolStatus = 0;
                    //armtsk.EquipControllerId = 1;
                    armtsk.StorageArea = StorageArea;
                    armtsk.WhoolBarCode = spool.Spool.FdTagNo;
                    armtsk.ProductType = tskconfig.Item3.ToString();
                    spool.Updated = DateTime.Now;
                    spool.IsOccupied = 2;
                    listarmtsk.Add(armtsk);
                    #endregion
                }

                var creaequptsk = supply1.OrderByDescending(x => x.Created).Take(tskconfig.Item2).OrderBy(x => x.AStation);
                if (creaequptsk.Count() < tskconfig.Item2)
                {
                    logger.InfoMethod("没有足够的叫料设备");
                    return 0;
                }
                foreach (var equtsk in creaequptsk)
                {
                    equtsk.Status = 1;
                    equtsk.TaskGuid = guid;
                    equtsk.Updated = createtime;
                    agvtsk.EquipIdListActual = agvtsk.EquipIdListActual + ";" + equtsk.EquipContollerId.ToString();
                }
                agvtsk.TaskType = 2;
                if (needstoeageno == 0)
                    agvtsk.Status = 2;
                #region 更新EquipTask 更新暂存库位置状态 创建AGVTask 创建龙门Task
                agvtsk.StorageArea = storageno;
                agvtsk.StorageLineNo = lineno;
                agvtsk.EquipIdListActual = agvtsk.EquipIdListActual.Trim(';');
                agvtsk.EquipIdListTarget = TaskConfig.AGVStation(storageno, lineno);
                logger.InfoMethod("=================================================================================");
                this.BusinessLogic.EquipTaskViewProvider.Update(null, creaequptsk.ToArray());
                logger.InfoMethod("更新EquipTask状态成功," + guid.ToString());
                this.BusinessLogic.MidStorageProvider.UpdateMidStore(null, mids.ToArray());
                logger.InfoMethod("更新暂存库位置状态成功," + guid.ToString());
                this.BusinessLogic.AGVTasksProvider.CreateAGVTask(agvtsk, null); //同时创建AGVTask
                logger.InfoMethod("创建AGVTask成功," + guid.ToString());
                this.BusinessLogic.RobotArmTaskProvider.InsertArmTask(listarmtsk, null);
                logger.InfoMethod("创建龙门Task成功," + guid.ToString());
                logger.InfoMethod("=================================================================================");
                #endregion
                return 1;//一次只执行一个龙门Task 
                #endregion
            }
            return 0;
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
    public class LinePLCCmd
    {
        /// <summary>
        /// 上位机状态/发送:0/1切换1秒脉冲
        /// </summary>
        public int SERVER_STATUS { get; set; }
        /// <summary>
        /// 输送线体状态:1：自动 2.手动 3.故障
        /// </summary>
        public int LINE_STATUS { get; set; }
        /// <summary>
        /// 直通线轮子数量
        /// </summary>
        public int ONLINE_WHOOLCOUNT { get; set; }
        /// <summary>
        /// 直通线轮子型号 物料种类:1(8个),2(12个) 
        /// </summary>
        public int INSTORE_ProductType { get; set; }
        /// <summary>
        /// 出库线轮子型号 物料种类:1(8个),2(12个) 
        /// </summary>
        public int OUTSTORE_ProductType { get; set; }
        public int BACKUP3 { get; set; }
        /// <summary>
        /// 1入库;2出库
        /// </summary>
        public int IN_OR_OUT { get; set; }
        /// <summary>
        /// ExceptionTag
        /// </summary>
        public int ExceptionTag { get; set; }
        public int BACKUP6 { get; set; }
        public int BACKUP7 { get; set; }
        /// <summary>
        /// 直通口入库:1.可以叫车  0.不允叫车
        /// </summary>
        public int ONLINE_INSTORE_CALLAGV { get; set; }
        /// <summary>
        /// 直通口入库卸车完成:1.完成  0.未完成
        /// </summary>
        public int ONLINE_INSTORE_UNLOAD { get; set; }
        /// <summary>
        /// 当前物料扫码失败 1.成功  2.失败
        /// </summary>
        public int ONLINE_SCANN_STATUS { get; set; }
        /// <summary>
        /// 小车装载到直通线完成信号 0未完成;1完成
        /// </summary>
        public int ONLINE_LOADSUCCESS { get; set; }
        /// <summary>
        /// 扫码后物料流水号:收到扫码信息反馈给我个流水号1-30000
        /// </summary>
        public int ONLINE_BARCODE_SerialNumber { get; set; }
        /// <summary>
        /// 直通口入库物料流水号:1-30000
        /// </summary>
        public int ONLINE_INSTORE_SerialNumber { get; set; }
        /// <summary>
        /// 直通口入库允许龙门取:1允许,0不允许
        /// </summary>
        public int ONLINE_INSTORE_ALLOW_GET { get; set; }
        /// <summary>
        /// 直通口出库允许龙门放:1允许,0不允许
        /// </summary>
        public int ONLINE_OUTSTORE_ALLOW_SET { get; set; }
        /// <summary>
        /// 直通口出库允许叫车:1允许,0不允许
        /// </summary>
        public int ONLINE_OUTSTORE_CALLAGV { get; set; }
        /// <summary>
        /// 直通口出库装车完毕:1完成,0未完成
        /// </summary>
        public int ONLINE_OUTSTORE_UNLOAD { get; set; }

        /// <summary>
        /// 单独出库口1.允许放  0.不允许放
        /// </summary>
        public int OUTSTORE_ARM_SET { get; set; }
        /// <summary>
        /// 单独出库口可以叫车:1允许,0不允许
        /// </summary>
        public int OUTSTORE_CALLAGV { get; set; }
        /// <summary>
        /// 单独出库口装车完毕:1完成,0未完成
        /// </summary>
        public int OUTSTORE_LOAD { get; set; }
        public int OUTSTORE_BACK0 { get; set; }
        /// <summary>
        /// 直通口出库流水号
        /// </summary>
        public int ONLINE_SeqNo_Write { get; set; }
        /// <summary>
        /// 直通口出库反馈流水号
        /// </summary>
        public int ONLINE_SeqNo_Read { get; set; }
        /// <summary>
        /// 单独出库口流水号
        /// </summary>
        public int OUTSTORE_SeqNo_Write { get; set; }
        /// <summary>
        /// 单独出库口反馈流水号
        /// </summary>
        public int OUTSTORE_SeqNo_Read { get; set; }
        public int OUTSTORE_BACK5 { get; set; }
        public int OUTSTORE_BACK6 { get; set; }

        /// <summary>
        /// 异常口允许龙门放1.允许放  0.不允许放
        /// </summary>
        public int ExLine_ARM_SET { get; set; }
        /// <summary>
        /// 异常口线体是否满:1.满  0.不满
        /// </summary>
        public int ExLine_ISENOUGH { get; set; }
        /// <summary>
        /// 扫码异常口是否满:1.满  0.不满
        /// </summary>
        public int ExLine_SCANN_ISENOUGH { get; set; }
        public int ExLine_BACK0 { get; set; }
        public int ExLine_BACK1 { get; set; }
        public int ExLine_BACK2 { get; set; }
    }

}
