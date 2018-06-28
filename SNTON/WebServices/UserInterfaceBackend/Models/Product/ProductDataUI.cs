using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.Product
{
    public class ProductDataUI
    {
        public long Id { get; set; }
        /// <summary>
        /// 单丝 
        /// </summary>

        public string ProductType { get; set; }

        /// <summary>
        /// M1
        /// </summary>

        public string ProductNo { get; set; }

        /// <summary>
        /// ProductionType
        /// </summary>

        public string Const { get; set; }

        /// <summary>
        /// WS18/WS44
        /// </summary>

        public string CName { get; set; }

        /// <summary>
        /// 电镀类型
        /// </summary>

        public string PlatingType { get; set; }
        /// <summary>
        /// LR配比
        /// </summary>

        public string LRRatio { get; set; }

        /// <summary>
        /// 长度
        /// </summary>

        public int Length { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public byte SeqNo { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string Created { get; set; }
        /// <summary>
        /// 是否删除-1
        /// </summary>
        public short IsDeleted { get; set; }
    }
}
