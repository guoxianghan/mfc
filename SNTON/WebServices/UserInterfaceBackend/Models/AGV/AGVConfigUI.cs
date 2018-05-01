using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.AGV
{
   public class AGVConfigUI
    {
        public long Id { get; set; }

        /// <summary>
        ///  
        /// </summary> 
        public string Name { get; set; }

        /// <summary>
        ///  
        /// </summary> 
        public string PlantNo { get; set; }

    }

    public class AGVConfigInfoUI
    {
        public long Id { get; set; }

        /// <summary>
        ///  
        /// </summary> 
        public float X { get; set; }

        /// <summary>
        ///  
        /// </summary> 
        public float Y { get; set; }
        public string Status { get; set; }
        public bool IsHasSpools { get; set; }        
    }
    public class AGVDetailUI
    {
        public long Id { get; set; }

        /// <summary>
        ///  
        /// </summary> 
        public string Name { get; set; }

        /// <summary>
        ///  
        /// </summary> 
        public string PlantNo { get; set; }
        public string Description { get; set; }
        public List<string> BarCodes { get; set; } = new List<string>();
    }
}
