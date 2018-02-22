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
            //qrcode = $"./StorageArea{StorageArea}QrCode.json";
        }

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
        [ConfigBoundProperty("RobotArmID")]
        public string RobotArmID = "";
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
            //threadchecktask.Start();
            thread_ReadBarCode.Start();
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
        public override void ReadDervice()
        {
            base.ReadDervice();
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
                agvtsk.Updated = DateTime.Now;
                this.BusinessLogic.AGVTasksProvider.UpdateEntity(agvtsk);
                logger.InfoMethod($"{StorageArea}号库{line}线体收到接料请求,将AGV请求状态改为2,guid:" + agvtsk.TaskGuid.ToString());
            }
        }

        public Queue<string> barcodeQueue { get; set; } = new Queue<string>();
        //int _InStoreLineWhoolsCount = 0;
        int _BARCODE_SerialNumber = 0;//小于30000 
        int readbarcode = 0;
        /// <summary>
        /// 直通线读二维码功能,直通线出库功能
        /// </summary>
        void ReadBarCode()
        {
            Neutrino ne = new Neutrino() { TheName = "ReadBarCode" };
            ne.AddField("BARCODE0", "0");
            ne.AddField("BARCODE1", "0");
            ne.AddField("BARCODE2", "0");
            ne.AddField("BARCODE_SerialNumber", "0");
            ne.AddField("ONLINE_WHOOLCOUNT", "0");//直通线轮子数量
            ne.AddField("ONLINE_LOADSUCCESS", "0");//\直通线轮子数量
            ne.AddField("ExLine_SCANN_ISENOUGH", "0");//扫码异常口1.满  0.不满

            Neutrino neclear = new Neutrino() { TheName = "ClearBarCode" };
            //neclear.AddField("BARCODE_SerialNumber", _BARCODE_SerialNumber.ToString());
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
                this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "暂存库扫码异常口满", Source = "直通线", MsgLevel = 6 });
                return;
            }
            int ONLINE_WHOOLCOUNT = ne.GetInt("ONLINE_WHOOLCOUNT");
            var b0 = BitConverter.GetBytes(neutrino.GetInt("BARCODE0"));
            var b1 = BitConverter.GetBytes(neutrino.GetInt("BARCODE1"));
            var b2 = BitConverter.GetBytes(neutrino.GetInt("BARCODE2"));
            int issuccess = neutrino.GetInt("ONLINE_LOADSUCCESS");//小车到直通线对接的完成标志 0未完成;1完成
            string barcode = (asciiEncoding.GetString(b0) + asciiEncoding.GetString(b1) + asciiEncoding.GetString(b2)).Replace("\0", "");
            
            //Move to the area when it is ready for usage
            //By Song@2018.01.24.
            barcodeQueue = GetbarcodeQueue();
            if (issuccess == 1)
            {
                //goto GOLOGIC;
                int i = InLineRobotArmTask2();//1入库;2出库
                if (i == 0)
                {
                    logger.WarnMethod(this.StorageArea + "号暂存库剩余库位数量不足");
                    this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "号暂存库剩余库位数量不足", Source = this.StorageArea + "号暂存库", MsgLevel = 6 });
                    //goto GOON;
                    return;
                }
                i = i == 3 ? 2 : i;
                if (i == 2)
                    neclear.AddField("ONLINE_SeqNo_Write", seq.ToString());

                neclear.AddField("IN_OR_OUT", i.ToString());
                neclear.AddField("ONLINE_LOADSUCCESS", "0");
                barcodeQueue.Clear();
                //清空barcodeQueue序列化文件
                SavebarcodeQueue();
                //neclear.AddField("INSTORE_ProductType", "2");
                //Send data to device
                //By Song@2018.01.24
                SendData(neclear);
                return;

            }

            if (string.IsNullOrEmpty(barcode.Trim()))
                return;
            readbarcode++;
            //To trace exception barcode
            //By Song@2018.01.24
            string barcodeReadingInfo = string.Empty;
            if (barcode.Length < 6 && readbarcode <= 3)
            {
                //By Song@2018.01.15
                //logger.InfoMethod(string.Format("{0} to get barcode {1}", readbarcode, barcode));
                //string barcodeReadingInfo = string.Format("Barcode1:{0}/{1}, Barcode2:{2}/{3}, Barcode3:{4}/{5} at {6} reading attemp",
                //                            neutrino.GetField("BARCODE0"),
                //                            Encoding.GetEncoding(1251).GetString(b0),
                //                            neutrino.GetField("BARCODE1"),
                //                            Encoding.GetEncoding(1251).GetString(b1),
                //                            neutrino.GetField("BARCODE2"),
                //                            Encoding.GetEncoding(1251).GetString(b2),
                //                            readbarcode);
                //By Song@2018.01.24
                barcodeReadingInfo = string.Format("Barcode({0}) is read at Storage {1} after {2} reading attempt", 
                                                   barcode, 
                                                   StorageArea, 
                                                   readbarcode);
                File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcodeReadingInfo + "\r\n");
                //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + this.StorageArea + "号暂存库," + barcode + "\r\n");
                //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + this.StorageArea + "号暂存库," + barcodeReadingInfo + "\r\n");
                this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "二维码:" + barcode, MsgLevel = 5, Source = this.StorageArea + "号暂存库扫码异常" });
                return;
            }
            //Add log to trace the spool to exception
            //By Song@2018.01.24
            else
            {
                barcodeReadingInfo = barcodeReadingInfo + string.Format("Storage:{0}, Barcode:{1}, TryCount:{2}", StorageArea, barcode, readbarcode);
            }
            readbarcode = 0;

            //barcodeQueue = GetbarcodeQueue();
            //if (string.IsNullOrEmpty(barcode.Trim()) || barcodeQueue.ToList().Exists(x => x.Contains(barcode)))
            //By Song@2018.01.24
            if (barcodeQueue.ToList().Exists(x => x.Contains(barcode)))
            {
                //By Song@2018.01.14
                //barcodeReadingInfo = barcodeReadingInfo + string.Format(" Empty barcode or Already scanned");
                //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcodeReadingInfo + "\r\n");
                return;
            }
            //barcodeQueue读取本地序列化文件
            if (barcodeQueue.Count > 0)
            {
                var id = barcodeQueue.Select(x => Convert.ToInt16(x.Split('_')[1]));
                _BARCODE_SerialNumber = id.OrderByDescending(x => x).FirstOrDefault();
            }

            if (_BARCODE_SerialNumber == 30000)
                _BARCODE_SerialNumber = 1;
            else _BARCODE_SerialNumber++;
            #region MesToSNTON
            try
            {
                int re = 0;
                if (barcode.Trim().Length == 6 && barcode.Trim() != "999999")
                {
                    char lr = char.MinValue;
                    #region MyRegion
                    re = MesToSNTON(barcode, out lr);
                    if (re == 1)
                    {
                        if (!barcodeQueue.ToList().Exists(x => x.Contains(barcode)))
                        {
                            barcodeQueue.Enqueue(barcode + "_" + _BARCODE_SerialNumber.ToString());
                            SavebarcodeQueue();
                            //序列化保存到本地
                        }
                        neclear.AddField("ExceptionTag", "1");
                    }
                    else if (re == -1)
                    {
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "错误的LR面信息:" + lr.ToString() + "," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5 });
                        logger.WarnMethod("错误的LR面信息," + barcode + "," + lr.ToString());
                        neclear.AddField("ExceptionTag", "2");
                        //By Song@2018.01.24.
                        barcodeReadingInfo = barcodeReadingInfo + "(Reason: Wrong L/R) => Exception route";
                        //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcode + " 错误的LR面信息\r\n");
                    }
                    else if (re == 0)
                    {
                        logger.WarnMethod("MES系统不存在此二维码," + barcode);
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "MES系统不存在此二维码," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5 });
                        neclear.AddField("ExceptionTag", "2");
                        //By Song@2018.01.24.
                        barcodeReadingInfo = barcodeReadingInfo + "(Reason: No MES info) => Exception route";
                        //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcode + " MES系统不存在此二维码\r\n");
                    }
                    else
                    {
                        logger.WarnMethod("添加到SNTON失败," + barcode);
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "添加到SNTON失败," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5 });
                        neclear.AddField("ExceptionTag", "2");
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
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "扫码失败," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5 });
                        logger.WarnMethod("扫码失败," + barcode);
                    }
                    else
                    {
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "错误的二维码," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5 });
                        logger.WarnMethod("错误的二维码," + barcode);
                    }
                    neclear.AddField("ExceptionTag", "2");
                    //By Song@2018.01.24
                    barcodeReadingInfo = barcodeReadingInfo + "(Reason: Invalid barcode) => Exception route";
                    //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcode + "\r\n");
                }
            }
            catch (Exception ex)
            {
                this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "添加到SNTON失败," + barcode, Source = this.StorageArea + "号暂存库扫码异常", MsgLevel = 5 });
                neclear.AddField("ExceptionTag", "2");
                logger.ErrorMethod(barcode + " MesToSNTON失败", ex);
                //By Song@2018.01.24
                barcodeReadingInfo = barcodeReadingInfo + "(Reason: Exception processing) => Exception route";
                //File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcode + "\r\n");
            }
            #endregion
            //By Song@2018.01.24
            File.AppendAllText("./barcode.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + barcodeReadingInfo + "\r\n");

            //GOLOGIC:
            //if (issuccess == 1)
            //{
            //    int i = InLineRobotArmTask2();//1入库;2出库
            //    if (i == 0)
            //    {
            //        logger.WarnMethod(this.StorageArea + "号暂存库剩余库位数量不足");
            //        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "号暂存库剩余库位数量不足", Source = this.StorageArea + "号暂存库", MsgLevel = 6 });
            //        goto GOON;
            //    }
            //    i = i == 3 ? 2 : i;
            //    if (i == 2)
            //        neclear.AddField("ONLINE_SeqNo_Write", seq.ToString());

            //    neclear.AddField("IN_OR_OUT", i.ToString());
            //    neclear.AddField("ONLINE_LOADSUCCESS", "0");
            //    barcodeQueue.Clear();
            //    //清空barcodeQueue序列化文件
            //    SavebarcodeQueue();
            //    //neclear.AddField("INSTORE_ProductType", "2");
            //}
            SendData(neclear);
            //GOON:
            //{
            //}
        }
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
                    string code = codeseq.Substring(0, 6);
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
        int seq = 1;
        Random ran = new Random(DateTime.Now.Second);
        /// <summary>
        /// 检测是否有能够创建AGV任务的EquipTask
        /// </summary>
        /// <param name="storageno"></param>
        /// <param name="supply1">按照单丝标准书分组的单组设备任务</param>
        /// <returns>1创建任务;0没有创建任务</returns>
        private int NewMethod(short storageno, IGrouping<string, EquipTaskViewEntity> supply1, int inlinecount, int lineno = 1)
        {
            if (seq == 1 || seq == 255)
                seq = ran.Next(1, 255);
            else seq++;
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
                logger.InfoMethod(storageno + "号暂存库中的单丝不满足出库数量,单丝作业标准书:" + supply1.Key.Trim() + ",地面滚筒id:" + sb.ToString().Trim(','));
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
        int MesToSNTON(string barcode, out char lr)
        {
            lr = char.MinValue;
            SpoolsEntity spool = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcode(barcode, null);
            if (spool != null)
                return 1;
            var messpool = this.BusinessLogic.MESSystemProvider.GetMESSpool(barcode, null);//从MES系统查轮子信息存到本地数据库
            if (messpool == null)
                messpool = this.BusinessLogic.MESSystemProvider.GetMESSpool(barcode, null);//从MES系统查轮子信息存到本地数据库
            if (messpool == null)
                messpool = this.BusinessLogic.MESSystemProvider.GetMESSpool(barcode, null);//从MES系统查轮子信息存到本地数据库
            if (messpool == null)
                return 0;
            if (messpool.BobbinNo != 'L' && messpool.BobbinNo != 'R')
            {
                if (messpool.BobbinNo.HasValue)
                    lr = messpool.BobbinNo.Value;
                return -1;
            }
            spool = new SpoolsEntity();
            spool.FdTagNo = barcode;
            if (messpool != null)
            {
                spool.Created = DateTime.Now;
                spool.IsDeleted = 0;
                spool.ProductType = messpool.CName;
                spool.CName = messpool.CName;
                spool.Length = messpool.Length;
                spool.FdTagNo = messpool.FdTagNo;
                spool.StructBarCode = messpool.StructBarCode;
                spool.GroupID = messpool.GroupID;
                spool.BobbinNo = messpool.BobbinNo.Value;
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
            string js = Newtonsoft.Json.JsonConvert.SerializeObject(barcodeQueue);
            var by = Encoding.UTF8.GetBytes(js);
            File.WriteAllBytes($"./StorageArea{StorageArea}QrCode.json", by);
        }
        Queue<string> GetbarcodeQueue()
        {
            if (!File.Exists($"./StorageArea{StorageArea}QrCode.json"))
                return new Queue<string>();
            Queue<string> obj = new Queue<string>();
            try
            {
                string json = File.ReadAllText($"./StorageArea{StorageArea}QrCode.json");
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Queue<string>>(json);
            }
            catch (Exception ex)
            {
                logger.WarnMethod("反序列化barcodeQueue失败", ex);
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
