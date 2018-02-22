using SNTON.WebServices.UserInterfaceBackend.Models.EquipWatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.EquipWatch
{
    public class InStoreageLineResponse : ResponseBase
    {
        public List<InStoreageLineDataUI> data { get; set; } = new List<InStoreageLineDataUI>();
    }
}
