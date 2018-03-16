// Copyright (c) 2016 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;
using SNTON.Constants;
using SNTON.WebServices.UserInterfaceBackend.Requests;
using SNTON.WebServices.UserInterfaceBackend.Responses;
using log4net;
using VI.MFC;
using VI.MFC.Utils.ConfigBinder;
using VI.MFC.Web.Service;
using Newtonsoft.Json;
using SNTON.WebServices.UserInterfaceBackend.Models;
using SNTON.WebServices.UserInterfaceBackend.Responses.Equip;
using SNTON.WebServices.UserInterfaceBackend.Models.Equip;
using System.Collections.Generic;
using SNTON.WebServices.UserInterfaceBackend.Responses.AGV;
using SNTON.WebServices.UserInterfaceBackend.Requests.AGV;
using SNTON.WebServices.UserInterfaceBackend.Requests.SystemParameters;
using SNTON.WebServices.UserInterfaceBackend.Responses.MidStorage;
using VI.MFC.Logging;
using SNTON.WebServices.UserInterfaceBackend.Responses.RobotArmTask;
using SNTON.WebServices.UserInterfaceBackend.Responses.EquipWatch;
using SNTON.WebServices.UserInterfaceBackend.Responses.Product;

namespace SNTON.WebServices.UserInterfaceBackend
{
    public class UserInterfaceBackend : VIWebServiceHoster, IUserInterfaceBackend
    {

        #region Initialization

        private ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

#pragma warning disable 649

        [ConfigBoundProperty("BusinessLogicId")]
        private static string businessLogicId;

#pragma warning restore 649

        public BusinessLogic.IUserInterfaceBackend businessLogic;

        private BusinessLogic.IUserInterfaceBackend BusinessLogic
        {
            get
            {
                Kernel.Glue.RetrieveComponentInstance(ref businessLogic, businessLogicId, this);
                return businessLogic;
            }
        }

        /// <summary>
        /// Default Constructor. Do not do anything with this.
        /// </summary>
        protected UserInterfaceBackend()
        {
        }

        /// <summary>
        /// Constructor. 
        /// 
        /// Note: Only used for constructor dependency injection for unittests.
        ///       Never use this constructor for any other purpose.
        /// </summary>
        protected UserInterfaceBackend(BusinessLogic.IUserInterfaceBackend businessLogic)
        {
            this.businessLogic = businessLogic;
        }

        /// <summary>
        /// Creates an instance of the SortReport Archive.
        /// This method will be called by the MFC bootstrap loader during MFC startup.
        /// </summary>
        /// <param name="theConfigNode">XML configuration Node used.</param>
        /// <returns>reference to newly created Archivist.</returns>
        public static IUserInterfaceBackend Create(XmlNode theConfigNode)
        {
            var component = new UserInterfaceBackend();
            component.Init(theConfigNode);
            return component;
        }


        public override void ValidateParameters()
        {
            base.ValidateParameters();

            if (string.IsNullOrWhiteSpace(businessLogicId))
            {
                throw new ArgumentException("Please specify a valid BusinessLogicId");
            }
        }

        public override void ReadParameters(XmlNode theConfigNode)
        {
            ConfigBinder.Bind(theConfigNode, this);
            base.ReadParameters(theConfigNode);
        }

        protected override void AddAdditionalBindings(ref ServiceHost host)
        {
            var binding = new WebHttpBinding
            {
                MaxBufferPoolSize = 5000000,
                MaxBufferSize = 5000000,
                MaxReceivedMessageSize = 5000000,
                TransferMode = TransferMode.Streamed,
                ReceiveTimeout = new TimeSpan(0, 0, 30),
                ReaderQuotas = { MaxArrayLength = 5000000 }
            };

            InitServices(host.AddServiceEndpoint(typeof(IUserInterfaceBackend), binding, ""));

            //            host.AddServiceEndpoint(typeof(IUserInterfaceBackend), new WebHttpBinding(), "");
        }

        private static void InitServices(ServiceEndpoint endpoint)
        {
            foreach (var behavior in endpoint.Contract.Operations.Select(operation => operation.Behaviors.Find<DataContractSerializerOperationBehavior>()))
            {
                behavior.MaxItemsInObjectGraph = 2147483647;
            }
        }
        #endregion Initialization

        #region Message related functionality
        public object NewMessageGet(long msgLevel)
        {
            MessageResponse obj = new MessageResponse();
            try
            {
                //logger.InfoMethod($"NewMessageGet request msgLevel is {msgLevel}, time stamp: {DateTime.Now.ToString()}");
                Console.WriteLine("NewMessageGet" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                obj = BusinessLogic.NewMessageGet(msgLevel);
                Console.WriteLine("end with NewMessageGet" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //logger.InfoMethod(string.Format("NewMessageGet response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<MessageResponse>(ex);
            }
            return obj;
        }
        public ResponseDataBase MessageSource()
        {
            ResponseDataBase obj = new ResponseDataBase();
            try
            {
                Console.WriteLine("MessageSource" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                obj = this.BusinessLogic.MessageSource();
                Console.WriteLine("end with MessageSource" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseDataBase>(ex);
            }
            return obj;
        }
        public object MessageSearch(MessageSearchRequest searchRequest)
        {
            MessageResponse obj = null;
            try
            {
                logger.InfoMethod($"报警信息查询MessageSearch, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                if ((searchRequest.endTime.HasValue && searchRequest.startTime.HasValue) && ((searchRequest.endTime < searchRequest.startTime)))
                {
                    var e = new Exception("开始日期不能大于结束日期");
                    obj = ResponseBase.GetResponseByException<MessageResponse>(e);
                    return obj;
                }
                //Console.WriteLine($"MessageSearch request searchRequest is {JsonConvert.SerializeObject(searchRequest)}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.MessageSearch(searchRequest);

            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<MessageResponse>(ex);
            }
            finally
            {
                logger.InfoMethod($"end with 报警信息查询MessageSearch, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            return obj;
        }
        #endregion
        #region System parameter
        public object SystemParameterQuery()
        {
            SystemParametersResponse obj = null;
            try
            {
                logger.InfoMethod($"SystemParameterQuery request , time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.SystemParameterQuery();
                //logger.InfoMethod(string.Format("SystemParameterQuery response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<SystemParametersResponse>(ex);
            }
            return obj;
        }

        public object SaveSystemParameter(SystemParametersRequest request)
        {
            ResponseBase obj = null;
            try
            {
                logger.InfoMethod($"SaveSystemParameter request data is {JsonConvert.SerializeObject(request)}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.SystemParameterSave(request);
                //logger.InfoMethod(string.Format("SaveSystemParameter response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseBase>(ex);
            }
            return obj;
        }

        public object SpoolsByBarcodeGet(string barcode)
        {
            SpoolsResponse obj = null;
            try
            {
                logger.InfoMethod($"GetSpoolsByBarcode request barcode is {barcode} , time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetSpoolsByBarcode(barcode);
                //logger.InfoMethod(string.Format("GetSpoolsByBarcode response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<SpoolsResponse>(ex);
            }
            return obj;
        }

        public object SpoolsByMidStorageIdGet(long midStorageId)
        {
            SpoolsResponse obj = null;
            try
            {
                logger.InfoMethod($"GetSpoolsByMidStorageId request midStorageId is {midStorageId}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetSpoolsByMidStorageId(midStorageId);
                //logger.InfoMethod(string.Format("GetSpoolsByMidStorageId response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<SpoolsResponse>(ex);
            }
            return obj;
        }

        public object SpoolsByProudctTypeGet(string proudctType)
        {
            SpoolsResponse obj = null;
            try
            {
                logger.InfoMethod($"GetSpoolsByProudctType request proudctType is {proudctType}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetSpoolsByProudctType(proudctType);
                //logger.InfoMethod(string.Format("GetSpoolsByProudctType response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<SpoolsResponse>(ex);
            }
            return obj;
        }
        #endregion

        #region Equip

        public object EquipConfigStatus(short planno)
        {
            EquipConfigStatusResponse obj = null;
            try
            {
                logger.InfoMethod($"EquipConfigStatus request planno is {planno}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.EquipConfigStatus(planno);
                //logger.InfoMethod(string.Format("EquipConfigStatus response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipConfigStatusResponse>(ex);
            }
            return obj;
        }

        public object EquipConfigInfo(long id)
        {
            EquipConfigInfoResponse obj = null;
            try
            {
                logger.InfoMethod($"EquipConfigInfo request id is {id}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.EquipConfigInfo(id);
                //logger.InfoMethod(string.Format("EquipConfigInfo response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipConfigInfoResponse>(ex);
            }
            return obj;
        }

        public object EquipProductionsByGroupID(long groupid)
        {
            EquipProductionResponse obj = null;
            try
            {
                logger.InfoMethod($"EquipProductionsByGroupID request groupid is {groupid}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.EquipProductionsByGroupID(groupid);
                //logger.InfoMethod(string.Format("EquipProductionsByGroupID response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipProductionResponse>(ex);
            }
            return obj;
        }

        public object EquipProductionsByProductType(string producttype)
        {
            EquipProductionResponse obj = null;
            try
            {
                logger.InfoMethod($"EquipProductionsByProductType request producttype is {producttype}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.EquipProductionsByProductType(producttype);
                //logger.InfoMethod(string.Format("EquipProductionsByProductType response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipProductionResponse>(ex);
            }
            return obj;
        }

        public object EquipProductionsSearch(EquipProductionSearchRequest search)
        {
            EquipProductionResponse obj = null;
            try
            {
                logger.InfoMethod($"EquipProductionsSearch request search is {JsonConvert.SerializeObject(search)}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.EquipProductionsSearch(search);
                logger.InfoMethod(string.Format("EquipProductionsSearch response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipProductionResponse>(ex);
            }
            return obj;
        }

        public object GetAllEquipControllerConfig()
        {
            EquipControllerConfigResponse obj = null;
            try
            {
                logger.InfoMethod($"EquipProductionsSearch request , time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetAllEquipControllerConfig();
                logger.InfoMethod(string.Format("EquipProductionsSearch response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipControllerConfigResponse>(ex);
            }
            return obj;
        }

        public object GetEquipControllerConfigByPlantNo(string plantno)
        {
            EquipControllerConfigResponse obj = null;
            try
            {
                logger.InfoMethod($"GetEquipControllerConfigByPlantNo request plantno is {plantno}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetEquipControllerConfigByPlantNo(plantno);
                logger.InfoMethod(string.Format("GetEquipControllerConfigByPlantNo response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipControllerConfigResponse>(ex);
            }
            return obj;
        }

        public object GetEquipControllerConfigByCtlName(string controllername)
        {
            EquipControllerConfigResponse obj = null;
            try
            {
                logger.InfoMethod($"GetEquipControllerConfigByCtlName request controllername is {controllername}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetEquipControllerConfigByCtlName(controllername);
                //logger.InfoMethod(string.Format("GetEquipControllerConfigByCtlName response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipControllerConfigResponse>(ex);
            }
            return obj;
        }

        public object GetEquipControllerConfigById(long id)
        {

            EquipControllerConfigResponse obj = null;
            try
            {
                logger.InfoMethod($"GetEquipControllerConfigById request id is {id}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetEquipControllerConfigById(id);
                //logger.InfoMethod(string.Format("GetEquipControllerConfigById response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipControllerConfigResponse>(ex);
            }
            return obj;
        }

        public object SaveEquipProductionList(List<EquipProductionDataUI> list, string oper)
        {
            ResponseBase obj = null;
            try
            {
                logger.InfoMethod($"EquipProductionsByProductType request list is {list}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.SaveEquipProductionList(list, oper);
                //logger.InfoMethod(string.Format("SaveEquipProductionList response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseBase>(ex);
            }
            return obj;
        }

        public object EquipConfigs(short planno)
        {
            EquipConfigResponse obj = null;
            try
            {
                //logger.InfoMethod($"EquipConfigs request planno is {planno}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.EquipConfigs(planno);
                //logger.InfoMethod(string.Format("EquipConfigs response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipConfigResponse>(ex);
            }
            return obj;
        }
        public object UpdateEquipProduction(EquipProductionDataUI product, string oper)
        {
            ResponseBase obj = null;
            try
            {
                logger.InfoMethod($"UpdateEquipProduction request product is {product}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.UpdateEquipProduction(product, oper);
                //logger.InfoMethod(string.Format("UpdateEquipProduction response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseBase>(ex);
            }
            return obj;
        }
        #endregion

        #region AGV
        public object AGVConfig(string plantno)
        {
            AGVConfigResponse obj = null;
            try
            {
                //logger.InfoMethod($"AGVConfig request plantno is {plantno}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetAGVConfig(plantno);
                //logger.InfoMethod(string.Format("AGVConfig response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<AGVConfigResponse>(ex);
            }
            return obj;
        }

        public object AGVDetail(long id)
        {
            AGVDetailResponse obj = null;
            try
            {
                //logger.InfoMethod($"AGVDetail request id is {id}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetAGVDetail(id);
                //logger.InfoMethod(string.Format("AGVDetail response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<AGVDetailResponse>(ex);
            }
            return obj;
        }

        public object AGVConfigInfo(string plantno)
        {
            ResponseBase obj = null;
            try
            {
                //logger.InfoMethod($"AGVConfigInfo request plantno is {plantno}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetAGVConfigInfo(plantno);
                //logger.InfoMethod(string.Format("AGVConfigInfo response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseBase>(ex);
            }
            return obj;
        }
        public object CreateEquipTask(int id, int tasktype, int plantno)
        {
            ResponseBase obj = null;
            try
            {
                //logger.InfoMethod($"SaveSystemParameter request id is {id}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.CreateEquipTask(id, tasktype, plantno);
                //logger.InfoMethod(string.Format("SaveSystemParameter response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseBase>(ex);
            }
            return obj;
        }
        public object HistoryAGVRoute(AGVRuteSearchRequest search)
        {
            AGVRouteArchiveResponse obj = null;
            try
            {
                //logger.InfoMethod($"AGVRouteArchiveResponse request search is {JsonConvert.SerializeObject(search)}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.HistoryAGVRoute(search);
                //logger.InfoMethod(string.Format("AGVRouteArchiveResponse response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<AGVRouteArchiveResponse>(ex);
            }
            return obj;
        }
        #endregion
        #region

        public object MidStorageBase(int areaid)
        {
            MidStorageBaseResponse obj = null;
            try
            {
                //logger.InfoMethod($"MidStorage request areaid is {areaid}, time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.GetMidStorageBase(areaid);
                //logger.InfoMethod(string.Format("MidStorage response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<MidStorageBaseResponse>(ex);
            }
            finally
            {
                logger.InfoMethod($"end with get MidStorage, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            return obj;
        }

        public object MidStorageDetail(int areaid)
        {
            MidStorageDetailResponse obj = null;
            try
            {
                logger.InfoMethod($"MidStorageDetail, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.GetMidStorageDetail(areaid);
                //logger.InfoMethod(string.Format("MidStorage response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<MidStorageDetailResponse>(ex);
            }
            finally
            {
                logger.InfoMethod($"end with get MidStorageDetail, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            return obj;
        }
        public object MidStorageDescription(int areaid)
        {
            MidStorageCountResponse obj;
            try
            {
                //logger.InfoMethod($"MidStorageDescription,areaid:{areaid}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.MidStorageDescription(areaid);
                //logger.InfoMethod($"end with MidStorageDescription,areaid:{areaid}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<MidStorageCountResponse>(ex);
            }
            //Console.WriteLine(JsonConvert.SerializeObject(obj));
            return obj;
        }
        public object MidStorageInfo(int areaid)
        {
            //Console.WriteLine(areaid);
            MidStorageInfoResponse obj = null;
            try
            {
                //logger.InfoMethod($"MidStorage request areaid is {areaid}, time stamp: {DateTime.Now.ToString()}");
                //Console.WriteLine($"MidStorage request areaid is {areaid}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.GetMidStorageInfo(areaid);
                //Console.WriteLine($"end with MidStorage request areaid is {areaid}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                //logger.InfoMethod(string.Format("MidStorage response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<MidStorageInfoResponse>(ex);
            }
            return obj;
        }
        public object MidStorageDetailByID(int storageid, int id)
        {
            MidStorageDetailResponse obj = null;
            try
            {
                //logger.InfoMethod($"MidStorage request id is {id}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.GetMidStorageDetailById(storageid, id);
                //logger.InfoMethod(string.Format("MidStorage response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<MidStorageDetailResponse>(ex);
            }
            finally
            {
                logger.InfoMethod($"end to MidStorageDetailByID time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            return obj;
        }
        #endregion



        #region test
        public object TTT()
        {
            BusinessLogic.TTT();
            return null;
        }
        #endregion

        public ResponseDataBase ExcepitonLineOutStore(byte PlantNo, int storage, string OriginalId)
        {
            ResponseDataBase obj = null;
            try
            {
                logger.InfoMethod($"暂存库批量出库ExcepitonLineOutStore request storage is {storage}, ids is { JsonConvert.SerializeObject(OriginalId)}time stamp: {DateTime.Now.ToString()}");
                obj = BusinessLogic.ExcepitonLineOutStore(PlantNo, storage, OriginalId.Split(';'));
                //logger.InfoMethod(string.Format("SaveEquipProductionList response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseDataBase>(ex);
            }
            return obj;
        }

        public object EquipCtrlStatus(short plantno)
        {
            ResponseBase obj = null;
            try
            {
                //logger.InfoMethod($"EquipCtrlStatus request planno is {plantno}");
                logger.InfoMethod($"EquipCtrlStatus request, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.EquipCtrlStatus(plantno);
                //logger.InfoMethod(string.Format("SaveEquipProductionList response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseBase>(ex);
            }
            finally
            {
                //logger.InfoMethod($"end with get EquipCtrlStatus, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            return obj;
        }

        public EquipTaskStatusResponse EquipTaskStatus(byte PlantNo, byte storageareaid)
        {
            EquipTaskStatusResponse obj = null;
            try
            {
                //logger.InfoMethod($"EquipTaskStatus request planno is {storageareaid}");
                Console.WriteLine($"EquipTaskStatus request,PlantNo:{PlantNo},storageareaid:{storageareaid}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.EquipTaskStatus(PlantNo, storageareaid);
                //logger.InfoMethod(string.Format("EquipTaskStatus response is {0}", JsonConvert.SerializeObject(obj)));
                Console.WriteLine($"end with EquipTaskStatus request,PlantNo:{PlantNo},storageareaid:{storageareaid}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipTaskStatusResponse>(ex);
            }
            return obj;
        }

        public ResponseBase DeleteEquipTask(long taskid)
        {
            ResponseBase obj = null;
            try
            {
                logger.InfoMethod($"DeleteEquipTask request taskid is {taskid}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.DeleteEquipTask(taskid);
                //logger.InfoMethod(string.Format("DeleteEquipTask response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseBase>(ex);
            }
            return obj;
        }

        public EquipCallInfoResponse EquipCallInfor(int plantno, int equipId)
        {
            EquipCallInfoResponse obj = null;
            try
            {
                logger.InfoMethod($"EquipCallInfor request plantno is {plantno} and equipId is {equipId}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                Console.WriteLine($"EquipCallInfor request plantno is {plantno} and equipId is {equipId}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.EquipCallInfor(plantno, equipId);
                //logger.InfoMethod(string.Format("DeleteEquipTask response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<EquipCallInfoResponse>(ex);
            }
            return obj;

        }

        public object ExceptionRobotArmTask()
        {
            RobotArmTaskResponse obj = null;
            try
            {
                Console.WriteLine($"获取正在运行的龙门任务ExceptionRobotArmTask time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.ExceptionRobotArmTask();
                //logger.InfoMethod(string.Format("RunningRobotArmTask response is {0}", JsonConvert.SerializeObject(obj)));
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<RobotArmTaskResponse>(ex);
            }
            finally
            {
                Console.WriteLine($"end with 获取正在运行的龙门任务ExceptionRobotArmTask time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            return obj;
        }

        public ResponseDataBase SetExceptionRobotArmTask(long id, int status)
        {
            ResponseDataBase obj = null;
            try
            {
                logger.InfoMethod($"龙门任务异常处理SetExceptionRobotArmTask,id:{id},status:{status}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.ExceptionRobotArmTask(id, status);
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseDataBase>(ex);
            }
            return obj;
        }



        public ResponseDataBase StoreageStatus()
        {
            ResponseDataBase obj = new ResponseDataBase();

            try
            {
                Console.WriteLine($"暂存库运行状态StoreageStatus, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.StoreageStatus();
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseDataBase>(ex);
            }
            return obj;
        }
        /// <summary>
        /// AGV实时轨迹
        /// </summary>
        /// <returns></returns>
        public object RealTimeAGVRoute()
        {
            AGVRouteResponse obj = new AGVRouteResponse();
            try
            {
                //logger.InfoMethod($"AGV实时轨迹RealTimeAGVRoute, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.RealTimeAGVRoute();
                //logger.InfoMethod($"end with AGV实时轨迹RealTimeAGVRoute, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<AGVRouteResponse>(ex);
            }
            return obj;

        }
        #region 直通线
        /// <summary>
        /// 查看直通线状态和线上的二维码
        /// </summary>
        /// <returns></returns>
        public InStoreageLineResponse InStoreageLineStatus()
        {
            InStoreageLineResponse obj = new InStoreageLineResponse();
            try
            {
                logger.InfoMethod($"InStoreageLineStatus, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.InStoreageLineStatus();
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<InStoreageLineResponse>(ex);
            }
            finally
            {
                logger.InfoMethod($"end with get InStoreageLineStatus, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            return obj;
        }
        /// <summary>
        /// 清直通线功能
        /// </summary>
        /// <param name="plantno"></param>
        /// <param name="storeageareaid"></param>
        /// <returns></returns>
        public ResponseDataBase ExceptionInStoreLine(int plantno, int storeageareaid)
        {
            ResponseDataBase obj = new ResponseDataBase();

            try
            {
                logger.InfoMethod($"清直通线功能ExceptionInStoreLine,plantno:{plantno},storeageareaid:{storeageareaid}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.ExceptionInStoreLine(plantno, storeageareaid);
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseDataBase>(ex);
            }
            return obj;
        }

        public ResponseDataBase SendToExceptionFlow(int plantno, int storeageareaid, int type)
        {
            ResponseDataBase obj = new ResponseDataBase();

            try
            {
                logger.InfoMethod($"发送扫码口指令SendToExceptionFlow,plantno:{plantno},storeageareaid:{storeageareaid},type:{type}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.SendToExceptionFlow(plantno, storeageareaid, type);
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseDataBase>(ex);
            }
            return obj;
        }

        public AGVTaskResponse RunningAGVTask()
        {
            AGVTaskResponse obj = new AGVTaskResponse();

            try
            {
                obj = BusinessLogic.RunningAGVTask();
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<AGVTaskResponse>(ex);
            }
            return obj;
        }

        public ResponseDataBase SetAGVTaskStatus(long id, int status)
        {
            ResponseDataBase obj = new ResponseDataBase();
            try
            {
                logger.InfoMethod($"重发AGV指令SetAGVTaskStatus,id:{id},status:{status}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.SetAGVTaskStatus(id, status);
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseDataBase>(ex);
            }
            return obj;
        }

        public object GetProduct()
        {
            ProductResponse obj = new ProductResponse();
            try
            {
                //logger.InfoMethod($"重发AGV指令SetAGVTaskStatus,id:{id},status:{status}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.GetProduct();
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ProductResponse>(ex);
            }
            return obj;
        }

        public object SaveProductLRRadio(int id, string lrratio, int seqno)
        {
            ResponseDataBase obj = new ResponseDataBase();
            try
            {
                //logger.InfoMethod($"重发AGV指令SetAGVTaskStatus,id:{id},status:{status}, time stamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                obj = BusinessLogic.SaveProductLRRadio(id, lrratio, seqno);
            }
            catch (Exception ex)
            {
                obj = ResponseBase.GetResponseByException<ResponseDataBase>(ex);
            }
            return obj;
        }
        #endregion
    }
}
