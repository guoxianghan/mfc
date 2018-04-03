using Newtonsoft.Json;
using SNTON.Entities.DBTables.AGV;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.MES;
using SNTON.Entities.DBTables.Message;
using SNTON.Entities.DBTables.MidStorage;
using SNTON.Entities.DBTables.RobotArmTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC;
using VI.MFC.COM;
using VI.MFC.Logging;
using VI.MFC.Logic;
using VI.MFC.Utils;
using VI.MFC.Utils.ConfigBinder;
using static SNTON.Constants.SNTONConstants;

namespace SNTON.Components.ComLogic
{
    public class EquipLineLogic2 : ComLogic
    {
        //private VIThreadEx threadequiptask;
        private VIThreadEx thread_ReadEquipLine;
        private VIThreadEx thread_WriteEquipLine;
        private VIThreadEx thread_heartbeat;
        //private VIThreadEx thread_plctest;

        public EquipLineLogic2()
        {
            //threadequiptask = new VIThreadEx(CheckEquipTask, null, "Check AGV task Ready", 1000);
            thread_ReadEquipLine = new VIThreadEx(ReadEquipLine, null, "Read EquipLine Status", 1000);
            thread_WriteEquipLine = new VIThreadEx(WriteEquipLine, null, "thread for WriteEquipLine", 3000);
            thread_heartbeat = new VIThreadEx(heartbeat, null, "heartbeat", 1000);
            //thread_plctest = new VIThreadEx(PLCTest, null, "PLCTest", 4000);
        }
        protected override void StartInternal()
        {
            //thread_plctest.Start();
            thread_ReadEquipLine.Start();
            //thread_WriteEquipLine.Start();
            thread_heartbeat.Start();
            base.StartInternal();
        }
        public new static EquipLineLogic2 Create(XmlNode node)
        {
            EquipLineLogic2 a = new EquipLineLogic2();
            a.Init(node);
            return a;
        }
        [ConfigBoundProperty("AGVSequencer")]
#pragma warning disable 649
        private string sequencerproviderid;
#pragma warning restore 649
        private VI.MFC.Components.Sequencer.Sequencehandler.VLSequencer Sequencerid;
        public VI.MFC.Components.Sequencer.Sequencehandler.VLSequencer AGVSequencer
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref Sequencerid, sequencerproviderid);
                return Sequencerid;
            }
        }
        List<EquipConfigerEntity> equipcmd = null;
        List<string> _warningMessageList = new List<string>();
        /// <summary>
        /// 给光电
        /// </summary>
        void SendCreateAGV()
        {
            if (equipcmd == null)
                equipcmd = this.BusinessLogic.EquipConfigerProvider.GetEquipConfigerEntities(null).FindAll(x => x.PLCNo == PLCNo);
            //var equiptsk = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities($"[STATUS]=1 AND PlantNo={PlantNo} AND PLCNo={PLCNo}", null);
            //16收到中控系统的调车指令,小车分配成功
            var agvtsks = this.BusinessLogic.AGVTasksProvider.GetAGVTasks($"[STATUS] IN(8,16,19) AND [PlantNo]={PlantNo} and id>=15008 AND PLCNo={PLCNo}", null);
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
                    n.TheName = "SendAGVRunning";
                    #region 通知地面滚筒准备接收
                    var cmd = equipcmd.FirstOrDefault(x => x.EquipFlag.Trim() == task.EquipFlag.Trim());
                    sbequipname.Append(cmd.EquipName);
                    //Thread.Sleep(1000);

                    if (!n.FieldExists(cmd.WAStatus))
                        n.AddField(cmd.WAStatus, item.TaskType.ToString());
                    bool r = false;
                    for (int i = 0; i < 3; i++)
                    {
                        Thread.Sleep(1000);
                        r = MXParser.SendData(n, 3);
                    }
                    if (!r)
                    {
                        logger.WarnMethod("给光电失败,EquipTask:" + JsonConvert.SerializeObject(task));
                        continue;
                    }
                    else
                    {
                        logger.WarnMethod("给光电成功,EquipTask:" + JsonConvert.SerializeObject(task));
                    }
                    #endregion
                }
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



        List<EquipConfigerEntity> _EquipCmdList { get; set; } = null;
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
                logger.ErrorMethod("", ex);
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
                    ne.AddField(item.LStatus.Trim(), "0");//读取消标记1出拉空轮; 2入拉满轮 W100
                    ne.AddField(item.EStatus2.Trim(), "0");//是否可取消标记1出拉空轮; 2入拉满轮

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
            WriteEquipLine();
        }
        void ParserNe(List<EquipConfigerEntity> list, Neutrino ne)
        {
            int inOrout, agvrunning, recive, cancel, iscancel;

            foreach (var item in list)
            {//LStatus1
                inOrout = ne.GetInt(item.LWCS.Trim());// 1出拉空轮; 2入拉满轮 w100
                agvrunning = ne.GetInt(item.WAStatus.Trim());// 1，已接收滚筒出料请求；2，已接收入料至滚筒请求 w400光电 
                recive = ne.GetInt(item.AGVDisStatus.Trim());//已收到请求1,2有料 w500 
                cancel = ne.GetInt(item.LStatus.Trim());//读取消标记1出拉空轮; 2入拉满轮
                iscancel = ne.GetInt(item.EStatus2.Trim());//不可取消标记1出拉空轮; 2入拉满轮

                // 0初始化EquipTask,1创建AGVTask和龙门Task,2正在抓取,3,抓取完毕,4等待调度AGV,5已调度AGV,6AGV运行中(等待送料或拉空轮), 7任务完成(拉空论或满轮),8任务失败, 10库里单丝不够,9已通知地面滚筒创建任务,11可取消,准备取消, 12已取消,3无法取消,14无法取消,回复取消完毕
                var equiptsks = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"[EquipContollerId]='{item.ControlID}' and Status IN(0,1,2,3,4,5,6,9,10) AND PLCNo={PLCNo}");
                if (equiptsks == null) equiptsks = new List<EquipTaskEntity>();
                if (inOrout != 0 || cancel != 0 || agvrunning != 0 || recive != 0 || iscancel != 0)
                {

                }
                if (inOrout != 0)
                {
                    #region 判断是否绑定作业标准书
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
                    bool r = false;

                    #region 创建任务
                    if (equiptsks == null || equiptsks.Count == 0)
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
                        logger.InfoMethod("已创建该设备的任务,Created:" + equiptsks.FirstOrDefault().Created + ",EquipTaskID:" + equiptsks.FirstOrDefault().Id + ",EquipContollerId:" + equiptsks.FirstOrDefault().EquipContollerId);
                    }
                    #endregion

                    #region 把之前的不同类型的任务删除掉
                    if (equiptsks != null && equiptsks.FindAll(x => x.TaskType != inOrout).Count != 0)
                    {
                        var updatedelete = equiptsks.FindAll(x => x.TaskType != inOrout);
                        if (updatedelete != null && updatedelete.Count != 0)
                        {
                            updatedelete.ForEach(x => x.IsDeleted = 1);//把之前的任务删除掉
                            this.BusinessLogic.EquipTaskProvider.UpdateEntity(updatedelete, null);
                        }
                    }
                    #endregion
                }
                if (cancel != 0)
                {
                    if (equiptsks == null || equiptsks.Count == 0)
                    {
                        this.BusinessLogic.EquipTaskProvider.CreateEquipTask(new EquipTaskEntity() { Length = item.MachStructCode.ProdCodeStructMark4.ProdLength, Created = DateTime.Now, EquipContollerId = item.ControlID, ProductType = item.MachStructCode.ProdCodeStructMark4?.CName, Status = 0, TaskType = Convert.ToByte(cancel), TaskLevel = 5, PlantNo = PlantNo, Supply1 = item.MachStructCode.ProdCodeStructMark3.Supply1, PLCNo = PLCNo, IsCancel = 1 });//创建入任务
                        equiptsks = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"[EquipContollerId]='{item.ControlID}' and Status IN(0) AND PLCNo={PLCNo} AND  IsCancel =" + cancel);
                    }
                    #region 判断是否可取消
                    var equiptsk = equiptsks.FirstOrDefault(x => x.TaskType == cancel);
                    if (equiptsk != null && (equiptsk.Status == 0 || equiptsk.Status == 10 || equiptsk.Status == 9))
                    {
                        equiptsk.IsCancel = 1;//可取消
                        this.BusinessLogic.EquipTaskProvider.UpdateEntity(equiptsk, null);
                    }
                    if (equiptsk != null && (equiptsk.Status != 0 && equiptsk.Status != 10 && equiptsk.Status != 9))
                    {
                        equiptsk.IsCancel = 3;//不可取消
                        this.BusinessLogic.EquipTaskProvider.UpdateEntity(equiptsk, null);
                    }
                    if (equiptsk != null)
                    {
                        equiptsks.Remove(equiptsk);
                        if (equiptsks.Count != 0)
                        {
                            equiptsks.ForEach(x => x.IsDeleted = 1);//把之前的任务删除掉
                            this.BusinessLogic.EquipTaskProvider.UpdateEntity(equiptsks, null);
                        }
                    }
                    #endregion
                }
            }
        }

        void WriteEquipLine()
        {
            //写收到请求,收到任务
            //写创建任务,给光电
            //写是否可取消
            //var equiptsks = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"[PLCNO]='{this.PLCNo}' and Status IN(0,1,2,3,4,5,6,9,10,11,12,13)");
            var equiptsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities($"[PLCNO]='{this.PLCNo}' and Status IN(0,9) AND IsCancel IN (0,3,4)", null);
            if (equiptsks == null || equiptsks.Count == 0)
                equiptsks = new List<EquipTaskViewEntity>();
            if (_EquipCmdList == null)
                _EquipCmdList = this.BusinessLogic.EquipConfigerProvider.EquipConfigers.FindAll(x => x.PLCNo == PLCNo);
            var answer = equiptsks.FindAll(x => x.Status == 0);
            Neutrino nwrite = new Neutrino();
            nwrite.TheName = "WriteEquipLineStatus";//通知PLC已经收到请求
            if (answer != null && answer.Count != 0)
            {
                answer.ForEach(x =>
            {
                var e = _EquipCmdList.FirstOrDefault(y => y.ControlID == x.EquipContollerId);
                x.Status = 9;
                nwrite.AddField(e.AGVDisStatus.Trim(), x.TaskType.ToString());
            });
                SendData(nwrite);
                int answerresult = this.BusinessLogic.EquipTaskViewProvider.UpdateStatus(null, 9, (from i in answer select i.Id).ToArray());
            }
            equiptsks = equiptsks.OrderBy(x => x.Id).ToList();
            DateTime dt = DateTime.Now;
            var kong = equiptsks.FindAll(x => x.TaskType == 1 && x.Status == 9);
            var manlun = equiptsks.FindAll(x => x.TaskType == 2 && x.Status == 9);
            #region 创建拉空轮的任务 
            AGVTasksEntity agvout = null;
            var agvroutekongline = kong.GroupBy(x => x.AGVRoute.Trim());
            foreach (var item in agvroutekongline)
            { //创建拉空轮的AGV任务
                //创建成功后需要更改EquipTask状态
                //调度AGV成功之后更改EquipTask状态和写滚筒请求状态
                var equiptsk = item.Take(2).OrderBy(x => x.AStation).ToList();
                if (equiptsk.Count != 2)
                    continue;
                Guid g = Guid.NewGuid();
                agvout = new AGVTasksEntity() { Created = DateTime.Now, TaskType = 1, PlantNo = 3, Status = 2, TaskLevel = 6, TaskGuid = g, IsDeleted = 0, ProductType = equiptsk[0].ProductType };
                agvout.EquipIdListActual = equiptsk[0].EquipContollerId.ToString() + ";" + equiptsk[1].EquipContollerId.ToString();
                agvout.EquipIdListTarget = "";
                agvout.StorageArea = StorageArea;
                agvout.StorageLineNo = 0;
                equiptsk[0].Status = 1;
                equiptsk[0].TaskGuid = g;
                equiptsk[0].Updated = dt;
                equiptsk[1].Status = 1;
                equiptsk[1].TaskGuid = g;
                equiptsk[1].Updated = dt;
                agvout.TaskLevel = 6;
                agvout.PLCNo = PLCNo;
                bool r = this.BusinessLogic.SqlCommandProvider.EmptyAGVTask(equiptsk, agvout);
                if (r)
                    logger.ErrorMethod("更新拉空轮任务成功:" + JsonConvert.SerializeObject(equiptsk));
                else
                {
                    logger.ErrorMethod("更新拉空轮任务失败:" + JsonConvert.SerializeObject(equiptsk));
                }
            }

            #endregion
            CheckEquipTaskToCreate(manlun);
            /*
            //回复地面滚筒是否可取消
            */
            CancelSign();
            SendCreateAGV();
        }
        void CancelSign()
        {
            var equiptsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities($"[PLCNO]='{this.PLCNo}' AND IsCancel IN(0,1,3)", null);
            if (equiptsks == null)
                return;
            Neutrino nwrite = new Neutrino();
            nwrite.TheName = "WriteEquipLineCancel";//通知PLC已经收到请求
            foreach (var item in equiptsks)
            {
                var e = _EquipCmdList.FirstOrDefault(y => y.ControlID == item.EquipContollerId);
                if (item.Status == 10)
                {
                    item.IsDeleted = 1;
                    nwrite.AddField(e.EStatus1.Trim(), item.TaskType.ToString());
                }
                else
                {
                    switch (item.IsCancel)
                    {
                        case 1:
                            item.IsCancel = 2;
                            item.IsDeleted = 1;
                            nwrite.AddField(e.EStatus1.Trim(), item.TaskType.ToString());
                            break;
                        case 3:
                            item.IsCancel = 4;
                            nwrite.AddField(e.EStatus1.Trim(), item.TaskType.ToString());
                            break;
                        default:
                            break;
                    }
                }
            }
            bool iscancel = SendData(nwrite);
            int answerresult = this.BusinessLogic.EquipTaskViewProvider.Update(null, equiptsks.ToArray());
        }
        /// <summary>
        /// 检测是否可以创建送满轮任务
        /// </summary>
        /// <param name="list"></param>
        void CheckEquipTaskToCreate(List<EquipTaskViewEntity> list)
        {
            var agvrunningtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTasks("IsDeleted=0 and TaskType=2 AND [Status] IN(1,2,4,8)", null);
            var armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskStatus in(0,1,2,3)");//找到正在执行的ArmTask
            if (armtsks == null)
                armtsks = new List<RobotArmTaskEntity>();
            var group = list.GroupBy(x => x.AGVRoute.Trim());
            var routeStructcode = from i in list
                                  group new EquipTaskViewEntity() { Supply1 = i.Supply1.Trim(), AGVId = i.AGVId, AGVRoute = i.AGVRoute.Trim(), AGVStatus = i.AGVStatus, AStation = i.AStation, BStation = i.BStation, Created = i.Created, Deleted = i.Deleted, EquipContollerId = i.EquipContollerId, EquipFlag = i.EquipFlag, Id = i.Id, IsDeleted = i.IsDeleted, Length = i.Length, Length2 = i.Length2, PlantNo = i.PlantNo, PLCNo = i.PLCNo, ProductType = i.ProductType, Source = i.Source, Status = i.Status, StorageArea = i.StorageArea, Supply2 = i.Supply2, SupplyQty1 = i.SupplyQty1, SupplyQty2 = i.SupplyQty2, TaskGuid = i.TaskGuid, TaskLevel = i.TaskLevel, TaskType = i.TaskType, TitleProdName = i.TitleProdName, Updated = i.Updated }
                                  by new { i.AGVRoute, i.Supply1 }
                                  into t
                                  select t;
            foreach (var item in routeStructcode)
            {
                #region MyRegion 
                if (item.Count() < 2)
                    continue;
                bool iscreated = false;
                int result = 0;
                var equiptsks = item.OrderBy(x => x.Id).Take(2).ToList();
                var supply1 = equiptsks.GroupBy(x => x.Supply1.Trim()).ToList()[0];
                for (short i = 1; i <= 3; i++)
                {
                    ///直通口检测是否可以创建送满轮任务
                    //agvrunningtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTasks("IsDeleted=0 and TaskType=2 AND [Status] IN(1,2,3,4,8)", null);
                    //if (agvrunningtsk.FindAll(x => x.StorageArea == i && x.StorageLineNo == 2).Count >= 2)
                    //    continue;//直通线缓存满
                    iscreated = InStoreHasSpools(equiptsks.ToList(), i);
                    if (iscreated)
                    {
                        equiptsks.ToList().ForEach(x => x.Status = 4);
                        int count = this.BusinessLogic.EquipTaskViewProvider.Update(null, equiptsks.ToArray());
                        if (count != 0)
                            break;
                    }
                }
                if (!iscreated)
                {
                    for (short storeno = 1; storeno <= 3; storeno++)
                    {
                        armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskStatus in(0,1,2,3)");//找到正在执行的ArmTask
                        agvrunningtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTasks($"IsDeleted=0 and TaskType=2 AND [Status] IN(1,2,4,8,9) AND PLCNo={PLCNo}", null);
                        if (agvrunningtsk == null) agvrunningtsk = new List<AGVTasksEntity>();
                        #region MyRegion
                        if (armtsks.FindAll(x => x.StorageArea == storeno).Count >= 3)
                            continue;//有正在执行的龙门任务
                        if (agvrunningtsk.FindAll(x => x.StorageArea == storeno && x.StorageLineNo == 1).Count >= 3)
                            continue;//出库线体满
                                     ///从暂存库创建出库任务
                        result = NewMethodLR(storeno, supply1, 0, 1);
                        if (result == 1)
                        {
                            equiptsks.ToList().ForEach(x => x.Status = 1);
                            iscreated = true;
                            this.BusinessLogic.EquipTaskViewProvider.Update(null, equiptsks.ToArray());
                            break;
                        }
                        else
                        {
                            //不再显示库里单丝不足
                            if (storeno == 2 || storeno == 3)
                            {
                                equiptsks.ToList().ForEach(x => { x.Status = 10; x.IsCancel = 1; });
                                this.BusinessLogic.EquipTaskViewProvider.Update(null, equiptsks.ToArray());
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// 直通口是否有准备好待接收的单丝
        /// </summary>
        /// <param name="equiptsk"></param>
        /// <param name="storeageno"></param>
        /// <returns></returns>
        bool InStoreHasSpools(List<EquipTaskViewEntity> equiptsks, int storeageno)
        {
            equiptsks = equiptsks.OrderBy(x => x.AStation).ToList();
            var equiptsk = equiptsks[0];
            var list = this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.GetInStoreToOutStoreSpool(storeageno, this.PlantNo, null);
            if (list == null || list.Count == 0)
                return false;
            //var outstore = list.FirstOrDefault(x => x.Status == 3);
            //if (outstore == null)
            //    return false;
            var equip = this.BusinessLogic.EquipConfigerProvider.EquipConfigers.FirstOrDefault(x => x.ControlID == equiptsk.EquipContollerId);
            var machstructcode = this.BusinessLogic.tblProdCodeStructMachProvider.GettblProdCodeStructMachs(null, equip.MachCode.Trim());
            if (machstructcode == null || machstructcode.Count == 0)
                return false;
            tblProdCodeStructMachEntity prod = machstructcode[0];
            if (prod.ProdCodeStructMark4 == null)
            {
                return false;
            }
            var instore = list.FindAll(x => x.Status == 3 && x.StructBarCode.Trim() == prod.ProdCodeStructMark4.StructBarCode.Trim());
            if (instore == null)
                return false;
            instore.ForEach(x => x.Status = 8);//等待申请调度AGV
            int count = this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.UpdateEntity(null, instore.ToArray());
            if (count == 0)
                return false;
            var agvtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTask($"TaskGuid in ('{instore[0].Guid.ToString()}')");
            if (agvtsk != null)
            {
                agvtsk.Status = 2;
                agvtsk.StorageLineNo = 2;
                agvtsk.EquipIdListActual = equiptsks[0].EquipContollerId.ToString() + ";" + equiptsks[1].EquipContollerId.ToString();
                //agvtsk.EquipIdListTarget = TaskConfig.AGVStation(storeageno, 2);
                agvtsk.Updated = DateTime.Now;
                equiptsks.ForEach(x => x.TaskGuid = agvtsk.TaskGuid);
                equiptsks.ForEach(x => x.Status = 4);
                bool r = this.BusinessLogic.AGVTasksProvider.UpdateEntity(agvtsk);
                return r;
            }
            else return false;
        }

        /// <summary>
        /// 检测是否有能够创建AGV任务的EquipTask
        /// 1创建任务;0没有创建任务,-1库里单丝不够
        /// </summary>
        /// <param name="storageno"></param>
        /// <param name="supply1">按照单丝标准书分组的单组设备任务</param>
        /// <returns>1创建任务;0没有创建任务</returns>
        private int NewMethodLR(short storageno, IGrouping<string, EquipTaskViewEntity> supply1, int inlinecount, int lineno, params char[] lrs)
        {
            //if (seq == 1 || seq == 255)
            //    seq = ran.Next(1, 255);
            //else seq++; 
            int seq = Convert.ToInt32(System.Text.Encoding.Default.GetString(this.AGVSequencer.GetNextSequence()));
            EquipTaskViewEntity exequiptsk = supply1.FirstOrDefault();
            //判断lr
            var tskconfig = TaskConfig.GetEnoughAGVEquipCount(exequiptsk.ProductType.Trim());//8 / 12
            if (tskconfig.Item1 == 0)
            {
                this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = exequiptsk.ProductType.Trim(), MsgLevel = 6, Source = "未知的单丝型号" });
                return 0;
            }
            var creaequptsk = supply1.OrderBy(x => x.Id).Take(tskconfig.Item2).OrderBy(x => x.AStation).ToList();
            if (creaequptsk.Count() < tskconfig.Item2)
            {
                //logger.InfoMethod("没有足够的叫料设备");
                return 0;
            }
            /*
                tskconfig.Item1=8 LR 1:1
                tskconfig.Item1=12 LR 2:1
            */
            int l = lrs.ToList().FindAll(x => x == 'L').Count;
            int r = lrs.ToList().FindAll(x => x == 'R').Count;
            int needstoeageno = tskconfig.Item1 - inlinecount;
            //var mids = this.BusinessLogic.MidStorageSpoolsProvider.GetMidStorageByArea(storageno, null);
            var mids = this.BusinessLogic.MidStorageSpoolsProvider.GetMidStorages($"IsOccupied=1 AND StructBarCode='{supply1.Key.Trim()}' AND  StorageArea={storageno}", null);
            if (mids == null || mids.Count == 0)
            {
                //logger.WarnMethod("没有该长度的单丝");
                return -1;
            }
            //mids = mids.FindAll(x => x.IsOccupied == 1 && x.Spool.StructBarCode != null && !string.IsNullOrEmpty(x.Spool.StructBarCode) && x.Spool.StructBarCode.Trim() == supply1.Key.Trim());
            var needlspools = mids.OrderBy(x => x.Updated).ToList().FindAll(x => x.BobbinNo == 'L').Take(tskconfig.Item4 - l).ToList();
            var needrspools = mids.OrderBy(x => x.Updated).ToList().FindAll(x => x.BobbinNo == 'R').Take(tskconfig.Item5 - r).ToList();
            if (tskconfig.Item4 > needlspools.Count())
            {
                StringBuilder sb = new StringBuilder();
                supply1.ToList().ForEach(x => sb.Append(x.EquipContollerId + ","));
                this.BusinessLogic.EquipTaskProvider.UpdateStatus(10, null, supply1.ToList().Select(x => x.Id).ToArray());
                //logger.InfoMethod(storageno + "号暂存库中的\"L\"单丝不满足出库数量,单丝作业标准书:" + supply1.Key.Trim() + ",地面滚筒id:" + sb.ToString().Trim(','));
                /*
                更新设备任务状态10,料不够
                */
                return -1;
            }
            if (tskconfig.Item5 > needrspools.Count())
            {
                StringBuilder sb = new StringBuilder();
                supply1.ToList().ForEach(x => sb.Append(x.EquipContollerId + ","));
                this.BusinessLogic.EquipTaskProvider.UpdateStatus(10, null, supply1.ToList().Select(x => x.Id).ToArray());
                //logger.InfoMethod(storageno + "号暂存库中的\"R\"单丝不满足出库数量,单丝作业标准书:" + supply1.Key.Trim() + ",地面滚筒id:" + sb.ToString().Trim(','));

                /*
                更新设备任务状态10,料不够
                */
                return -1;
            }
            if (needstoeageno > mids.Count)
            {
                StringBuilder sb = new StringBuilder();
                supply1.ToList().ForEach(x => sb.Append(x.EquipContollerId + ","));
                this.BusinessLogic.EquipTaskProvider.UpdateStatus(10, null, supply1.ToList().Select(x => x.Id).ToArray());
                //logger.InfoMethod(storageno + "号暂存库中的\"R\"单丝不满足出库数量,单丝作业标准书:" + supply1.Key.Trim() + ",地面滚筒id:" + sb.ToString().Trim(','));
                /*
                更新设备任务状态10,料不够
                */
                return -1;
            }
            //暂存库单丝轮数量足够创建一车任务并且需要该类型单丝轮的设备也满一车
            //创建龙门Task和AGVTask
            //更新EquipTask状态
            //更新库位状态
            var guid = Guid.NewGuid();
            //通过GUID标记一个龙门任务单元
            DateTime createtime = DateTime.Now;
            RobotArmTaskEntity armtsk = null;
            List<RobotArmTaskEntity> listarmtsk = new List<RobotArmTaskEntity>();
            var agvtsk = new AGVTasksEntity() { Created = createtime, SeqNo = seq, TaskGuid = guid, PlantNo = PlantNo, ProductType = exequiptsk.ProductType, Status = 0, TaskLevel = 5, PLCNo = PLCNo };
            int seqno = 0;
            string ctrlid = creaequptsk[0].EquipContollerId + ";" + creaequptsk[1].EquipContollerId;
            foreach (var item in needlspools)
            {
                item.IsOccupied = 4;
                armtsk = CreateOutStoreArmTask(storageno, lineno, tskconfig, guid, createtime, item, seq);
                armtsk.SeqNo = seqno++;
                armtsk.EquipControllerId = "0";
                //armtsk.EquipControllerId = creaequptsk[0].EquipContollerId + "," + creaequptsk[1].EquipContollerId;
                listarmtsk.Add(armtsk);
            }
            foreach (var item in needrspools)
            {
                item.IsOccupied = 4;
                armtsk = CreateOutStoreArmTask(storageno, lineno, tskconfig, guid, createtime, item, seq);
                armtsk.SeqNo = seqno++;
                armtsk.EquipControllerId = "0";
                //armtsk.EquipControllerId = creaequptsk[0].EquipContollerId + "," + creaequptsk[1].EquipContollerId;
                listarmtsk.Add(armtsk);
            }


            List<string> sqlcmds = new List<string>();
            StringBuilder equiptsksql = new StringBuilder($"UPDATE SNTON.EquipTask SET [Status]=1,TaskGuid='{guid}',Updated='{createtime.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE ID IN(");
            foreach (var equtsk in creaequptsk)
            {
                equtsk.Status = 1;
                equtsk.TaskGuid = guid;
                equtsk.Updated = createtime;
                equiptsksql.Append(equtsk.Id.ToString() + ",");
                agvtsk.EquipIdListActual = agvtsk.EquipIdListActual + ";" + equtsk.EquipContollerId.ToString();
            }
            sqlcmds.Add(equiptsksql.ToString().Trim(',') + ")");

            agvtsk.TaskType = 2;
            if (needstoeageno == 0)
                agvtsk.Status = 2;
            #region 更新EquipTask 更新暂存库位置状态 创建AGVTask 创建龙门Task
            agvtsk.StorageArea = storageno;
            agvtsk.StorageLineNo = lineno;
            agvtsk.EquipIdListActual = agvtsk.EquipIdListActual.Trim(';');
            agvtsk.EquipIdListTarget = TaskConfig.AGVStation(storageno, lineno);

            agvtsk.PLCNo = this.PLCNo;
            List<MidStorageSpoolsEntity> updamid = new List<MidStorageSpoolsEntity>();
            updamid.AddRange(needlspools);
            updamid.AddRange(needrspools);
            creaequptsk.ForEach(x => x.Status = 1);
            logger.InfoMethod("############################################################################################################");
            bool result = this.BusinessLogic.SqlCommandProvider.OutStoreageTask(creaequptsk, updamid, agvtsk, listarmtsk, null);
            logger.InfoMethod("###创建龙门出库任务:" + result + "," + guid.ToString());
            //this.BusinessLogic.EquipTaskViewProvider.Update(null, creaequptsk.ToArray());
            //this.BusinessLogic.MidStorageSpoolsProvider.UpdateMidStore(null, needlspools.ToArray());
            //this.BusinessLogic.MidStorageSpoolsProvider.UpdateMidStore(null, needrspools.ToArray());
            //this.BusinessLogic.AGVTasksProvider.CreateAGVTask(agvtsk, null); //同时创建AGVTask   
            //this.BusinessLogic.RobotArmTaskProvider.InsertArmTask(listarmtsk, null);
            //logger.InfoMethod(JsonConvert.SerializeObject(sqlcmds));
            logger.InfoMethod("############################################################################################################");
            #endregion
            if (result)
                return 1;//一次只执行一个龙门Task  
            else return 0;
        }
        private RobotArmTaskEntity CreateOutStoreArmTask(short storageno, int lineno, Tuple<int, int, int, int, int> tskconfig, Guid guid, DateTime createtime, MidStorageSpoolsEntity spool, int seq)
        {
            RobotArmTaskEntity armtsk = new RobotArmTaskEntity();
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
            armtsk.CName = spool.CName;
            if (lineno == 1)
                armtsk.TaskType = 0;
            else
                armtsk.TaskType = 1;
            armtsk.SpoolStatus = 0;
            //armtsk.EquipControllerId = 1;
            armtsk.StorageArea = storageno;
            armtsk.WhoolBarCode = spool.Spool.FdTagNo.Trim();
            armtsk.ProductType = tskconfig.Item3.ToString();
            spool.Updated = createtime;
            spool.IsOccupied = 4;
            return armtsk;
        }

    }
}
