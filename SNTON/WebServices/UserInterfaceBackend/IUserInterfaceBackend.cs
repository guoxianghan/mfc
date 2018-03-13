// Copyright (c) 2016 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using SNTON.WebServices.UserInterfaceBackend.Requests;
using SNTON.WebServices.UserInterfaceBackend.Responses;
using SNTON.WebServices.UserInterfaceBackend.Models;
using SNTON.WebServices.UserInterfaceBackend.Models.Equip;
using SNTON.WebServices.UserInterfaceBackend.Responses.Equip;
using SNTON.WebServices.UserInterfaceBackend.Responses.AGV;
using SNTON.WebServices.UserInterfaceBackend.Requests.AGV;
using SNTON.WebServices.UserInterfaceBackend.Requests.SystemParameters;
using SNTON.WebServices.UserInterfaceBackend.Responses.MidStorage;
using SNTON.WebServices.UserInterfaceBackend.Responses.RobotArmTask;
using SNTON.WebServices.UserInterfaceBackend.Responses.EquipWatch;
using SNTON.WebServices.UserInterfaceBackend.Responses.Product;

namespace SNTON.WebServices.UserInterfaceBackend
{
    [ServiceContract]
    public interface IUserInterfaceBackend
    {
        #region Message functionality
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Message/NewMessageGet?msgLevel={msgLevel}", Method = "GET",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(MessageResponse))]
        object NewMessageGet(long msgLevel);

        [WebInvoke(UriTemplate = "Message/MessageSource", Method = "GET",
               ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(ResponseDataBase))]
        ResponseDataBase MessageSource();

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Message/MessagesSearch", Method = "PUT",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(MessageResponse))]
        object MessageSearch(MessageSearchRequest searchRequest);
        #endregion
        #region System parameter functionality
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "SystemParameter/SystemParameterQuery", Method = "GET",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(SystemParametersResponse))]
        object SystemParameterQuery();

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "SystemParameter/SystemParameterSave?", Method = "POST",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(ResponseBase))]
        object SaveSystemParameter(SystemParametersRequest request);
        #endregion
        #region Spools info
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Spools/SpoolsByBarcodeGet?barcode={barcode}", Method = "GET",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(SpoolsResponse))]
        object SpoolsByBarcodeGet(string barcode);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Spools/SpoolsInMidStorageGet?midStorageId={midStorageId}", Method = "GET",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(SpoolsResponse))]
        object SpoolsByMidStorageIdGet(long midStorageId);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Spools/SpoolsByproudctTypeGet?proudctType={proudctType}", Method = "GET",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(SpoolsResponse))]
        object SpoolsByProudctTypeGet(string proudctType);
        #endregion

        #region EquipmentConfig
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipConfigs?plantno={plantno}", Method = "GET",
               ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipConfigResponse))]
        object EquipConfigs(short plantno);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipConfigStatus?plantno={plantno}", Method = "GET",
               ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipConfigStatusResponse))]
        object EquipConfigStatus(short plantno);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipCtrlStatus?plantno={plantno}", Method = "GET",
               ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipConfigStatusResponse))]
        object EquipCtrlStatus(short plantno);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipConfigInfo?id={id}", Method = "GET",
               ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipConfigInfoResponse))]
        object EquipConfigInfo(long id);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipTaskStatus?plantno={plantno}&storageareaid={storageareaid}", Method = "GET",
               ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipTaskStatusResponse))]
        EquipTaskStatusResponse EquipTaskStatus(byte plantno, byte storageareaid);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/DeleteEquipTask", Method = "POST",
               ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(ResponseBase))]
        ResponseBase DeleteEquipTask(long taskid);
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipCallInfor?plantno={plantno}&EquipId={EquipId}", Method = "GET",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipCallInfoResponse))]
        EquipCallInfoResponse EquipCallInfor(int plantno, int EquipId);
        #endregion


        #region EquipConfigController EquipProduct

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipProductionsByGroupID?groupid={groupid}", Method = "GET",
                ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipProductionResponse))]
        object EquipProductionsByGroupID(long groupid);
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipProductionsByProductType?producttype={producttype}", Method = "GET",
              ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipProductionResponse))]
        object EquipProductionsByProductType(string producttype);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipProductionsSearch", Method = "POST",
              ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipProductionResponse))]
        object EquipProductionsSearch(EquipProductionSearchRequest search);
        //EquipControllerConfig
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/AllEquipControllerConfig", Method = "GET",
             ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipControllerConfigResponse))]
        object GetAllEquipControllerConfig();

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipControllerConfigByPlantNo?plantno={plantno}", Method = "GET",
             ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipControllerConfigResponse))]
        object GetEquipControllerConfigByPlantNo(string plantno);
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipControllerConfigByCtlName?controllername={controllername}", Method = "GET",
             ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipControllerConfigResponse))]
        object GetEquipControllerConfigByCtlName(string controllername);
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/EquipControllerConfigById?id={id}", Method = "GET",
             ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(EquipControllerConfigResponse))]
        object GetEquipControllerConfigById(long id);




        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/SaveEquipProductionList", Method = "POST",
             ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ResponseBase))]
        object SaveEquipProductionList(List<EquipProductionDataUI> list, string oper);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/UpdateEquipProduction", Method = "POST",
             ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ResponseBase))]
        object UpdateEquipProduction(EquipProductionDataUI product, string oper);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">呼叫按钮id</param>
        /// <param name="tasktype">1拉空轮,2拉满轮</param>
        /// <param name="plantno">车间号3</param>
        /// <returns></returns>
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Equipment/CreateEquipTask", Method = "POST",
             ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ResponseBase))]
        object CreateEquipTask(int id, int tasktype, int plantno);
        //List<EquipControllerConfigEntity> GetAllEquipControllerConfig();
        //List<EquipControllerConfigEntity> GetEquipControllerConfigByPlantNo(string plantno);
        //List<EquipControllerConfigEntity> GetEquipControllerConfigByCtlName(string controllername);
        //EquipControllerConfigEntity GetEquipControllerConfigById(long id);

        #endregion
        #region AGV
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "AGV/AGVConfig?plantno={plantno}", Method = "GET",
        ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(AGVConfigResponse))]
        object AGVConfig(string plantno);
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "AGV/AGVDetail?id={id}", Method = "GET",
        ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(AGVDetailResponse))]
        object AGVDetail(long id);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "AGV/AGVConfigInfo?plantno={plantno}", Method = "GET",
        ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(AGVConfigInfoResponse))]
        object AGVConfigInfo(string plantno);

        [WebInvoke(UriTemplate = "AGV/HistoryAGVRoute", Method = "POST",
        ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(AGVRouteArchiveResponse))]
        object HistoryAGVRoute(AGVRuteSearchRequest search);


        [WebInvoke(UriTemplate = "AGV/RealTimeAGVRoute", Method = "GET",
        ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(AGVRouteResponse))]
        object RealTimeAGVRoute();


        [WebInvoke(UriTemplate = "AGV/RunningAGVTask", Method = "GET",
        ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(AGVTaskResponse))]
        AGVTaskResponse RunningAGVTask();

        [WebInvoke(UriTemplate = "AGV/SetAGVTaskStatus", Method = "POST",
        ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(AGVTaskResponse))]
        ResponseDataBase SetAGVTaskStatus(long id, int status);

        #endregion
        #region MidStorage
        [WebInvoke(UriTemplate = "MidStorage/MidStorageBase?areaid={areaid}", Method = "GET",
      ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(MidStorageBaseResponse))]
        object MidStorageBase(int areaid);
        [WebInvoke(UriTemplate = "MidStorage/MidStorageDetail?areaid={areaid}", Method = "GET",
      ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(MidStorageDetailResponse))]
        object MidStorageDetail(int areaid);
        [WebInvoke(UriTemplate = "MidStorage/MidStorageInfo?areaid={areaid}", Method = "GET",
      ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(MidStorageInfoResponse))]
        object MidStorageInfo(int areaid);
        [WebInvoke(UriTemplate = "MidStorage/MidStorageDetailByID?storageid={storageid}&id={id}", Method = "GET",
    ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(MidStorageDetailResponse))]
        object MidStorageDetailByID(int storageid, int id);
        [WebInvoke(UriTemplate = "MidStorage/MidStorageDescription?areaid={areaid}", Method = "GET",
    ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(MidStorageCountResponse))]
        object MidStorageDescription(int areaid);

        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "MidStorage/ExcepitonLineOutStore?PlantNo={PlantNo}&storage={storage}&OriginalId={OriginalId}", Method = "GET",
           ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ResponseDataBase))]
        ResponseDataBase ExcepitonLineOutStore(byte PlantNo, int storage, string OriginalId);

        #endregion

        #region EquipTask

        #endregion

        #region RobotArmTask
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "RobotArmTask/ExceptionRobotArmTask", Method = "GET",
           ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(RobotArmTaskResponse))]
        object ExceptionRobotArmTask();
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status">0重发;1校正;2完成</param>
        /// <returns></returns>
        /// [OperationContract]
        [DataContractFormat]
        [WebInvoke(UriTemplate = "RobotArmTask/SetExceptionRobotArmTask", Method = "POST",
           ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ResponseDataBase))]
        ResponseDataBase SetExceptionRobotArmTask(long id, int status);
        #region 直通线处理


        /// <summary>
        /// 清直通线
        /// </summary>
        /// <param name="plantno"></param>
        /// <param name="storeageareaid"></param>
        /// <returns></returns>
        [DataContractFormat]
        [WebInvoke(UriTemplate = "MidStorage/ExceptionInStoreLine", Method = "POST",
           ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ResponseDataBase))]
        ResponseDataBase ExceptionInStoreLine(int plantno, int storeageareaid);
        /// <summary>
        /// 发送扫码口指令
        /// </summary>
        /// <param name="plantno"></param>
        /// <param name="storeageareaid"></param>
        /// <param name="type">1正常入库;2到异常口</param>
        /// <returns></returns>
        [DataContractFormat]
        [WebInvoke(UriTemplate = "MidStorage/SendToExceptionFlow", Method = "POST",
           ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ResponseDataBase))]
        ResponseDataBase SendToExceptionFlow(int plantno, int storeageareaid, int type);
        /// <summary>
        /// 查看直通线上的二维码
        /// </summary>
        /// <returns></returns>
        [DataContractFormat]
        [WebInvoke(UriTemplate = "MidStorage/InStoreageLineStatus", Method = "GET",
           ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(InStoreageLineResponse))]
        InStoreageLineResponse InStoreageLineStatus();


        /// <summary>
        /// 龙门库状态
        /// </summary>
        /// <returns></returns>
        [DataContractFormat]
        [WebInvoke(UriTemplate = "MidStorage/MidStorageStatus", Method = "GET",
           ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ResponseDataBase))]
        ResponseDataBase StoreageStatus();
        #endregion
        #region test 
        [WebInvoke(UriTemplate = "MIT/TTT", Method = "GET",
       ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        [ServiceKnownType(typeof(ResponseBase))]
        object TTT();
        #endregion

        #region Product
        [DataContractFormat]
        [WebInvoke(UriTemplate = " Product/GetProduct", Method = "GET",
         ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ProductResponse))]
        ProductResponse GetProduct();
        /// <summary>
        /// 更改报警比例
        /// </summary>
        /// <returns></returns>
        [DataContractFormat]
        [WebInvoke(UriTemplate = "Product/SaveProductLRRatio", Method = "POST",
           ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        [ServiceKnownType(typeof(ResponseDataBase))]
        ResponseDataBase SaveProductLRRadio(int id, string lrratio);

        #endregion
    }
}
