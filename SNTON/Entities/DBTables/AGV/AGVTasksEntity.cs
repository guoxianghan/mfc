using SNTON.Constants;
using SNTON.Entities.DBTables.Equipments;
using SNTON.Entities.DBTables.RobotArmTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Entities.DBTables.AGV
{
    /// <summary>
    /// DB table entity class
    /// </summary>
    public class AGVTasksEntity : EntityBase
    {

        /// <summary>
        /// TaskNo
        /// </summary>
        [DataMember]
        public virtual long TaskNo { get; set; }

        /// <summary>
        /// 0=未知，1拉空轮,2拉满轮
        ///1=3#和5#车间，调度AGV到中间暂存库拉半成品料送至机组
        ///2=4#车间，调度AGV送半成品料至中间暂存库存放
        ///3=4#车间，调度AGV至中间暂存库拉空轮
        /// </summary>
        [DataMember]
        public virtual byte TaskType { get; set; }
        /// <summary>
        /// 线体任务流水号
        /// </summary>
        [DataMember]
        public virtual int SeqNo { get; set; }
        /// <summary>
        /// 暂存库线体编号,1出库线;2直通线
        /// </summary>
        [DataMember]
        public virtual int StorageLineNo { get; set; }
        /// <summary>
        /// 暂存库号1,2,3
        /// </summary>
        [DataMember]
        public virtual int StorageArea { get; set; }

        /// <summary>
        /// AGVId
        /// </summary>
        [DataMember]
        public virtual short AGVId { get; set; }

        /// <summary>
        /// 任务停靠点
        /// 上料点,暂存库线体停靠点编号,分号分隔
        /// </summary>
        [DataMember]
        public virtual string EquipIdListTarget { get; set; } = "";

        /// <summary>
        /// 实际停靠点和进度
        /// 分号分割的设备ID
        /// </summary>
        [DataMember]
        public virtual string EquipIdListActual { get; set; } = "";
        /// <summary>
        /// ProductType
        /// </summary>
        [DataMember]
        public virtual string ProductType { get; set; } = "";

        /// <summary>
        /// 0,创建;
        /// 1,正在执行抓轮子;
        /// 2,线体准备好后,Ready 生成AGVTasks的TaskNo;
        /// 3,小车从线体接收完轮子后将状态改完Release
        /// 4,已发送
        /// 7,直通口单丝已到位
        /// 8,收到回复
        /// 16,该任务正在运行,但尚未完成
        /// 17,已通知地面滚筒
        /// 32,对取消进行确认
        /// 64,由于未处理异常的原因而完成的任务
        /// 128, 已成功完成执行的任务
        /// </summary>
        [DataMember]
        public virtual int Status { get; set; }
        /// <summary>
        /// AGV任务级别
        /// 0,最低
        /// 2,出库口
        /// 4,异常出库
        /// 6直通线
        /// </summary>
        [DataMember]
        public virtual int TaskLevel { get; set; }
        [DataMember]
        public virtual int PLCNo { get; set; }
        /// <summary>
        /// PlantNo
        /// </summary>
        [DataMember]
        public virtual byte PlantNo { get; set; }

        /// <summary>
        /// 与RobotArmTask关联的GUID
        /// </summary>
        [DataMember]
        public virtual Guid TaskGuid { get; set; }
        public virtual bool IsHasSpools
        {
            get
            {
                SNTONConstants.AGVTaskStatus st = (SNTONConstants.AGVTaskStatus)Status;

                switch (st)
                {
                    case SNTONConstants.AGVTaskStatus.Canceled:
                    case SNTONConstants.AGVTaskStatus.Created:
                    case SNTONConstants.AGVTaskStatus.Faulted:
                    case SNTONConstants.AGVTaskStatus.Finished:
                    case SNTONConstants.AGVTaskStatus.Ready:
                        return false;
                    case SNTONConstants.AGVTaskStatus.Received:
                    case SNTONConstants.AGVTaskStatus.Running:
                    case SNTONConstants.AGVTaskStatus.Sent:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public virtual List<Spools.SpoolsEntity> Spools { get; set; } = new List<Spools.SpoolsEntity>();
        public virtual List<RobotArmTaskEntity> _RobotArmTasks { get; set; }
        public virtual List<EquipTaskEntity> _EquipTasks { get; set; }
        public virtual List<EquipTaskView2Entity> _EquipTasks2 { get; set; }
    }
}