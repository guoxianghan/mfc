﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SNTON.Components.FieldsDescription
{
    public class MXField
    {

        public MXField(XmlNode fieldNode)
        {
            Name = fieldNode.Attributes["Name"]?.Value;
            Type = fieldNode.Attributes["Type"]?.Value;
            int i = 0;
            int.TryParse(fieldNode.Attributes["Length"].Value, out i);
            Length = i;
            Value = fieldNode.Attributes["Value"]?.Value;
            Split = fieldNode.Attributes["Split"]?.Value;
        }
        public MXField()
        { }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Length { get; set; }
        public string Value { get; set; }
        public string Split { get; set; }
    }
}
