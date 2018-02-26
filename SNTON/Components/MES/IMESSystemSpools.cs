using NHibernate;
using SNTON.Entities.DBTables.MES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.MES
{
    public interface IMESSystemSpools
    {
        MESSystemSpoolsEntity GetMESSpool(string barcode, IStatelessSession session = null);
        List<MESSystemSpoolsEntity> GetMESSpools(string barcode, IStatelessSession session = null);
    }
}
