using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.FieldsDescription
{
    public interface IMXFieldsDescription
    {
        MXField GetMXField(string fieldName);
        List<string> GetAllKeys();
    }
}
