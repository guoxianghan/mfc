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
        bool Try2ReadData2(List<OPCUADataBlock> dataBlockList, short maxReadCount = 1);
        void SubscribeEvent(string tag, Action<bool, dynamic> action);
    }
}
