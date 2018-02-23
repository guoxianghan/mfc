using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VI.MFC.COM;

namespace SNTON.Com
{
    public interface IMXCommModule : ICommModule
    {
        //bool SendData(List<MXDataBlock> data);
        bool Try2SendData(List<MXDataBlock> dataBlockList, short maxTryReadCount = 1);
        Neutrino Try2ReadData(List<MXDataBlock> dataBlockList, short maxReadCount = 1);
        /// <summary>
        /// Try to read data with reading result sign feedback
        /// </summary>
        /// <param name="dataBlock">Data Block information</param>
        /// <param name="maxReadCount">max read count while connection is unavailable</param>
        /// <returns>return true or false sign, data neutrino</returns>
        Tuple<bool, Neutrino> Try2ReadDataWithSign(List<MXDataBlock> dataBlockList, short maxReadCount = 1);

    }
}
