using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.EquipWatch
{
    /// <summary>
    /// 直通线
    /// </summary>
    public class InStoreageLineDataUI
    {
        public int StoreageNo { get; set; }
        public List<string> QrCodes { get; set; }
    }
}
