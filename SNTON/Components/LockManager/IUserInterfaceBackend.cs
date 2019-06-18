using System;
using FedEx.WebServices;
using FedEx.WebServices.UserInterfaceBackend.Requests;
using FedEx.WebServices.UserInterfaceBackend.Responses;

namespace SNTON.BusinessLogic
{
    /// <summary>
    /// Business logic for the user interface backend goes here.
    /// Basically, these methods will be called primarily by the 
    /// RESTful service layer.
    /// </summary>
    public interface IUserInterfaceBackendd
    {
        #region Sortingplan related functionality

        /// <summary>
        /// Requests all sorting plans for given sorter.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sorterId">Requested sorter ID.</param>
        /// <returns></returns>
        SortingplanListGetResponse SortingplanListGet(long userId, long sorterId);

        /// <summary>
        /// Add a new or modify existing sorting plan (without roles).
        /// </summary>
        /// <param name="sortPlan">sorting plan without roles. Add new if id is 0. </param>
        /// <returns></returns>
        ResponseBase SortingplanItemPut(SortingplanPutRequest sortPlan);

        /// <summary>
        /// Copy sortingplan with all rules
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID which will be copied into new one</param>        
        /// <param name="copyName">Name of copied sorting plan</param>
        /// <param name="oldName">Old Name of copied sorting plan</param>
        /// <returns></returns>
        ResponseBase SortingplanCopyGet(long userId, long sortingplanId, string copyName, string oldName);

        /// <summary>
        /// Delete sorting plan with all rules
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID which will be deleted</param>
        /// <returns></returns>
        ResponseBase SortingplanDeleteGet(long userId, long sortingplanId);

        /// <summary>
        /// Activate/Deactivate sorting plan
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID which will be activated/decativated</param>
        /// <returns></returns>
        ResponseBase SortingplanActivateGet(long userId, long sortingplanId);

        /// <summary>
        /// Requests sorting plan rules for given sorting plan and rule type
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID</param>
        /// <param name="ruleType">Rule type string</param>
        /// <returns></returns>
        SortingplanRulesGetResponse SortingplanRulesGet(long userId, long sortingplanId, string ruleType);

        /// <summary>
        /// Add a new or modify existing sorting plan rule .
        /// </summary>
        /// <param name="request">sorting plan rule. Add new one if ID is 0, update existing one if it not 0.</param>
        /// <returns></returns>
        ResponseBase SortingplanRulePut(SortingplanRulePutRequest request);

        /// <summary>
        /// Delete sorting plan rule
        /// </summary>
        /// <param name="request">request</param>
        /// <returns></returns>
        ResponseBase SortingplanRuleDeletePut(SortingplanRuleDeletePutRequest request);

        /// <summary>
        /// Deactivate sorting plan
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID which will be activated/decativated</param>
        /// <returns></returns>
        ResponseBase SortingplanDeactivateGet(long userId, long sortingplanId);

        #endregion
        
        #region Systemlog related functionality

        /// <summary>
        /// Requests information stored in the system log. Information is ordered by database ID ascending.
        /// </summary>
        /// <param name="userId">UserId asking for this information.</param>
        /// <param name="startTime">Filter of the start time</param>
        /// <param name="endTime">Filter end time</param>
        /// <param name="maxRecords">Will get only the specified number of records. 
        ///                          If the number of records returned is the same as this maximum
        ///                          number, there may be more records waiting to be queried starting with the
        ///                          last ID of the previous query.</param>
        /// <param name="startId">   May be specified to get only records with database ID >startId. This is useful
        ///                          in subsequent calls to to get remaining records. Should be 0 for 
        ///                          the first call.</param>
        SystemLogGetResponse SystemlogGet(long userId, DateTime startTime, DateTime endTime, long maxRecords, long startId);

        #endregion

        #region Operationlog related functionality

        /// <summary>
        /// Requests information stored in the operation log. Information is ordered by database ID ascending.
        /// </summary>
        /// <param name="userId">UserId asking for this information.</param>
        /// <param name="startTime">Filter of the start time</param>
        /// <param name="endTime">Filter end time</param>
        /// <param name="maxRecords">Will get only the specified number of records. 
        ///                          If the number of records returned is the same as this maximum
        ///                          number, there may be more records waiting to be queried starting with the
        ///                          last ID of the previous query.</param>
        /// <param name="startId">   May be specified to get only records with database ID >startId. This is useful
        ///                          in subsequent calls to to get remaining records. Should be 0 for 
        ///                          the first call.</param>
        OperationLogGetResponse OperationlogGet(long userId, DateTime startTime, DateTime endTime, long maxRecords, long startId);

        #endregion

        #region Interception Parcel related functionality

        /// <summary>
        /// Requests information stored for the Interception parcels.
        /// Information is ordered by database ID ascending.
        /// </summary>
        /// <param name="userId">UserId asking for this information.</param>
        /// <param name="startTime">Filter of the start time</param>
        /// <param name="endTime">Filter end time</param>
        /// <param name="maxRecords">Will get only the specified number of records. 
        ///                          If the number of records returned is the same as this maximum
        ///                          number, there may be more records waiting to be queried starting with the
        ///                          last ID of the previous query.</param>
        /// <param name="startId">   May be specified to get only records with database ID >startId. This is useful
        ///                          in subsequent calls to to get remaining records. Should be 0 for 
        ///                          the first call.</param>
        InterceptionParcelReportGetResponse InterceptionParcelReportGet(long userId, DateTime startTime, DateTime endTime, long maxRecords, long startId);

        /// <summary>
        /// Requests current imported Interception list.
        /// Information is ordered by database ID ascending.
        /// </summary>
        /// <param name="userId">UserId asking for this information.</param>
        /// <returns></returns>
        InterceptionImportGetResponse InterceptionImportGet(long userId);

        /// <summary>
        /// Import new Interception list.
        /// <param name="newInterceptionList">New interception list</param>
        /// </summary>
        /// <returns></returns>
        InterceptionImportPutResponse InterceptionImportPut(InterceptionImportPutRequest newInterceptionList);

        /// <summary>
        /// Delete current active Interception list.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="interceptionSessionID">Session ID of interception list for deleting</param>
        /// <returns></returns>
        ResponseBase InterceptionImportDeleteGet(long userId, long interceptionSessionID);

        #endregion

        #region Logout related functionality

        /// <summary>
        /// Event that user is log out.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="ipAddress">IP address of client machine</param>
        /// <returns></returns>
        ResponseBase LogoutPut(string userName, string ipAddress);

        #endregion

        #region Selection and Handling codes functionality

        /// <summary>
        /// Requests current imported selection and handling codes.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="codeType">Requested code type (Selection 'S' or Handling 'H' code type)</param>
        /// <returns></returns>
        ImportSelHandlingGetResponse SelectionHandlingImportGet(long userId, string codeType);

        /// <summary>
        /// Import new list with selection and handle codes.
        /// <param name="request">list of codes for import</param>
        /// </summary>
        /// <returns></returns>
        ImportSelHandlingPutResponse SelectionHandlingImportPut(ImportSelHandlingPutRequest request);

        #endregion

        #region Self-pickup related functionality

        /// <summary>
        /// Requests self-pickup lists.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sorterId">Sorter ID</param>
        /// <returns></returns>
        SelfPickupListGetResponse SelfPickupListsGet(long userId, long sorterId);

        /// <summary>
        /// Update self-pickup list (description and assigned chutes).
        /// </summary>
        /// <param name="request">Request for changing.</param>
        /// <returns></returns>
        ResponseBase SelfPickupListUpdatePut(SelfPickupListUpdatePutRequest request);

        /// <summary>
        /// Requests selected self-pickup list.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="selfPickupListId">Requested self pickup List ID</param>
        /// <returns></returns>
        SelfPickupGetResponse SelfPickupGet(long userId, long selfPickupListId);

        /// <summary>
        /// Import self-pickup list
        /// <param name="request">Requested data for saving</param>
        /// </summary>
        /// <returns></returns>
        SelfPickupImportPutResponse SelfPickupImportPut(SelfPickupImportPutRequest request);

        /// <summary>
        /// Delete requested pickup-list
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="selfPickupListId">Requested self pickup List ID</param>
        /// <returns></returns>
        ResponseBase SelfPickupDeleteGet(long userId, long selfPickupListId);

        /// <summary>
        ///  Requests selfpickup parcel stored in the parcel. Information is ordered by database ID ascending.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sorterId">ID of sorter.</param>
        /// <param name="selfPickupListId">Requested self pickup List ID</param>
        /// <param name="startTime">Filter of the start time</param>
        /// <param name="endTime">Filter end time</param>
        /// <param name="maxRecords">Will get only the specified number of records. 
        ///                          If the number of records returned is the same as this maximum
        ///                          number, there may be more records waiting to be queried starting with the
        ///                          last ID of the previous query.</param>
        /// <param name="startId">   May be specified to get only records with database ID >startId. This is useful
        ///                          in subsequent calls to to get remaining records. Should be 0 for 
        ///                          the first call.</param>
        /// <returns></returns>
        ParcelReportGetResponse SelfPickupReportGet(long userId,
                                                    long sorterId,
                                                    long selfPickupListId,
                                                    DateTime startTime,
                                                    DateTime endTime,
                                                    long maxRecords,
                                                    long startId);

        #endregion

        #region Exception chutes related functionality

        /// <summary>
        /// Returns all exception chutes for requested sorter
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sorterId">ID of sorter name defined in FX_SORTER. </param>
        /// <returns></returns>
        ExceptionChuteGetResponse ExceptionChutesGet(long userId, long sorterId);

        /// <summary>
        /// Update exception chutes for one sorter
        /// </summary>
        /// <param name="request">Requested changes for updating.</param>
        /// <returns></returns>
        ResponseBase ExceptionChutesPut(ExceptionChutePutRequest request);

        #endregion

        #region Sorter info related functionality

        /// <summary>
        /// Returns info about sorters
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sorterType">Sorter type defined in FedExConstants.SorterDirection. if null or empty sttring return all sorters</param>
        /// <returns></returns>
        SorterInfoGetResponse SorterInfoGet(long userId, string sorterType = null);

        /// <summary>
        /// Returns sorter chute info
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sorterId"></param>
        /// <param name="ruleType"></param>
        /// <returns></returns>
        SorterChuteInfoGetResponse SorterChuteInfoGet(long userId, long sorterId, string ruleType, long planId);

        #endregion

        #region SortReportArchive related functionality

        /// <summary>
        ///  Requests parcel stored in the Sort Report Archive table. Information is ordered by database ID ascending.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="barcode">barcode used for search items.</param>
        /// <param name="startTime">Filter of the start time</param>
        /// <param name="endTime">Filter end time</param>
        /// <param name="maxRecords">Will get only the specified number of records. 
        ///                          If the number of records returned is the same as this maximum
        ///                          number, there may be more records waiting to be queried starting with the
        ///                          last ID of the previous query.</param>
        /// <param name="startId">   May be specified to get only records with database ID >startId. This is useful
        ///                          in subsequent calls to to get remaining records. Should be 0 for 
        ///                          the first call.</param>
        /// <returns></returns>
        SortReportArchiveGetResponse SortReportArchiveGet(long userId,
                                                    string barcode,
                                                    DateTime startTime,
                                                    DateTime endTime,
                                                    long maxRecords,
                                                    long startId);

        #endregion SortReportArchive related functionality

    }
}
