using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VI.MFC.Components.Parser;
using VI.MFC.COM;

namespace SNTON.Components.Parser
{
    public interface IMXParser : IParser
    {
        Tuple<bool, Neutrino> ReadData(Neutrino neu2Read, bool withReadResultSign = true, short maxReadCount = 1);
        bool SendData(Neutrino neu2Send, short maxSendCount = 1);

    }
}
