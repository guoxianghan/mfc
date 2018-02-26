using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models
{
   public class EquipProductionSearchRequest: SearchCondition
    {
        public long equipid;
        public long groupid;
        public string producttype;
        public string oper;
    }
}
