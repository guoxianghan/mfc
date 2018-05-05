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
        Tuple<bool, Neutrino> ReadData(string name, params string[] dbname);
        Tuple<bool, int> ReadData(string dbname, string thename = "", bool withReadResultSign = true, short maxReadCount = 1);
        bool SendData(Neutrino neu2Send, short maxSendCount = 1);
        bool SendData(string dbname, int value = 0, string thename = "", short maxSendCount = 1);
    }
}
