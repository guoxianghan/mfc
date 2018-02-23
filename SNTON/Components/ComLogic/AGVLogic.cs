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
            thread_AliveReq = new VIThreadEx(AliveReq, null, "send AliveAck", 1000);
            thread_AGVStatusReq = new VIThreadEx(AGVStatusReq, null, "send AGVStatusReq", 20000);
        }
        protected override void StartInternal()
        {
            base.StartInternal();
            //thread_AliveReq.Start();
            thread_AGVStatusReq.Start();
            thread_CheckAGVTaskSend.Start();
            //thread_CreateAGVTask.Start(); 
        }

        private void CheckAGVTaskSend()
        {

            //Neutrino re = new Neutrino();
            //re.AddField("TELEGRAMID", SNTONAGVCommunicationProtocol.AGVCmdExeReportAck.PadRight(20).ToString());
            //re.AddField("Action", "1");
            //re.AddField("TaskNo", "123");
            //Send(re);
            //return;
            var tmp = BusinessLogic.AGVTasksProvider.GetAGVTasks(" Status in (2) and isdeleted=0");
            //tmp = BusinessLogic.AGVTasksProvider.GetAGVTasks("TaskNo=100000000000001822");
            //HLCallCmd(tmp[0]);
            //return;
            if (tmp != null)
                foreach (var item in tmp)
                {
                    if (item.Status == (byte)AGVTaskStatus.Ready)
                    {
                        var eqtsk = BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"TaskGuid='{item.TaskGuid.ToString()}'", null);
                        if (item.TaskNo == 0)
                            item.TaskNo = Convert.ToInt64(this.Sequencer.GetNextSequenceNo());
                        HLCallCmd(item);
                        item.Status = (byte)AGVTaskStatus.Sent;
                        item.Updated = DateTime.Now;
                        BusinessLogic.AGVTasksProvider.UpdateEntity(item, null);
                        logger.InfoMethod("向小车发送Sent指令");
                        break;
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
                    SaveAGVRoute(neutrino);
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
                    status = (byte)SNTONConstants.AGVTaskStatus.Received;
                    BusinessLogic.AGVTasksProvider.SaveAGVTaskStatus(TaskNo, status);
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
            var agvtsk = BusinessLogic.AGVTasksProvider.GetAGVTaskEntityByTaskNo(TaskNo, null);
            agvtsk.Updated = DateTime.Now;
            agvid = neutrino.GetIntOrDefault("AGVID");
            var eqtsk = BusinessLogic.EquipTaskProvider.GetEquipTaskEntityNotDeleted($"TaskGuid='{agvtsk.TaskGuid.ToString()}'", null);

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
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = $"小车指令重复,小车id:{agvid},开始执行:guid:" + agvtsk.TaskGuid.ToString() + ",status:" + status + ",agvtsk.Status:" + agvtsk.Status, Source = "AGV指令重复", MsgLevel = 7 });
                        return;
                    }
                    eqtsk?.ForEach(x => x.Status = 5);
                    logger.InfoMethod($"AGV调度成功,小车id:{agvid},开始执行:guid:" + agvtsk.TaskGuid.ToString() + ",status:" + status + ",TaskNo:" + TaskNo);
                    break;
                case 2://执行结束
                    status = (byte)AGVTaskStatus.Finished;
                    if (agvtsk.Status >= status)
                    {
                        //logger.InfoMethod($"是哪个sb重发了指令,小车id:{agvid},开始执行:guid:" + agvtsk.TaskGuid.ToString() + ",status:" + status + status + ",agvtsk.Status:" + agvtsk.Status);
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = $"小车指令重复,小车id:{agvid},任务完成:guid:" + agvtsk.TaskGuid.ToString() + ",status:" + status + ",agvtsk.Status:" + agvtsk.Status, Source = "AGV指令重复", MsgLevel = 7 });
                        return;
                    }
                    this.BusinessLogic.AGVRouteProvider.DeleteAGVRoute(agvid, null);
                    eqtsk?.ForEach(x => x.Status = 7);
                    logger.InfoMethod($"AGV调度成功,小车id;{agvid},任务完成:guid:" + agvtsk.TaskGuid.ToString() + ",status:" + status + ",TaskNo:" + TaskNo);
                    break;
                default:
                    logger.WarnMethod("未知的AGV调度指令,AGVCmdExe->Action:" + Action);
                    return;
                    //It is not correct processing
                    //By Song@2018.01.14.
                    //break;

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
                //BusinessLogic.EquipTaskProvider.UpdateStatus(16, null);
                BusinessLogic.AGVTasksProvider.UpdateEntity(agvtsk, null);
                re.AddField("Action", "1");
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to update agvtask status,taskno is " + TaskNo, e);
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
            try
            {
                int PlantNo = Convert.ToInt32(neutrino.GetField("PlantNo"));
                short agvid = Convert.ToInt16(neutrino.GetField("AGVID"));
                string x = neutrino.GetField("CurrentX");
                string y = neutrino.GetField("CurrentY");
                short speed = Convert.ToInt16(neutrino.GetField("Speed"));
                AGVRouteEntity a = new AGVRouteEntity() { AGVId = agvid, Created = DateTime.Now, Speed = speed, X = x, Y = y };
                AGVRouteArchiveEntity en = new AGVRouteArchiveEntity() { AGVId = agvid, Created = DateTime.Now, Speed = speed, X = x, Y = y };
                this.BusinessLogic.AGVRouteProvider.AddAGVRoute(null, a);
                this.BusinessLogic.AGVRouteArchiveProvider.AddAGVRoute(null, en);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("File to SaveAGVRoute,neutrino is " + JsonConvert.SerializeObject(neutrino), e, "SaveAGVRoute");
            }

        }
    }

}
