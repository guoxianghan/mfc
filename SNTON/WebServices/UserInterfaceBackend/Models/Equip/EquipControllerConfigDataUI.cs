using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.Equip
{
    public class EquipControllerConfigDataUI
    {
        public long Id { get; set; }
        /// <summary>
        /// PlantNo
        /// </summary>

        public byte PlantNo { get; set; }

        /// <summary>
        /// EquipControllerName
        /// </summary>

        public string EquipControllerName { get; set; }

        /// <summary>
        /// Description
        /// </summary>

        public string Description { get; set; }
    }
}
