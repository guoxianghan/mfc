using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.AGV_KJ_Interface
{
    public class AGV_KJ_InterfaceDataUI
    {
       
        public  long ID
        {
            get;
            set;
        }

        /// <summary>
        /// DeviceID
        /// </summary>
       
        public  string DeviceID { get; set; }

        /// <summary>
        /// 输送线出口编号。0-不指定出口，由系统自动分配。1/2/3/4-指定出口。
        /// </summary>
       
        public  string ConveyorID { get; set; }

        /// <summary>
        /// -1：（AGV）预备任务0：（AGV）新任务 1：(科捷)输送线已接收（有库存,准备出库）2：（AGV）接收确认 3：(科捷)出库完成 4：（AGV）出库完成确认 5：（科捷）缓存到位 6：（AGV）正在取货//7：(科捷)任务完成 8：（AGV）完成确认（删除） 删除：松动
        /// </summary>
       
        public  int Status { get; set; }

        /// <summary>
        /// outOfStock
        /// </summary>       
        public  int outOfStock { get; set; }

        /// <summary>
        /// issuetime
        /// </summary>       
        public  DateTime? issuetime { get; set; }

        /// <summary>
        /// time_0
        /// </summary>       
        public  DateTime? time_0 { get; set; }

        /// <summary>
        /// time_1
        /// </summary>       
        public  DateTime? time_1 { get; set; }

        /// <summary>
        /// time_2
        /// </summary>       
        public  DateTime? time_2 { get; set; }

        /// <summary>
        /// time_3
        /// </summary>       
        public  DateTime? time_3 { get; set; }

        /// <summary>
        /// time_4
        /// </summary>       
        public  DateTime? time_4 { get; set; }

        /// <summary>
        /// time_5
        /// </summary>       
        public  DateTime? time_5 { get; set; }

        /// <summary>
        /// time_6
        /// </summary>       
        public  DateTime? time_6 { get; set; }

        /// <summary>
        /// time_7
        /// </summary>       
        public  DateTime? time_7 { get; set; }

        /// <summary>
        /// time_8
        /// </summary>       
        public  DateTime? time_8 { get; set; }
        /// <summary>
        /// time_8
        /// </summary>       
        public  DateTime? Created { get; set; }

        /// <summary>
        /// StorageArea
        /// </summary>       
        public  int StorageArea { get; set; }

        /// <summary>
        /// TaskGuid
        /// </summary>       
        public  Guid TaskGuid { get; set; }

        /// <summary>
        /// PlatNo
        /// </summary>       
        public  byte PlatNo { get; set; }

        /// <summary>
        /// SeqNo
        /// </summary>       
        public  int SeqNo { get; set; }

        /// <summary>
        /// Count
        /// </summary>       
        public  int Count { get; set; }
    }
}
