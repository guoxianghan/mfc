using System;
using SNTON.WebServices;
using SNTON.WebServices.UserInterfaceBackend.Requests;
using SNTON.WebServices.UserInterfaceBackend.Responses;
using SNTON.WebServices.UserInterfaceBackend.Responses.Equip;
using SNTON.WebServices.UserInterfaceBackend.Models;
using SNTON.WebServices.UserInterfaceBackend.Models.Equip;
using System.Collections.Generic;
using SNTON.WebServices.UserInterfaceBackend.Responses.AGV;
using SNTON.WebServices.UserInterfaceBackend.Requests.AGV;
using SNTON.WebServices.UserInterfaceBackend.Requests.SystemParameters;
using SNTON.WebServices.UserInterfaceBackend.Responses.MidStorage;
using SNTON.WebServices.UserInterfaceBackend.Responses.RobotArmTask;
using SNTON.WebServices.UserInterfaceBackend.Responses.EquipWatch;
using SNTON.WebServices.UserInterfaceBackend.Responses.Product;

namespace SNTON.BusinessLogic
{
    /// <summary>
    /// Business logic for the user interface backend goes here.
    /// Basically, these methods will be called primarily by the 
    /// RESTful service layer.
    /// </summary>
    public interface IUserInterfaceBackend
    {
        MessageResponse NewMessageGet(long msgLevel);
        MessageResponse MessageSearch(MessageSearchRequest searchRequest);
        ResponseDataBase MessageSource();
        SystemParametersResponse SystemParameterQuery();
        ResponseBase SystemParameterSave(SystemParametersRequest request);
        SpoolsResponse GetSpoolsByBarcode(string barcode);

        SpoolsResponse GetSpoolsByMidStorageId(long midStorageId);

        SpoolsResponse GetSpoolsByProudctType(string proudctType);
        EquipCallInfoResponse EquipCallInfor(int plantno, int EquipId);
        EquipConfigResponse EquipConfigs(short plantno);
        EquipTaskStatusResponse EquipTaskStatus(byte PlantNo, byte storageareaid);
        EquipConfigStatusResponse EquipConfigStatus(short plantno);
        EquipConfigStatusResponse EquipCtrlStatus(short planno);
        MidStorageCountResponse MidStorageDescription(int areaid);
        EquipConfigInfoResponse EquipConfigInfo(long id);
        #region EquipProduction
        EquipProductionResponse EquipProductionsByGroupID(long groupid);
        EquipProductionResponse EquipProductionsByProductType(string producttype);
        EquipProductionResponse EquipProductionsSearch(EquipProductionSearchRequest search);
        EquipControllerConfigResponse GetAllEquipControllerConfig();
        EquipControllerConfigResponse GetEquipControllerConfigByPlantNo(string plantno);
        EquipControllerConfigResponse GetEquipControllerConfigByCtlName(string controllername);
        EquipControllerConfigResponse GetEquipControllerConfigById(long id);
        ResponseBase SaveEquipProductionList(List<EquipProductionDataUI> list, string oper);
        ResponseBase UpdateEquipProduction(EquipProductionDataUI product, string oper);
        AGVConfigResponse GetAGVConfig(string plantno);
        AGVDetailResponse GetAGVDetail(long id);
        AGVConfigInfoResponse GetAGVConfigInfo(string plantno);
        AGVRouteArchiveResponse HistoryAGVRoute(AGVRuteSearchRequest search);
        AGVRouteResponse RealTimeAGVRoute();
        AGVTaskResponse RunningAGVTask();
        ResponseDataBase SetAGVTaskStatus(long id, int status);
        ProductResponse GetProduct();
        ResponseDataBase SaveProductLRRadio(int id, string lrratio, int seqno, byte IsWarnning);
        #endregion

        #region
        MidStorageBaseResponse GetMidStorageBase(int storagearea);
        MidStorageDetailResponse GetMidStorageDetail(int storagearea);
        MidStorageDetailResponse GetMidStorageDetailById(int storageid, int id);
        MidStorageInfoResponse GetMidStorageInfo(int storagearea);
        #endregion

        #region EquipTask 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrlid"></param>
        /// <param name="inorout"></param>
        /// <returns></returns>
        ResponseBase CreateEquipTask(int id, int TaskType, int PlantNo);
        #endregion

        ResponseBase TTT();
        /// <summary>
        /// 异常口出库
        /// </summary>
        /// <param name="PlantNo">车间</param>
        /// <param name="storage"></param>
        /// <param name="barcodes"></param>
        /// <returns></returns>
        ResponseDataBase ExcepitonLineOutStore(byte PlantNo, int storage, params string[] barcodes);
        ResponseBase DeleteEquipTask(long taskid);
        RobotArmTaskResponse ExceptionRobotArmTask();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status">0重发;1校正;2完成</param>
        /// <returns></returns>
        ResponseDataBase ExceptionRobotArmTask(long id, int status);
        #region 直通线


        /// <summary>
        /// 清直通线功能
        /// </summary>
        /// <param name="plantno"></param>
        /// <param name="storeageareaid"></param>
        /// <returns></returns>
        ResponseDataBase ExceptionInStoreLine(int plantno, int storeageareaid);
        InStoreageLineResponse InStoreageLineStatus();
        ResponseDataBase StoreageStatus();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plantno"></param>
        /// <param name="storeageareaid"></param>
        /// <param name="type"> 1正常入库;2到异常口</param>
        /// <returns></returns>
        ResponseDataBase SendToExceptionFlow(int plantno, int storeageareaid, int type);
        #endregion
    }
}

