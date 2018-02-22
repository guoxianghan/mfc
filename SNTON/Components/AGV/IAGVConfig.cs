using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Entities.DBTables.AGV;
using NHibernate;
using SNTON.WebServices.UserInterfaceBackend.Requests.AGV;

namespace SNTON.Components.AGV
{
    public interface IAGVConfig
    {
        /// <summary>
        /// Get AGV info by name
        /// </summary>
        /// <param name="agvName">AGV name</param>
        /// <param name="theSession">Database session</param>        
        /// <returns>AGV configuration</returns>
        AGVConfigEntity GetAGVByName(string agvName, IStatelessSession theSession);

        /// <summary>
        /// Get AGV info by Id
        /// </summary>
        /// <param name="Id">AGV Id</param>
        /// <param name="theSession">Database session</param>>
        /// <returns>AGV configuration</returns>
        AGVConfigEntity GetAGVById(long Id, IStatelessSession theSession);
        List<AGVConfigEntity> GetAllAGVConfig(IStatelessSession session);
        Dictionary<long, AGVConfigEntity> _DicAGVConfig { get; set; }





        void AddAGVConfig(List<AGVConfigEntity> list, IStatelessSession session = null); 
        void SaveAGVConfig(IStatelessSession session = null,params AGVConfigEntity[] agvs);
    }
}
