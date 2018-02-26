using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.MidStorage
{
    public class MidStorageBaseDataUI
    {
        public long Id { get; set; }
        /// <summary>
        /// 库区ID
        /// </summary>
        public int StorageArea { get; set; }
        /// <summary>
        /// HCoordinate
        /// </summary> 
        public int X { get; set; }

        /// <summary>
        /// VCoordinate
        /// </summary> 
        public int Y { get; set; }
        /// <summary>
        /// Description
        /// </summary> 
        //public string Description { get; set; }
        /// <summary>
        /// 库位状态:-1不可见区域,0禁用,1没有轮子(可放置),3有轮子(被占用),4待抓取,5待放置
        /// </summary>
        public int Status { get; set; }
        public string Barcodes { get; set; }
        /// <summary>
        /// 暂存库原始ID
        /// </summary>
        public int OriginalId { get; set; }
        public bool TimeOut { get; set; }
    }
}
