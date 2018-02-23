using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models
{
    public class SystemParametersUI
    {
        /// <summary>
        /// 系统参数的唯一标识ID
        /// </summary>
        public long Id;

        /// <summary>
        /// 系统参数名称
        /// </summary>
        public string ParamName;

        /// <summary>
        /// 系统参数值
        /// </summary>
        public string Value;

        /// <summary>
        /// TextBox 0 ,ComboBox 1,RadioButton 2,CheckBox 3      
        /// 系统参数值显示形式 
        /// </summary>
        public string ValueType;

        /// <summary>
        /// 系统参数详细描述
        /// </summary>
        public string Description;

        /// <summary>
        /// 系统参数值对应的显示列表
        /// </summary>
        //public Dictionary<string, string> SelectValue;
        public string SelectValue="";
        public int Seqno { get; set; }
        public SystemParametersUI()
        {
            //SelectValue = new Dictionary<string, string>();
        }
    }
}
