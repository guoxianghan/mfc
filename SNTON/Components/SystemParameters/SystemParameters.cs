using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SNTON.Entities.DBTables.SystemParameters;
using SNTON.Components.CleanUp;
using System.Reflection;
using log4net;
using VI.MFC.Logging;
using System.Xml;
using SNTON.WebServices.UserInterfaceBackend.Requests.SystemParameters;
using Newtonsoft.Json;

namespace SNTON.Components.SystemParameters
{
    public class SystemParameters : CleanUpBrokerBase, ISystemParameters
    {

        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EntityDbTable = "SystemParametersEntity";
        private const string DatabaseDbTable = "SNTON.SystemParameters";

        // only for unittest
        //private readonly Dictionary<long, EmployeeEnt> employeeList = new Dictionary<long, EmployeeEnt>();

        #region Class constructor
        /// <summary>
        /// Static class creation
        /// </summary>
        /// <param name="configNode"></param>
        /// <returns></returns>
        public static SystemParameters Create(XmlNode configNode)
        {
            var broker = new SystemParameters();
            broker.Init(configNode);
            return broker;
        }
        /// <summary>
        /// Constructor called by bootstrap loader. 
        /// </summary>
        public SystemParameters()
        {

        }
        /// <summary>
        /// PLACEHOLDER: Please extend if required.
        /// Constructor which will only be used for injecting inner class dependencies.
        /// Injecting glue based dependencies should be done directly in the unittest code.
        /// It is only for Unit Test.
        /// </summary>
        /// <param name="dependency"></param>
        public SystemParameters(object dependency)
        {
            if (dependency == null) // Not called by unittest. We have to instantiate the real object.
            {

            }
        }
        #endregion
        #region Override method
        /// <summary>
        /// Override from base class
        /// Get the class information
        /// </summary>
        /// <returns></returns>
        public override string GetInfo()
        {
            return EntityDbTable + " broker class";
        }

        /// <summary>
        /// Start the broker
        /// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();//start the cleanup thread
            logger.InfoMethod(EntityDbTable + " broker started.");
        }

        /// <summary>
        /// Here we may load data from the database during startup in case we were
        /// a caching broker. Non-caching broker (preferred) may not do anything here.
        /// </summary>
        public override void ReadBrokerData()
        {
        }
        #endregion

        public IList<SystemParametersEntity> GetSystemParamrters(IStatelessSession session)
        {
            IList<SystemParametersEntity> ret = null;

            if (session == null)
            {
                ret = BrokerDelegate(() => GetSystemParamrters(session), ref session);
                return ret;
            }
            try
            {
                protData.EnterReadLock();
                var tmp = ReadSqlList<SystemParametersEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                if (tmp.Any())
                {
                    var config = ReadSqlList<SystemParametersConfigurationEntity>(session, $"SELECT * FROM SNTON.SystemParametersConfiguration WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted);
                    foreach (var item in tmp)
                    {
                        if (item.DisplayFormat != 0 && item.DisplayFormat != 1)
                        {
                            var t = config.FindAll(x => x.SysParamId == item.Id);
                            if (t == null)
                                continue;
                            foreach (var i in t)
                            {
                                item.SelectValue.Add(new KeyValuePair<string, string>(i.Value.Trim(), i.DisplayValue.Trim()));
                            }
                        }
                    }
                    ret = tmp.ToList();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to get GetSystemParamrters", e);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return ret;
        }

        public void SaveSystemParameters(SystemParametersRequest request, IStatelessSession session)
        {
            if (request == null)
            {
                //request = new SystemParametersRequest();
                //request.data.Add(new WebServices.UserInterfaceBackend.Requests.SystemParameters.SystemParameters() { id = 23, value = "fads" });
                //string json = JsonConvert.SerializeObject(request);
            }
            if (session == null)
            {
                BrokerDelegate(() => SaveSystemParameters(request, session), ref session);
                return;
            }
            try
            {
                StringBuilder id = new StringBuilder();
                request?.data?.ForEach(x => id.Append(x.id + ","));
                if (id.Length == 0)
                { return; }
                List<SystemParametersEntity> r = ReadList<SystemParametersEntity>(session, " FROM " + EntityDbTable + " WHERE ID in (" + id.ToString() + ")");

                foreach (var item in r)
                {
                    item.Updated = DateTime.Now;
                    var t = request.data.FirstOrDefault(x => x.id == item.Id);
                    if (t != null)
                    {
                        item.ParameterValue = t.value;
                    }
                }

                if (r != null)
                {
                    Update(session, r);
                    logger.InfoMethod(string.Format("Save data  is {0}   SystemParametersEntity  to DB successfully", JsonConvert.SerializeObject(request)));
                }
                else
                    logger.ErrorMethod("Not find  SystemParameters by ID");

            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to save SystemParameters", e);
            }
        }

        public SystemParametersEntity GetSystemParamrters(long id, IStatelessSession session)
        {
            SystemParametersEntity para = null;
            if (session == null)
            {
                para = BrokerDelegate(() => GetSystemParamrters(id, session), ref session);
                return para;
            }
            try
            {
                para = ReadSql<SystemParametersEntity>(session, "SELECT * FROM " + DatabaseDbTable + " WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND id=" + id);
                if (para.DisplayFormat != 0 && para.DisplayFormat != 1)
                {
                    var config = ReadSqlList<SystemParametersConfigurationEntity>(session, $"SELECT * FROM {DatabaseDbTable} WHERE ISDELETED=" + Constants.SNTONConstants.DeletedTag.NotDeleted + " AND SysParamId=" + id);
                    if (config != null)
                        foreach (var i in config)
                        {
                            para.SelectValue.Add(new KeyValuePair<string, string>(i.Value.Trim(), i.DisplayValue.Trim()));
                        }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {

            }
            return para;
        }

        public int GetSystemParametersSpoolTimeOut(IStatelessSession session)
        {
            int i = 0;
            if (session == null)
            {
                i = BrokerDelegate(() => GetSystemParametersSpoolTimeOut(session), ref session);
                return i;
            }
            try
            {
                protData.EnterReadLock();
                var obj = GetSystemParamrters(1, session);
                i = Convert.ToInt32(obj.ParameterValue);
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("filed to Get SystemParametersSpool TimeOut ", ex);
            }
            finally
            {
                protData.ExitReadLock();
            }
            return i;
        }
    }
}
