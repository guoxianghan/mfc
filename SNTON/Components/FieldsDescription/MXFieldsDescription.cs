using System;
using System.Collections.Generic;
using VI.MFC.Components;
using VI.MFC;
using System.Xml;
using log4net;
using System.Reflection;

namespace SNTON.Components.FieldsDescription
{
    public class MXFieldsDescription : VIRuntimeComponent, IViSupportingComponent, IMXFieldsDescription
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, MXField> mxFieldsDic = new Dictionary<string, MXField>();
        public static MXFieldsDescription Create(XmlNode configNode)
        {
            MXFieldsDescription mxFields = new MXFieldsDescription();
            mxFields.Init(configNode);
            return mxFields;
        }

        public List<string> GetAllKeys()
        {
            return new List<string>(mxFieldsDic.Keys);
        }
        #region Begin of Interface implementaion
        public MXField GetMXField(string fieldName)
        {
            return mxFieldsDic[fieldName];
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

                MXField field = new MXField(item);
                mxFieldsDic.Add(field.Name, field);
            }
        }
        #endregion End of Override method
    }
}
