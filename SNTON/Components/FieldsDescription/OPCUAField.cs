using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SNTON.Components.FieldsDescription
{
    public class OPCUAField
    {
        public OPCUAField(XmlNode fieldNode)
        {
            Name = fieldNode.Attributes["Name"]?.Value;
            Type = fieldNode.Attributes["Type"]?.Value; 
            Value = fieldNode.Attributes["Value"]?.Value; 
        }
        public OPCUAField()
        { }
        public string Name { get; set; }
        public string Type { get; set; } 
        public string Value { get; set; } 
    }
}
