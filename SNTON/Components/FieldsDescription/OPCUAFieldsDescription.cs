using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VI.MFC;
using VI.MFC.Components;

namespace SNTON.Components.FieldsDescription
{
    public class OPCUAFieldsDescription : VIRuntimeComponent, IViSupportingComponent, IOPCUAFieldsDescription
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, OPCUAField> OPCUAFieldsDic = new Dictionary<string, OPCUAField>();
        public static OPCUAFieldsDescription Create(XmlNode configNode)
        {
            OPCUAFieldsDescription OPCUAFields = new OPCUAFieldsDescription();
            OPCUAFields.Init(configNode);
            return OPCUAFields;
        }

        public List<string> GetAllKeys()
        {
            return new List<string>(OPCUAFieldsDic.Keys);
        }
        #region Begin of Interface implementaion
        public OPCUAField GetOPCUAField(string fieldName)
        {
            return OPCUAFieldsDic[fieldName];
        }
        #endregion End of Interface implementation
        #region Begin of Override method
        protected override void ReadParameters(XmlNode configNode)
        {
            //读取所有节点
            base.ReadParameters(configNode);
            // 
            foreach (XmlNode item in configNode.ChildNodes[0].ChildNodes)
            {
                if (item.OuterXml.StartsWith("<!--"))
                    continue;

                OPCUAField field = new OPCUAField(item);
                OPCUAFieldsDic.Add(field.Name, field);
            }
        }
        #endregion End of Override method
    }
}
