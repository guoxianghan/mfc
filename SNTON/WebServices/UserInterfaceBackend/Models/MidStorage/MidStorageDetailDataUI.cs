using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.MidStorage
{
    public class MidStorageDetailDataUI
    {
        public long Id { get; set; }
        /// <summary>
        /// 库位状态:-1不可见区域,0禁用,1没有轮子(可放置),3有轮子(被占用),4待抓取,5待放置,6空白区域
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 库区ID
        /// </summary>
        public int StorageArea { get; set; }
        /// <summary>
        /// 条码数组,逗号分割
        /// </summary>
        public string Barcodes { get; set; }
        public int Length { get; set; }
        public char BobbinNo { get; set; }
        public string Cname { get; set; }
        public string StructBarCode { get; set; }
        /// <summary>
        /// 规格 0.30ST
        /// </summary>
        public string Const { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 是否超时
        /// </summary>
        public bool IsTimeOut { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime? InStoreageTime { get; set; }
        public int OriginalId { get;  set; }
    }
    /// <summary>
    /// 库区,单丝类型的数量统计
    /// </summary>
    public class MidStorageCountDataUI
    {
        public int StorageArea { get; set; }
        public string Length { get; set; }
        public string Cname { get; set; }
        public string StructBarCode { get; set; }
        public string BobbinNo { get; set; }
        public int R { get; set; }
        public int L { get; set; }
        public int other { get; set; }
        public int Count { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// 规格 0.30ST
        /// </summary>
        public string Const { get; set; }
    }
}
