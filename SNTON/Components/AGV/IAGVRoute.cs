using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Entities.DBTables.AGV;
using NHibernate;

namespace SNTON.Components.AGV
{
    public interface IAGVRoute
    {
        /// <summary>
        /// Get AGV route by agvId
        /// </summary>
        /// <param name="agvId">AGV Id</param>
        /// <param name="theSession">Database session</param>
        /// <returns>AGV route entity</returns>
        List<AGVRouteEntity> GetAGVRoute(long agvId, IStatelessSession theSession = null);
        int DeleteAGVRoute(long agvId, IStatelessSession theSession = null);
        /// <summary>
        /// Save AGV routes list
        /// </summary>
        /// <param name="agvRoutesList"></param>
        /// <param name="theSession">Database session</param>
        /// <return>void</return>
        void SaveAGVRoute(List<AGVRouteEntity> agvRoutesList, IStatelessSession theSession = null);
        void AddAGVRoute(IStatelessSession session = null, params AGVRouteEntity[] routes);
        /// <summary>
        /// 获取所有小车最新的位置及状态
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        List<AGVRouteEntity> GetAllAGVRute(IStatelessSession session = null);
        Dictionary<short, AGVRouteEntity> RealTimeAGVRute { get; set; }
        Dictionary<short, List<AGVRouteEntity>> RealTimeAGVRute2 { get; set; }
    }
}
