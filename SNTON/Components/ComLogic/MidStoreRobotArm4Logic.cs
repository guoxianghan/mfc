using log4net;
using SNTON.Com;
using SNTON.Constants;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.MidStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VI.MFC;
using VI.MFC.COM;
using VI.MFC.Logging;
using VI.MFC.Logic;
using VI.MFC.Utils;
using VI.MFC.Utils.ConfigBinder;
using static SNTON.Constants.SNTONConstants;
using Newtonsoft.Json;
using System.IO;
using SNTON.Components.RobotArm;
using SNTON.Entities.DBTables.RobotArmTask;
using System.Xml;
using SNTON.Entities.DBTables.AGV;
using System.Diagnostics;
using SNTON.Entities.DBTables.PLCAddressCode;
using SNTON.Entities.DBTables.Message;
using System.Threading;

namespace SNTON.Components.ComLogic
{
    /// <summary>
    /// 暂存库龙门
    /// </summary>
    public class MidStoreRobotArm4Logic : ComLogic
    {
        //private VIThreadEx thread_PullingCreateRobotArmTask;
        private VIThreadEx thread_RuningRobotArmTask;
        private VIThreadEx thread_ReadDervice;
        private VIThreadEx thread_warninginfo;
        public new static MidStoreRobotArm4Logic Create(XmlNode configNode)
        {
            MidStoreRobotArm4Logic mxParser = new MidStoreRobotArm4Logic();
            mxParser.Init(configNode);
            return mxParser;
        }
        public List<MachineWarnningCodeEntity> _WarnningCode = new List<MachineWarnningCodeEntity>();
        List<string> _warningMessageList = new List<string>();
        public override void Init(XmlNode configNode)
        {
            base.Init(configNode);
        }
        //写轮询方法
        //轮询任务,看暂存库有没有需要的轮子,若有,执行抓取命令
        public MidStoreRobotArm4Logic()
        {
            //this.Parser
            //   this.CommModule.Parser 
            thread_RuningRobotArmTask = new VIThreadEx(RuningRobotArmTask, null, "RuningRobotArmTask", 1000);
            //thread_CheckRobotArmTask = new VIThreadEx(CheckRobotArmTask, null, "CheckRobotArmTask", 1000);
            thread_ReadDervice = new VIThreadEx(ReadDervice, null, "ReadDervice Status", 1000);
            thread_warninginfo = new VIThreadEx(ReadWarningInfo, null, "ReadMidStoreRobotArmWarnning", 5000);
        }
        protected override void StartInternal()
        {
            thread_ReadDervice.Start();
            thread_RuningRobotArmTask.Start();
            thread_warninginfo.Start();
            base.StartInternal();
        }


        /// <summary>
        /// RobotArmID
        /// </summary>
        [ConfigBoundProperty("RobotArmID")]
        public string RobotArmID = "";

        /// <summary>
        /// 龙门报警
        /// </summary>
        public bool IsWarning = false;
        /// <summary>
        /// 龙门是否允许下发指令
        /// </summary>
        public bool IsCanSend = true;
        public override void OnConnect()
        {
            base.OnConnect();
        }
        public override void OnDisconnect()
        {
            base.OnDisconnect();
        }
        void ReadWarningInfo()
        {
            IsAuto();
            SendWarning();
            _WarnningCode = this.BusinessLogic.MachineWarnningCodeProvider.MachineWarnningCodes.FindAll(x => x.MachineCode == 1);
            if (_WarnningCode == null)
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
                int plcvalue = n.Item2.GetInt(item.Key.Trim());

                if (plcvalue != 0)
                {
                    var bit = _WarnningCode.FirstOrDefault(x => x.AddressName.Trim() == item.Key && x.BIT == plcvalue);
                    if (bit == null)
                        continue;
                    if (!bit.Value)
                    {
                        bit.Value = true;
                        this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = this.StorageArea + "号龙门" + bit.Description.Trim(), Source = this.StorageArea + "号龙门报警", MsgLevel = 7, MidStoreage = this.StorageArea });
                    }
                    _WarnningCode.FindAll(x => x.AddressName.Trim() == item.Key && x.BIT != plcvalue).ForEach(x => x.Value = false);
                }
                else
                    _WarnningCode.ForEach(x => x.Value = false);

            }

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
        public override void Process(Neutrino theNeutrino, PostProcessCallback callback)
        {
            base.Process(theNeutrino, callback);
        }

        /// <summary>
        /// 处理接收的消息
        /// </summary>
        /// <param name="neutrino"></param>
        /// <returns></returns>
        //[Obsolete("重写收发消息的机制,改方法暂时已不需要,用ReadDervice代替")]
        protected override bool ProcessMessage(Neutrino neutrino)
        {
            return base.ProcessMessage(neutrino);
        }

        /// <summary>
        ///  龙门状态，0自动无故障，1退出自动无故障，2故障
        /// </summary>
        /// <returns></returns>
        int IsAuto()
        {
            Neutrino ne = new Neutrino();
            ne.TheName = "IsAuto";
            ne.AddField("res_LONGMEN_STATE", "0");
            var n = this.MXParser.ReadData(ne);
            if (n.Item1)
            {
                IsWarning = false;
                if (n.Item2.GetInt("res_LONGMEN_STATE") == 0)
                {
                    IsWarning = false;
                }
                else
                {
                    IsWarning = true;
                }
                return n.Item2.GetInt("res_LONGMEN_STATE");
            }
            else
            {
                IsWarning = true;
                return 2;
            }

        }
        public override void ReadDervice()
        {
            Int32 isauto = IsAuto();
            if (isauto != 0)
            {
                IsWarning = true;
                return;
            }
            IsWarning = false;
            Random ran = new Random();
            int i = ran.Next(1, 20);
            Stopwatch watch = Stopwatch.StartNew();//创建一个监听器
            watch.Start();
            Neutrino neu = new Neutrino();
            neu.TheName = "ReadMidStoreRobotArm_" + this.RobotArmID;
            neu.AddField("res_COMMAND_AUTO", "0");
            neu.AddField("res_STORE_NO_AUTO", "0");
            neu.AddField("res_OUT_STORE_LOCATION_AUTO", "0");
            neu.AddField("res_WHEEL_L_AUTO", "0");
            neu.AddField("res_WHEEL_GET_HIGH_AUTO", "0");
            neu.AddField("res_WHEEL_SET_HIGH_AUTO", "0");
            neu.AddField("res_MaterialType_AUTO", "0");
            neu.AddField("res_BACKUP1_AUTO", "0");
            neu.AddField("res_BACKUP2_AUTO", "0");
            neu.AddField("res_BACKUP3_AUTO", "0");
            neu.AddField("res_COMMAND_UNAUTO", "0");
            neu.AddField("res_STORE_NO_UNAUTO", "0");
            neu.AddField("res_OUT_STORE_LOCATION_UNAUTO", "0");
            neu.AddField("res_WHEEL_L_UNAUTO", "0");
            neu.AddField("res_WHEEL_GET_HIGH_UNAUTO", "0");
            neu.AddField("res_WHEEL_SET_HIGH_UNAUTO", "0");
            neu.AddField("res_MaterialType_UNAUTO", "0");
            neu.AddField("res_BACKUP1_UNAUTO", "0");
            neu.AddField("res_BACKUP2_UNAUTO", "0");
            neu.AddField("res_LONGMEN_STATE", "0");
            neu.AddField("res_Warning0", "0");
            neu.AddField("res_Warning1", "0");
            neu.AddField("res_Warning2", "0");

            if (i == 2)
            {
                logger.InfoMethod("开始读取" + this.StorageArea + "号龙门完成指令:" + watch.ElapsedMilliseconds);
            }

            var ne = this.MXParser.ReadData(neu);

            if (i == 2)
            {
                logger.InfoMethod("读取" + this.StorageArea + "号龙门完成指令耗时:" + watch.ElapsedMilliseconds);
                watch.Restart();
            }
            if (!ne.Item1)
            {
                return;
            }
            Neutrino neutrino = ne.Item2;
            var midres = ParserNeu(neutrino);

            if (!IsWarning && midres.res_LONGMEN_STATE != 0)
            {
                IsWarning = true;
                logger.WarnMethod($"{StorageArea}号龙门退出自动或故障");
                this.BusinessLogic.MessageInfoProvider.Add(null, new Entities.DBTables.Message.MessageEntity() { Created = DateTime.Now, MsgContent = $"{StorageArea}号龙门故障", Source = "龙门故障", MsgLevel = 7, MidStoreage = this.StorageArea });
            }

            if (i == 2)
            {
                logger.InfoMethod("" + this.StorageArea + "号龙门ParserNeu对象转换耗时:" + watch.ElapsedMilliseconds);
                watch.Restart();
            }
            //string cmd = neu.TheName.Trim();
            //var keys = neutrino.GetAllKeys();
            var date = DateTime.Now;
            var armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"SpoolStatus=1 and TaskStatus=2 AND RobotArmID='{this.RobotArmID}' AND PlantNo={this.PlantNo} and StorageArea='{StorageArea}'", null);//找到正在抓取的轮子

            if (i == 2)
            {
                logger.InfoMethod("读取" + this.StorageArea + "号龙门任务GetRobotArmTasks耗时:" + watch.ElapsedMilliseconds);
                watch.Restart();
            }
            RobotArmTaskEntity armtsk = null;
            if (armtsks != null && armtsks.Count != 0)
                armtsk = armtsks[0];
            else return;

            armtsk.Updated = date;
            var midstores = this.BusinessLogic.MidStorageSpoolsProvider.GetMidStorages($"StorageArea={StorageArea} and SeqNo=" + armtsk.FromWhere, null);

            if (i == 2)
            {
                logger.InfoMethod("读取" + this.StorageArea + "号暂存库GetMidStorages耗时:" + watch.ElapsedMilliseconds);
                watch.Restart();
            }
            var midstore = midstores[0];
            midstore.Updated = date;
            int receivearmcomm = 0;
            int sendarmcmd = ConvertRobotArmCommand(armtsk.TaskType);//下发龙门指令.出库入库
            int storageseqno = 0;//库位号
            if (midres.res_LONGMEN_STATE == 0)
            {
                receivearmcomm = midres.res_COMMAND_AUTO;//指令码：1入库，2出库，3异常口处理，4异常口出库 
                storageseqno = midres.res_STORE_NO_AUTO;
            }
            else if (midres.res_LONGMEN_STATE == 1)
            {
                receivearmcomm = midres.res_COMMAND_UNAUTO;//指令码：1入库，2出库，3异常口处理，4异常口出库 
                storageseqno = midres.res_STORE_NO_UNAUTO;
            }
            else
            {
                if (!IsWarning)
                {
                    IsWarning = true;
                    logger.WarnMethod($"{StorageArea}号龙门退出自动或故障");
                    this.BusinessLogic.MessageInfoProvider.Add(null, new Entities.DBTables.Message.MessageEntity() { Created = DateTime.Now, MsgContent = $"{StorageArea}号龙门故障", Source = "龙门故障", MsgLevel = 7, MidStoreage = this.StorageArea });
                }
                return;
            }


            if (storageseqno == armtsk.FromWhere) //&& receivearmcomm == sendarmcmd
            {
                #region MyRegion
                if (armtsk.TaskType == 0 || armtsk.TaskType == 1 || armtsk.TaskType == 3 || armtsk.TaskType == 4)
                {//出库
                    armtsk.SpoolStatus = 2;
                    midstore.Updated = date;
                    midstore.IsOccupied = 0;
                    midstore.IdsList = "";
                }
                else if (armtsk.TaskType == 2)
                {//入库
                    armtsk.SpoolStatus = 2;
                    midstore.Updated = date;
                    midstore.IsOccupied = 1;
                }
                else if (armtsk.TaskType == 4)
                {
                    armtsk.SpoolStatus = 2;
                    midstore.Updated = date;
                    midstore.IsOccupied = 0;
                    logger.WarnMethod("直通线到异常口尚未处理" + JsonConvert.SerializeObject(armtsk));
                    logger.WarnMethod("龙门指令码://指令码：1入库，2出库，3异常口处理，4异常口出库 ");
                    logger.WarnMethod($"发送库位号:{armtsk.FromWhere},响应库位号:{storageseqno};发送出入库指令:{sendarmcmd},响应出入库指令:{receivearmcomm}");
                    return;
                }
                else
                {
                    //logger.WarnMethod("未知的龙门响应指令" + JsonConvert.SerializeObject(armtsk));
                    //logger.WarnMethod("龙门指令码://指令码：1入库，2出库，3异常口处理，4异常口出库 ");
                    //logger.WarnMethod($"发送库位号:{armtsk.FromWhere},响应库位号:{storageseqno};发送出入库指令:{sendarmcmd},响应出入库指令:{receivearmcomm}");
                    return;
                }
                #endregion

                this.BusinessLogic.MidStorageProvider.UpdateMidStore(null, midstore);
                logger.InfoMethod("成功更新库位状态:armtsk.id=" + armtsk.Id);

                if (i == 2)
                {
                    logger.InfoMethod("读取暂存库GetMidStorages耗时:" + watch.ElapsedMilliseconds);
                    watch.Restart();
                }
                this.BusinessLogic.RobotArmTaskProvider.UpdateArmTask(armtsk);
                if (i == 2)
                {
                    logger.InfoMethod("更新龙门任务UpdateArmTask耗时:" + watch.ElapsedMilliseconds);
                    watch.Restart();
                }
                logger.InfoMethod("成功更新正在抓取的轮子状态,出库," + JsonConvert.SerializeObject(armtsk));
                //判断是不是最后一个
                bool r = this.BusinessLogic.RobotArmTaskProvider.SetArmTasksUnitStatus(armtsk.TaskGroupGUID);
                if (i == 2)
                {
                    logger.InfoMethod("判断是不是最后一个SetArmTasksUnitStatus耗时:" + watch.ElapsedMilliseconds);
                    watch.Restart();
                }
                if (r)
                {//如果是最后一个
                    var equiptsks = this.BusinessLogic.EquipTask5Provider.GetEquipTask5($"TaskGuid='{armtsk.TaskGroupGUID}'", null);
                    if (equiptsks != null && equiptsks.Count != 0)
                    {
                        equiptsks.ForEach(x => { x.Status = 4; });
                        this.BusinessLogic.EquipTask5Provider.UpdateEquipTask5(null, equiptsks.ToArray());
                    }
                    if (i == 2)
                    {
                        logger.InfoMethod("更新任务状态UpdateStatus耗时:" + watch.ElapsedMilliseconds);
                        watch.Restart();
                    }
                }
            }
            else
            {
                if (i == 2)
                {
                    logger.InfoMethod("本次" + this.StorageArea + "号龙门任务未完成耗时:" + watch.ElapsedMilliseconds);
                    watch.Restart();
                }
                //logger.WarnMethod(this.StorageArea + "号龙门响应指令有误");
                //logger.WarnMethod("龙门指令码://指令码：1入库，2出库，3异常口处理，4异常口出库 ");
                //logger.WarnMethod($"发送库位号:{armtsk.FromWhere},响应库位号:{storageseqno};发送出入库指令:{sendarmcmd},响应出入库指令:{receivearmcomm}");
                return;
            }
            if (i == 2)
                logger.InfoMethod("执行判断龙门完成指令耗时:" + watch.ElapsedMilliseconds);
        }


        MidStoreRobotArmNeu ParserNeu(Neutrino neutrino)
        {
            MidStoreRobotArmNeu n = new MidStoreRobotArmNeu();
            n.req_FLAGBIT = neutrino.GetIntOrDefault("req_FLAGBIT");
            n.req_COMMAND = neutrino.GetIntOrDefault("req_COMMAND");
            n.req_STORE_NO = neutrino.GetIntOrDefault("req_STORE_NO");
            n.req_OUT_STORE_LOCATION = neutrino.GetIntOrDefault("req_OUT_STORE_LOCATION");
            n.req_WHEEL_L = neutrino.GetIntOrDefault("req_WHEEL_L");
            n.req_WHEEL_GET_HIGH = neutrino.GetIntOrDefault("req_WHEEL_GET_HIGH");
            n.req_WHEEL_SET_HIGH = neutrino.GetIntOrDefault("req_WHEEL_SET_HIGH");
            n.req_MaterialType = neutrino.GetIntOrDefault("req_MaterialType");
            n.req_BACKUP1 = neutrino.GetIntOrDefault("req_BACKUP1");
            n.req_BACKUP2 = neutrino.GetIntOrDefault("req_BACKUP2");
            n.res_COMMAND_AUTO = neutrino.GetIntOrDefault("res_COMMAND_AUTO");
            n.res_STORE_NO_AUTO = neutrino.GetIntOrDefault("res_STORE_NO_AUTO");
            n.res_OUT_STORE_LOCATION_AUTO = neutrino.GetIntOrDefault("res_OUT_STORE_LOCATION_AUTO");
            n.res_WHEEL_L_AUTO = neutrino.GetIntOrDefault("res_WHEEL_L_AUTO");
            n.res_WHEEL_GET_HIGH_AUTO = neutrino.GetIntOrDefault("res_WHEEL_GET_HIGH_AUTO");
            n.res_WHEEL_SET_HIGH_AUTO = neutrino.GetIntOrDefault("res_WHEEL_SET_HIGH_AUTO");
            n.res_MaterialType_AUTO = neutrino.GetIntOrDefault("res_MaterialType_AUTO");
            n.res_BACKUP1_AUTO = neutrino.GetIntOrDefault("res_BACKUP1_AUTO");
            n.res_BACKUP2_AUTO = neutrino.GetIntOrDefault("res_BACKUP2_AUTO");
            n.res_BACKUP3_AUTO = neutrino.GetIntOrDefault("res_BACKUP2_AUTO");
            n.res_COMMAND_UNAUTO = neutrino.GetIntOrDefault("res_COMMAND_UNAUTO");
            n.res_STORE_NO_UNAUTO = neutrino.GetIntOrDefault("res_STORE_NO_UNAUTO");
            n.res_OUT_STORE_LOCATION_UNAUTO = neutrino.GetIntOrDefault("res_OUT_STORE_LOCATION_UNAUTO");
            n.res_WHEEL_L_UNAUTO = neutrino.GetIntOrDefault("res_WHEEL_L_UNAUTO");
            n.res_WHEEL_GET_HIGH_UNAUTO = neutrino.GetIntOrDefault("res_WHEEL_GET_HIGH_UNAUTO");
            n.res_WHEEL_SET_HIGH_UNAUTO = neutrino.GetIntOrDefault("res_WHEEL_SET_HIGH_UNAUTO");
            n.res_MaterialType_UNAUTO = neutrino.GetIntOrDefault("res_MaterialType_UNAUTO");
            n.res_BACKUP1_UNAUTO = neutrino.GetIntOrDefault("res_BACKUP1_UNAUTO");
            n.res_BACKUP2_UNAUTO = neutrino.GetIntOrDefault("res_BACKUP2_UNAUTO");
            n.res_LONGMEN_STATE = neutrino.GetIntOrDefault("res_LONGMEN_STATE");
            n.res_Warning0 = neutrino.GetIntOrDefault("res_Warning0");
            n.res_Warning1 = neutrino.GetIntOrDefault("res_Warning1");
            n.res_Warning2 = neutrino.GetIntOrDefault("res_Warning2");

            return n;
        }
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="req_COMMAND">指令码：1入库，2出库，3直通线抓到异常口处理，4暂存库到异常口出库 </param>
        /// <param name="req_STORE_NO">库位号</param>
        /// <param name="req_OUT_STORE_LOCATION">出库位置号</param>
        /// <param name="req_WHEEL_L"></param>
        /// <param name="req_WHEEL_GET_HIGH"></param>
        /// <param name="req_WHEEL_SET_HIGH"></param>
        /// <param name="req_MaterialType"></param>
        /// <param name="req_BACKUP1"></param>
        /// <param name="req_BACKUP2"></param>
        void SendCommand(int req_COMMAND, int req_STORE_NO, int req_OUT_STORE_LOCATION, int req_WHEEL_L, int req_WHEEL_GET_HIGH, int req_WHEEL_SET_HIGH, int req_MaterialType, int req_BACKUP1, int req_BACKUP2)
        {
            Neutrino ne = new Neutrino();
            //ne.AddField("req_FLAGBIT", "2");
            //ne.AddField("req_COMMAND", req_COMMAND.ToString());
            //ne.AddField("req_STORE_NO", req_STORE_NO.ToString());
            //ne.AddField("req_OUT_STORE_LOCATION", req_OUT_STORE_LOCATION.ToString());
            //ne.AddField("req_WHEEL_L", req_WHEEL_L.ToString());
            //ne.AddField("req_WHEEL_GET_HIGH", req_WHEEL_GET_HIGH.ToString());
            //ne.AddField("req_MaterialType", req_MaterialType.ToString());
            //ne.AddField("req_BACKUP1", req_BACKUP1.ToString());
            //ne.AddField("req_BACKUP2", req_BACKUP1.ToString());


            ne.AddField("req_COMMAND", req_COMMAND.ToString());
            ne.AddField("req_STORE_NO", req_STORE_NO.ToString());
            ne.AddField("req_OUT_STORE_LOCATION", req_OUT_STORE_LOCATION.ToString());
            ne.AddField("req_WHEEL_L", req_WHEEL_L.ToString());
            ne.AddField("req_WHEEL_GET_HIGH", req_WHEEL_GET_HIGH.ToString());
            ne.AddField("req_WHEEL_SET_HIGH", req_WHEEL_SET_HIGH.ToString());
            ne.AddField("req_MaterialType", req_MaterialType.ToString());
            ne.AddField("req_BACKUP1", req_BACKUP1.ToString());
            ne.AddField("req_BACKUP2", req_BACKUP2.ToString());
            ne.AddField("req_FLAGBIT", "2");

            //ne = ParserMid(mid);
            SendData(ne);
        }
        /// <summary>
        /// 检测龙门进度状态
        /// </summary>
        void CheckRobotArmTask()
        {
            //if (MidStoreLineLogicProvider.LineCmd != LineCmd.None)
            //    return;
            MidStoreRobotArmNeu mid = new MidStoreRobotArmNeu();
            mid.req_COMMAND = 2;
            mid.req_FLAGBIT = 0;
            Neutrino ne = new Neutrino();
            ne.AddField("req_COMMAND", "2");
            ne.AddField("req_FLAGBIT", "0");

            int result = LineStatus(0);
            result = LineStatus(1);
            result = LineStatus(2);
            result = LineStatus(3);
            result = LineStatus(4);
            // req_COMMAND指令码：1入库 直通线到暂存库，2出库 暂存库到直通线或出库口，3直通线抓到异常口处理，4暂存库到异常口出库 

            /// req_COMMAND">指令码：1入库，2出库，3异常口处理，4异常口出库</param>
            /// <param name="req_STORE_NO">库位号</param>
            /// <param name="req_OUT_STORE_LOCATION">出库位置号 线体编号</param>
            /// <param name="req_WHEEL_L"></param>
            /// <param name="req_WHEEL_GET_HIGH"></param>
            /// <param name="req_WHEEL_SET_HIGH"></param>
            /// <param name="req_MaterialType"></param>
            /// <param name="req_BACKUP1"></param>
            /// <param name="req_BACKUP2"></param>
            //SendCommand(2, 6, 1, 250, 2200, 2200, 3, 0, 0);
            SendData(ne);
            //this.Parser
        }

        /// <summary>
        /// 检测数据库中已经准备好的ArmTask,通知龙门从暂存库逐个抓取轮子到出库口
        /// </summary>
        void RuningRobotArmTask()
        {
            Stopwatch watch = Stopwatch.StartNew();//创建一个监听器
            SendWarning();
            watch.Start();
            Neutrino ne = new Neutrino();
            ne.TheName = "ReadingRobotArmStatus";
            ne.AddField("req_FLAGBIT", "0");
            ne.AddField("res_LONGMEN_STATE", "0");//龙门状态，0自动无故障，1退出自动无故障，2故障
            var n = this.MXParser.ReadData(ne);
            if (!n.Item1)
            {
                return;
            }
            int isauto = n.Item2.GetInt("res_LONGMEN_STATE");
            int i = n.Item2.GetIntOrDefault("req_FLAGBIT");
            if (isauto != 0)
            {
                if (!IsWarning)
                {
                    IsWarning = true;
                    logger.WarnMethod(this.StorageArea + "号龙门退出自动或故障");
                    this.BusinessLogic.MessageInfoProvider.Add(null, new Entities.DBTables.Message.MessageEntity() { Created = DateTime.Now, MsgContent = this.RobotArmID + "号龙门退出自动或故障", Source = this.RobotArmID + "号龙门报警", MsgLevel = 7, MidStoreage = this.StorageArea });
                }
                return;
            }
            IsWarning = false;
            if (i != 1)
            {//龙门是否允许下发指令 
                if (IsCanSend)
                    IsCanSend = false;
                return;
            }
            IsCanSend = true;
            //TaskStatus 任务状态:-1失效;0创建;1正在抓取;2抓取完毕;3等待AGV接收;4AGV接收完毕;5抓取失败;6接收失败;7任务失败
            var armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskStatus IN (0,1,2) AND StorageArea='{StorageArea}' AND RobotArmID='{this.RobotArmID}'", null);
            if (armtsks == null || armtsks.Count == 0)
                return;
            RobotArmTaskEntity armtskrunning = null;
            /*
            龙门任务单元抓取完后,更新任务状态
            */
            armtskrunning = armtsks.FirstOrDefault(x => x.TaskStatus == 2);
            if (armtskrunning == null)
            {//如果没有正在执行的龙门任务,则找优先级高的
                armtskrunning = armtsks.OrderByDescending(x => x.TaskLevel).ToArray()[0];
                var agvtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTask($"TaskGuid in ('{armtskrunning.TaskGroupGUID.ToString()}')");
                if (agvtsk != null && agvtsk.Status != 1)
                {
                    agvtsk.Status = 1;
                    this.BusinessLogic.AGVTasksProvider.UpdateEntity(agvtsk);
                }
                //else 从直通线直接入库不需要AGVTask
            }
            var armtsksrunning = armtsks.FindAll(x => x.TaskGroupGUID == armtskrunning.TaskGroupGUID);//获取该正在抓取的龙门任务单元
            armtskrunning = armtsksrunning.OrderBy(x => x.SeqNo).FirstOrDefault(x => x.SpoolStatus == 1);
            if (armtskrunning != null)//有正在抓取的轮子,不下达抓取命令
                return;
            armtskrunning = armtsksrunning.OrderBy(x => x.SeqNo).FirstOrDefault(x => x.SpoolStatus == 0);

            if (armtskrunning == null)
            {
                armtsksrunning.ForEach(x => x.TaskStatus = 9);
                this.BusinessLogic.RobotArmTaskProvider.UpdateArmTasks(armtsksrunning, null);
                //logger.WarnMethod("该任务所有龙门任务已执行完,TaskGroupGUID:" + armtskrunning.TaskGroupGUID.ToString());
                return;
            }
            try
            {
                if (armtskrunning.TaskType == 2)
                {
                    int ONLINE_INSTORE_RobotArmSeqNo = this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).ONLINE_INSTORE_RobotArmSeqNo();
                    if (ONLINE_INSTORE_RobotArmSeqNo == 0)
                    {
                        //this.BusinessLogic.MessageInfoProvider.Add(null, new Entities.DBTables.Message.MessageEntity() { Created = DateTime.Now, MsgContent = $"{this.StorageArea}号龙门库直通线流水号:{ONLINE_INSTORE_RobotArmSeqNo},龙门任务流水号:{armtskrunning.SpoolSeqNo},单丝二维码:{armtskrunning.WhoolBarCode.Trim()}", MsgLevel = 7, Source = $"{this.StorageArea}号龙门库直通线流水号错误" });
                        return;
                    }
                    else if (ONLINE_INSTORE_RobotArmSeqNo != armtskrunning.SpoolSeqNo && armtskrunning.TaskType != 1)
                    {
                        string msg = $"{this.StorageArea}号龙门库直通线流水号:{ONLINE_INSTORE_RobotArmSeqNo},龙门任务流水号:{armtskrunning.SpoolSeqNo},单丝二维码:{armtskrunning.WhoolBarCode.Trim()}";
                        if (_warningMessageList.FindAll(x => x == msg).Count <= 10)
                            _warningMessageList.Add(msg);
                        else
                        {
                            this.BusinessLogic.MessageInfoProvider.Add(null, new Entities.DBTables.Message.MessageEntity() { Created = DateTime.Now, MsgContent = msg, MsgLevel = 7, Source = $"{this.StorageArea}号龙门库直通线流水号错误", MidStoreage = this.StorageArea });
                            _warningMessageList.RemoveAll(x => x == msg);
                        }
                        return;
                    }
                }
                armtskrunning.Updated = DateTime.Now;
                int index = armtsksrunning.FindIndex(x => x == armtskrunning);
                if (index == 0)
                {//将设备任务改成正在抓取
                    var equiptsking = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"TASKGUID='{armtskrunning.TaskGroupGUID.ToString()}'", null);
                    if (equiptsking != null)
                    {
                        equiptsking.ForEach(x => x.Status = 2);
                        this.BusinessLogic.EquipTaskProvider.UpdateEntity(equiptsking, null);
                    }
                }
                int cmd = ConvertRobotArmCommand(armtskrunning.TaskType);
                /*
                SendCommand前需要判断线体状态
                */
                // req_COMMAND指令码：1入库 直通线到暂存库，2出库 暂存库到直通线或出库口，3直通线抓到异常口处理，4暂存库到异常口出库 
                //int line = LineStatus(armtskrunning.TaskType);
                //int line = 0;
                int armcode = 0;
                //18/34/44
                Thread.Sleep(1000);
                switch (armtskrunning.CName.Trim())
                {
                    #region MyRegion
                    case "WS18":
                        armcode = 1;
                        SendCommand(cmd, armtskrunning.FromWhere, armtskrunning.ToWhere, 200, 1540, 1540, armcode, 0, 0);//250, 2200, 2200, 3, 0, 0
                        this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).SendProductType(cmd, "2");
                        break;
                    case "WS34":
                        armcode = 2;
                        SendCommand(cmd, armtskrunning.FromWhere, armtskrunning.ToWhere, 200, 2620, 2620, armcode, 0, 0);//250, 2200, 2200, 3, 0, 0
                        this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).SendProductType(cmd, "2");
                        break;
                    case "WS44":
                        armcode = 3;
                        SendCommand(cmd, armtskrunning.FromWhere, armtskrunning.ToWhere, 250, 2200, 2200, armcode, 0, 0);//250, 2200, 2200, 3, 0, 0
                        this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).SendProductType(cmd, "1");
                        break;
                    default:
                        logger.WarnMethod("未知的单丝类型:armtskrunning.CName:" + armtskrunning.CName);
                        BusinessLogic.MessageInfoProvider.Add(null, new Entities.DBTables.Message.MessageEntity() { Created = DateTime.Now, MsgContent = "未知的单丝类型:" + armtskrunning.CName.Trim(), Source = "未知的单丝类型", MsgLevel = 6, MidStoreage = this.StorageArea });
                        break;
                        #endregion
                }

                armtskrunning.SpoolStatus = 1;
                armtskrunning.TaskStatus = 2;
                #region 告诉线体任务流水号
                if (armtskrunning.ToWhere == 1)
                {//正常出库口
                    this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).WriteSeqNo("OUTSTORE_SeqNo_Write", armtskrunning.AGVSeqNo);
                }
                else if (armtskrunning.ToWhere == 2)
                {//直通线
                    this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).WriteSeqNo("ONLINE_SeqNo_Write", armtskrunning.AGVSeqNo);
                }
                #endregion
                if (!armtsksrunning.Exists(x => x.SpoolStatus == 0))//如果是最后一个,清空入库出库型号
                {
                    this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).ClearLineProductType();
                }
                this.BusinessLogic.RobotArmTaskProvider.UpdateArmTaskStatus(armtskrunning.TaskGroupGUID, 2);
                this.BusinessLogic.RobotArmTaskProvider.UpdateArmTask(armtskrunning);
                logger.InfoMethod("send outstore command success :" + armtskrunning.FromWhere.ToString());

            }
            catch (Exception e)
            {
                logger.ErrorMethod("filed to send outstroe command", e);
            }
            finally
            {
                watch.Stop();
                Random ran = new Random();
                int t = ran.Next(1, 20);
                if (t == 2)
                    logger.InfoMethod("轮询发送龙门指令耗时:" + watch.ElapsedMilliseconds);
            }
        }
        /// <summary>
        /// 发送龙门线体报警
        /// </summary>
        void SendWarning()
        {
            try
            {
                Neutrino ne = new Neutrino();
                ne.TheName = "龙门线体报警";
                int alarm = 0;
                if (this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea) != null && (this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).IsScanEnough || this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).IsStoreageEnough || this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).IsWarning))
                    alarm = 1;
                if (this.IsWarning)
                    alarm = 1;
                ne.AddField("res_Warning3", alarm.ToString());
                SendData(ne);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("发送龙门线体报警信息出错", ex);
            }
            //ne = ParserMid(mid);
        }
        /// <summary>
        /// 根据任务类型,判断龙门的命令
        /// req_COMMAND指令码：1入库 直通线到暂存库，2出库 暂存库到直通线或出库口，3直通线到异常口处理，4暂存库到异常口出库 
        /// TaskType                       : 0从暂存库到出库线;1从暂存库到直通线;2从直通线到暂存库(入库);3直通线到异常口;4从暂存库到异常口;
        /// </summary>
        /// <param name="TaskType"></param>
        /// <returns></returns>
        private int ConvertRobotArmCommand(int TaskType)
        {
            int cmd = 0;
            switch (TaskType)
            {
                case 0:
                case 1:
                    cmd = 2;
                    break;
                case 2:
                    cmd = 1;
                    break;
                case 3:
                    cmd = 3;
                    break;
                case 4:
                    cmd = 4;
                    break;
                default:
                    break;
            }

            return cmd;
        }

        /// <summary>
        /// TaskType                       : 0从暂存库到出库线;1从暂存库到直通线;2从直通线到暂存库(入库);3直通线到异常口;4从暂存库到异常口;
        /// </summary>
        /// <param name="TaskType"></param>
        /// <returns>返回1的时候表示允许</returns>
        int LineStatus(int TaskType)
        {
            int cmd = -1;
            switch (TaskType)
            {
                case 0:
                    //0从暂存库到出库线 出库
                    cmd = this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).ReadLineStatus("OUTSTORE_ARM_SET");
                    break;
                case 1:
                    //1从暂存库到直通线 出库
                    cmd = this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).ReadLineStatus("ONLINE_OUTSTORE_ALLOW_SET");
                    break;
                case 2:
                    //2从直通线到暂存库(入库);
                    cmd = this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).ReadLineStatus("ONLINE_INSTORE_ALLOW_GET");
                    break;
                case 3:
                    //3直通线到异常口; 
                    int i = this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).ReadLineStatus("ONLINE_OUTSTORE_ALLOW_SET");
                    cmd = this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).ReadLineStatus("ExLine_ARM_SET");
                    if (i == cmd && i == 1)
                    { cmd = 1; }
                    else
                    {
                        cmd = i == 1 ? cmd : i;
                    }
                    break;
                case 4:
                    //4从暂存库到异常口 出库                 
                    cmd = this.BusinessLogic.GetMidStoreLineLogic(this.StorageArea).ReadLineStatus("ExLine_ARM_SET");
                    break;
                default:
                    throw new NotImplementedException("未识别的TaskType,Value:" + TaskType);
            }
            return cmd;
        }
    }
}
