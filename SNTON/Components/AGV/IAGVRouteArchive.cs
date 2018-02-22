using NHibernate;
using SNTON.Entities.DBTables.AGV;
using SNTON.WebServices.UserInterfaceBackend.Requests.AGV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.AGV
{
    public interface IAGVRouteArchive
    {
        /// <summary>
        /// Get AGV route archive info by agv Id, start time and end time
        /// </summary>
        /// <param name="agvId">AGV Id</param>
        /// <param name="startTime">Start time to get data</param>
        /// <param name="endTime">End time to get data</param>
        /// <param name="theSession">Database session</param>
        /// <returns>List of agv route archive</returns>
        List<AGVRouteArchiveEntity> GetAGVRouteArchive(long agvId, DateTime startTime, DateTime endTime, IStatelessSession session = null);
        /// <summary>
        /// Save AGV routes list
        /// </summary>
        /// <param name="agvRoutesList"></param>
        /// <param name="theSession">Database session</param>
        /// <return>void</return>
        void SaveAGVRoute(List<AGVRouteArchiveEntity> agvRoutesList, IStatelessSession theSession = null);
        void AddAGVRoute(IStatelessSession session = null, params AGVRouteArchiveEntity[] routes);
        /// <summary>
        /// 查询所有小车的历史轨迹
        /// </summary>
        /// <param name="search"></param>
        /// <param name="session"></param>
        /// <returns>当前页数据,总条数,总页数</returns>
        Tuple<List<AGVRouteArchiveEntity>, int, int> GetAllHistoryAGVRoute(AGVRuteSearchRequest search, IStatelessSession session = null);
        /// <summary>
        /// 查询一个小车的历史轨迹
        /// </summary>
        /// <param name="search"></param>
        /// <param name="session"></param>
        /// <returns>当前页数据,总条数,总页数</returns>
        Tuple<List<AGVRouteArchiveEntity>, int, int> GetHistoryAGVRoute(AGVRuteSearchRequest search, IStatelessSession session = null);
    }
}
