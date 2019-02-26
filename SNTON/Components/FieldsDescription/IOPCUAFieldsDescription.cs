using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.FieldsDescription
{
   public interface IOPCUAFieldsDescription
    {
        OPCUAField GetOPCUAField(string fieldName);
        List<string> GetAllKeys();
    }
}
