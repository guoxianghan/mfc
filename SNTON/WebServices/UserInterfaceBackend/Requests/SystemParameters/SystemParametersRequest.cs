using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Requests.SystemParameters
{
    public class SystemParametersRequest
    {
        public List<SystemParameters> data { get; set; } = new List<SystemParameters>();
    }

    public class SystemParameters
    {
        public long id { get; set; }
        public string value { get; set; }
    }
}
