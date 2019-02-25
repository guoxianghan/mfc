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
using System.Xml;

namespace SNTON.Components.ComLogic
{
    public class AGVLogic : ComLogic
    {
        private VIThreadEx thread_CheckAGVTaskSend;
        //private VIThreadEx thread_CreateAGVTask;
        private VIThreadEx thread_AliveReq;
        private VIThreadEx thread_AGVStatusReq;

        public new static AGVLogic Create(XmlNode node)
        {
            AGVLogic a = new AGVLogic();
            a.Init(node);
            return a;
        }
        public AGVLogic()
        {
            thread_CheckAGVTaskSend = new VIThreadEx(CheckAGVTaskSend, null, "Check AGV task Send", 2000);
            //To avoid the compiling error
            //By Song@2018.01.14.
            //thread_CreateAGVTask = new VIThreadEx(CreateAGVTask, null, "Create AGV task Send", 2000);
            thread_AliveReq = new VIThreadEx(AliveReq, null, "send AliveAck", 2000);
            thread_AGVStatusReq = new VIThreadEx(AGVStatusReq, null, "send AGVStatusReq", 1000);
        }
        protected override void StartInternal()
        {
            base.StartInternal();
            thread_AliveReq.Start();
            //thread_AGVStatusReq.Start();
            thread_CheckAGVTaskSend.Start();
            //thread_CreateAGVTask.Start(); 
        }

        private void CheckAGVTaskSend()
        {
            if (this.BusinessLogic.AGVBYSStatusProvider == null)
            {

            }

            //Neutrino re = new Neutrino();
            //re.AddField("TELEGRAMID", SNTONAGVCommunicationProtocol.AGVCmdExeReportAck.PadRight(20).ToString());
            //re.AddField("Action", "1");
            //re.AddField("TaskNo", "123");
            //Send(re);
            //return;
            var tmp = BusinessLogic.AGVTasksProvider.GetAGVTasks($" (Status in (2) and isdeleted=0 ) OR (IsDeleted=0 AND [Status]=4 AND Updated<='{DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss")}')");
            //tmp = BusinessLogic.AGVTasksProvider.GetAGVTasks("ID=32813");
            //HLCallCmd(tmp[0]);
            //return;
            if (tmp == null)
                return;
            tmp = tmp.OrderByDescending(x => x.TaskType).ToList();
            foreach (var item in tmp)
            {
                if (item.Status == (byte)AGVTaskStatus.Ready || item.Status == 4)
                {
                    //var eqtsk = BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"TaskGuid='{item.TaskGuid.ToString()}'", null);
                    if (item.TaskNo == 0)
                        item.TaskNo = Convert.ToInt64(this.Sequencer.GetNextSequenceNo());
                    item.Status = (byte)AGVTaskStatus.Sent;
                    item.Updated = DateTime.Now;
                    bool r = BusinessLogic.AGVTasksProvider.UpdateEntity(item, null);
                    if (r)
                    {
                        HLCallCmd(item);
                        logger.InfoMethod("向小车发送Sent指令成功" + Environment.NewLine + JsonConvert.SerializeObject(item));
                    }
                    else
                        logger.InfoMethod("向小车发送Sent指令失败" + Environment.NewLine + JsonConvert.SerializeObject(item));
                }
            }
        }
        /// <summary>
        /// 检测设备任务,创建对应的AGVTask,只创建拉走轮子的AGVTask
        /// </summary>
        [Obsolete("弃用的方法,改用EquipTaskLogic的InitRobotAGVTask方法代替")]
        public void CreateAGVTask()
        {
            #region 创建拉空轮任务
            var equiptask = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere("Status='0' and TaskType='1'");
            if (equiptask == null || equiptask.Count == 0)
                return;
            var grouptasktype = equiptask.GroupBy(x => x.TaskType);
            foreach (var items in grouptasktype)
            {
                if (items.Key == 1)
                {//出,创建拉走轮子的AGVtask
                    foreach (var item in items)
                    {
                        BusinessLogic.AGVTasksProvider.CreateAGVTask(new AGVTasksEntity() { Created = DateTime.Now, EquipIdListTarget = item.EquipContollerId.ToString(), TaskLevel = 2, Status = (byte)AGVTaskStatus.Ready, TaskType = 0, StorageLineNo = 0, StorageArea = StorageArea });
                        item.Status = 1;
                        this.BusinessLogic.EquipTaskProvider.UpdateEntity(item);
                    }
                }
                else
                {//入

                }
            }
            #endregion
            //To avoid compiling error
            //By Song@2018.01.14
            //return;
            #region 从AGVTask表中检测有没有正在抓取的任务或者已经抓取完的任务
            var agvtsks = this.BusinessLogic.AGVTasksProvider.GetAGVTasks("Status IN(0,1)", null);
            var agvtskrunning = agvtsks.FirstOrDefault(x => x.Status == 1);

            agvtsks = agvtsks.OrderByDescending(x => x.TaskLevel).ToList();//找优先级高的
            var agvtskcreate = agvtsks.FirstOrDefault(x => x.Status == 0);

            if (agvtskrunning != null)
            {//有正在执行抓轮子的agvtsk,判断龙门抓取完没,线体准备好没,如果线体准备好,则改状态为ready
                //var robotarmtsk = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"guid='{agvtskrunning.TaskGuid.ToString()}'", null);
                //if (robotarmtsk[0].TaskStatus == 4)
                //{
                //    agvtskrunning.Status = 2;//改状态为ready
                //    try
                //    {
                //        this.BusinessLogic.AGVTasksProvider.UpdateEntity(agvtskrunning);
                //    }
                //    catch (Exception e)
                //    {
                //        logger.ErrorMethod("filed to update agvtsk status", e);
                //    }
                //}
            }
            else if (agvtskcreate != null)
            {
                agvtskcreate.Status = 1;
                try
                {
                    this.BusinessLogic.AGVTasksProvider.UpdateEntity(agvtskcreate);
                }
                catch (Exception e)
                {
                    logger.ErrorMethod("filed to update agvtsk status", e);
                }
            }
            #endregion
        }

        /// <summary>
        /// 上位机调度系统根据现场设备的指令调度AGV执行相应的运料，送料和拉空轮等指令
        /// </summary>
        /// <param name="item"></param>
        private void HLCallCmd(AGVTasksEntity item)
        {
            Neutrino re = new Neutrino();
            re.AddField("TELEGRAMID", SNTONAGVCommunicationProtocol.HLCallCmd.PadRight(20).ToString());
            re.AddField("PlantNo", PlantNo.ToString());
            re.AddField("TaskType", item.TaskType.ToString());
            re.AddField("SrcLocations", item.EquipIdListTarget);
            re.AddField("DestLocations", item.EquipIdListActual);
            re.AddField("TaskNo", Convert.ToString(item.TaskNo, 16));
            Send(re);
        }



        /// <summary>
        /// 处理接收的消息
        /// </summary>
        /// <param name="neutrino"></param>
        /// <returns></returns>
        protected override bool ProcessMessage(Neutrino neutrino)
        {
            string cmd = neutrino.TheName.Trim();
            var keys = neutrino.GetAllKeys();
            string TELEGRAMID = neutrino.GetField("TELEGRAMID").Trim();
            string SEQUENCE = neutrino.GetField("SEQUENCE");
            Console.WriteLine("cmd:" + cmd + ",TELEGRAMID:" + TELEGRAMID);
            switch (cmd)
            {
                case SNTONAGVCommunicationProtocol.AGVCmdExe:
                    AGVCmdExe(neutrino);
                    break;
                case SNTONAGVCommunicationProtocol.AGVCmdExeReport:
                    AGVCmdExeReport(neutrino);
                    break;
                case SNTONAGVCommunicationProtocol.AGVInfo:
                    AGVInfo(neutrino);
                    break;
                case SNTONAGVCommunicationProtocol.AGVRoute:
                    //SaveAGVRoute(neutrino);
                    break;
                case SNTONAGVCommunicationProtocol.AGVStatus:
                    SaveAgvStatues(neutrino);
                    break;
                case SNTONAGVCommunicationProtocol.AliveAck:
                    AliveAck(neutrino);
                    break;
                case SNTONAGVCommunicationProtocol.HLCallCmdAck:
                    HLCallCmdAck(neutrino);
                    break;
                default:
                    Console.WriteLine("未知的响应指令,cmd:" + cmd + ",TELEGRAMID:" + TELEGRAMID);
                    logger.WarnMethod("未知的响应指令,cmd:" + cmd + ",TELEGRAMID:" + TELEGRAMID);
                    break;
            }
            return base.ProcessMessage(neutrino);
        }

        /// <summary>
        /// 心跳电文响应
        /// </summary>
        /// <param name="neutrino"></param>
        private void AliveAck(Neutrino neutrino)
        {
            // throw new NotImplementedException();
        }
        /// <summary>
        /// 心跳电文请求
        /// </summary>
        private void AliveReq()
        {
            Neutrino neutrino = new Neutrino();
            neutrino.AddField("TELEGRAMID", SNTONAGVCommunicationProtocol.AliveReq.PadRight(20));
            Send(neutrino);
            AGVRoute();

        }
        void AGVRoute()
        {
            var routelocation = this.BusinessLogic.AGVBYSStatusProvider._AGVBYSStatusCache;
            var weblocation = this.BusinessLogic.Agv_three_configProvider._AllAgv_three_config;
            agv_three_configEntity act = new agv_three_configEntity();
            foreach (var item in routelocation)
            {
                float x = item.LocationX;
                float y = item.LocationY;
                //if (x != 0 || y != 0)
                //{
                //    weblocation.ForEach(c =>
                //    {
                //        c.Dev_x = Math.Abs(x - c.fac_X);
                //        c.Dev_y = Math.Abs(y - c.fac_Y);
                //    });
                //    var weblocationx = weblocation.OrderBy(c => c.StDev).Take(20);
                //    act = weblocationx.FirstOrDefault();
                //}
                //else
                //    act = this.BusinessLogic.Agv_three_configProvider._originLocation;
                if (this.BusinessLogic.AGVRouteProvider.RealTimeAGVRute.ContainsKey(item.AGVID))
                {
                    this.BusinessLogic.AGVRouteProvider.RealTimeAGVRute[item.AGVID] = new AGVRouteEntity() { AGVId = item.AGVID, agv_id = item.AGVID, Created = DateTime.Now, fac_x = act.fac_x, fac_y = act.fac_y, X = x, Y = y, Status = item.Status };
                }
                else
                {
                    this.BusinessLogic.AGVRouteProvider.RealTimeAGVRute.Add(item.AGVID, new AGVRouteEntity() { AGVId = item.AGVID, agv_id = act.agv_id, Created = DateTime.Now, fac_x = act.fac_x, fac_y = act.fac_y, X = x, Y = y, Status= item.Status });
                }
            }
        }
        private void AGVStatusReq()
        {//100000000000000002
            Neutrino re = new Neutrino();
            re.AddField("TELEGRAMID", SNTONAGVCommunicationProtocol.AGVStatusReq.PadRight(20));
            re.AddField("PlantNo", "3");

            //Neutrino re = new Neutrino();
            //re.AddField("TELEGRAMID", SNTONAGVCommunicationProtocol.AGVCmdExeAck.PadRight(20).ToString());
            //re.AddField("TaskNo", "444");
            //re.AddField("Action", "1");
            Send(re);
        }

        /// <summary>
        /// 当AGV系统收到上位机调度系统的调度指令后，分析完毕指令，将分析结果上传给AGV时，触发该消息
        /// </summary>
        /// <param name="neutrino"></param>
        private void HLCallCmdAck(Neutrino neutrino)
        {
            var keys = neutrino.GetAllKeys();
            long TaskNo = long.Parse(neutrino.GetField("TaskNo"), System.Globalization.NumberStyles.AllowHexSpecifier);
            byte Ack = Convert.ToByte(neutrino.GetField("Ack"));
            try
            {
                byte status = 0;
                if (Ack == 1)
                {
                    status = (byte)SNTONConstants.AGVTaskStatus.Received;//收到请求,等待调度AGV
                    logger.InfoMethod($"AGV中控收到请求,TaskNo:" + TaskNo + ",status:" + status);
                    var agvtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTaskEntityByTaskNo(TaskNo, null);
                    agvtsk.Status = status;
                    bool r = BusinessLogic.AGVTasksProvider.UpdateStatus(agvtsk.Id, status);
                    if (r) { logger.InfoMethod("AGV中控收到请求后修改状态成功,TaskNo:" + TaskNo + ",status:" + status); }
                    else { logger.InfoMethod("AGV中控收到请求后修改状态失败,TaskNo:" + TaskNo + ",status:" + status); }
                    var equiptsks = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntityNotDeleted($"TaskGuid='{agvtsk.TaskGuid.ToString()}'", null);
                    if (equiptsks != null)
                    {
                        foreach (var item in equiptsks)
                        {
                            item.Status = 5;//4等待申请,5已申请
                            item.Updated = DateTime.Now;
                        }
                        this.BusinessLogic.EquipTaskProvider.UpdateEntity(equiptsks, null);
                    }
                    /*
                    等待调度AGV
                    equiptasks .status=4
                    */
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("File to HLCallCmdAck,neutrino is " + JsonConvert.SerializeObject(neutrino), e, "HLCallCmdAck");
            }
        }

        /// <summary>
        /// AGV系统执行完上位机调度系统的指令后，将报告结果发送给上位机调度系统时，触发该消息
        /// </summary>
        /// <param name="neutrino"></param>
        private void AGVCmdExeReport(Neutrino neutrino)
        {
            Neutrino re = new Neutrino();
            re.AddField("TELEGRAMID", SNTONAGVCommunicationProtocol.AGVCmdExeReportAck.PadRight(20).ToString());
            long TaskNo = long.Parse(neutrino.GetField("TaskNo"), System.Globalization.NumberStyles.AllowHexSpecifier);
            int PlantNo = Convert.ToInt32(neutrino.GetField("PlantNo"));
            short agvid = Convert.ToInt16(neutrino.GetField("AGVID"));
            short TaskType = Convert.ToInt16(neutrino.GetField("TaskType"));
            string SrcLocations = neutrino.GetField("SrcLocations");
            string DestLocations = neutrino.GetField("DestLocations");
            try
            {
                var agvtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTaskEntityByTaskNo(TaskNo, null);
                agvtsk.EquipIdListActual = SrcLocations;
                agvtsk.EquipIdListTarget = DestLocations;
                agvtsk.Updated = DateTime.Now;
                agvtsk.Status = (byte)AGVTaskStatus.Finished;
                var equiptsks = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntityNotDeleted($"TaskGuid='{agvtsk.TaskGuid.ToString()}'", null);
                foreach (var item in equiptsks)
                {
                    item.Status = 7;
                    item.Updated = DateTime.Now;
                }
                if (equiptsks != null)
                    this.BusinessLogic.EquipTaskProvider.UpdateEntity(equiptsks, null);
                logger.InfoMethod("AGV系统反馈,任务完成,更新设备任务状态成功,guid:" + agvtsk.TaskGuid.ToString());
                //var robottsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskGuid='{agvtsk.TaskGuid.ToString()}'", null);

                this.BusinessLogic.AGVTasksProvider.UpdateEntity(agvtsk, null);
                logger.InfoMethod("AGV系统反馈,任务完成,更新AGV任务状态成功,guid:" + agvtsk.TaskGuid.ToString());
                re.AddField("Action", "1");
            }
            catch (Exception e)
            {
                logger.ErrorMethod("File to AGVCmdExeReport,neutrino is " + JsonConvert.SerializeObject(neutrino), e);
                re.AddField("Action", "0");
            }
            Send(re);
        }

        /// <summary>
        /// AGV 系统通知上位机调度系统关于调度指令的运行情况
        /// </summary>
        private void AGVCmdExe(Neutrino neutrino)
        {
            long TaskNo = long.Parse(neutrino.GetField("TaskNo"), System.Globalization.NumberStyles.HexNumber);
            byte Action = Convert.ToByte(neutrino.GetField("Action"));
            int agvid = 0;
            var dt = DateTime.Now;
            var agvtsk = BusinessLogic.AGVTasksProvider.GetAGVTaskEntityByTaskNo(TaskNo, null);
            agvtsk.Updated = dt;
            agvid = neutrino.GetIntOrDefault("AGVID");
            var eqtsk = BusinessLogic.EquipTaskProvider.GetEquipTaskEntityNotDeleted($"TaskGuid='{agvtsk.TaskGuid.ToString()}'", null);
            var outstoreagespools = this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.GetInStoreToOutStoreSpoolEntity($"GUID='{agvtsk.TaskGuid.ToString()}'", null);
            byte status = 0;
            switch (Action)
            {
                case 0://未知状态
                    break;
                case 1://开始执行
                    status = (byte)AGVTaskStatus.Running;
                    if (agvtsk.Status >= status)
                    {
                        //logger.InfoMethod($"是哪个sb重发了指令,小车id:{agvid},开始执行:guid:" + agvtsk.TaskGuid.ToString() + ",status:" + status + ",agvtsk.Status:" + agvtsk.Status);
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = dt, MsgContent = $"小车指令重复,小车id:{agvid},开始执行:guid:" + agvtsk.TaskGuid.ToString() + ",status:" + status + ",agvtsk.Status:" + agvtsk.Status, Source = "AGV指令重复", MsgLevel = 7 });
                        //return;
                    }
                    eqtsk?.ForEach(x => { x.Status = 6; x.Updated = dt; });
                    agvtsk.AGVId = Convert.ToInt16(agvid);
                    logger.InfoMethod($"AGV调度成功,{JsonConvert.SerializeObject(agvtsk)}");
                    #region 更新直通口任务状态 
                    if (outstoreagespools != null)
                    {
                        outstoreagespools.ForEach(x => { x.Status = 16; x.Updated = dt; });
                        this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.UpdateEntity(null, outstoreagespools.ToArray());
                    }
                    #endregion
                    break;
                case 2://执行结束
                    status = (byte)AGVTaskStatus.Finished;
                    if (agvtsk.Status >= status)
                    {
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = dt, MsgContent = $"小车指令重复,小车id:{agvid},任务完成:guid:" + agvtsk.TaskGuid.ToString() + ",status:" + status + ",agvtsk.Status:" + agvtsk.Status, Source = "AGV指令重复", MsgLevel = 7, MidStoreage = agvtsk.StorageArea });
                    }
                    this.BusinessLogic.AGVRouteProvider.DeleteAGVRoute(agvid, null);
                    eqtsk?.ForEach(x => { x.Status = 7; x.Updated = dt; });
                    #region 任务完成后,删除直通口记录 
                    if (outstoreagespools != null)
                    {
                        outstoreagespools.ForEach(x => { x.Status = 128; x.Updated = dt; });
                        this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.UpdateEntity(null, outstoreagespools.ToArray());
                    }
                    #endregion
                    logger.InfoMethod($"AGV任务完成,{JsonConvert.SerializeObject(agvtsk)}");
                    break;
                default:
                    logger.WarnMethod("未知的AGV调度指令,AGVCmdExe->Action:" + Action);
                    return;

            }

            Neutrino re = new Neutrino();
            re.AddField("TELEGRAMID", SNTONAGVCommunicationProtocol.AGVCmdExeAck.PadRight(20).ToString());
            re.AddField("TaskNo", Convert.ToString(agvtsk.TaskNo, 16));
            try
            {
                agvtsk.Status = status;
                agvtsk.AGVId = Convert.ToInt16(agvid);
                if (eqtsk != null)
                    BusinessLogic.EquipTaskProvider.UpdateEntity(eqtsk, null);
                bool r = BusinessLogic.AGVTasksProvider.UpdateEntity(agvtsk, null);
                if (!r) r = BusinessLogic.AGVTasksProvider.UpdateEntity(agvtsk, null);
                if (r)
                {
                    logger.InfoMethod("更新AGV任务状态成功" + JsonConvert.SerializeObject(agvtsk) + ",status:" + status);
                }
                else
                { logger.InfoMethod("更新AGV任务状态失败" + JsonConvert.SerializeObject(agvtsk) + ",status:" + status); }
                re.AddField("Action", "1");
            }
            catch (Exception e)
            {
                logger.ErrorMethod("更新AGV任务状态失败,taskno is " + TaskNo + ",status:" + status, e);
                re.AddField("Action", "0");
            }
            Send(re);
        }
        /// <summary>
        /// 收到的回复
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


        public override void Exit()
        {
            base.Exit();
            if (thread_CheckAGVTaskSend != null)
            {
                thread_CheckAGVTaskSend.Stop(1000);
            }
            //if (thread_CreateAGVTask != null)
            //{
            //    thread_CreateAGVTask.Stop(1000);
            //}
        }

        void SaveAgvStatues(Neutrino neutrino)
        {
            int PlantNo = Convert.ToInt32(neutrino.GetField("PlantNo"));
            var Status = neutrino.GetField("Status").Trim().ToArray();
            var list = this.BusinessLogic.AGVConfigProvider.GetAllAGVConfig(null);
            var thisplantagv = list.FindAll(x => x.PlantNo == PlantNo).OrderBy(x => x.SeqNo).ToList();
            var dt = DateTime.Now;
            for (int i = 0; i < thisplantagv.Count - 1; i++)
            {

                var agv = thisplantagv[i];

                byte status = Convert.ToByte(Status[i].ToString());
                /// <summary>
                /// 0=AGV处于待命状态;
                ///1=AGV处于空闲状态;
                ///2=AGV处于任务执行状态;
                ///3=AGV处于故障状态;
                /// </summary>
                if (status == 3)
                {
                    //找正在执行的AGV任务,若有,则将该任务标记为故障
                    //找实时轨迹里面最新的坐标,并将其状态改为3
                }
                if (this.BusinessLogic.AGVStatusProvider._DicAGVStatus.ContainsKey(agv.Id))
                {
                    this.BusinessLogic.AGVStatusProvider._DicAGVStatus[agv.Id].Status = Convert.ToByte(Status[i].ToString());
                    this.BusinessLogic.AGVStatusProvider._DicAGVStatus[agv.Id].Updated = dt;
                }
                else
                {
                    AGVStatusEntity stat = new AGVStatusEntity() { AGVId = (short)agv.Id, Status = Convert.ToByte(Status[i].ToString()), Created = dt, Updated = dt };
                    this.BusinessLogic.AGVStatusProvider._DicAGVStatus.Add(agv.Id, stat);
                }
            }
            try
            {
                this.BusinessLogic.AGVStatusProvider.AddAGVStatus(this.BusinessLogic.AGVStatusProvider._DicAGVStatus.Values.ToList().FindAll(x => x.Id == 0));
                this.BusinessLogic.AGVStatusProvider.SaveAGVStatus(null, this.BusinessLogic.AGVStatusProvider._DicAGVStatus.Values.ToList().FindAll(x => x.Id != 0).ToArray());
            }
            catch (Exception e)
            {
                logger.ErrorMethod("File to SaveAGVStatues,neutrino is " + JsonConvert.SerializeObject(neutrino), e, "SaveAGVStatues");
            }
        }
        void AGVInfo(Neutrino neutrino)
        {
            int PlantNo = Convert.ToInt32(neutrino.GetField("PlantNo"));
            string AGVID = neutrino.GetField("AGVID");
            int MinPower = neutrino.GetInt("MinPower");
            int MsgNo = neutrino.GetInt("MsgNo");
            AGVConfigEntity agv = this.BusinessLogic.AGVConfigProvider.GetAGVByName(AGVID, null);
            if (agv == null)
            {
                logger.Error("con't find AGVConfigEntity by agvname when agvname is " + AGVID);
                return;
            }
            AGVStatusEntity vsta = this.BusinessLogic.AGVStatusProvider._DicAGVStatus[agv.Id];
            if (vsta == null)
            {
                logger.Error("con't find AGVStatusEntity by agvid when agvid is " + agv.Id);
                return;
            }
            vsta.MinPower = (byte)MinPower;
            this.BusinessLogic.AGVStatusProvider.SaveAGVStatus(null, vsta);
            MessageEntity mess = new MessageEntity() { Created = DateTime.Now, MsgLevel = MessageType.Info, Source = AGVID };
            mess.MsgContent = "";//此处根据消息编号获取消息内容
            List<MessageEntity> l = new List<MessageEntity>();
            l.Add(mess);
            this.BusinessLogic.MessageInfoProvider.SaveMessages(l);
        }
        private void SaveAGVRoute(Neutrino neutrino)
        {
            //float l_x = 2000;
            //float l_y = 2000;
            try
            {//02 41 47 56 52 6F 75 74 65 20 20 20 20 20 20 20 20 20 20 20 20 30 30 30 30 30 30 30 30 30 31 20 20 20 20 20 20 20 20 20 33 30 30 30 30 30 30 30 30 31 32 30 30 30 30 37 30 37 30 32 30 34 39 39 34 39 30 30 31 31 36 33 34 35 37 38 35 64 38 61 36 30 33 35 03
                long SEQUENCE = neutrino.GetLongOrDefault("SEQUENCE");
                int PlantNo = Convert.ToInt32(neutrino.GetField("PlantNo"));
                short agvid = Convert.ToInt16(neutrino.GetField("AGVID"));
                float x = 0;
                float y = 0;

                //if (neutrino.GetField("CurrentX").TrimStart('0') != "")
                //    x = Convert.ToSingle(neutrino.GetField("CurrentX").TrimStart('0'));
                //if (neutrino.GetField("CurrentY").TrimStart('0') != "")
                //    y = Convert.ToSingle(neutrino.GetField("CurrentY").TrimStart('0'));

                var agvsystem = this.BusinessLogic.AGVBYSStatusProvider._AGVBYSStatusCache.FirstOrDefault(c => c.AGVID == agvid);
                if (agvsystem != null)
                {
                    x = agvsystem.LocationX;
                    y = agvsystem.LocationY;
                }
                agv_three_configEntity act = null;
                if (x != 0 || y != 0)
                {

                    var weblocation = new List<agv_three_configEntity>();
                    //    weblocation = this.BusinessLogic.Agv_three_configProvider._AllAgv_three_config;
                    //    //weblocation = this.BusinessLogic.Agv_three_configProvider._AllAgv_three_config.FindAll(t => Math.Abs(x - t.fac_X) <= l_x && Math.Abs(y - t.fac_Y) <= l_y);
                    //    weblocation.ForEach(c =>
                    //    {
                    //        c.Dev_x = Math.Abs(x - c.fac_X);
                    //        c.Dev_y = Math.Abs(y - c.fac_Y);
                    //    });
                    //    var weblocationx = weblocation.OrderBy(c => c.StDev).ToList().Take(20);
                    //    act = weblocationx.FirstOrDefault();
                    //}
                    //else
                    //    act = this.BusinessLogic.Agv_three_configProvider._originLocation;
                    act = new agv_three_configEntity();
                    short speed = Convert.ToInt16(neutrino.GetField("Speed"));
                    byte status = 0;
                    long TaskNo = 0;
                    try
                    {
                        status = Convert.ToByte(neutrino.GetField("StatusAGVRoute"));
                        TaskNo = long.Parse(neutrino.GetField("TaskNoAGVRoute"), System.Globalization.NumberStyles.HexNumber);
                    }
                    catch (Exception e)
                    {
                        logger.ErrorMethod($"解析AGVRoute status或TaskNo出错,SEQUENCE:{SEQUENCE},neutrino is " + JsonConvert.SerializeObject(neutrino), e, "SaveAGVRoute");
                    }
                    var tmp = new AGVRouteEntity() { AGVId = agvid, Created = DateTime.Now, Speed = speed, X = x, Y = y, Status = status, agv_id = agvid, fac_x = act.fac_x, fac_y = act.fac_y };
                    var agvroutelist = this.BusinessLogic.AGVRouteProvider.RealTimeAGVRute2[agvid];


                    if (this.BusinessLogic.AGVRouteProvider.RealTimeAGVRute.ContainsKey(agvid))
                    {
                        this.BusinessLogic.AGVRouteProvider.RealTimeAGVRute[agvid] = tmp;
                    }
                    else
                    {
                        this.BusinessLogic.AGVRouteProvider.RealTimeAGVRute.Add(agvid, tmp);
                    }
                    if (agvroutelist.Count >= 5)
                    {
                        agvroutelist.RemoveAt(0);
                    }
                    agvroutelist.Add(tmp);
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("File to SaveAGVRoute,neutrino is " + JsonConvert.SerializeObject(neutrino), e, "SaveAGVRoute");
            }

        }
    }

}
