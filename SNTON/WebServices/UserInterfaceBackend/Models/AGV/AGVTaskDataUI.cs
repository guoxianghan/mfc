using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.AGV
{
    public class AGVTaskDataUI
    {
        public long id { get; set; }
        public int AGVId { get; set; }
        public List<string> EquipNames { get; set; } = new List<string>();
        /// <summary>
        /// 暂存库Id
        /// </summary>
        public int Storeageid { get; set; }
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
        public int Status { get; set; }
        /// <summary>
        /// 线体任务流水号
        /// </summary>
        public int SeqNo { get; set; }
        /// <summary>
        /// 1出库线;2直通线
        /// </summary>
        public int LineNo { get; set; }
        /// <summary>
        /// 1拉空轮;2拉满轮
        /// </summary>
        public int TaskType { get; set; }
        public DateTime Created { get; set; }
    }
}
