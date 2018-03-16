using SNTON.Constants;
using SNTON.Entities.DBTables.AGV;
using SNTON.Entities.DBTables.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VI.MFC.COM;
using VI.MFC.Utils;
using VI.MFC.Utils.ConfigBinder;
using static SNTON.Constants.SNTONConstants;
using VI.MFC.Logic;
using VI.MFC.Logging;
using Newtonsoft.Json;
using SNTON.Entities.DBTables.Equipments;
using System.Xml;
using System.Collections;
using SNTON.Entities.DBTables.MES;
using System.Threading;

namespace SNTON.Components.ComLogic
{
    /// <summary>
    /// 车间设备线体:0无任务,1需要送轮子,2
    /// </summary>
    public class EquipLineLogic : ComLogic
    {
        //private VIThreadEx threadequiptask;
        private VIThreadEx thread_ReadEquipLineStatus;
        private VIThreadEx thread_SendCreateAGV;
        private VIThreadEx thread_heartbeat;
        //private VIThreadEx thread_plctest;

        public EquipLineLogic()
        {
            //threadequiptask = new VIThreadEx(CheckEquipTask, null, "Check AGV task Ready", 1000);
            thread_ReadEquipLineStatus = new VIThreadEx(ReadEquipLineStatus, null, "Check EquipLine Status", 1000);
            thread_SendCreateAGV = new VIThreadEx(SendCreateAGV, null, "thread for SendCreateAGV", 3000);
            thread_heartbeat = new VIThreadEx(heartbeat, null, "heartbeat", 1000);
            //thread_plctest = new VIThreadEx(PLCTest, null, "PLCTest", 4000);
        }
        protected override void StartInternal()
        {
            //thread_plctest.Start();
            thread_ReadEquipLineStatus.Start();
            thread_SendCreateAGV.Start();
            thread_heartbeat.Start();
            base.StartInternal();
        }
        public new static EquipLineLogic Create(XmlNode node)
        {
            EquipLineLogic a = new EquipLineLogic();
            a.Init(node);
            return a;
        }
        List<EquipConfigerEntity> equipcmd = null;

        void SendCreateAGV()
        {
            if (equipcmd == null)
                equipcmd = this.BusinessLogic.EquipConfigerProvider.GetEquipConfigerEntities(null);
            //var equiptsk = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities($"[STATUS]=1 AND PlantNo={PlantNo} AND PLCNo={PLCNo}", null);
            //16收到中控系统的调车指令,小车分配成功
            var agvtsks = this.BusinessLogic.AGVTasksProvider.GetAGVTasks($"[STATUS] IN(8,16,19) AND [PlantNo]={PlantNo} and id>=15008", null);
            if (agvtsks == null || agvtsks.Count == 0)
                return;

            List<long> idequiptsks = new List<long>();
            StringBuilder sbequipname = new StringBuilder();
            foreach (var item in agvtsks)
            {
                #region 处理每一个AGV任务
                var EquipTask = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewNotDeleted($"TaskGuid='{item.TaskGuid.ToString()}' AND PLCNo={PLCNo}", null);
                if (EquipTask == null || EquipTask.Count == 0)
                    continue;
                foreach (var task in EquipTask)
                {
                    sbequipname.Clear();
                    Neutrino n = new Neutrino();
                    n.TheName = "SendCreateAGV";
                    #region 通知地面滚筒准备接收
                    Neutrino readwas = new Neutrino();
                    readwas.TheName = "ReadWAStatus";
                    var cmd = equipcmd.FirstOrDefault(x => x.EquipFlag.Trim() == task.EquipFlag.Trim());
                    sbequipname.Append(cmd.EquipName);
                    readwas.AddField(cmd.WAStatus, "0");
                    var readr = MXParser.ReadData(readwas, true);
                    Thread.Sleep(1000);
                    if (!readr.Item1)
                    {
                        continue;
                    }
                    //By Song@2018.01.20
                    //int wasagv = readr.GetIntOrDefault(cmd.WAStatus);
                    int wasagv = readr.Item2.GetIntOrDefault(cmd.WAStatus);
                    if (wasagv != 0)
                    {
                        task.Status = 6;
                        this.BusinessLogic.EquipTaskViewProvider.Update(null, task);
                        //this.BusinessLogic.EquipTaskViewProvider.Update(null, 6, task.Id);
                        continue;
                    }
                    if (!n.FieldExists(cmd.WAStatus))
                        n.AddField(cmd.WAStatus, item.TaskType.ToString());
                    task.Status = 9;
                    bool r = MXParser.SendData(n, 3);
                    Thread.Sleep(1000);
                    if (!r)
                    {
                        logger.WarnMethod("通知地面滚筒准备接收失败,EquipTask:" + JsonConvert.SerializeObject(task));
                        continue;
                    }
                    else
                    {
                        logger.WarnMethod("通知地面滚筒准备接收,EquipTask:" + JsonConvert.SerializeObject(task));
                    }
                    var light = MXParser.ReadData(n, true);
                    Thread.Sleep(1000);
                    #region 重新读取刚刚写入的光电,验证有没有写入成功
                    if (!light.Item1)
                    {
                        logger.WarnMethod("读取光电值失败,设备Id:" + sbequipname.ToString().Trim(','));
                        continue;
                    }
                    else
                    {
                        int kv = n.GetIntOrDefault(cmd.WAStatus);
                        if (kv == 0)
                        {//光电写入失败 
                            logger.WarnMethod("光电写入失败:" + cmd.ControlID + "," + sbequipname);
                            continue;
                        }
                        else
                        {
                            task.Status = 6;
                            int cout = this.BusinessLogic.EquipTaskViewProvider.Update(null, task);
                            //if (cout == 0)
                            //    continue;
                            logger.WarnMethod("光电写入成功:" + cmd.ControlID + "," + sbequipname);
                        }

                    }
                    #endregion

                    logger.InfoMethod("已通知地面滚筒准备接收:" + cmd.WAStatus + "," + item.TaskType.ToString() + $",TaskGuid:{item.TaskGuid.ToString()}");
                    #endregion
                }
                //if (!EquipTask.Exists(x => x.Status == 6))
                //    continue;
                if (EquipTask.Exists(x => x.Status != 6))
                    continue;
                //判断2个是否都写成功  
                if (item.Status == 16)
                    item.Status = 19;
                else if (item.Status == 8)
                    item.Status = 9;
                else item.Status = 20;
                this.BusinessLogic.AGVTasksProvider.UpdateEntity(item, null);
                #endregion
            }
        }
        /// <summary>


        [ConfigBoundProperty("PLCNo")]
        public int PLCNo = 0;

        public override void OnConnect()
        {
            base.OnConnect();
        }
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
        int BACKUP3 = 0;
        void heartbeat()
        {
            BACKUP3 = BACKUP3 == 0 ? 1 : 0;
            Neutrino nwrite = new Neutrino();
            nwrite.TheName = "heartbeatwrite" + PLCNo;
            Neutrino nread = new Neutrino();
            nread.TheName = "heartbeatread" + PLCNo;

            nwrite.AddField("requestPLC", BACKUP3.ToString());
            nread.AddField("responsePLC", "0");

            var result = MXParser.ReadData(nread);
            if (!result.Item1)
            {
                logger.WarnMethod(nread.TheName + "心跳读取失败");
            }
            bool r = SendData(nwrite);
            if (!r)
            {
                logger.WarnMethod(nwrite.TheName + "心跳发送失败");
            }
        }
        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        /// <param name="neutrino"></param>
        /// <returns></returns>
        protected override bool ProcessMessage(Neutrino neutrino)
        {
            return base.ProcessMessage(neutrino);
        }
        List<string> machcodes = new List<string>();


        /// <summary>
        /// 定时读取设备线体的状态,判断送接轮子
        /// </summary>
        void ReadEquipLineStatus()
        {
            // LWCS1_B00 滚筒>WCS请求调度AGV 读
            // LStatus_1 滚筒状态 
            // EStatus_1_1 设备1状态
            // EStatus_2_1 设备2状态
            // WAStatus_1_B500 WCS已调度AGV
            // AGVDisStatus_1 AGV调度状态 1未调度;2已调度
            Neutrino ne = new Neutrino();

            Neutrino nwrite = new Neutrino();
            nwrite.TheName = "WriteEquipLineStatus";
            ne.TheName = "ReadEquipLineStatus";
            //this.MXParser.ReadData(ne);
            //var cmdlist = this.BusinessLogic.EquipCommandProvider._AllEquipCommandList.FindAll(x => x.PLCNo == PLCNo);
            var cmdlist = this.BusinessLogic.EquipConfigerProvider.EquipConfigers.FindAll(x => x.PLCNo == PLCNo);
            foreach (var item in cmdlist)
            {
                ne.AddField(item.LWCS.Trim(), "0");
                ne.AddField(item.WAStatus.Trim(), "0");
                ne.AddField(item.AGVDisStatus.Trim(), "0");
                //break;
            }
            //Neutrino plcstatus = null;
            //By Song@2018.01.20
            Console.WriteLine($"开始读取PLC{PLCNo}:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Tuple<bool, Neutrino> plcstatus = MXParser.ReadData(ne, true);
            //By Song@2018.01.20
            if (!plcstatus.Item1)
            {
                Console.WriteLine($"读取PLC{PLCNo}失败:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return;
            }
            Console.WriteLine($"读取PLC{PLCNo}成功:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            if (machcodes.Count == 0)
                foreach (var item in cmdlist)
                {
                    machcodes.Add(item.MachCode.Trim());
                }
            List<tblProdCodeStructMachEntity> machstructcode = null;
            try
            {
                machstructcode = this.BusinessLogic.tblProdCodeStructMachProvider.GettblProdCodeStructMachs(null, machcodes.ToArray());
            }
            catch (Exception)
            {

            }
            foreach (var item in cmdlist)
            {
                item.MachStructCode = machstructcode.FirstOrDefault(x => x.MachCode == item.MachCode.Trim());
            }
            //var kong = cmdlist.FindAll(x => x.MachStructCode == null).Select(x => x.MachCode);
            /*
            此处要根据地面滚筒ID获取该设备所需要的轮子规格
            */
            //string cmd = plcstatus.TheName.Trim();
            Neutrino plcNeutrino = plcstatus.Item2;
            //string cmd = plcNeutrino.TheName.Trim();
            var keys = plcNeutrino.GetAllKeys();
            foreach (var item in cmdlist)
            {
                int inOrout = plcNeutrino.GetInt(item.LWCS.Trim());// 1出拉空轮; 2入拉满轮
                int equipstatus = plcNeutrino.GetInt(item.WAStatus.Trim());//是否已调度AGV 1，已接收滚筒出料请求；2，已接收入料至滚筒请求 光电
                int status = plcNeutrino.GetInt(item.AGVDisStatus.Trim());//地面滚筒请求 AGVDisStatus
                #region 创建equiptsk
                if (equipstatus == 0 && inOrout != 0 && status == 0)
                {//未调度
                    bool r = false;
                    if (item.MachStructCode == null)
                    {
                        //logger.WarnMethod("该设备作业标准书未绑定:" + item.EquipList[0].EquipName.Trim() + "," + item.EquipList[1].EquipName.Trim());
                        Console.WriteLine("该设备作业标准书未绑定:" + item.EquipList[0].EquipName.Trim() + "," + item.EquipList[1].EquipName.Trim());
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "该设备作业标准书未绑定:" + item.EquipList[0].EquipName.Trim() + "," + item.EquipList[1].EquipName.Trim(), MsgLevel = 5, Source = "作业标准书未绑定" });
                        continue;
                    }
                    else if (item.MachStructCode.ProdCodeStructMark4 == null)
                    {
                        //logger.WarnMethod("单丝作业标准书未绑定:" + item.MachStructCode.StructBarCode + "," + item.EquipList[0].EquipName.Trim() + "," + item.EquipList[1].EquipName.Trim());
                        Console.WriteLine("单丝作业标准书未绑定:" + item.MachStructCode.StructBarCode + "," + item.EquipList[0].EquipName.Trim() + "," + item.EquipList[1].EquipName.Trim());
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "单丝作业标准书未绑定:" + item.MachStructCode.StructBarCode + "," + item.EquipList[0].EquipName.Trim() + "," + item.EquipList[1].EquipName.Trim(), MsgLevel = 5, Source = "单丝作业标准书未绑定" });
                        continue;
                    }
                    var equiptsk = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"[EquipContollerId]='{item.ControlID}' and Status IN(0,1,2,3,4,5,6,9,10)");

                    if (equiptsk == null || equiptsk.Count == 0)
                    {
                        if (inOrout == 1)
                        {//创建拉空轮任务
                            r = this.BusinessLogic.EquipTaskProvider.CreateEquipTask(new EquipTaskEntity() { Length = item.MachStructCode.ProdCodeStructMark4.ProdLength, Created = DateTime.Now, EquipContollerId = item.ControlID, ProductType = item.MachStructCode.ProdCodeStructMark4?.CName, Status = 0, TaskType = 1, TaskLevel = 6, PlantNo = PlantNo, Supply1 = item.MachStructCode.ProdCodeStructMark3.Supply1 });//创建出任务
                            logger.InfoMethod("创建拉空轮任务, item.ControlID" + item.ControlID.ToString() + "result:" + r);
                        }
                        else if (inOrout == 2)//创建拉满轮任务
                        {
                            r = this.BusinessLogic.EquipTaskProvider.CreateEquipTask(new EquipTaskEntity() { Length = item.MachStructCode.ProdCodeStructMark4.ProdLength, Created = DateTime.Now, EquipContollerId = item.ControlID, ProductType = item.MachStructCode.ProdCodeStructMark4?.CName, Status = 0, TaskType = 2, TaskLevel = 5, PlantNo = PlantNo, Supply1 = item.MachStructCode.ProdCodeStructMark3.Supply1 });//创建入任务
                            logger.InfoMethod("创建拉满轮任务, item.ControlID" + item.ControlID.ToString() + "result:" + r);
                        }

                    }
                    else
                    {
                        logger.InfoMethod("已创建该设备的任务,Created:" + equiptsk.FirstOrDefault().Created + ",EquipTaskID:" + equiptsk.FirstOrDefault().Id + ",EquipContollerId:" + equiptsk.FirstOrDefault().EquipContollerId);
                    }
                    if (equiptsk != null && equiptsk.FindAll(x => x.TaskType != inOrout).Count != 0)
                    {
                        var updatedelete = equiptsk.FindAll(x => x.TaskType != inOrout);
                        if (updatedelete != null && updatedelete.Count != 0)
                        {
                            updatedelete.ForEach(x => x.IsDeleted = 1);//把之前的任务删除掉
                            this.BusinessLogic.EquipTaskProvider.UpdateEntity(updatedelete, null);
                        }
                    }
                    if (r)
                    {
                        logger.InfoMethod("通知地面滚筒收到任务请求:" + item.AGVDisStatus.Trim() + "," + inOrout.ToString());
                        nwrite.AddField(item.AGVDisStatus.Trim(), inOrout.ToString());
                    }
                }
                #endregion

            }
            if (nwrite.Count != 0)
            {
                SendData(nwrite); //通知PLC已经创建任务
                //bool r = MXParser.TrySendDataWithResult(nwrite);

            }


        }
    }

    public class EquipLineCMD
    {
        #region 滚筒>WCS请求调度AGV
        public int LWCS1 { get; set; }
        public int LWCS2 { get; set; }
        public int LWCS3 { get; set; }
        public int LWCS4 { get; set; }
        public int LWCS5 { get; set; }
        public int LWCS6 { get; set; }
        public int LWCS7 { get; set; }
        public int LWCS8 { get; set; }
        public int LWCS9 { get; set; }
        public int LWCS10 { get; set; }
        public int LWCS11 { get; set; }
        public int LWCS12 { get; set; }
        public int LWCS13 { get; set; }
        public int LWCS14 { get; set; }
        public int LWCS15 { get; set; }
        public int LWCS16 { get; set; }
        public int LWCS17 { get; set; }
        public int LWCS18 { get; set; }
        public int LWCS19 { get; set; }
        public int LWCS20 { get; set; }
        public int LWCS21 { get; set; }
        public int LWCS22 { get; set; }
        public int LWCS23 { get; set; }
        public int LWCS24 { get; set; }
        public int LWCS25 { get; set; }
        public int LWCS26 { get; set; }
        public int LWCS27 { get; set; }
        public int LWCS28 { get; set; }
        public int LWCS29 { get; set; }
        public int LWCS30 { get; set; }
        public int LWCS31 { get; set; }
        public int LWCS32 { get; set; }
        public int LWCS33 { get; set; }
        public int LWCS34 { get; set; }
        public int LWCS35 { get; set; }
        public int LWCS36 { get; set; }
        public int LWCS37 { get; set; }
        public int LWCS38 { get; set; }
        public int LWCS39 { get; set; }
        public int LWCS40 { get; set; }
        public int LWCS41 { get; set; }
        public int LWCS42 { get; set; }
        public int LWCS43 { get; set; }
        public int LWCS44 { get; set; }
        public int LWCS45 { get; set; }
        public int LWCS46 { get; set; }
        public int LWCS47 { get; set; }
        public int LWCS48 { get; set; }
        public int LWCS49 { get; set; }
        public int LWCS50 { get; set; }
        public int LWCS51 { get; set; }
        public int LWCS52 { get; set; }
        public int LWCS53 { get; set; }
        public int LWCS54 { get; set; }
        public int LWCS55 { get; set; }
        public int LWCS56 { get; set; }
        public int LWCS57 { get; set; }
        public int LWCS58 { get; set; }
        public int LWCS59 { get; set; }
        public int LWCS60 { get; set; }
        public int LWCS61 { get; set; }
        public int LWCS62 { get; set; }
        public int LWCS63 { get; set; }
        public int LWCS64 { get; set; }
        public int LWCS65 { get; set; }
        public int LWCS66 { get; set; }
        public int LWCS67 { get; set; }
        public int LWCS68 { get; set; }
        public int LWCS69 { get; set; }
        public int LWCS70 { get; set; }
        public int LWCS71 { get; set; }
        public int LWCS72 { get; set; }
        public int LWCS73 { get; set; }
        public int LWCS74 { get; set; }
        public int LWCS75 { get; set; }
        public int LWCS76 { get; set; }
        public int LWCS77 { get; set; }
        public int LWCS78 { get; set; }
        public int LWCS79 { get; set; }
        public int LWCS80 { get; set; }
        public int LWCS81 { get; set; }
        public int LWCS82 { get; set; }
        public int LWCS83 { get; set; }
        public int LWCS84 { get; set; }
        public int LWCS85 { get; set; }
        public int LWCS86 { get; set; }
        public int LWCS87 { get; set; }
        public int LWCS88 { get; set; }
        public int LWCS89 { get; set; }
        public int LWCS90 { get; set; }
        public int LWCS91 { get; set; }
        public int LWCS92 { get; set; }
        public int LWCS93 { get; set; }
        public int LWCS94 { get; set; }
        public int LWCS95 { get; set; }
        public int LWCS96 { get; set; }
        public int LWCS97 { get; set; }
        public int LWCS98 { get; set; }
        public int LWCS99 { get; set; }
        public int LWCS100 { get; set; }
        public int LWCS101 { get; set; }
        public int LWCS102 { get; set; }
        public int LWCS103 { get; set; }
        public int LWCS104 { get; set; }
        public int LWCS105 { get; set; }
        public int LWCS106 { get; set; }
        public int LWCS107 { get; set; }
        public int LWCS108 { get; set; }
        public int LWCS109 { get; set; }
        public int LWCS110 { get; set; }
        public int LWCS111 { get; set; }
        public int LWCS112 { get; set; }
        public int LWCS113 { get; set; }
        public int LWCS114 { get; set; }
        public int LWCS115 { get; set; }
        public int LWCS116 { get; set; }
        public int LWCS117 { get; set; }
        public int LWCS118 { get; set; }
        public int LWCS119 { get; set; }
        public int LWCS120 { get; set; }
        public int LWCS121 { get; set; }
        public int LWCS122 { get; set; }
        public int LWCS123 { get; set; }
        public int LWCS124 { get; set; }
        public int LWCS125 { get; set; }
        public int LWCS126 { get; set; }
        public int LWCS127 { get; set; }
        public int LWCS128 { get; set; }
        public int LWCS129 { get; set; }
        public int LWCS130 { get; set; }
        public int LWCS131 { get; set; }
        public int LWCS132 { get; set; }
        public int LWCS133 { get; set; }
        public int LWCS134 { get; set; }
        public int LWCS135 { get; set; }
        public int LWCS136 { get; set; }
        public int LWCS137 { get; set; }
        public int LWCS138 { get; set; }
        public int LWCS139 { get; set; }
        public int LWCS140 { get; set; }
        public int LWCS141 { get; set; }
        public int LWCS142 { get; set; }
        public int LWCS143 { get; set; }
        public int LWCS144 { get; set; }
        public int LWCS145 { get; set; }
        public int LWCS146 { get; set; }
        public int LWCS147 { get; set; }
        public int LWCS148 { get; set; }
        public int LWCS149 { get; set; }
        public int LWCS150 { get; set; }
        public int LWCS151 { get; set; }
        public int LWCS152 { get; set; }
        public int LWCS153 { get; set; }
        public int LWCS154 { get; set; }
        public int LWCS155 { get; set; }
        public int LWCS156 { get; set; }
        public int LWCS157 { get; set; }
        public int LWCS158 { get; set; }
        public int LWCS159 { get; set; }
        public int LWCS160 { get; set; }
        public int LWCS161 { get; set; }
        public int LWCS162 { get; set; }
        public int LWCS163 { get; set; }
        public int LWCS164 { get; set; }
        public int LWCS165 { get; set; }
        public int LWCS166 { get; set; }
        public int LWCS167 { get; set; }
        public int LWCS168 { get; set; }
        public int LWCS169 { get; set; }
        public int LWCS170 { get; set; }
        public int LWCS171 { get; set; }
        public int LWCS172 { get; set; }
        public int LWCS173 { get; set; }
        public int LWCS174 { get; set; }
        public int LWCS175 { get; set; }
        public int LWCS176 { get; set; }
        public int LWCS177 { get; set; }
        public int LWCS178 { get; set; }
        public int LWCS179 { get; set; }
        public int LWCS180 { get; set; }
        public int LWCS181 { get; set; }
        public int LWCS182 { get; set; }
        public int LWCS183 { get; set; }
        public int LWCS184 { get; set; }
        public int LWCS185 { get; set; }
        public int LWCS186 { get; set; }
        public int LWCS187 { get; set; }
        public int LWCS188 { get; set; }
        public int LWCS189 { get; set; }
        public int LWCS190 { get; set; }
        public int LWCS191 { get; set; }
        public int LWCS192 { get; set; }
        public int LWCS193 { get; set; }
        public int LWCS194 { get; set; }
        public int LWCS195 { get; set; }
        public int LWCS196 { get; set; }
        public int LWCS197 { get; set; }
        public int LWCS198 { get; set; }
        public int LWCS199 { get; set; }
        public int LWCS200 { get; set; }
        public int LWCS201 { get; set; }
        public int LWCS202 { get; set; }
        public int LWCS203 { get; set; }
        public int LWCS204 { get; set; }
        public int LWCS205 { get; set; }
        public int LWCS206 { get; set; }
        public int LWCS207 { get; set; }
        public int LWCS208 { get; set; }
        public int LWCS209 { get; set; }
        public int LWCS210 { get; set; }
        public int LWCS211 { get; set; }
        public int LWCS212 { get; set; }
        public int LWCS213 { get; set; }
        public int LWCS214 { get; set; }
        public int LWCS215 { get; set; }
        public int LWCS216 { get; set; }
        public int LWCS217 { get; set; }
        public int LWCS218 { get; set; }
        public int LWCS219 { get; set; }
        public int LWCS220 { get; set; }
        public int LWCS221 { get; set; }
        public int LWCS222 { get; set; }
        public int LWCS223 { get; set; }
        public int LWCS224 { get; set; }
        public int LWCS225 { get; set; }
        public int LWCS226 { get; set; }
        public int LWCS227 { get; set; }
        public int LWCS228 { get; set; }
        public int LWCS229 { get; set; }
        public int LWCS230 { get; set; }
        public int LWCS231 { get; set; }
        public int LWCS232 { get; set; }
        public int LWCS233 { get; set; }
        public int LWCS234 { get; set; }
        public int LWCS235 { get; set; }
        public int LWCS236 { get; set; }
        public int LWCS237 { get; set; }
        public int LWCS238 { get; set; }
        public int LWCS239 { get; set; }
        public int LWCS240 { get; set; }
        public int LWCS241 { get; set; }
        public int LWCS242 { get; set; }
        public int LWCS243 { get; set; }
        public int LWCS244 { get; set; }
        public int LWCS245 { get; set; }
        public int LWCS246 { get; set; }
        public int LWCS247 { get; set; }
        public int LWCS248 { get; set; }
        public int LWCS249 { get; set; }
        public int LWCS250 { get; set; }
        public int LWCS251 { get; set; }
        public int LWCS252 { get; set; }
        public int LWCS253 { get; set; }
        public int LWCS254 { get; set; }
        public int LWCS255 { get; set; }
        public int LWCS256 { get; set; }

        #endregion
        #region 线体状态
        public int LStatus1 { get; set; }
        public int LStatus2 { get; set; }
        public int LStatus3 { get; set; }
        public int LStatus4 { get; set; }
        public int LStatus5 { get; set; }
        public int LStatus6 { get; set; }
        public int LStatus7 { get; set; }
        public int LStatus8 { get; set; }
        public int LStatus9 { get; set; }
        public int LStatus10 { get; set; }
        public int LStatus11 { get; set; }
        public int LStatus12 { get; set; }
        public int LStatus13 { get; set; }
        public int LStatus14 { get; set; }
        public int LStatus15 { get; set; }
        public int LStatus16 { get; set; }
        public int LStatus17 { get; set; }
        public int LStatus18 { get; set; }
        public int LStatus19 { get; set; }
        public int LStatus20 { get; set; }
        public int LStatus21 { get; set; }
        public int LStatus22 { get; set; }
        public int LStatus23 { get; set; }
        public int LStatus24 { get; set; }
        public int LStatus25 { get; set; }
        public int LStatus26 { get; set; }
        public int LStatus27 { get; set; }
        public int LStatus28 { get; set; }
        public int LStatus29 { get; set; }
        public int LStatus30 { get; set; }
        public int LStatus31 { get; set; }
        public int LStatus32 { get; set; }
        public int LStatus33 { get; set; }
        public int LStatus34 { get; set; }
        public int LStatus35 { get; set; }
        public int LStatus36 { get; set; }
        public int LStatus37 { get; set; }
        public int LStatus38 { get; set; }
        public int LStatus39 { get; set; }
        public int LStatus40 { get; set; }
        public int LStatus41 { get; set; }
        public int LStatus42 { get; set; }
        public int LStatus43 { get; set; }
        public int LStatus44 { get; set; }
        public int LStatus45 { get; set; }
        public int LStatus46 { get; set; }
        public int LStatus47 { get; set; }
        public int LStatus48 { get; set; }
        public int LStatus49 { get; set; }
        public int LStatus50 { get; set; }
        public int LStatus51 { get; set; }
        public int LStatus52 { get; set; }
        public int LStatus53 { get; set; }
        public int LStatus54 { get; set; }
        public int LStatus55 { get; set; }
        public int LStatus56 { get; set; }
        public int LStatus57 { get; set; }
        public int LStatus58 { get; set; }
        public int LStatus59 { get; set; }
        public int LStatus60 { get; set; }
        public int LStatus61 { get; set; }
        public int LStatus62 { get; set; }
        public int LStatus63 { get; set; }
        public int LStatus64 { get; set; }
        public int LStatus65 { get; set; }
        public int LStatus66 { get; set; }
        public int LStatus67 { get; set; }
        public int LStatus68 { get; set; }
        public int LStatus69 { get; set; }
        public int LStatus70 { get; set; }
        public int LStatus71 { get; set; }
        public int LStatus72 { get; set; }
        public int LStatus73 { get; set; }
        public int LStatus74 { get; set; }
        public int LStatus75 { get; set; }
        public int LStatus76 { get; set; }
        public int LStatus77 { get; set; }
        public int LStatus78 { get; set; }
        public int LStatus79 { get; set; }
        public int LStatus80 { get; set; }
        public int LStatus81 { get; set; }
        public int LStatus82 { get; set; }
        public int LStatus83 { get; set; }
        public int LStatus84 { get; set; }
        public int LStatus85 { get; set; }
        public int LStatus86 { get; set; }
        public int LStatus87 { get; set; }
        public int LStatus88 { get; set; }
        public int LStatus89 { get; set; }
        public int LStatus90 { get; set; }
        public int LStatus91 { get; set; }
        public int LStatus92 { get; set; }
        public int LStatus93 { get; set; }
        public int LStatus94 { get; set; }
        public int LStatus95 { get; set; }
        public int LStatus96 { get; set; }
        public int LStatus97 { get; set; }
        public int LStatus98 { get; set; }
        public int LStatus99 { get; set; }
        public int LStatus100 { get; set; }
        public int LStatus101 { get; set; }
        public int LStatus102 { get; set; }
        public int LStatus103 { get; set; }
        public int LStatus104 { get; set; }
        public int LStatus105 { get; set; }
        public int LStatus106 { get; set; }
        public int LStatus107 { get; set; }
        public int LStatus108 { get; set; }
        public int LStatus109 { get; set; }
        public int LStatus110 { get; set; }
        public int LStatus111 { get; set; }
        public int LStatus112 { get; set; }
        public int LStatus113 { get; set; }
        public int LStatus114 { get; set; }
        public int LStatus115 { get; set; }
        public int LStatus116 { get; set; }
        public int LStatus117 { get; set; }
        public int LStatus118 { get; set; }
        public int LStatus119 { get; set; }
        public int LStatus120 { get; set; }
        public int LStatus121 { get; set; }
        public int LStatus122 { get; set; }
        public int LStatus123 { get; set; }
        public int LStatus124 { get; set; }
        public int LStatus125 { get; set; }
        public int LStatus126 { get; set; }
        public int LStatus127 { get; set; }
        public int LStatus128 { get; set; }
        public int LStatus129 { get; set; }
        public int LStatus130 { get; set; }
        public int LStatus131 { get; set; }
        public int LStatus132 { get; set; }
        public int LStatus133 { get; set; }
        public int LStatus134 { get; set; }
        public int LStatus135 { get; set; }
        public int LStatus136 { get; set; }
        public int LStatus137 { get; set; }
        public int LStatus138 { get; set; }
        public int LStatus139 { get; set; }
        public int LStatus140 { get; set; }
        public int LStatus141 { get; set; }
        public int LStatus142 { get; set; }
        public int LStatus143 { get; set; }
        public int LStatus144 { get; set; }
        public int LStatus145 { get; set; }
        public int LStatus146 { get; set; }
        public int LStatus147 { get; set; }
        public int LStatus148 { get; set; }
        public int LStatus149 { get; set; }
        public int LStatus150 { get; set; }
        public int LStatus151 { get; set; }
        public int LStatus152 { get; set; }
        public int LStatus153 { get; set; }
        public int LStatus154 { get; set; }
        public int LStatus155 { get; set; }
        public int LStatus156 { get; set; }
        public int LStatus157 { get; set; }
        public int LStatus158 { get; set; }
        public int LStatus159 { get; set; }
        public int LStatus160 { get; set; }
        public int LStatus161 { get; set; }
        public int LStatus162 { get; set; }
        public int LStatus163 { get; set; }
        public int LStatus164 { get; set; }
        public int LStatus165 { get; set; }
        public int LStatus166 { get; set; }
        public int LStatus167 { get; set; }
        public int LStatus168 { get; set; }
        public int LStatus169 { get; set; }
        public int LStatus170 { get; set; }
        public int LStatus171 { get; set; }
        public int LStatus172 { get; set; }
        public int LStatus173 { get; set; }
        public int LStatus174 { get; set; }
        public int LStatus175 { get; set; }
        public int LStatus176 { get; set; }
        public int LStatus177 { get; set; }
        public int LStatus178 { get; set; }
        public int LStatus179 { get; set; }
        public int LStatus180 { get; set; }
        public int LStatus181 { get; set; }
        public int LStatus182 { get; set; }
        public int LStatus183 { get; set; }
        public int LStatus184 { get; set; }
        public int LStatus185 { get; set; }
        public int LStatus186 { get; set; }
        public int LStatus187 { get; set; }
        public int LStatus188 { get; set; }
        public int LStatus189 { get; set; }
        public int LStatus190 { get; set; }
        public int LStatus191 { get; set; }
        public int LStatus192 { get; set; }
        public int LStatus193 { get; set; }
        public int LStatus194 { get; set; }
        public int LStatus195 { get; set; }
        public int LStatus196 { get; set; }
        public int LStatus197 { get; set; }
        public int LStatus198 { get; set; }
        public int LStatus199 { get; set; }
        public int LStatus200 { get; set; }
        public int LStatus201 { get; set; }
        public int LStatus202 { get; set; }
        public int LStatus203 { get; set; }
        public int LStatus204 { get; set; }
        public int LStatus205 { get; set; }
        public int LStatus206 { get; set; }
        public int LStatus207 { get; set; }
        public int LStatus208 { get; set; }
        public int LStatus209 { get; set; }
        public int LStatus210 { get; set; }
        public int LStatus211 { get; set; }
        public int LStatus212 { get; set; }
        public int LStatus213 { get; set; }
        public int LStatus214 { get; set; }
        public int LStatus215 { get; set; }
        public int LStatus216 { get; set; }
        public int LStatus217 { get; set; }
        public int LStatus218 { get; set; }
        public int LStatus219 { get; set; }
        public int LStatus220 { get; set; }
        public int LStatus221 { get; set; }
        public int LStatus222 { get; set; }
        public int LStatus223 { get; set; }
        public int LStatus224 { get; set; }
        public int LStatus225 { get; set; }
        public int LStatus226 { get; set; }
        public int LStatus227 { get; set; }
        public int LStatus228 { get; set; }
        public int LStatus229 { get; set; }
        public int LStatus230 { get; set; }
        public int LStatus231 { get; set; }
        public int LStatus232 { get; set; }
        public int LStatus233 { get; set; }
        public int LStatus234 { get; set; }
        public int LStatus235 { get; set; }
        public int LStatus236 { get; set; }
        public int LStatus237 { get; set; }
        public int LStatus238 { get; set; }
        public int LStatus239 { get; set; }
        public int LStatus240 { get; set; }
        public int LStatus241 { get; set; }
        public int LStatus242 { get; set; }
        public int LStatus243 { get; set; }
        public int LStatus244 { get; set; }
        public int LStatus245 { get; set; }
        public int LStatus246 { get; set; }
        public int LStatus247 { get; set; }
        public int LStatus248 { get; set; }
        public int LStatus249 { get; set; }
        public int LStatus250 { get; set; }
        public int LStatus251 { get; set; }
        public int LStatus252 { get; set; }
        public int LStatus253 { get; set; }
        public int LStatus254 { get; set; }
        public int LStatus255 { get; set; }
        public int LStatus256 { get; set; }

        #endregion
        #region 设备1状态

        public int EStatus1_1 { get; set; }
        public int EStatus1_2 { get; set; }
        public int EStatus1_3 { get; set; }
        public int EStatus1_4 { get; set; }
        public int EStatus1_5 { get; set; }
        public int EStatus1_6 { get; set; }
        public int EStatus1_7 { get; set; }
        public int EStatus1_8 { get; set; }
        public int EStatus1_9 { get; set; }
        public int EStatus1_10 { get; set; }
        public int EStatus1_11 { get; set; }
        public int EStatus1_12 { get; set; }
        public int EStatus1_13 { get; set; }
        public int EStatus1_14 { get; set; }
        public int EStatus1_15 { get; set; }
        public int EStatus1_16 { get; set; }
        public int EStatus1_17 { get; set; }
        public int EStatus1_18 { get; set; }
        public int EStatus1_19 { get; set; }
        public int EStatus1_20 { get; set; }
        public int EStatus1_21 { get; set; }
        public int EStatus1_22 { get; set; }
        public int EStatus1_23 { get; set; }
        public int EStatus1_24 { get; set; }
        public int EStatus1_25 { get; set; }
        public int EStatus1_26 { get; set; }
        public int EStatus1_27 { get; set; }
        public int EStatus1_28 { get; set; }
        public int EStatus1_29 { get; set; }
        public int EStatus1_30 { get; set; }
        public int EStatus1_31 { get; set; }
        public int EStatus1_32 { get; set; }
        public int EStatus1_33 { get; set; }
        public int EStatus1_34 { get; set; }
        public int EStatus1_35 { get; set; }
        public int EStatus1_36 { get; set; }
        public int EStatus1_37 { get; set; }
        public int EStatus1_38 { get; set; }
        public int EStatus1_39 { get; set; }
        public int EStatus1_40 { get; set; }
        public int EStatus1_41 { get; set; }
        public int EStatus1_42 { get; set; }
        public int EStatus1_43 { get; set; }
        public int EStatus1_44 { get; set; }
        public int EStatus1_45 { get; set; }
        public int EStatus1_46 { get; set; }
        public int EStatus1_47 { get; set; }
        public int EStatus1_48 { get; set; }
        public int EStatus1_49 { get; set; }
        public int EStatus1_50 { get; set; }
        public int EStatus1_51 { get; set; }
        public int EStatus1_52 { get; set; }
        public int EStatus1_53 { get; set; }
        public int EStatus1_54 { get; set; }
        public int EStatus1_55 { get; set; }
        public int EStatus1_56 { get; set; }
        public int EStatus1_57 { get; set; }
        public int EStatus1_58 { get; set; }
        public int EStatus1_59 { get; set; }
        public int EStatus1_60 { get; set; }
        public int EStatus1_61 { get; set; }
        public int EStatus1_62 { get; set; }
        public int EStatus1_63 { get; set; }
        public int EStatus1_64 { get; set; }
        public int EStatus1_65 { get; set; }
        public int EStatus1_66 { get; set; }
        public int EStatus1_67 { get; set; }
        public int EStatus1_68 { get; set; }
        public int EStatus1_69 { get; set; }
        public int EStatus1_70 { get; set; }
        public int EStatus1_71 { get; set; }
        public int EStatus1_72 { get; set; }
        public int EStatus1_73 { get; set; }
        public int EStatus1_74 { get; set; }
        public int EStatus1_75 { get; set; }
        public int EStatus1_76 { get; set; }
        public int EStatus1_77 { get; set; }
        public int EStatus1_78 { get; set; }
        public int EStatus1_79 { get; set; }
        public int EStatus1_80 { get; set; }
        public int EStatus1_81 { get; set; }
        public int EStatus1_82 { get; set; }
        public int EStatus1_83 { get; set; }
        public int EStatus1_84 { get; set; }
        public int EStatus1_85 { get; set; }
        public int EStatus1_86 { get; set; }
        public int EStatus1_87 { get; set; }
        public int EStatus1_88 { get; set; }
        public int EStatus1_89 { get; set; }
        public int EStatus1_90 { get; set; }
        public int EStatus1_91 { get; set; }
        public int EStatus1_92 { get; set; }
        public int EStatus1_93 { get; set; }
        public int EStatus1_94 { get; set; }
        public int EStatus1_95 { get; set; }
        public int EStatus1_96 { get; set; }
        public int EStatus1_97 { get; set; }
        public int EStatus1_98 { get; set; }
        public int EStatus1_99 { get; set; }
        public int EStatus1_100 { get; set; }
        public int EStatus1_101 { get; set; }
        public int EStatus1_102 { get; set; }
        public int EStatus1_103 { get; set; }
        public int EStatus1_104 { get; set; }
        public int EStatus1_105 { get; set; }
        public int EStatus1_106 { get; set; }
        public int EStatus1_107 { get; set; }
        public int EStatus1_108 { get; set; }
        public int EStatus1_109 { get; set; }
        public int EStatus1_110 { get; set; }
        public int EStatus1_111 { get; set; }
        public int EStatus1_112 { get; set; }
        public int EStatus1_113 { get; set; }
        public int EStatus1_114 { get; set; }
        public int EStatus1_115 { get; set; }
        public int EStatus1_116 { get; set; }
        public int EStatus1_117 { get; set; }
        public int EStatus1_118 { get; set; }
        public int EStatus1_119 { get; set; }
        public int EStatus1_120 { get; set; }
        public int EStatus1_121 { get; set; }
        public int EStatus1_122 { get; set; }
        public int EStatus1_123 { get; set; }
        public int EStatus1_124 { get; set; }
        public int EStatus1_125 { get; set; }
        public int EStatus1_126 { get; set; }
        public int EStatus1_127 { get; set; }
        public int EStatus1_128 { get; set; }
        public int EStatus1_129 { get; set; }
        public int EStatus1_130 { get; set; }
        public int EStatus1_131 { get; set; }
        public int EStatus1_132 { get; set; }
        public int EStatus1_133 { get; set; }
        public int EStatus1_134 { get; set; }
        public int EStatus1_135 { get; set; }
        public int EStatus1_136 { get; set; }
        public int EStatus1_137 { get; set; }
        public int EStatus1_138 { get; set; }
        public int EStatus1_139 { get; set; }
        public int EStatus1_140 { get; set; }
        public int EStatus1_141 { get; set; }
        public int EStatus1_142 { get; set; }
        public int EStatus1_143 { get; set; }
        public int EStatus1_144 { get; set; }
        public int EStatus1_145 { get; set; }
        public int EStatus1_146 { get; set; }
        public int EStatus1_147 { get; set; }
        public int EStatus1_148 { get; set; }
        public int EStatus1_149 { get; set; }
        public int EStatus1_150 { get; set; }
        public int EStatus1_151 { get; set; }
        public int EStatus1_152 { get; set; }
        public int EStatus1_153 { get; set; }
        public int EStatus1_154 { get; set; }
        public int EStatus1_155 { get; set; }
        public int EStatus1_156 { get; set; }
        public int EStatus1_157 { get; set; }
        public int EStatus1_158 { get; set; }
        public int EStatus1_159 { get; set; }
        public int EStatus1_160 { get; set; }
        public int EStatus1_161 { get; set; }
        public int EStatus1_162 { get; set; }
        public int EStatus1_163 { get; set; }
        public int EStatus1_164 { get; set; }
        public int EStatus1_165 { get; set; }
        public int EStatus1_166 { get; set; }
        public int EStatus1_167 { get; set; }
        public int EStatus1_168 { get; set; }
        public int EStatus1_169 { get; set; }
        public int EStatus1_170 { get; set; }
        public int EStatus1_171 { get; set; }
        public int EStatus1_172 { get; set; }
        public int EStatus1_173 { get; set; }
        public int EStatus1_174 { get; set; }
        public int EStatus1_175 { get; set; }
        public int EStatus1_176 { get; set; }
        public int EStatus1_177 { get; set; }
        public int EStatus1_178 { get; set; }
        public int EStatus1_179 { get; set; }
        public int EStatus1_180 { get; set; }
        public int EStatus1_181 { get; set; }
        public int EStatus1_182 { get; set; }
        public int EStatus1_183 { get; set; }
        public int EStatus1_184 { get; set; }
        public int EStatus1_185 { get; set; }
        public int EStatus1_186 { get; set; }
        public int EStatus1_187 { get; set; }
        public int EStatus1_188 { get; set; }
        public int EStatus1_189 { get; set; }
        public int EStatus1_190 { get; set; }
        public int EStatus1_191 { get; set; }
        public int EStatus1_192 { get; set; }
        public int EStatus1_193 { get; set; }
        public int EStatus1_194 { get; set; }
        public int EStatus1_195 { get; set; }
        public int EStatus1_196 { get; set; }
        public int EStatus1_197 { get; set; }
        public int EStatus1_198 { get; set; }
        public int EStatus1_199 { get; set; }
        public int EStatus1_200 { get; set; }
        public int EStatus1_201 { get; set; }
        public int EStatus1_202 { get; set; }
        public int EStatus1_203 { get; set; }
        public int EStatus1_204 { get; set; }
        public int EStatus1_205 { get; set; }
        public int EStatus1_206 { get; set; }
        public int EStatus1_207 { get; set; }
        public int EStatus1_208 { get; set; }
        public int EStatus1_209 { get; set; }
        public int EStatus1_210 { get; set; }
        public int EStatus1_211 { get; set; }
        public int EStatus1_212 { get; set; }
        public int EStatus1_213 { get; set; }
        public int EStatus1_214 { get; set; }
        public int EStatus1_215 { get; set; }
        public int EStatus1_216 { get; set; }
        public int EStatus1_217 { get; set; }
        public int EStatus1_218 { get; set; }
        public int EStatus1_219 { get; set; }
        public int EStatus1_220 { get; set; }
        public int EStatus1_221 { get; set; }
        public int EStatus1_222 { get; set; }
        public int EStatus1_223 { get; set; }
        public int EStatus1_224 { get; set; }
        public int EStatus1_225 { get; set; }
        public int EStatus1_226 { get; set; }
        public int EStatus1_227 { get; set; }
        public int EStatus1_228 { get; set; }
        public int EStatus1_229 { get; set; }
        public int EStatus1_230 { get; set; }
        public int EStatus1_231 { get; set; }
        public int EStatus1_232 { get; set; }
        public int EStatus1_233 { get; set; }
        public int EStatus1_234 { get; set; }
        public int EStatus1_235 { get; set; }
        public int EStatus1_236 { get; set; }
        public int EStatus1_237 { get; set; }
        public int EStatus1_238 { get; set; }
        public int EStatus1_239 { get; set; }
        public int EStatus1_240 { get; set; }
        public int EStatus1_241 { get; set; }
        public int EStatus1_242 { get; set; }
        public int EStatus1_243 { get; set; }
        public int EStatus1_244 { get; set; }
        public int EStatus1_245 { get; set; }
        public int EStatus1_246 { get; set; }
        public int EStatus1_247 { get; set; }
        public int EStatus1_248 { get; set; }
        public int EStatus1_249 { get; set; }
        public int EStatus1_250 { get; set; }
        public int EStatus1_251 { get; set; }
        public int EStatus1_252 { get; set; }
        public int EStatus1_253 { get; set; }
        public int EStatus1_254 { get; set; }
        public int EStatus1_255 { get; set; }
        public int EStatus1_256 { get; set; }

        #endregion
        #region 设备2状态
        public int EStatus2_1 { get; set; }
        public int EStatus2_2 { get; set; }
        public int EStatus2_3 { get; set; }
        public int EStatus2_4 { get; set; }
        public int EStatus2_5 { get; set; }
        public int EStatus2_6 { get; set; }
        public int EStatus2_7 { get; set; }
        public int EStatus2_8 { get; set; }
        public int EStatus2_9 { get; set; }
        public int EStatus2_10 { get; set; }
        public int EStatus2_11 { get; set; }
        public int EStatus2_12 { get; set; }
        public int EStatus2_13 { get; set; }
        public int EStatus2_14 { get; set; }
        public int EStatus2_15 { get; set; }
        public int EStatus2_16 { get; set; }
        public int EStatus2_17 { get; set; }
        public int EStatus2_18 { get; set; }
        public int EStatus2_19 { get; set; }
        public int EStatus2_20 { get; set; }
        public int EStatus2_21 { get; set; }
        public int EStatus2_22 { get; set; }
        public int EStatus2_23 { get; set; }
        public int EStatus2_24 { get; set; }
        public int EStatus2_25 { get; set; }
        public int EStatus2_26 { get; set; }
        public int EStatus2_27 { get; set; }
        public int EStatus2_28 { get; set; }
        public int EStatus2_29 { get; set; }
        public int EStatus2_30 { get; set; }
        public int EStatus2_31 { get; set; }
        public int EStatus2_32 { get; set; }
        public int EStatus2_33 { get; set; }
        public int EStatus2_34 { get; set; }
        public int EStatus2_35 { get; set; }
        public int EStatus2_36 { get; set; }
        public int EStatus2_37 { get; set; }
        public int EStatus2_38 { get; set; }
        public int EStatus2_39 { get; set; }
        public int EStatus2_40 { get; set; }
        public int EStatus2_41 { get; set; }
        public int EStatus2_42 { get; set; }
        public int EStatus2_43 { get; set; }
        public int EStatus2_44 { get; set; }
        public int EStatus2_45 { get; set; }
        public int EStatus2_46 { get; set; }
        public int EStatus2_47 { get; set; }
        public int EStatus2_48 { get; set; }
        public int EStatus2_49 { get; set; }
        public int EStatus2_50 { get; set; }
        public int EStatus2_51 { get; set; }
        public int EStatus2_52 { get; set; }
        public int EStatus2_53 { get; set; }
        public int EStatus2_54 { get; set; }
        public int EStatus2_55 { get; set; }
        public int EStatus2_56 { get; set; }
        public int EStatus2_57 { get; set; }
        public int EStatus2_58 { get; set; }
        public int EStatus2_59 { get; set; }
        public int EStatus2_60 { get; set; }
        public int EStatus2_61 { get; set; }
        public int EStatus2_62 { get; set; }
        public int EStatus2_63 { get; set; }
        public int EStatus2_64 { get; set; }
        public int EStatus2_65 { get; set; }
        public int EStatus2_66 { get; set; }
        public int EStatus2_67 { get; set; }
        public int EStatus2_68 { get; set; }
        public int EStatus2_69 { get; set; }
        public int EStatus2_70 { get; set; }
        public int EStatus2_71 { get; set; }
        public int EStatus2_72 { get; set; }
        public int EStatus2_73 { get; set; }
        public int EStatus2_74 { get; set; }
        public int EStatus2_75 { get; set; }
        public int EStatus2_76 { get; set; }
        public int EStatus2_77 { get; set; }
        public int EStatus2_78 { get; set; }
        public int EStatus2_79 { get; set; }
        public int EStatus2_80 { get; set; }
        public int EStatus2_81 { get; set; }
        public int EStatus2_82 { get; set; }
        public int EStatus2_83 { get; set; }
        public int EStatus2_84 { get; set; }
        public int EStatus2_85 { get; set; }
        public int EStatus2_86 { get; set; }
        public int EStatus2_87 { get; set; }
        public int EStatus2_88 { get; set; }
        public int EStatus2_89 { get; set; }
        public int EStatus2_90 { get; set; }
        public int EStatus2_91 { get; set; }
        public int EStatus2_92 { get; set; }
        public int EStatus2_93 { get; set; }
        public int EStatus2_94 { get; set; }
        public int EStatus2_95 { get; set; }
        public int EStatus2_96 { get; set; }
        public int EStatus2_97 { get; set; }
        public int EStatus2_98 { get; set; }
        public int EStatus2_99 { get; set; }
        public int EStatus2_100 { get; set; }
        public int EStatus2_101 { get; set; }
        public int EStatus2_102 { get; set; }
        public int EStatus2_103 { get; set; }
        public int EStatus2_104 { get; set; }
        public int EStatus2_105 { get; set; }
        public int EStatus2_106 { get; set; }
        public int EStatus2_107 { get; set; }
        public int EStatus2_108 { get; set; }
        public int EStatus2_109 { get; set; }
        public int EStatus2_110 { get; set; }
        public int EStatus2_111 { get; set; }
        public int EStatus2_112 { get; set; }
        public int EStatus2_113 { get; set; }
        public int EStatus2_114 { get; set; }
        public int EStatus2_115 { get; set; }
        public int EStatus2_116 { get; set; }
        public int EStatus2_117 { get; set; }
        public int EStatus2_118 { get; set; }
        public int EStatus2_119 { get; set; }
        public int EStatus2_120 { get; set; }
        public int EStatus2_121 { get; set; }
        public int EStatus2_122 { get; set; }
        public int EStatus2_123 { get; set; }
        public int EStatus2_124 { get; set; }
        public int EStatus2_125 { get; set; }
        public int EStatus2_126 { get; set; }
        public int EStatus2_127 { get; set; }
        public int EStatus2_128 { get; set; }
        public int EStatus2_129 { get; set; }
        public int EStatus2_130 { get; set; }
        public int EStatus2_131 { get; set; }
        public int EStatus2_132 { get; set; }
        public int EStatus2_133 { get; set; }
        public int EStatus2_134 { get; set; }
        public int EStatus2_135 { get; set; }
        public int EStatus2_136 { get; set; }
        public int EStatus2_137 { get; set; }
        public int EStatus2_138 { get; set; }
        public int EStatus2_139 { get; set; }
        public int EStatus2_140 { get; set; }
        public int EStatus2_141 { get; set; }
        public int EStatus2_142 { get; set; }
        public int EStatus2_143 { get; set; }
        public int EStatus2_144 { get; set; }
        public int EStatus2_145 { get; set; }
        public int EStatus2_146 { get; set; }
        public int EStatus2_147 { get; set; }
        public int EStatus2_148 { get; set; }
        public int EStatus2_149 { get; set; }
        public int EStatus2_150 { get; set; }
        public int EStatus2_151 { get; set; }
        public int EStatus2_152 { get; set; }
        public int EStatus2_153 { get; set; }
        public int EStatus2_154 { get; set; }
        public int EStatus2_155 { get; set; }
        public int EStatus2_156 { get; set; }
        public int EStatus2_157 { get; set; }
        public int EStatus2_158 { get; set; }
        public int EStatus2_159 { get; set; }
        public int EStatus2_160 { get; set; }
        public int EStatus2_161 { get; set; }
        public int EStatus2_162 { get; set; }
        public int EStatus2_163 { get; set; }
        public int EStatus2_164 { get; set; }
        public int EStatus2_165 { get; set; }
        public int EStatus2_166 { get; set; }
        public int EStatus2_167 { get; set; }
        public int EStatus2_168 { get; set; }
        public int EStatus2_169 { get; set; }
        public int EStatus2_170 { get; set; }
        public int EStatus2_171 { get; set; }
        public int EStatus2_172 { get; set; }
        public int EStatus2_173 { get; set; }
        public int EStatus2_174 { get; set; }
        public int EStatus2_175 { get; set; }
        public int EStatus2_176 { get; set; }
        public int EStatus2_177 { get; set; }
        public int EStatus2_178 { get; set; }
        public int EStatus2_179 { get; set; }
        public int EStatus2_180 { get; set; }
        public int EStatus2_181 { get; set; }
        public int EStatus2_182 { get; set; }
        public int EStatus2_183 { get; set; }
        public int EStatus2_184 { get; set; }
        public int EStatus2_185 { get; set; }
        public int EStatus2_186 { get; set; }
        public int EStatus2_187 { get; set; }
        public int EStatus2_188 { get; set; }
        public int EStatus2_189 { get; set; }
        public int EStatus2_190 { get; set; }
        public int EStatus2_191 { get; set; }
        public int EStatus2_192 { get; set; }
        public int EStatus2_193 { get; set; }
        public int EStatus2_194 { get; set; }
        public int EStatus2_195 { get; set; }
        public int EStatus2_196 { get; set; }
        public int EStatus2_197 { get; set; }
        public int EStatus2_198 { get; set; }
        public int EStatus2_199 { get; set; }
        public int EStatus2_200 { get; set; }
        public int EStatus2_201 { get; set; }
        public int EStatus2_202 { get; set; }
        public int EStatus2_203 { get; set; }
        public int EStatus2_204 { get; set; }
        public int EStatus2_205 { get; set; }
        public int EStatus2_206 { get; set; }
        public int EStatus2_207 { get; set; }
        public int EStatus2_208 { get; set; }
        public int EStatus2_209 { get; set; }
        public int EStatus2_210 { get; set; }
        public int EStatus2_211 { get; set; }
        public int EStatus2_212 { get; set; }
        public int EStatus2_213 { get; set; }
        public int EStatus2_214 { get; set; }
        public int EStatus2_215 { get; set; }
        public int EStatus2_216 { get; set; }
        public int EStatus2_217 { get; set; }
        public int EStatus2_218 { get; set; }
        public int EStatus2_219 { get; set; }
        public int EStatus2_220 { get; set; }
        public int EStatus2_221 { get; set; }
        public int EStatus2_222 { get; set; }
        public int EStatus2_223 { get; set; }
        public int EStatus2_224 { get; set; }
        public int EStatus2_225 { get; set; }
        public int EStatus2_226 { get; set; }
        public int EStatus2_227 { get; set; }
        public int EStatus2_228 { get; set; }
        public int EStatus2_229 { get; set; }
        public int EStatus2_230 { get; set; }
        public int EStatus2_231 { get; set; }
        public int EStatus2_232 { get; set; }
        public int EStatus2_233 { get; set; }
        public int EStatus2_234 { get; set; }
        public int EStatus2_235 { get; set; }
        public int EStatus2_236 { get; set; }
        public int EStatus2_237 { get; set; }
        public int EStatus2_238 { get; set; }
        public int EStatus2_239 { get; set; }
        public int EStatus2_240 { get; set; }
        public int EStatus2_241 { get; set; }
        public int EStatus2_242 { get; set; }
        public int EStatus2_243 { get; set; }
        public int EStatus2_244 { get; set; }
        public int EStatus2_245 { get; set; }
        public int EStatus2_246 { get; set; }
        public int EStatus2_247 { get; set; }
        public int EStatus2_248 { get; set; }
        public int EStatus2_249 { get; set; }
        public int EStatus2_250 { get; set; }
        public int EStatus2_251 { get; set; }
        public int EStatus2_252 { get; set; }
        public int EStatus2_253 { get; set; }
        public int EStatus2_254 { get; set; }
        public int EStatus2_255 { get; set; }
        public int EStatus2_256 { get; set; }

        #endregion
        #region WCS调度AGV 光电

        public int WAStatus1 { get; set; }
        public int WAStatus2 { get; set; }
        public int WAStatus3 { get; set; }
        public int WAStatus4 { get; set; }
        public int WAStatus5 { get; set; }
        public int WAStatus6 { get; set; }
        public int WAStatus7 { get; set; }
        public int WAStatus8 { get; set; }
        public int WAStatus9 { get; set; }
        public int WAStatus10 { get; set; }
        public int WAStatus11 { get; set; }
        public int WAStatus12 { get; set; }
        public int WAStatus13 { get; set; }
        public int WAStatus14 { get; set; }
        public int WAStatus15 { get; set; }
        public int WAStatus16 { get; set; }
        public int WAStatus17 { get; set; }
        public int WAStatus18 { get; set; }
        public int WAStatus19 { get; set; }
        public int WAStatus20 { get; set; }
        public int WAStatus21 { get; set; }
        public int WAStatus22 { get; set; }
        public int WAStatus23 { get; set; }
        public int WAStatus24 { get; set; }
        public int WAStatus25 { get; set; }
        public int WAStatus26 { get; set; }
        public int WAStatus27 { get; set; }
        public int WAStatus28 { get; set; }
        public int WAStatus29 { get; set; }
        public int WAStatus30 { get; set; }
        public int WAStatus31 { get; set; }
        public int WAStatus32 { get; set; }
        public int WAStatus33 { get; set; }
        public int WAStatus34 { get; set; }
        public int WAStatus35 { get; set; }
        public int WAStatus36 { get; set; }
        public int WAStatus37 { get; set; }
        public int WAStatus38 { get; set; }
        public int WAStatus39 { get; set; }
        public int WAStatus40 { get; set; }
        public int WAStatus41 { get; set; }
        public int WAStatus42 { get; set; }
        public int WAStatus43 { get; set; }
        public int WAStatus44 { get; set; }
        public int WAStatus45 { get; set; }
        public int WAStatus46 { get; set; }
        public int WAStatus47 { get; set; }
        public int WAStatus48 { get; set; }
        public int WAStatus49 { get; set; }
        public int WAStatus50 { get; set; }
        public int WAStatus51 { get; set; }
        public int WAStatus52 { get; set; }
        public int WAStatus53 { get; set; }
        public int WAStatus54 { get; set; }
        public int WAStatus55 { get; set; }
        public int WAStatus56 { get; set; }
        public int WAStatus57 { get; set; }
        public int WAStatus58 { get; set; }
        public int WAStatus59 { get; set; }
        public int WAStatus60 { get; set; }
        public int WAStatus61 { get; set; }
        public int WAStatus62 { get; set; }
        public int WAStatus63 { get; set; }
        public int WAStatus64 { get; set; }
        public int WAStatus65 { get; set; }
        public int WAStatus66 { get; set; }
        public int WAStatus67 { get; set; }
        public int WAStatus68 { get; set; }
        public int WAStatus69 { get; set; }
        public int WAStatus70 { get; set; }
        public int WAStatus71 { get; set; }
        public int WAStatus72 { get; set; }
        public int WAStatus73 { get; set; }
        public int WAStatus74 { get; set; }
        public int WAStatus75 { get; set; }
        public int WAStatus76 { get; set; }
        public int WAStatus77 { get; set; }
        public int WAStatus78 { get; set; }
        public int WAStatus79 { get; set; }
        public int WAStatus80 { get; set; }
        public int WAStatus81 { get; set; }
        public int WAStatus82 { get; set; }
        public int WAStatus83 { get; set; }
        public int WAStatus84 { get; set; }
        public int WAStatus85 { get; set; }
        public int WAStatus86 { get; set; }
        public int WAStatus87 { get; set; }
        public int WAStatus88 { get; set; }
        public int WAStatus89 { get; set; }
        public int WAStatus90 { get; set; }
        public int WAStatus91 { get; set; }
        public int WAStatus92 { get; set; }
        public int WAStatus93 { get; set; }
        public int WAStatus94 { get; set; }
        public int WAStatus95 { get; set; }
        public int WAStatus96 { get; set; }
        public int WAStatus97 { get; set; }
        public int WAStatus98 { get; set; }
        public int WAStatus99 { get; set; }
        public int WAStatus100 { get; set; }
        public int WAStatus101 { get; set; }
        public int WAStatus102 { get; set; }
        public int WAStatus103 { get; set; }
        public int WAStatus104 { get; set; }
        public int WAStatus105 { get; set; }
        public int WAStatus106 { get; set; }
        public int WAStatus107 { get; set; }
        public int WAStatus108 { get; set; }
        public int WAStatus109 { get; set; }
        public int WAStatus110 { get; set; }
        public int WAStatus111 { get; set; }
        public int WAStatus112 { get; set; }
        public int WAStatus113 { get; set; }
        public int WAStatus114 { get; set; }
        public int WAStatus115 { get; set; }
        public int WAStatus116 { get; set; }
        public int WAStatus117 { get; set; }
        public int WAStatus118 { get; set; }
        public int WAStatus119 { get; set; }
        public int WAStatus120 { get; set; }
        public int WAStatus121 { get; set; }
        public int WAStatus122 { get; set; }
        public int WAStatus123 { get; set; }
        public int WAStatus124 { get; set; }
        public int WAStatus125 { get; set; }
        public int WAStatus126 { get; set; }
        public int WAStatus127 { get; set; }
        public int WAStatus128 { get; set; }
        public int WAStatus129 { get; set; }
        public int WAStatus130 { get; set; }
        public int WAStatus131 { get; set; }
        public int WAStatus132 { get; set; }
        public int WAStatus133 { get; set; }
        public int WAStatus134 { get; set; }
        public int WAStatus135 { get; set; }
        public int WAStatus136 { get; set; }
        public int WAStatus137 { get; set; }
        public int WAStatus138 { get; set; }
        public int WAStatus139 { get; set; }
        public int WAStatus140 { get; set; }
        public int WAStatus141 { get; set; }
        public int WAStatus142 { get; set; }
        public int WAStatus143 { get; set; }
        public int WAStatus144 { get; set; }
        public int WAStatus145 { get; set; }
        public int WAStatus146 { get; set; }
        public int WAStatus147 { get; set; }
        public int WAStatus148 { get; set; }
        public int WAStatus149 { get; set; }
        public int WAStatus150 { get; set; }
        public int WAStatus151 { get; set; }
        public int WAStatus152 { get; set; }
        public int WAStatus153 { get; set; }
        public int WAStatus154 { get; set; }
        public int WAStatus155 { get; set; }
        public int WAStatus156 { get; set; }
        public int WAStatus157 { get; set; }
        public int WAStatus158 { get; set; }
        public int WAStatus159 { get; set; }
        public int WAStatus160 { get; set; }
        public int WAStatus161 { get; set; }
        public int WAStatus162 { get; set; }
        public int WAStatus163 { get; set; }
        public int WAStatus164 { get; set; }
        public int WAStatus165 { get; set; }
        public int WAStatus166 { get; set; }
        public int WAStatus167 { get; set; }
        public int WAStatus168 { get; set; }
        public int WAStatus169 { get; set; }
        public int WAStatus170 { get; set; }
        public int WAStatus171 { get; set; }
        public int WAStatus172 { get; set; }
        public int WAStatus173 { get; set; }
        public int WAStatus174 { get; set; }
        public int WAStatus175 { get; set; }
        public int WAStatus176 { get; set; }
        public int WAStatus177 { get; set; }
        public int WAStatus178 { get; set; }
        public int WAStatus179 { get; set; }
        public int WAStatus180 { get; set; }
        public int WAStatus181 { get; set; }
        public int WAStatus182 { get; set; }
        public int WAStatus183 { get; set; }
        public int WAStatus184 { get; set; }
        public int WAStatus185 { get; set; }
        public int WAStatus186 { get; set; }
        public int WAStatus187 { get; set; }
        public int WAStatus188 { get; set; }
        public int WAStatus189 { get; set; }
        public int WAStatus190 { get; set; }
        public int WAStatus191 { get; set; }
        public int WAStatus192 { get; set; }
        public int WAStatus193 { get; set; }
        public int WAStatus194 { get; set; }
        public int WAStatus195 { get; set; }
        public int WAStatus196 { get; set; }
        public int WAStatus197 { get; set; }
        public int WAStatus198 { get; set; }
        public int WAStatus199 { get; set; }
        public int WAStatus200 { get; set; }
        public int WAStatus201 { get; set; }
        public int WAStatus202 { get; set; }
        public int WAStatus203 { get; set; }
        public int WAStatus204 { get; set; }
        public int WAStatus205 { get; set; }
        public int WAStatus206 { get; set; }
        public int WAStatus207 { get; set; }
        public int WAStatus208 { get; set; }
        public int WAStatus209 { get; set; }
        public int WAStatus210 { get; set; }
        public int WAStatus211 { get; set; }
        public int WAStatus212 { get; set; }
        public int WAStatus213 { get; set; }
        public int WAStatus214 { get; set; }
        public int WAStatus215 { get; set; }
        public int WAStatus216 { get; set; }
        public int WAStatus217 { get; set; }
        public int WAStatus218 { get; set; }
        public int WAStatus219 { get; set; }
        public int WAStatus220 { get; set; }
        public int WAStatus221 { get; set; }
        public int WAStatus222 { get; set; }
        public int WAStatus223 { get; set; }
        public int WAStatus224 { get; set; }
        public int WAStatus225 { get; set; }
        public int WAStatus226 { get; set; }
        public int WAStatus227 { get; set; }
        public int WAStatus228 { get; set; }
        public int WAStatus229 { get; set; }
        public int WAStatus230 { get; set; }
        public int WAStatus231 { get; set; }
        public int WAStatus232 { get; set; }
        public int WAStatus233 { get; set; }
        public int WAStatus234 { get; set; }
        public int WAStatus235 { get; set; }
        public int WAStatus236 { get; set; }
        public int WAStatus237 { get; set; }
        public int WAStatus238 { get; set; }
        public int WAStatus239 { get; set; }
        public int WAStatus240 { get; set; }
        public int WAStatus241 { get; set; }
        public int WAStatus242 { get; set; }
        public int WAStatus243 { get; set; }
        public int WAStatus244 { get; set; }
        public int WAStatus245 { get; set; }
        public int WAStatus246 { get; set; }
        public int WAStatus247 { get; set; }
        public int WAStatus248 { get; set; }
        public int WAStatus249 { get; set; }
        public int WAStatus250 { get; set; }
        public int WAStatus251 { get; set; }
        public int WAStatus252 { get; set; }
        public int WAStatus253 { get; set; }
        public int WAStatus254 { get; set; }
        public int WAStatus255 { get; set; }
        public int WAStatus256 { get; set; }

        #endregion
        #region AGV调度状态

        public int AGVDisStatus1 { get; set; }
        public int AGVDisStatus2 { get; set; }
        public int AGVDisStatus3 { get; set; }
        public int AGVDisStatus4 { get; set; }
        public int AGVDisStatus5 { get; set; }
        public int AGVDisStatus6 { get; set; }
        public int AGVDisStatus7 { get; set; }
        public int AGVDisStatus8 { get; set; }
        public int AGVDisStatus9 { get; set; }
        public int AGVDisStatus10 { get; set; }
        public int AGVDisStatus11 { get; set; }
        public int AGVDisStatus12 { get; set; }
        public int AGVDisStatus13 { get; set; }
        public int AGVDisStatus14 { get; set; }
        public int AGVDisStatus15 { get; set; }
        public int AGVDisStatus16 { get; set; }
        public int AGVDisStatus17 { get; set; }
        public int AGVDisStatus18 { get; set; }
        public int AGVDisStatus19 { get; set; }
        public int AGVDisStatus20 { get; set; }
        public int AGVDisStatus21 { get; set; }
        public int AGVDisStatus22 { get; set; }
        public int AGVDisStatus23 { get; set; }
        public int AGVDisStatus24 { get; set; }
        public int AGVDisStatus25 { get; set; }
        public int AGVDisStatus26 { get; set; }
        public int AGVDisStatus27 { get; set; }
        public int AGVDisStatus28 { get; set; }
        public int AGVDisStatus29 { get; set; }
        public int AGVDisStatus30 { get; set; }
        public int AGVDisStatus31 { get; set; }
        public int AGVDisStatus32 { get; set; }
        public int AGVDisStatus33 { get; set; }
        public int AGVDisStatus34 { get; set; }
        public int AGVDisStatus35 { get; set; }
        public int AGVDisStatus36 { get; set; }
        public int AGVDisStatus37 { get; set; }
        public int AGVDisStatus38 { get; set; }
        public int AGVDisStatus39 { get; set; }
        public int AGVDisStatus40 { get; set; }
        public int AGVDisStatus41 { get; set; }
        public int AGVDisStatus42 { get; set; }
        public int AGVDisStatus43 { get; set; }
        public int AGVDisStatus44 { get; set; }
        public int AGVDisStatus45 { get; set; }
        public int AGVDisStatus46 { get; set; }
        public int AGVDisStatus47 { get; set; }
        public int AGVDisStatus48 { get; set; }
        public int AGVDisStatus49 { get; set; }
        public int AGVDisStatus50 { get; set; }
        public int AGVDisStatus51 { get; set; }
        public int AGVDisStatus52 { get; set; }
        public int AGVDisStatus53 { get; set; }
        public int AGVDisStatus54 { get; set; }
        public int AGVDisStatus55 { get; set; }
        public int AGVDisStatus56 { get; set; }
        public int AGVDisStatus57 { get; set; }
        public int AGVDisStatus58 { get; set; }
        public int AGVDisStatus59 { get; set; }
        public int AGVDisStatus60 { get; set; }
        public int AGVDisStatus61 { get; set; }
        public int AGVDisStatus62 { get; set; }
        public int AGVDisStatus63 { get; set; }
        public int AGVDisStatus64 { get; set; }
        public int AGVDisStatus65 { get; set; }
        public int AGVDisStatus66 { get; set; }
        public int AGVDisStatus67 { get; set; }
        public int AGVDisStatus68 { get; set; }
        public int AGVDisStatus69 { get; set; }
        public int AGVDisStatus70 { get; set; }
        public int AGVDisStatus71 { get; set; }
        public int AGVDisStatus72 { get; set; }
        public int AGVDisStatus73 { get; set; }
        public int AGVDisStatus74 { get; set; }
        public int AGVDisStatus75 { get; set; }
        public int AGVDisStatus76 { get; set; }
        public int AGVDisStatus77 { get; set; }
        public int AGVDisStatus78 { get; set; }
        public int AGVDisStatus79 { get; set; }
        public int AGVDisStatus80 { get; set; }
        public int AGVDisStatus81 { get; set; }
        public int AGVDisStatus82 { get; set; }
        public int AGVDisStatus83 { get; set; }
        public int AGVDisStatus84 { get; set; }
        public int AGVDisStatus85 { get; set; }
        public int AGVDisStatus86 { get; set; }
        public int AGVDisStatus87 { get; set; }
        public int AGVDisStatus88 { get; set; }
        public int AGVDisStatus89 { get; set; }
        public int AGVDisStatus90 { get; set; }
        public int AGVDisStatus91 { get; set; }
        public int AGVDisStatus92 { get; set; }
        public int AGVDisStatus93 { get; set; }
        public int AGVDisStatus94 { get; set; }
        public int AGVDisStatus95 { get; set; }
        public int AGVDisStatus96 { get; set; }
        public int AGVDisStatus97 { get; set; }
        public int AGVDisStatus98 { get; set; }
        public int AGVDisStatus99 { get; set; }
        public int AGVDisStatus100 { get; set; }
        public int AGVDisStatus101 { get; set; }
        public int AGVDisStatus102 { get; set; }
        public int AGVDisStatus103 { get; set; }
        public int AGVDisStatus104 { get; set; }
        public int AGVDisStatus105 { get; set; }
        public int AGVDisStatus106 { get; set; }
        public int AGVDisStatus107 { get; set; }
        public int AGVDisStatus108 { get; set; }
        public int AGVDisStatus109 { get; set; }
        public int AGVDisStatus110 { get; set; }
        public int AGVDisStatus111 { get; set; }
        public int AGVDisStatus112 { get; set; }
        public int AGVDisStatus113 { get; set; }
        public int AGVDisStatus114 { get; set; }
        public int AGVDisStatus115 { get; set; }
        public int AGVDisStatus116 { get; set; }
        public int AGVDisStatus117 { get; set; }
        public int AGVDisStatus118 { get; set; }
        public int AGVDisStatus119 { get; set; }
        public int AGVDisStatus120 { get; set; }
        public int AGVDisStatus121 { get; set; }
        public int AGVDisStatus122 { get; set; }
        public int AGVDisStatus123 { get; set; }
        public int AGVDisStatus124 { get; set; }
        public int AGVDisStatus125 { get; set; }
        public int AGVDisStatus126 { get; set; }
        public int AGVDisStatus127 { get; set; }
        public int AGVDisStatus128 { get; set; }
        public int AGVDisStatus129 { get; set; }
        public int AGVDisStatus130 { get; set; }
        public int AGVDisStatus131 { get; set; }
        public int AGVDisStatus132 { get; set; }
        public int AGVDisStatus133 { get; set; }
        public int AGVDisStatus134 { get; set; }
        public int AGVDisStatus135 { get; set; }
        public int AGVDisStatus136 { get; set; }
        public int AGVDisStatus137 { get; set; }
        public int AGVDisStatus138 { get; set; }
        public int AGVDisStatus139 { get; set; }
        public int AGVDisStatus140 { get; set; }
        public int AGVDisStatus141 { get; set; }
        public int AGVDisStatus142 { get; set; }
        public int AGVDisStatus143 { get; set; }
        public int AGVDisStatus144 { get; set; }
        public int AGVDisStatus145 { get; set; }
        public int AGVDisStatus146 { get; set; }
        public int AGVDisStatus147 { get; set; }
        public int AGVDisStatus148 { get; set; }
        public int AGVDisStatus149 { get; set; }
        public int AGVDisStatus150 { get; set; }
        public int AGVDisStatus151 { get; set; }
        public int AGVDisStatus152 { get; set; }
        public int AGVDisStatus153 { get; set; }
        public int AGVDisStatus154 { get; set; }
        public int AGVDisStatus155 { get; set; }
        public int AGVDisStatus156 { get; set; }
        public int AGVDisStatus157 { get; set; }
        public int AGVDisStatus158 { get; set; }
        public int AGVDisStatus159 { get; set; }
        public int AGVDisStatus160 { get; set; }
        public int AGVDisStatus161 { get; set; }
        public int AGVDisStatus162 { get; set; }
        public int AGVDisStatus163 { get; set; }
        public int AGVDisStatus164 { get; set; }
        public int AGVDisStatus165 { get; set; }
        public int AGVDisStatus166 { get; set; }
        public int AGVDisStatus167 { get; set; }
        public int AGVDisStatus168 { get; set; }
        public int AGVDisStatus169 { get; set; }
        public int AGVDisStatus170 { get; set; }
        public int AGVDisStatus171 { get; set; }
        public int AGVDisStatus172 { get; set; }
        public int AGVDisStatus173 { get; set; }
        public int AGVDisStatus174 { get; set; }
        public int AGVDisStatus175 { get; set; }
        public int AGVDisStatus176 { get; set; }
        public int AGVDisStatus177 { get; set; }
        public int AGVDisStatus178 { get; set; }
        public int AGVDisStatus179 { get; set; }
        public int AGVDisStatus180 { get; set; }
        public int AGVDisStatus181 { get; set; }
        public int AGVDisStatus182 { get; set; }
        public int AGVDisStatus183 { get; set; }
        public int AGVDisStatus184 { get; set; }
        public int AGVDisStatus185 { get; set; }
        public int AGVDisStatus186 { get; set; }
        public int AGVDisStatus187 { get; set; }
        public int AGVDisStatus188 { get; set; }
        public int AGVDisStatus189 { get; set; }
        public int AGVDisStatus190 { get; set; }
        public int AGVDisStatus191 { get; set; }
        public int AGVDisStatus192 { get; set; }
        public int AGVDisStatus193 { get; set; }
        public int AGVDisStatus194 { get; set; }
        public int AGVDisStatus195 { get; set; }
        public int AGVDisStatus196 { get; set; }
        public int AGVDisStatus197 { get; set; }
        public int AGVDisStatus198 { get; set; }
        public int AGVDisStatus199 { get; set; }
        public int AGVDisStatus200 { get; set; }
        public int AGVDisStatus201 { get; set; }
        public int AGVDisStatus202 { get; set; }
        public int AGVDisStatus203 { get; set; }
        public int AGVDisStatus204 { get; set; }
        public int AGVDisStatus205 { get; set; }
        public int AGVDisStatus206 { get; set; }
        public int AGVDisStatus207 { get; set; }
        public int AGVDisStatus208 { get; set; }
        public int AGVDisStatus209 { get; set; }
        public int AGVDisStatus210 { get; set; }
        public int AGVDisStatus211 { get; set; }
        public int AGVDisStatus212 { get; set; }
        public int AGVDisStatus213 { get; set; }
        public int AGVDisStatus214 { get; set; }
        public int AGVDisStatus215 { get; set; }
        public int AGVDisStatus216 { get; set; }
        public int AGVDisStatus217 { get; set; }
        public int AGVDisStatus218 { get; set; }
        public int AGVDisStatus219 { get; set; }
        public int AGVDisStatus220 { get; set; }
        public int AGVDisStatus221 { get; set; }
        public int AGVDisStatus222 { get; set; }
        public int AGVDisStatus223 { get; set; }
        public int AGVDisStatus224 { get; set; }
        public int AGVDisStatus225 { get; set; }
        public int AGVDisStatus226 { get; set; }
        public int AGVDisStatus227 { get; set; }
        public int AGVDisStatus228 { get; set; }
        public int AGVDisStatus229 { get; set; }
        public int AGVDisStatus230 { get; set; }
        public int AGVDisStatus231 { get; set; }
        public int AGVDisStatus232 { get; set; }
        public int AGVDisStatus233 { get; set; }
        public int AGVDisStatus234 { get; set; }
        public int AGVDisStatus235 { get; set; }
        public int AGVDisStatus236 { get; set; }
        public int AGVDisStatus237 { get; set; }
        public int AGVDisStatus238 { get; set; }
        public int AGVDisStatus239 { get; set; }
        public int AGVDisStatus240 { get; set; }
        public int AGVDisStatus241 { get; set; }
        public int AGVDisStatus242 { get; set; }
        public int AGVDisStatus243 { get; set; }
        public int AGVDisStatus244 { get; set; }
        public int AGVDisStatus245 { get; set; }
        public int AGVDisStatus246 { get; set; }
        public int AGVDisStatus247 { get; set; }
        public int AGVDisStatus248 { get; set; }
        public int AGVDisStatus249 { get; set; }
        public int AGVDisStatus250 { get; set; }
        public int AGVDisStatus251 { get; set; }
        public int AGVDisStatus252 { get; set; }
        public int AGVDisStatus253 { get; set; }
        public int AGVDisStatus254 { get; set; }
        public int AGVDisStatus255 { get; set; }
        public int AGVDisStatus256 { get; set; }
        #endregion

    }
}
