using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.MidStorage
{
   public class MidStorageInfoDataUI
    {
        public long Id { get; set; }
        /// <summary>
        /// 库位状态:-1不可见区域,0禁用,1没有轮子(可放置),3有轮子(被占用),4待抓取,5待放置
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 条码数组,逗号分割
        /// </summary>
        public string Barcodes { get; set; }
        /// <summary>
        /// 是否超时
        /// </summary>
        public bool IsTimeOut { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime? InStoreageTime { get; set; }
    }
}
