using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.Equip
{
    public class EquipTaskStatusDataUI
    {
        public long EquipTaskID { get; set; }
        public long EquipControllerId { get; set; }
        /// <summary>
        /// 控制的两个机台的编号3B-10-10
        /// </summary>
        public string EquipName { get; set; }
        public string Supply1 { get; set; }
        public int PlantNo { get; set; }
        /// <summary>
        /// 需要的单丝长度
        /// </summary>
        public int Length { get; set; }
        public string StorageArea { get; set; }
        public string AGVRoute { get; set; }
        public int TaskType { get; set; }
        /// <summary>
        /// 0初始化EquipTask,1创建AGVTask和龙门Task,2正在抓取,3,抓取完毕,4等待调度AGV,5已调度AGV,6AGV运行中,7任务完成(拉空论或满轮),8任务失败,9已通知地面滚筒创建好任务
        /// </summary>
        public int Status { get; set; }
        public DateTime Created { get; set; }
        public int TaskLevel { get; set; }
        public string ProductType { get; set; }
        public Guid TaskGuid { get; set; }
        public string AGVID { get; set; }

    }
}
