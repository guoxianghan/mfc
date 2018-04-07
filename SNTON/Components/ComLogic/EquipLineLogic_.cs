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
    /// 分区用的Logic
    /// 车间设备线体:0无任务,1需要送轮子,2
    /// </summary>
    public class EquipLineLogic_ : ComLogic
    {
        //private VIThreadEx threadequiptask;
        private VIThreadEx thread_ReadEquipLineStatus;
        private VIThreadEx thread_SendCreateAGV;
        private VIThreadEx thread_heartbeat;
        //private VIThreadEx thread_plctest;

        public EquipLineLogic_()
        {
            //threadequiptask = new VIThreadEx(CheckEquipTask, null, "Check AGV task Ready", 1000);
            thread_ReadEquipLineStatus = new VIThreadEx(ReadEquipLine, null, "Check EquipLine Status", 3000);
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
        public new static EquipLineLogic_ Create(XmlNode node)
        {
            EquipLineLogic_ a = new EquipLineLogic_();
            a.Init(node);
            return a;
        }
        //List<EquipConfigerEntity> equipcmd = null;
        List<EquipConfigerEntity> _EquipCmdList { get; set; } = null;
        List<string> machcodes = new List<string>();
        List<string> _warningMessageList = new List<string>();

        void SendCreateAGV()
        {
            if (_EquipCmdList == null)
                _EquipCmdList = this.BusinessLogic.EquipConfigerProvider.EquipConfigers.FindAll(x => x.PLCNo == PLCNo);
            //var equiptsk = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities($"[STATUS]=1 AND PlantNo={PlantNo} AND PLCNo={PLCNo}", null);
            //16收到中控系统的调车指令,小车分配成功
            var agvtsks = this.BusinessLogic.AGVTasksProvider.GetAGVTasks($"[STATUS] IN(8,16,19) AND [PlantNo]={PlantNo} and id>=15008 ", null);//AND PLCNo={PLCNo}
            if (agvtsks == null || agvtsks.Count == 0)
                return;

            List<long> idequiptsks = new List<long>();
            StringBuilder sbequipname = new StringBuilder();
            foreach (var item in agvtsks)
            {
                #region 处理每一个AGV任务
                var EquipTask = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewNotDeleted($"TaskGuid='{item.TaskGuid.ToString()}'", null);
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
                    var cmd = _EquipCmdList.FirstOrDefault(x => x.EquipFlag.Trim() == task.EquipFlag.Trim());
                    if (cmd == null)
                        continue;
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
                    r = MXParser.SendData(n, 3);
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
            catch (Exception ex)
            {
                logger.ErrorMethod("", ex);
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
                int recive = plcNeutrino.GetInt(item.AGVDisStatus.Trim());//地面滚筒请求 AGVDisStatus
                #region 创建equiptsk
                if (equipstatus == 0 && inOrout != 0 && recive == 0)
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
                            r = this.BusinessLogic.EquipTaskProvider.CreateEquipTask(new EquipTaskEntity() { Length = item.MachStructCode.ProdCodeStructMark4.ProdLength, Created = DateTime.Now, EquipContollerId = item.ControlID, ProductType = item.MachStructCode.ProdCodeStructMark4?.CName, Status = 0, TaskType = 1, TaskLevel = 6, PlantNo = PlantNo, Supply1 = item.MachStructCode.ProdCodeStructMark3.Supply1, PLCNo = PLCNo });//创建出任务
                            logger.InfoMethod("创建拉空轮任务, item.ControlID" + item.ControlID.ToString() + "result:" + r);
                        }
                        else if (inOrout == 2)//创建拉满轮任务
                        {
                            r = this.BusinessLogic.EquipTaskProvider.CreateEquipTask(new EquipTaskEntity() { Length = item.MachStructCode.ProdCodeStructMark4.ProdLength, Created = DateTime.Now, EquipContollerId = item.ControlID, ProductType = item.MachStructCode.ProdCodeStructMark4?.CName, Status = 0, TaskType = 2, TaskLevel = 5, PlantNo = PlantNo, Supply1 = item.MachStructCode.ProdCodeStructMark3.Supply1, PLCNo = PLCNo });//创建入任务
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
                SendData(nwrite);
                SendData(nwrite);
                //bool r = MXParser.TrySendDataWithResult(nwrite);
            }


        }

        void ReadEquipLine()
        {
            // LWCS1_B00 滚筒>WCS请求调度AGV 读
            // LStatus_1 滚筒状态 
            // EStatus_1_1 设备1状态
            // EStatus_2_1 设备2状态
            // WAStatus_1_B500 WCS已调度AGV
            // AGVDisStatus_1 AGV调度状态 1未调度;2已调度
            if (_EquipCmdList == null)
                _EquipCmdList = this.BusinessLogic.EquipConfigerProvider.EquipConfigers.FindAll(x => x.PLCNo == PLCNo);
            #region 绑定作业标准书
            if (machcodes.Count == 0)
                foreach (var item in _EquipCmdList)
                {
                    machcodes.Add(item.MachCode.Trim());
                }
            List<tblProdCodeStructMachEntity> machstructcode = null;
            try
            {
                machstructcode = this.BusinessLogic.tblProdCodeStructMachProvider.GettblProdCodeStructMachs(null, machcodes.ToArray());
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("绑定作业标准书失败", ex);
                return;
            }
            foreach (var item in _EquipCmdList)
            {
                item.MachStructCode = machstructcode.FirstOrDefault(x => x.MachCode == item.MachCode.Trim());
            }
            #endregion


            var agvroutegroup = _EquipCmdList.GroupBy(x => x.AGVRoute.Trim());
            foreach (var items in agvroutegroup)
            {
                Neutrino ne = new Neutrino();
                ne.TheName = "ReadEquipLine";

                foreach (var item in items)
                {
                    #region MyRegion
                    ne.AddField(item.LWCS.Trim(), "0");// 1出拉空轮; 2入拉满轮 W00
                    ne.AddField(item.WAStatus.Trim(), "0");//是否已调度AGV 1，已接收滚筒出料请求；2，已接收入料至滚筒请求 光电 W400
                    ne.AddField(item.AGVDisStatus.Trim(), "0");//地面滚筒请求 AGVDisStatus W500
                    //ne.AddField(item.LStatus.Trim(), "0");//读取消标记1出拉空轮; 2入拉满轮 W100
                    //ne.AddField(item.EStatus2.Trim(), "0");//是否可取消标记1出拉空轮; 2入拉满轮

                    #endregion
                }
                Console.WriteLine($"开始读取PLC {items.Key}:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"));
                Tuple<bool, Neutrino> plcstatus = MXParser.ReadData(ne, true);
                #region MyRegion
                //By Song@2018.01.20
                if (!plcstatus.Item1)
                {
                    Console.WriteLine($"读取PLC {items.Key}失败:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"));
                    continue;
                }
                Console.WriteLine($"读取PLC {items.Key}成功:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"));
                #endregion
                ParserNe(items.ToList(), plcstatus.Item2);
            }
        }
        void ParserNe(List<EquipConfigerEntity> list, Neutrino plcNeutrino)
        {
            int inOrout = 0; int agvrunning = 0; int recive = 0; //int cancel = 0; int iscancel = 0;
            Neutrino nwrite = new Neutrino();
            nwrite.TheName = "WriteEquipLineStatus";
            foreach (var item in list)
            {//LStatus1
                inOrout = plcNeutrino.GetInt(item.LWCS.Trim());// 1出拉空轮; 2入拉满轮
                agvrunning = plcNeutrino.GetInt(item.WAStatus.Trim());//是否已调度AGV 1，已接收滚筒出料请求；2，已接收入料至滚筒请求 光电
                recive = plcNeutrino.GetInt(item.AGVDisStatus.Trim());//地面滚筒请求 AGVDisStatus
                #region 创建equiptsk
                if (agvrunning == 0 && inOrout != 0 && recive == 0)
                {//未调度
                    bool r = false;
                    #region MyRegion
                    if (item.MachStructCode == null)
                    {
                        string source = "该设备作业标准书未绑定:" + item.EquipList[0].EquipName.Trim() + "," + item.EquipList[1].EquipName.Trim();
                        Console.WriteLine(source);
                        if (!_warningMessageList.Contains(source))
                        {
                            _warningMessageList.Add(source);
                            this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = source, MsgLevel = 5, Source = "作业标准书未绑定" });
                        }
                        continue;
                    }
                    else if (item.MachStructCode.ProdCodeStructMark4 == null)
                    {
                        string source = "单丝作业标准书未绑定:" + item.MachStructCode.StructBarCode + "," + item.EquipList[0].EquipName.Trim() + "," + item.EquipList[1].EquipName.Trim();
                        Console.WriteLine(source);
                        if (!_warningMessageList.Contains(source))
                        {
                            _warningMessageList.Add(source);
                            this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = source, MsgLevel = 5, Source = "单丝作业标准书未绑定" });
                        }
                        continue;
                    }
                    #endregion
                    var equiptsk = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"[EquipContollerId]='{item.ControlID}' and Status IN(0,1,2,3,4,5,6,9,10)");

                    if (equiptsk == null || equiptsk.Count == 0)
                    {
                        #region MyRegion
                        if (inOrout == 1)
                        {//创建拉空轮任务
                            r = this.BusinessLogic.EquipTaskProvider.CreateEquipTask(new EquipTaskEntity() { Length = item.MachStructCode.ProdCodeStructMark4.ProdLength, Created = DateTime.Now, EquipContollerId = item.ControlID, ProductType = item.MachStructCode.ProdCodeStructMark4?.CName, Status = 0, TaskType = 1, TaskLevel = 6, PlantNo = PlantNo, Supply1 = item.MachStructCode.ProdCodeStructMark3.Supply1, PLCNo = PLCNo });//创建出任务
                            logger.InfoMethod("创建拉空轮任务, item.ControlID" + item.ControlID.ToString() + "result:" + r);
                        }
                        else if (inOrout == 2)//创建拉满轮任务
                        {
                            r = this.BusinessLogic.EquipTaskProvider.CreateEquipTask(new EquipTaskEntity() { Length = item.MachStructCode.ProdCodeStructMark4.ProdLength, Created = DateTime.Now, EquipContollerId = item.ControlID, ProductType = item.MachStructCode.ProdCodeStructMark4?.CName, Status = 0, TaskType = 2, TaskLevel = 5, PlantNo = PlantNo, Supply1 = item.MachStructCode.ProdCodeStructMark3.Supply1, PLCNo = PLCNo });//创建入任务
                            logger.InfoMethod("创建拉满轮任务, item.ControlID" + item.ControlID.ToString() + "result:" + r);
                        }
                        #endregion
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
                Thread.Sleep(1000);
                SendData(nwrite); //通知PLC已经创建任务
                Thread.Sleep(1000);
                SendData(nwrite); //通知PLC已经创建任务
                //bool r = MXParser.TrySendDataWithResult(nwrite);
            }
        }
    }


}
