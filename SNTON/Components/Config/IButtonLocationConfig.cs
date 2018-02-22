using NHibernate;
using SNTON.Entities.DBTables.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Config
{
    public interface IButtonLocationConfig
    {
        ButtonLocationConfigEntity GetButtonLocationConfigEntityByID(long id, IStatelessSession session = null);
        List<ButtonLocationConfigEntity> GetAllButtonLocationConfigEntity(IStatelessSession session = null);
        List<ButtonLocationConfigEntity> _ButtonLocationConfigList { get; set; }
    }
}

