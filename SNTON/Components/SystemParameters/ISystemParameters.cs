using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.SystemParameters;
using SNTON.WebServices.UserInterfaceBackend.Requests.SystemParameters;

namespace SNTON.Components.SystemParameters
{
    public interface ISystemParameters
    {
        /// <summary>
        /// Get all system parameters
        /// </summary>
        /// <returns></returns>
        IList<SystemParametersEntity> GetSystemParamrters(IStatelessSession theSession);

        SystemParametersEntity GetSystemParamrters(long id, IStatelessSession session);
        int GetSystemParametersSpoolTimeOut(IStatelessSession session);
        /// <summary>
        /// Save system parameter value
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="paramValue"></param>
        /// <param name="theSession"></param>
        void SaveSystemParameters(SystemParametersRequest request, IStatelessSession theSession);
    }
}
