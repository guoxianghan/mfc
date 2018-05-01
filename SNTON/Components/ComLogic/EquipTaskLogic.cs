using Newtonsoft.Json;
using SNTON.Constants;
using SNTON.Entities.DBTables.AGV;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.InStoreToOutStore;
using SNTON.Entities.DBTables.MES;
using SNTON.Entities.DBTables.Message;
using SNTON.Entities.DBTables.MidStorage;
using SNTON.Entities.DBTables.RobotArmTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC.Logging;
using VI.MFC.Utils;
using static SNTON.Constants.SNTONConstants;

namespace SNTON.Components.ComLogic
{
    /// <summary>
    /// 根据地面滚筒请求，创建AGV任务
    /// </summary>
    public class EquipTaskLogic : ComLogic
    {
        public new static EquipTaskLogic Create(XmlNode configNode)
        {
            EquipTaskLogic equip = new EquipTaskLogic();
            equip.Init(configNode);
            return equip;
        }
        private VIThreadEx thread_ReadDervice;
        protected override void StartInternal()
        {
            thread_ReadDervice.Start();
            base.StartInternal();
        }
        public EquipTaskLogic()
        {
            //thread_ReadDervice = new VIThreadEx(InitRobotAGVTask, null, "thread for InitRobotAGVTask", 3000);
            thread_ReadDervice = new VIThreadEx(RunCreateTask, null, "thread for InitRobotAGVTask", 8000);
        }

        void RunCreateTask()
        {
            try
            {
                InitAGVUnionTask();
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("执行InitAGVUnionTask出错", ex);
            }
            try
            {
                InitRobotAGVTask();
            }
            catch (Exception ex)
            {

                logger.ErrorMethod("执行InitRobotAGVTask出错", ex);
            }
        }
        /// <summary>
        /// 轮询地面滚筒请求,分车间,暂存库,供料区域,AGV路线创建龙门AGV任务
        /// </summary>
        private void InitRobotAGVTask()
        {
            //int seq = getNextSeqNo();
            //this.BusinessLogic.AGVTasksProvider.CreateAGVTask(new AGVTasksEntity() {  Created=DateTime.Now, ProductType="WS44",SeqNo=seq, PlantNo=3}, null);
            int minutes = 20;
            var taskouttime = this.BusinessLogic.SystemParametersProvider.GetSystemParamrters(3, null);
            if (taskouttime != null)
                minutes = Convert.ToInt32(taskouttime.ParameterValue.Trim());
            var equiptsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities($"Status IN (0,10) AND PlantNo=3 AND Created<='{DateTime.Now.AddMinutes(-minutes)}'", null);
            //equiptsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities($"TASKGUID='D178EFE4-3AE5-4BC6-9C39-33D32A1AB81E'", null);
            //equiptsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities("Id IN(37029,37020)", null);
            var agvrunningtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTasks("IsDeleted=0 and TaskType=2 AND [Status] IN(1,2,3,4,8)", null);
            if (agvrunningtsk == null)
                agvrunningtsk = new List<AGVTasksEntity>();
            if (equiptsks == null || equiptsks.Count == 0)
                return;
            equiptsks = equiptsks.OrderBy(x => x.Id).ToList();
            var kong = equiptsks.FindAll(x => x.TaskType == 1);
            DateTime dt = DateTime.Now;
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
                agvout.PLCNo = equiptsk[0].PLCNo;
                bool r = this.BusinessLogic.SqlCommandProvider.EmptyAGVTask(equiptsk, agvout);
                if (r)
                    logger.ErrorMethod("更新拉空轮任务成功:" + JsonConvert.SerializeObject(equiptsk));
                else
                {
                    logger.ErrorMethod("更新拉空轮任务失败:" + JsonConvert.SerializeObject(equiptsk));
                }
            }

            #endregion

            var agvtsk2 = equiptsks.FindAll(x => x.TaskType == 2);
            CheckEquipTaskToCreate(agvtsk2);
        }
        /// <summary>
        /// 检测是否可以创建送满轮任务
        /// </summary>
        /// <param name="list"></param>
        void CheckEquipTaskToCreate(List<EquipTaskViewEntity> list)
        {
            var agvrunningtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTasks("IsDeleted=0 and TaskType=2 AND [Status] IN(1,2,3,4,8)", null);
            var armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskStatus in(0,1,2,3)");//找到正在执行的ArmTask
            if (armtsks == null)
                armtsks = new List<RobotArmTaskEntity>();
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
                    var tuple = InStoreHasSpools(equiptsks[0], i);
                    if (tuple != null)
                    {
                        var agvtsk = tuple.Item1;
                        var instore = tuple.Item2;
                        iscreated = this.BusinessLogic.SqlCommandProvider.InStoreToOutStoreLine(instore, agvtsk, equiptsks, null);
                        if (iscreated)
                        {
                            //equiptsks.ToList().ForEach(x => x.Status = 4);
                            //int count = this.BusinessLogic.EquipTaskViewProvider.Update(null, equiptsks.ToArray());
                            //if (count != 0)
                            break;
                        }
                    }
                }
                if (!iscreated)
                {
                    for (short storeno = 1; storeno <= 3; storeno++)
                    {
                        armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskStatus in(0,1,2,3)");//找到正在执行的ArmTask
                        agvrunningtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTasks("IsDeleted=0 and TaskType=2 AND [Status] IN(1,2,3,4,8,9)", null);
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
                            if (storeno == 2 || storeno == 3)
                            {
                                //equiptsks.ToList().ForEach(x => x.Status = 10);
                                //this.BusinessLogic.EquipTaskViewProvider.Update(null, equiptsks.ToArray());
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
        [Obsolete("弃用的方法,此方法未封装事务,用另一个重载方法代替,")]
        bool InStoreHasSpools(List<EquipTaskViewEntity> equiptsks, int storeageno)
        {
            equiptsks = equiptsks.OrderBy(x => x.AStation).ToList();
            var equiptsk = equiptsks[0];
            var list = this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.GetInStoreToOutStoreSpoolEntity($"StoreageNo={storeageno} AND PlantNo={PlantNo} AND Status=3", null);
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
            if (instore == null || instore.Count == 0)
                return false;
            instore.ForEach(x => x.Status = 8);//等待申请调度AGV
            int count = this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.UpdateEntity(null, instore.ToArray());
            var agvtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTask($"TaskGuid in ('{instore[0].Guid.ToString()}')");
            if (agvtsk != null)
            {
                agvtsk.Status = 2;
                agvtsk.PLCNo = equiptsks[0].PLCNo;
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
        /// 直通口是否有准备好待接收的单丝
        /// </summary>
        /// <param name="equiptsk"></param>
        /// <param name="storeageno"></param>
        /// <returns></returns>
        Tuple<AGVTasksEntity, List<InStoreToOutStoreSpoolViewEntity>> InStoreHasSpools(EquipTaskViewEntity equiptsk, int storeageno)
        {
            var list = this.BusinessLogic.InStoreToOutStoreSpoolViewProvider.GetInStoreToOutStoreSpool(storeageno, this.PlantNo, null);
            if (list == null || list.Count == 0)
                return null;
            var equip = this.BusinessLogic.EquipConfigerProvider.EquipConfigers.FirstOrDefault(x => x.ControlID == equiptsk.EquipContollerId);
            var machstructcode = this.BusinessLogic.tblProdCodeStructMachProvider.GettblProdCodeStructMachs(null, equip.MachCode.Trim());
            if (machstructcode == null || machstructcode.Count == 0)
                return null;
            tblProdCodeStructMachEntity prod = machstructcode[0];
            if (prod.ProdCodeStructMark4 == null)
            {
                return null;
            }
            var instore = list.FindAll(x => x.Status == 3 && x.StructBarCode.Trim() == prod.ProdCodeStructMark4.StructBarCode.Trim());
            if (instore == null || instore.Count == 0)
                return null;
            var agvtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTask($"TaskGuid in ('{instore[0].Guid.ToString()}')");
            if (agvtsk != null)
                return new Tuple<AGVTasksEntity, List<InStoreToOutStoreSpoolViewEntity>>(agvtsk, instore);
            else return null;
        }
        /*
        正常出库逻辑
        先判断那个库有单丝,
        如果
        */
        int seq = 1;
        int lastseqno = 1;
        Random ran = new Random(DateTime.Now.Second);

        List<int> SeqNo3 = new List<int>();
        int getNextSeqNo()
        {
            while (true)
            {
                if (seq == 1 || seq == 30000)
                    seq = ran.Next(1, 30000);
                else seq++;

                if (!SeqNo3.Exists(x => x == seq))
                {
                    lastseqno = seq;
                    if (SeqNo3.Count >= 6)
                        SeqNo3.RemoveAt(0);
                    SeqNo3.Add(lastseqno);
                    break;
                }
            }
            return lastseqno;
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

            int seq = getNextSeqNo();
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
            bool result = CreateOutStoreageTask(storageno, lineno, seq, exequiptsk, tskconfig, creaequptsk, needlspools, needrspools);
            if (result)
                return 1;//一次只执行一个龙门Task  
            else return 0;
        }

        //暂存库单丝轮数量足够创建一车任务并且需要该类型单丝轮的设备也满一车
        //创建龙门Task和AGVTask
        //更新EquipTask状态
        //更新库位状态
        private bool CreateOutStoreageTask(short storageno, int lineno, int seq, EquipTaskViewEntity exequiptsk, Tuple<int, int, int, int, int> tskconfig, List<EquipTaskViewEntity> creaequptsk, List<MidStorageSpoolsEntity> needlspools, List<MidStorageSpoolsEntity> needrspools)
        {
            var guid = Guid.NewGuid();
            //通过GUID标记一个龙门任务单元
            DateTime createtime = DateTime.Now;
            RobotArmTaskEntity armtsk = null;
            List<RobotArmTaskEntity> listarmtsk = new List<RobotArmTaskEntity>();
            var agvtsk = new AGVTasksEntity() { Created = createtime, SeqNo = seq, TaskGuid = guid, PlantNo = PlantNo, ProductType = exequiptsk.ProductType, Status = 0, TaskLevel = 5 };
            int seqno = 0;
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


            foreach (var equtsk in creaequptsk)
            {
                equtsk.Status = 1;
                equtsk.TaskGuid = guid;
                equtsk.Updated = createtime;
                agvtsk.EquipIdListActual = agvtsk.EquipIdListActual + ";" + equtsk.EquipContollerId.ToString();
            }
            agvtsk.EquipIdListActual = (from i in creaequptsk select i.EquipContollerId).ToArray().ToString(';');
            var ser = (from i in creaequptsk select i.TaskType).ToArray().ToString(' ').Replace(" ", "");
            agvtsk.TaskType = 2;
            if (ser == "2121")
            {
                agvtsk.TaskType = 3;
            }
            else if (ser == "2211")
            {
                agvtsk.TaskType = 4;
            }
            #region 更新EquipTask 更新暂存库位置状态 创建AGVTask 创建龙门Task
            agvtsk.StorageArea = storageno;
            agvtsk.StorageLineNo = lineno;
            agvtsk.EquipIdListActual = agvtsk.EquipIdListActual.Trim(';');
            agvtsk.EquipIdListTarget = TaskConfig.AGVStation(storageno, lineno);


            List<MidStorageSpoolsEntity> updamid = new List<MidStorageSpoolsEntity>();
            updamid.AddRange(needlspools);
            updamid.AddRange(needrspools);
            creaequptsk.ForEach(x => x.Status = 1);
            logger.InfoMethod("############################################################################################################");
            bool result = this.BusinessLogic.SqlCommandProvider.OutStoreageTask(creaequptsk, updamid, agvtsk, listarmtsk, null);
            logger.InfoMethod("###创建龙门出库任务:" + result + "," + JsonConvert.SerializeObject(agvtsk));
            logger.InfoMethod("############################################################################################################");
            #endregion
            return result;
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

        /// <summary>
        /// 根据收到的空轮满轮任务请求,创建AGVtask
        /// </summary>
        void InitAGVUnionTask()
        {
            var equiptsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities("Status IN (0,10) AND PlantNo=3", null);
            //equiptsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities("Id IN(37029,37020)", null);
            var agvrunningtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTasks("IsDeleted=0 and TaskType=2 AND [Status] IN(1,2,3,4,8)", null);
            if (agvrunningtsk == null)
                agvrunningtsk = new List<AGVTasksEntity>();
            if (equiptsks == null || equiptsks.Count == 0)
                return;
            equiptsks = equiptsks.OrderBy(x => x.Id).ToList();
            var routeStructcode = from i in equiptsks.FindAll(x => x.TaskType == 2)
                                  group new EquipTaskViewEntity() { Supply1 = i.Supply1.Trim(), AGVId = i.AGVId, AGVRoute = i.AGVRoute.Trim(), AGVStatus = i.AGVStatus, AStation = i.AStation, BStation = i.BStation, Created = i.Created, Deleted = i.Deleted, EquipContollerId = i.EquipContollerId, EquipFlag = i.EquipFlag, Id = i.Id, IsDeleted = i.IsDeleted, Length = i.Length, Length2 = i.Length2, PlantNo = i.PlantNo, PLCNo = i.PLCNo, ProductType = i.ProductType, Source = i.Source, Status = i.Status, StorageArea = i.StorageArea, Supply2 = i.Supply2, SupplyQty1 = i.SupplyQty1, SupplyQty2 = i.SupplyQty2, TaskGuid = i.TaskGuid, TaskLevel = i.TaskLevel, TaskType = i.TaskType, TitleProdName = i.TitleProdName, Updated = i.Updated }
                                  by new { i.AGVRoute, i.Supply1 }
                               into t
                                  select t;
            foreach (var items in routeStructcode)
            {//满
                if (items.Count() < 2)
                    continue;
                var kong = equiptsks.FindAll(x => x.AGVRoute.Trim() == items.Key.AGVRoute.Trim() && x.TaskType == 1);
                if (kong.Count < 2)
                    continue;
                var tsks = items.ToList();
                tsks.AddRange(kong);
                tsks = tsks.OrderBy(x => x.AStation).ToList();
                //同排 2211,2121  
                DateTime dt = DateTime.Now;

                var resultcom = PermutationAndCombination<EquipTaskViewEntity>.GetCombination(tsks.ToArray(), 4);
                List<EquipTaskViewEntity[]> list = new List<EquipTaskViewEntity[]>();
                foreach (var item in resultcom)
                {
                    var ser = (from i in item select i.TaskType).ToArray().ToString(' ').Replace(" ", "");
                    if (ser == "2121" || ser == "2211")
                        list.Add(item);
                }
                if (list.Count == 0)
                    continue;
                #region 找到时间最早的
                float time = float.MaxValue;
                int id = -1;
                for (int i = 0; i <= list.Count - 1; i++)
                {//找到时间最早的
                    var ids = SNTONConstants.ToAverage((from ci in list[i] select ci.Id).ToArray());
                    if (ids <= time)
                    {
                        time = ids;
                        id = i;
                    }
                }
                #endregion
                var finallytask = list[id].ToList();

                //*创建任务

                var mantsk = finallytask.FindAll(x => x.TaskType == 2);
                bool iscreated = false;
                int result = 0;
                for (short i = 1; i <= 3; i++)
                {
                    #region 直通口
                    ///直通口检测是否可以创建送满轮任务
                    var tuple = InStoreHasSpools(mantsk[0], i);
                    if (tuple != null)
                    {
                        var agvtsk = tuple.Item1;
                        var instore = tuple.Item2;
                        //
                        iscreated = this.BusinessLogic.SqlCommandProvider.InStoreToOutStoreLine(instore, agvtsk, finallytask, null);
                        if (iscreated)
                        {
                            iscreated = true;
                            logger.InfoMethod("创建直通口运动成功,guid:" + agvtsk.TaskGuid.ToString());
                            break;
                        }
                        else
                        {
                            logger.InfoMethod("创建直通口出库任务失败,guid:" + agvtsk.TaskGuid.ToString());
                        }
                    }
                    #endregion
                }
                if (!iscreated)
                {
                    #region 从暂存库获取
                    for (short storeno = 1; storeno <= 3; storeno++)
                    {
                        var armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskStatus in(0,1,2,3)");//找到正在执行的ArmTask
                        agvrunningtsk = this.BusinessLogic.AGVTasksProvider.GetAGVTasks("IsDeleted=0 and TaskType=2 AND [Status] IN(1,2,3,4,8,9)", null);
                        #region MyRegion
                        if (armtsks.FindAll(x => x.StorageArea == storeno).Count >= 3)
                            continue;//有正在执行的龙门任务
                        if (agvrunningtsk.FindAll(x => x.StorageArea == storeno && x.StorageLineNo == 1).Count >= 3)
                            continue;//出库线体满
                                     ///从暂存库创建出库任务
                        var tskconfig = TaskConfig.GetEnoughAGVEquipCount(mantsk[0].ProductType.Trim());//8 / 12
                        result = IsEnoughToOutStoreageTask(storeno, finallytask, 1, tskconfig);
                        if (result == 1)
                        {//创建 成功
                            iscreated = true;
                            break;
                        }
                        else
                        {
                            if (storeno == 2 || storeno == 3)
                            {
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// 检测是否有能够创建AGV任务的EquipTask
        /// 1创建任务;0没有创建任务,-1库里单丝不够
        /// </summary>
        /// <param name="storageno"></param>
        /// <param name="supply1">按照单丝标准书分组的单组设备任务</param>
        /// <returns>1创建任务;0没有创建任务</returns>
        private int IsEnoughToOutStoreageTask(short storageno, List<EquipTaskViewEntity> creaequptsk, int lineno, Tuple<int, int, int, int, int> tskconfig)
        {
            int seq = getNextSeqNo();
            var exequiptsk = creaequptsk.FirstOrDefault(x => x.TaskType == 2);
            //判断lr
            //var tskconfig = TaskConfig.GetEnoughAGVEquipCount(exequiptsk.ProductType.Trim());//8 / 12
            if (tskconfig.Item1 == 0)
            {
                this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = "未知的单丝型号," + exequiptsk.ProductType.Trim(), MsgLevel = 6, Source = "未知的单丝型号" });
                return 0;
            }

            int needstoeageno = tskconfig.Item1;
            var mids = this.BusinessLogic.MidStorageSpoolsProvider.GetMidStorages($"IsOccupied=1 AND StructBarCode='{exequiptsk.Supply1.Trim()}' AND  StorageArea={storageno}", null);
            if (mids == null || mids.Count == 0)
            {
                //logger.WarnMethod("没有该长度的单丝");
                return -1;
            }
            var needlspools = mids.OrderBy(x => x.Updated).ToList().FindAll(x => x.BobbinNo == 'L').Take(tskconfig.Item4).ToList();
            var needrspools = mids.OrderBy(x => x.Updated).ToList().FindAll(x => x.BobbinNo == 'R').Take(tskconfig.Item5).ToList();
            if (tskconfig.Item4 > needlspools.Count() || tskconfig.Item5 > needrspools.Count() || needstoeageno > mids.Count)
            {
                //更新设备任务状态10,料不够                
                return -1;
            }
            bool result = CreateOutStoreageTask(storageno, lineno, seq, exequiptsk, tskconfig, creaequptsk, needlspools, needrspools);
            if (result)
                return 1;//一次只执行一个龙门Task  
            else return 0;
        }
    }

}
