using log4net;
using SNTON.Constants;
using SNTON.Entities.DBTables.AGV;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.RobotArmTask;
using SNTON.Entities.DBTables.Spools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC;
using VI.MFC.COM;
using VI.MFC.Logging;
using VI.MFC.Logic;
using VI.MFC.Utils;
using VI.MFC.Utils.ConfigBinder;
using static SNTON.Constants.SNTONConstants;

namespace SNTON.Com
{
    public class ScannerCom : TCPClient
    {
        public ScannerCom()
        {

        }
        public new static ScannerCom Create(XmlNode xml)
        {
            ScannerCom c = new ScannerCom();
            c.Init(xml);
            return c;
        }
        public override void Init(XmlNode configNode)
        {
            base.Init(configNode);
        }
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [ConfigBoundProperty("ScannerHostName")]
        public string _HostName { get; set; }
        [ConfigBoundProperty("ScannerPort")]
        public int _Port { get; set; }
        public Queue<string> barcodeQueue { get; set; } = new Queue<string>();
        protected override void OnConnect()
        {
            this.HostName = this._HostName;
            this.Port = this._Port;
            this.Connect();
            base.OnConnect();
        }
        protected override void OnDisconnect()
        {
            this.Disconnect();
            base.OnDisconnect();
        }
        public override void Close()
        {
            base.Close();
        }
        /// <summary>
        /// 车间号
        /// </summary>
        [ConfigBoundProperty("PlantNo")]
        public byte PlantNo = 0;
        /// <summary>
        /// RobotArmID
        /// </summary>
        [ConfigBoundProperty("RobotArmID")]
        public string RobotArmID = "";
        /// <summary>
        /// The Business Logic Component responsible for giving us the data we need.
        /// </summary>
        [ConfigBoundProperty("BusinessLogicId")]
#pragma warning disable 649
        private string businessLogicId;
#pragma warning restore 649
        private BusinessLogic.BusinessLogic businessLogic;

        public BusinessLogic.BusinessLogic BusinessLogic
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref businessLogic, businessLogicId);
                return businessLogic;
            }
        }
        public event Func<Queue<string>, int> EndReceivedEvent;
        public bool IsEndReceived { get; set; } = false;
        protected override void DataReceived(byte[] data, int bytesRead)
        {
            string barcode = Encoding.UTF8.GetString(data);
            if (barcode == "end")
            {
                IsEndReceived = true;
                RunAGVTask();//如果该批轮子都到直通线上,就进行AGVTask判断
                if (EndReceivedEvent != null)
                {
                    EndReceivedEvent(barcodeQueue);
                }
            }
            else
            {//每取到一个轮子
                IsEndReceived = false;
                var whool = this.BusinessLogic.MESSystemProvider.GetMESSpool(barcode, null);//从MES系统查轮子信息存到本地数据库
                SpoolsEntity spool = new SpoolsEntity();
                spool.FdTagNo = barcode;
                if (whool != null)
                {
                    spool.Created = DateTime.Now;
                    spool.IsDeleted = 0;
                    spool.ProductType = "";
                   
                    //spool.MachCode = whool.MachCode; 
                }
                this.BusinessLogic.SpoolsProvider.Add(spool, null);
                barcodeQueue.Enqueue(barcode);
            }
            base.DataReceived(data, bytesRead);
        }
        void RunAGVTask()
        {
            //取条码,判断轮子类型,看是不是有需要的此类型轮子的EquipTask
            //如果有,创建RobotArmTask,如果没有,则入库
            string barcode = barcodeQueue.Peek();
            var spool = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcode(barcode, null);
            var robotarmtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskStatus=0", null);
            var equplinetsks = this.BusinessLogic.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"Status=0 AND ProductType='{spool.ProductType}'", null);
            var midstores = this.BusinessLogic.MidStorageProvider.GetMidStorages($"StorageArea = {PlantNo} and IsOccupied=0", null);//找出库里的空闲位置
            int taskwhoolscount = TaskConfig.GetAGVSpoolsCount(spool.ProductType);
            if (equplinetsks != null && equplinetsks.Count != 0)
            {
                //创建AGVTask和RobotArmTask,并优先级最高
                var guid = Guid.NewGuid();
                //通过GUID标记一个龙门任务单元
                DateTime createtime = DateTime.Now;
                RobotArmTaskEntity armtsk = null;
                int current = barcodeQueue.Count;
                var tmp = new List<EquipTaskEntity>();
                var agvtsk = new AGVTasksEntity() { Created = createtime, TaskGuid = guid, PlantNo = PlantNo, ProductType = spool.ProductType, Status = 0, TaskLevel = 6, TaskType = 2 };
                agvtsk.EquipIdListTarget = MidStoreLine.InStoreLine.ToString();
                foreach (var item in equplinetsks)
                {
                    if (taskwhoolscount == tmp.Count * 2)//满一车则跳出
                        break;
                    item.Status = 1;
                    agvtsk.EquipIdListTarget += "," + item.EquipContollerId.ToString();
                    tmp.Add(item);
                }
                int whools = tmp.Count * 2;//需要的总轮子的数量
                int rest = whools - barcodeQueue.Count;//还需要从暂存库抓取几个轮子 
                if (rest > 0)
                {
                    var armtskwhools = midstores.FindAll(x => x.Spool != null).FindAll(x => x.Spool.ProductType == spool.ProductType).Take(rest);
                    List<RobotArmTaskEntity> listarmtsk = new List<RobotArmTaskEntity>();
                    foreach (var item in armtskwhools)
                    {
                        armtsk = new RobotArmTaskEntity();
                        //抓到直通线的轮子
                        listarmtsk.Add(armtsk);
                        armtsk.Created = createtime;
                        armtsk.TaskGroupGUID = guid;
                        armtsk.FromWhere = item.SeqNo;
                        armtsk.PlantNo = PlantNo;
                        armtsk.RobotArmID = RobotArmID;
                        armtsk.TaskLevel = 6;
                        armtsk.TaskType = 1;
                        armtsk.ToWhere = MidStoreLine.InStoreLine;
                        armtsk.TaskStatus = 0;
                        armtsk.SpoolStatus = 0;
                        //armtsk.EquipControllerId = Convert.ToInt32(t.EquipContollerId);
                        //agvtsk.EquipIdListTarget += t.EquipContollerId.ToString();
                        armtsk.ProductType = spool.ProductType;
                    }
                    this.BusinessLogic.RobotArmTaskProvider.InsertArmTask(listarmtsk);
                    logger.InfoMethod("update RobotArmTask list success");
                    this.BusinessLogic.AGVTasksProvider.CreateAGVTask(agvtsk, null);
                    logger.InfoMethod("create agv task success");
                }
                else if (rest == 0)
                {//轮子数量正好 ,不需要从暂存库抓取,直接调用AGV
                    this.BusinessLogic.AGVTasksProvider.CreateAGVTask(agvtsk, null);
                    logger.InfoMethod("create agv task success");
                }
                else//rest < 0 直通线轮子多了
                {//将多余的轮子创建直通线入库任务
                    logger.InfoMethod("直通线轮子多了,将多余的轮子创建直通线入库任务,guid=" + guid.ToString());
                    var instore = midstores.FindAll(x => x.Spool == null).Take(rest).ToList();//取出空位置
                    List<RobotArmTaskEntity> instorearmtsks = new List<RobotArmTaskEntity>();
                    for (int i = rest; i > 0; i--)
                    {
                        barcode = barcodeQueue.Dequeue();

                        spool = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcode(barcode, null);
                        armtsk = new RobotArmTaskEntity();
                        //抓到直通线的轮子
                        instorearmtsks.Add(armtsk);
                        armtsk.Created = createtime;
                        armtsk.TaskGroupGUID = guid;
                        armtsk.FromWhere = instore[i].SeqNo;
                        instore[i].IdsList = spool.Id.ToString();
                        armtsk.PlantNo = PlantNo;
                        armtsk.RobotArmID = RobotArmID;
                        armtsk.TaskLevel = 7;
                        armtsk.TaskType = 2;
                        armtsk.ToWhere = MidStoreLine.InStoreLine;
                        armtsk.TaskStatus = 0;
                        armtsk.SpoolStatus = 0;
                    }
                    this.BusinessLogic.MidStorageProvider.UpdateMidStore(null, instore.ToArray());
                    this.BusinessLogic.RobotArmTaskProvider.InsertArmTask(instorearmtsks);
                    this.BusinessLogic.AGVTasksProvider.CreateAGVTask(agvtsk, null);
                    logger.InfoMethod("create agv task success");
                    logger.InfoMethod("直通线轮子多了,将多余的轮子创建直通线入库任务成功,guid=" + guid.ToString());
                }


            }
            else
            {
                logger.InfoMethod(" 没有需要该类型轮子的设备的任务,将所有轮子创建直通线入库任务");
                var guid = Guid.NewGuid();
                //通过GUID标记一个龙门任务单元
                DateTime createtime = DateTime.Now;
                List<RobotArmTaskEntity> instorearmtsks = new List<RobotArmTaskEntity>();
                var instore = midstores.FindAll(x => x.Spool == null).Take(barcodeQueue.Count).ToList();//取出空位置
                RobotArmTaskEntity armtsk = null;
                for (int i = 0; i <= instore.Count - 1; i++)
                {
                    barcode = barcodeQueue.Dequeue();

                    spool = this.BusinessLogic.SpoolsProvider.GetSpoolByBarcode(barcode, null);
                    armtsk = new RobotArmTaskEntity();
                    //抓到直通线的轮子
                    instorearmtsks.Add(armtsk);
                    armtsk.Created = createtime;
                    armtsk.TaskGroupGUID = guid;
                    armtsk.FromWhere = instore[i].SeqNo;
                    instore[i].IdsList = spool.Id.ToString();
                    armtsk.PlantNo = PlantNo;
                    armtsk.RobotArmID = RobotArmID;
                    armtsk.TaskLevel = 7;
                    armtsk.TaskType = 2;
                    armtsk.ToWhere = MidStoreLine.InStoreLine;
                    armtsk.TaskStatus = 0;
                    armtsk.SpoolStatus = 0;
                }

                this.BusinessLogic.MidStorageProvider.UpdateMidStore(null, instore.ToArray());
                logger.InfoMethod(" 没有需要该类型轮子的设备的任务,将所有轮子创建直通线入库任务成功");
            }




        }
    }
}
