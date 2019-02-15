using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.Equip
{
    public class EquipStatusDataUI
    {
        public short EquipId { get; set; }
        public byte Status { get; set; } = 1;
        public int ShowID { get; set; }
    }
}
