using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Requests.Spool
{
    public class SpoolTaskSearchRequest
    {

        /// <summary>
        /// 第几页
        /// </summary>
        public int pageNumber { get; set; }
        /// <summary>
        /// 每页多少条
        /// </summary>
        public int pageSize { get; set; }
        /// <summary>
        /// 入库时间1
        /// </summary>
        public DateTime? datetime1 { get; set; }
        /// <summary>
        /// 入库时间2
        /// </summary>
        public DateTime? datetime2 { get; set; }
        /// <summary>
        /// FdTagNo
        /// </summary>

        public string FdTagNo { get; set; } = "";

        /// <summary>
        /// ProductType
        /// </summary>

        public string ProductType { get; set; } = "";

        /// <summary>
        /// CName
        /// </summary>

        public string CName { get; set; } = "";

        /// <summary>
        /// Const
        /// </summary>

        public string Const { get; set; } = "";

        /// <summary>
        /// Length
        /// </summary>

        public int Length { get; set; }
        /// <summary>
        /// Length
        /// </summary>

        public int StorageArea { get; set; }

        /// <summary>
        /// BobbinNo
        /// </summary>

        public string BobbinNo { get; set; } = "";
    }
}
