using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.Equip
{
   public class EquipProductionDataUI
    {
        public long Id { get; set; }
        /// <summary>
        /// EquipId
        /// </summary>
        
        public  string EquipId { get; set; }

        /// <summary>
        /// GroupId
        /// </summary>
        
        public  short GroupId { get; set; }

        /// <summary>
        /// ProductType
        /// </summary>
        
        public  string ProductType { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        
        public  string Description { get; set; }

        /// <summary>
        /// Operator
        /// </summary>
        
        public  string Operator { get; set; }
        public string EmptySpoolType { get; set; }
    }
}
