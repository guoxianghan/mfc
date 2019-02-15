using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VI.MFC.COM;

namespace SNTON.Com 
{
    public interface IOPCUACommModule : ICommModule
    {

        //bool SendData(List<OPCUADataBlock> data);
        bool Try2SendData(List<OPCUADataBlock> dataBlockList, short maxTryReadCount = 1);
        Neutrino Try2ReadData(List<OPCUADataBlock> dataBlockList, short maxReadCount = 1);
        /// <summary>
        /// Try to read data with reading result sign feedback
        /// </summary>
        /// <param name="dataBlock">Data Block information</param>
        /// <param name="maxReadCount">max read count while connection is unavailable</param>
        /// <returns>return true or false sign, data neutrino</returns>
        Tuple<bool, Neutrino> Try2ReadDataWithSign(List<OPCUADataBlock> dataBlockList, short maxReadCount = 1);
    }
}
