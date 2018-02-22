using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.Equip
{
    public class EquipInfoDataUI
    {
        public long Id { get; set; }
        /// <summary>
        /// PlantNo
        /// </summary>

        public byte PlantNo { get; set; }

        /// <summary>
        /// EquipName
        /// </summary>

        public string EquipName { get; set; }

        /// <summary>
        /// EquipControllerId
        /// </summary>

        public int EquipControllerId { get; set; }

        /// <summary>
        /// HCoordinate
        /// </summary>

        public byte X { get; set; }

        /// <summary>
        /// VCoordinate
        /// </summary>

        public byte Y { get; set; }

        /// <summary>
        /// ControllerName
        /// </summary>

        public string ControllerName { get; set; }

        /// <summary>
        /// Description
        /// </summary>

        public string Description { get; set; }
        public byte Status { get; set; } //= "尚未获取到状态";
    }
}
