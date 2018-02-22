using Newtonsoft.Json;
using SNTON.Entities.DBTables.AGV;
using SNTON.Entities.DBTables.Equipments;
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
            thread_ReadDervice = new VIThreadEx(InitRobotAGVTask, null, "InitRobotAGVTask", 5000);
        }
        /// <summary>
        /// 轮询地面滚筒请求,分车间,暂存库,供料区域,AGV路线创建龙门AGV任务
        /// </summary>
        private void InitRobotAGVTask()
        {
            //int seq = getNextSeqNo();
            //this.BusinessLogic.AGVTasksProvider.CreateAGVTask(new AGVTasksEntity() {  Created=DateTime.Now, ProductType="WS44",SeqNo=seq, PlantNo=3}, null);
            var equiptsks = this.BusinessLogic.EquipTaskViewProvider.GetEquipTaskViewEntities("Status IN (0,10) AND PlantNo=3", null);
            if (equiptsks == null || equiptsks.Count == 0)
                return;
            equiptsks = equiptsks.OrderBy(x => x.Created).ToList();
            var kong = equiptsks.FindAll(x => x.TaskType == 1);
            DateTime dt = DateTime.Now;
            #region 创建拉空轮的任务 
            AGVTasksEntity agvout = null;
            var agvrouteline = kong.GroupBy(x => x.AGVRoute.Trim());
            foreach (var item in agvrouteline)
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
                bool r = this.BusinessLogic.SqlCommandProvider.EmptyAGVTask(equiptsk, agvout); 
                if (r)
                    logger.ErrorMethod("更新拉空轮任务成功:" + JsonConvert.SerializeObject(equiptsk));
                else
                {
                    logger.ErrorMethod("更新拉空轮任务失败:" + JsonConvert.SerializeObject(equiptsk));
                } 
            }

            #endregion
            var three_equiptsks = equiptsks.FindAll(x => x.StorageArea == "3" && x.TaskType == 2);
            var two_equiptsks = equiptsks.FindAll(x => x.StorageArea == "12" && x.TaskType == 2);

            var grouptwo = three_equiptsks.GroupBy(x => x.AGVRoute);
            var armtsks = this.BusinessLogic.RobotArmTaskProvider.GetRobotArmTasks($"TaskStatus in(0,1,2,3)");//找到正在执行的ArmTask
            if (armtsks == null)
                armtsks = new List<RobotArmTaskEntity>();
            #region 3#暂存库 拉满轮任务
            var threearmtsks = armtsks.FindAll(x => x.StorageArea == 3);
            if (threearmtsks.Count == 0)
            {//创建3#暂存库的龙门Task和AGVTask,同时更新EquipTask状态    
                CreateRobotAGVTask(3, three_equiptsks, 0, 1);
            }


            int iscreated = 0;
            var onearmtsks = armtsks.FindAll(x => x.StorageArea == 1);
            if (onearmtsks.Count == 0)
            {//创建1#暂存库的龙门Task和AGVTask,同时更新EquipTask状态    
                iscreated = CreateRobotAGVTask(1, two_equiptsks, 0, 1);
            }
            var twoarmtsks = armtsks.FindAll(x => x.StorageArea == 2);
            if (twoarmtsks.Count == 0 && iscreated == 0)
            {//创建2#暂存库的龙门Task和AGVTask,同时更新EquipTask状态    
                CreateRobotAGVTask(2, two_equiptsks, 0, 1);
            }

            #endregion

        }

        /*
        正常出库逻辑
        先判断那个库有单丝,
        如果
        */
        int seq = 1;
        int lastseqno = 1;
        Random ran = new Random(DateTime.Now.Second);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageno">暂存库编号,1,2,3</param>
        /// <param name="list">所有未分配任务的送料EquipTask</param> 
        /// <param name="inlinecount">直通线上的单丝数量</param>
        /// <param name="lineno">线体编号</param>
        ///<returns></returns>
        public int CreateRobotAGVTask(short storageno, List<EquipTaskViewEntity> list, int inlinecount, int lineno = 1)
        {
            int result = 0;
            //list = list.OrderBy(x => x.Created).ToList();
            List<EquipTaskViewEntity> Storage = null;
            if (storageno == 3)
                Storage = list.FindAll(x => x.StorageArea.Trim() == storageno.ToString());
            else Storage = list.FindAll(x => x.StorageArea.Trim() == "12");
            var agvroutes = Storage.GroupBy(x => x.AGVRoute.Trim());
            foreach (var item in agvroutes)
            {
                var supply1s = item.GroupBy(x => x.Supply1);//按照作业标准书编号分组
                foreach (var supply1 in supply1s)
                {//所有同种规格的单丝 
                    if (supply1.Count() < 2)
                        continue;
                    result = NewMethodLR(storageno, supply1, inlinecount, lineno);
                    if (result == 1)
                        return 1;
                    else if (result == -1 && (storageno == 2 || storageno == 3))
                    {
                        supply1.ToList().ForEach(x => x.Status = 10);
                        this.BusinessLogic.EquipTaskViewProvider.Update(null, supply1.ToArray());
                    }
                }
            }
            return 0;
        }
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
            var tskconfig = TaskConfig.GetEnoughAGVEquipCount(exequiptsk.ProductType);//8 / 12
            if (tskconfig.Item1 == 0)
            {
                this.BusinessLogic.MessageInfoProvider.Add(null, new MessageEntity() { Created = DateTime.Now, MsgContent = exequiptsk.ProductType.Trim(), MsgLevel = 6, Source = "未知的单丝型号" });
                return 0;
            }
            var creaequptsk = supply1.OrderByDescending(x => x.Created).Take(tskconfig.Item2).OrderBy(x => x.AStation).ToList();
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
            var agvtsk = new AGVTasksEntity() { Created = createtime, SeqNo = seq, TaskGuid = guid, PlantNo = PlantNo, ProductType = exequiptsk.ProductType, Status = 0, TaskLevel = 5 };
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


            List<MidStorageSpoolsEntity> updamid = new List<MidStorageSpoolsEntity>();
            updamid.AddRange(needlspools);
            updamid.AddRange(needrspools);
            logger.InfoMethod("############################################################################################################");
            bool result = this.BusinessLogic.SqlCommandProvider.OutStoreageTask(creaequptsk, updamid, agvtsk, listarmtsk);
            logger.InfoMethod("###创建龙门出库任务:" + result + "," + guid.ToString());
            //this.BusinessLogic.EquipTaskViewProvider.Update(null, creaequptsk.ToArray());
            //this.BusinessLogic.MidStorageSpoolsProvider.UpdateMidStore(null, needlspools.ToArray());
            //this.BusinessLogic.MidStorageSpoolsProvider.UpdateMidStore(null, needrspools.ToArray());
            //this.BusinessLogic.AGVTasksProvider.CreateAGVTask(agvtsk, null); //同时创建AGVTask   
            //this.BusinessLogic.RobotArmTaskProvider.InsertArmTask(listarmtsk, null);
            //logger.InfoMethod(JsonConvert.SerializeObject(sqlcmds));
            logger.InfoMethod("############################################################################################################");
            #endregion
            return 1;//一次只执行一个龙门Task  
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
