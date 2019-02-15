using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models
{
    public class SpoolDataUI
    {
        /// <summary>
        /// 工字轮唯一标识码
        /// </summary>
        public long SpoolId;

        /// <summary>
        /// 工字轮条码
        /// </summary>
        public string Barcode;

        /// <summary>
        /// 工字轮产品类型
        /// </summary>
        public string ProductType;

        /// <summary>
        /// 工字轮在中间暂存库的位置
        /// </summary>
        public string MidStorageLocation;

        /// <summary>
        /// 生产半成品工字轮机组名称
        /// </summary>
        public string EquipNameFrom;

        /// <summary>
        /// 生产半成品工字轮机组编码
        /// </summary>
        public int EquipIdFrom;

        /// <summary>
        /// 半成品工字轮到成品产品的生产机组名称
        /// </summary>
        public string EquipNameTo;

        /// <summary>
        /// 半成品工字轮到成品产品的生产机组编码
        /// </summary>
        public int EquipIdTo;

    }
}
