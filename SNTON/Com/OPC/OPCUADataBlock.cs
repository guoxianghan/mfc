using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Com
{
    public class OPCUADataBlock
    {
        public string Name { get; set; }
        /// <summary>
        /// 软元件名
        /// </summary>
        public string DBName { get; set; }
        /// <summary>
        /// 写入点数 
        /// </summary>
        public int DBLength { get; set; }
        /// <summary>
        /// 写入的软元件值
        /// </summary>
        public short[] DBDataIn2Bytes { get; set; }
        public int[] DBDataInIn4Bytes { get; set; }
        public string DBDataValue { get; set; }
        public int Result { get; set; } = 0;
        public string Type { get; set; }
    }
}
