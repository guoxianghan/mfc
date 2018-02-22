using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.Equip
{
   public class EquipConfigDataUI
    { 
        public long Id { get; set;}

        /// <summary>
        /// EquipName
        /// </summary> 
        public  string EquipName { get; set; } 

        /// <summary>
        /// HCoordinate
        /// </summary> 
        public  byte X { get; set; }

        /// <summary>
        /// VCoordinate
        /// </summary> 
        public  byte Y { get; set; }

        public byte Status { get; set; }

        public int GroupID { get; set; }
        public string GroupName { get; set; }
        /// <summary>
        /// Description
        /// </summary> 
        public  string Description { get; set; }
    }

}
