using SNTON.Constants;
using SNTON.WebServices.UserInterfaceBackend.Models.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.AGV
{
    public class AGVRouteArchiveResponse : ResponseBase
    {
        public int pagesize;
        public int pagecount;
        public List<AGVRouteArchiveUI> data = new List<AGVRouteArchiveUI>();
    }
    public class AGVRouteResponse : ResponseBase
    {
        public List<AGVRouteDataUI> data = new List<AGVRouteDataUI>();
    }
    public class AGVRouteListResponse : ResponseBase
    {
        public Dictionary<short, List<AGVRouteDataUI>> data = new Dictionary<short, List<AGVRouteDataUI>>();
    }
}
