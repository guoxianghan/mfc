// Copyright (c) 2016 Vanderlande Industries
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Vanderlande Industries. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Collections.Generic;
using System.Linq;
using FedEx.BusinessLogic.UserInterfaceBackend;
using FedEx.Components.ExceptionChuteInfo;
using FedEx.Constants;
using FedEx.Entities.DBTables.Chute;
using FedEx.Entities.DBTables.ExceptionChute;
using FedEx.Entities.DBTables.Interception;
using FedEx.Entities.DBTables.OperationsLog;
using FedEx.Entities.DBTables.SelectionAndHandCode;
using FedEx.Entities.DBTables.SelfPickup;
using SNTON.Entities.DBTables.Sorter;
using SNTON.Entities.DBTables.SortingPlan;
using SNTON.Entities.DBTables.SortReport;
using FedEx.Entities.DBTables.SystemLog;
using SNTON.WebServices;
using SNTON.WebServices.UserInterfaceBackend.Models;
using SNTON.WebServices.UserInterfaceBackend.Requests;
using SNTON.WebServices.UserInterfaceBackend.Responses;
using NHibernate;
using NHibernate.Util;
using VI.MFC.Logging;
using SNTON.Components.SortingPlan;
using Mono.CSharp;
using SNTON.Components.LockManager;
using SNTON.BusinessLogic;

namespace FedEx.BusinessLogic
{

    /// <summary>
    /// Business logic interface for the user interface backend goes here.
    /// Basically, these methods will be called primarily by the RESTful service layer.
    /// </summary>
    public partial class BusinessLogic : IUserInterfaceBackendd
    {
        #region Properites

        /// <summary>
        /// Maximum of interception import records got from configuration
        /// </summary>
        private int MaxInterceptionImportRecord
        {
            get
            {
                return Convert.ToInt32(ConfigValuesProvider.GetConfigValue(FedExConstants.ConfigureParameters.InterMaxImportRecord).Trim());
            }
        }

        /// <summary>
        /// Maximum of self-pickup import records got from configuration
        /// </summary>
        private int MaxSelfPickupImportRecord
        {
            get
            {
                return Convert.ToInt32(ConfigValuesProvider.GetConfigValue(FedExConstants.ConfigureParameters.SelfMaxImportRecord).Trim());
            }
        }

        /// <summary>
        /// Maximum of selection import codes got from configuration
        /// </summary>
        private int MaxSelectionImportCodes
        {
            get
            {
                return Convert.ToInt32(ConfigValuesProvider.GetConfigValue(FedExConstants.ConfigureParameters.SelectionMaxImportCodes).Trim());
            }
        }

        /// <summary>
        /// Maximum of handling import codes got from configuration
        /// </summary>
        private int MaxHandlingImportCodes
        {
            get
            {
                return Convert.ToInt32(ConfigValuesProvider.GetConfigValue(FedExConstants.ConfigureParameters.HandlingMaxImportCodes).Trim());
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Return user name from user ID using OPERATOR table.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User name, if not found returns null</returns>
        private string GetUsername(long userId)
        {
            var ret = string.Empty;

            try
            {
                var user = OperatorInfo.GetOperatorLoginById(userId);
                if (user != null)
                {
                    ret = user.OperatorLogin;
                }
            }
            catch
            {
                logger.ErrorMethod(String.Format("Cannot retrieve Username for userID: {0}", userId));
            }

            return ret;
        }

        /// <summary>
        /// Return user ID from user name using OPERATOR table.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <returns>User ID or 0 if not found</returns>
        private long GetUserId(string userName)
        {
            long ret = 0;

            try
            {
                var user = OperatorInfo.GetOperatorIdByLoginName(userName);
                if (user != null)
                {
                    ret = user.Id;
                }
            }
            catch
            {
                logger.ErrorMethod(String.Format("Cannot retrieve userID for Username: {0}", userName));
            }

            return ret;
        }

        /// <summary>
        ///  Return pickup chute id list by sorterId
        /// </summary>
        /// <param name="sorterId"></param>
        /// <returns></returns>
        private List<long> GetSelfPickupChuteIdBySorterId(long sorterId)
        {
            List<long> pickupChuteIds = new List<long>();

            //If sorter has self pickup configuration, must filter self pickup chute.
            SorterDetail detail = SorterInfo.GetSorterDetailBySorterIdAndKey(sorterId, Entities.DBTables.Sorter.SorterDetail.SorterCapabilities.MAXPICKUPCHUTECOUNT);
            if (int.Parse(detail.ValueAttr) > 0)
            {
                List<SelfPickupList> selfPickupList = SelfPickup.GetSelfPickUpListBySorter(sorterId);
                if (selfPickupList != null)
                {
                    
                    foreach (SelfPickupList item in selfPickupList)
                    {
                        List<SelfPickupChute> pickupChute = SelfPickup.GetSelfPickupChutesByPickupListID(item.Id);
                        if (pickupChute != null)
                        {
                            pickupChuteIds.AddRange(pickupChute.Select(p => p.ChuteID));
                        }
                    }
                }
            }

            return pickupChuteIds;
        }

        /// <summary>
        /// Return active sort plan rule chute id list by sorterId
        /// </summary>
        /// <param name="sorterId"></param>
        /// <returns></returns>
        private List<long> GetActivePlanRuleChuteBySorterId(long sorterId)
        {
            List<long> activeRuleChuteIds = new List<long>();

            //If sorter has active sort plan , must filter active sort plan rule chute.
            List<SortPlan> activeSortPlans = SortingPlan.GetActiveSortPlanList();
            SortPlan activeSortPlan = activeSortPlans.Where(s => s.SorterId == sorterId).FirstOrDefault();
            if (activeSortPlan != null)
            {
                List<SortingPlanRuleChutePair> ruleChutes = SortingPlan.GetActiveSortPlanRuleChuteList().Where(s => s.SortPlanId == activeSortPlan.Id).ToList();
                activeRuleChuteIds = ruleChutes.Select(r => r.ChuteId).ToList();
            }

            return activeRuleChuteIds;
        }


        #endregion
        
        #region SortingPlan related

        /// <summary>
        /// Requests all sorting plans for given sorter.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sorterId">Requested sorter ID.</param>
        /// <returns></returns>
        public SortingplanListGetResponse SortingplanListGet(long userId, long sorterId)
        {
            var ret = new SortingplanListGetResponse() {sorterId = sorterId, sortingPlans = new List<SortingplanData>()};
            var storyHandle = LockManagerProvider.Lock("SortingPlanRead");

            try
            {
                if (SorterInfo.GetSorter(sorterId) == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.ImportPagesIds.InvalidSorter,
                        //Arguments = new string[] { sorterId.ToString() }
                    };
                    logger.ErrorMethod(String.Format("SortingplanListGet(): Invalid sorter ID: {0}", sorterId));

                    throw new Exception();
                }

                var activeSortplans = SortingPlan.GetActiveSortingPlanSessionList();

                var sortingPlans = SortingPlan.GetSortingPlansBySorterId(sorterId);
                foreach (var sortPlan in sortingPlans)
                {
                    ret.sortingPlans.Add(new SortingplanData()
                    {
                        sortplanId = sortPlan.Id,
                        name = sortPlan.Description,
                        isActive = activeSortplans.Any(s => s.SortingPlanId == sortPlan.Id)
                    });
                }
            }
            catch (Exception e)
            {
                if (ret.Error == null)
                {
                    logger.ErrorMethod("SortingplanListGet() exception occured. ", e);
                    throw;
                }
            }
            finally
            {
                LockManager.Unlock(storyHandle);
            }

            return ret;
        }

        /// <summary>
        /// Validate Input parameters for SortingplanItemPut() methods
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private ResponseError ValidateSortingplanItemPutParameters(SortingplanPutRequest request)
        {
            ResponseError error = null;

            if (request == null)
            {
                error = new ResponseError() { Message = WebServices.Constants.ErrorMessages.RequestMissing };
                logger.ErrorMethod("SortingplanItemPut(): request is null");
            }

            // Is name name defined
            if (error == null && string.IsNullOrWhiteSpace(request.name))
            {
                error = new ResponseError() { Message = WebServices.Constants.SortingplanPageIds.SortPlanNameMissing };
                logger.ErrorMethod("SortingplanItemPut(): plan name is empty or null");
            }

            // Is sorter valid
            if (error == null && SorterInfo.GetSorter(request.sorterId) == null)
            {
                error = new ResponseError()
                {
                    Message = WebServices.Constants.ImportPagesIds.InvalidSorter,
                    //Arguments = new string[] { request.sorterId.ToString() }
                };
                logger.ErrorMethod(String.Format("SortingplanItemPut(): Invalid sorter ID: {0}", request.sorterId));
            }

            // Check does exist some sortplan for this sorter with same name
            if (error == null &&
                SortingPlan.GetSortingPlansBySorterId(request.sorterId).Any(
                                                                             s =>
                                                                             s.Id != request.sortingplanId &&
                                                                             s.Description.ToUpper().Equals(request.name.Trim().ToUpper())))
            {
                error = new ResponseError() { Message = WebServices.Constants.SortingplanPageIds.SortplanWithSameName };
                logger.ErrorMethod(String.Format("SortingplanItemPut(): Invalid sorter ID: {0}, the same name", request.sorterId));
            }

            return error;
        }

        /// <summary>
        /// Add a new or modify existing sorting plan (without roles).
        /// </summary>
        /// <param name="request">sorting plan without roles. Add new if id is 0. </param>
        /// <returns></returns>
        public ResponseBase SortingplanItemPut(SortingplanPutRequest request)
        {
            IStatelessSession theSession = null;
            var ret = new ResponseBase();
            var storyHandle = LockManager.Lock("SortingPlanWrite");

            try
            {
                ret.Error = ValidateSortingplanItemPutParameters(request);

                if (ret.Error != null)
                {
                    return ret;
                }

                List<OperationsLogEntry> operaLogs = new List<OperationsLogEntry>();
                OperationsLogEntry operaLog = new OperationsLogEntry()
                {
                    UserId = request.userId,
                    UserName = GetUsername(request.userId),
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                };

                //If sorting plan id is not equal 0, check whether sort plan exists by plan id
                SortPlan sortPlanforSave = null; 
                if (request.sortingplanId != 0 )
                {
                    sortPlanforSave = SortingPlan.GetSortPlanByPlanId(request.sortingplanId);
                    if (sortPlanforSave == null)
                    {
                        ret.Error = new ResponseError()
                        {
                            Message = WebServices.Constants.SortingplanPageIds.InvalidSortplan,
                            Arguments = new string[] { request.sortingplanId.ToString() }
                        };
                        logger.ErrorMethod(String.Format("SortingplanItemPut(): Invalid sorting plan with ID: {0}", request.sortingplanId));
                        return ret;
                    }

                    operaLog.OperatorAction = FedExConstants.OperatorActions.EditSortplan;
                    operaLog.IdsAction = WebServices.Constants.OperatorActionsIds.EditSortPlanName;
                    operaLog.NewValue = request.name;
                    operaLog.OldValue = request.oldName;
                    operaLog.P1 = sortPlanforSave.Description;
                    operaLog.P2 = request.oldName;
                    operaLog.P3 = request.name;
                }
                else
                {
                    sortPlanforSave = new SortPlan();
                    operaLog.OperatorAction = FedExConstants.OperatorActions.CreateSortplan;
                    operaLog.IdsAction = WebServices.Constants.OperatorActionsIds.CreateSortplan;
                    operaLog.P1 = request.name.Trim();
                }

                operaLogs.Add(operaLog);
                sortPlanforSave.Id = request.sortingplanId;
                sortPlanforSave.Description = request.name.Trim();
                sortPlanforSave.SorterId = request.sorterId;
                sortPlanforSave.UserId = request.userId;

                // Name of the transaction for statistics only
                theSession = SessionPool.GetStatelessSession("SortingplanItemPut");
                bool retry;
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        retry = false;

                        // Insert (ID == 0) a new record or update existing records
                        SortingPlan.SaveSortingPlan(sortPlanforSave, theSession);

                        //Insert operation logs
                        OperationsLog.AddEntries(operaLogs, theSession);

                        theSession.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(e, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                }
                while (retry);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SortingplanItemPut Exception occured. ", e);
                throw;
            }
            finally
            {
                LockManager.Unlock(storyHandle);
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }
            return ret;
        }

        /// <summary>
        /// Copy sortingplan with all rules
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID which will be copied into new one</param>
        /// <param name="copyName">Name of copied sorting plan</param>
        /// <param name="oldName">Original name of sortplan</param>
        /// <returns></returns>
        public ResponseBase SortingplanCopyGet(long userId, long sortingplanId, string copyName, string oldName)
        {
            IStatelessSession theSession = null;
            var ret = new ResponseBase();
            var storyHandle = LockManager.Lock("SortingPlanWrite");

            try
            {
                #region Validate

                // Validate input parameters
                var originalSortplan = SortingPlan.GetSortPlanByPlanId(sortingplanId);
                if (originalSortplan == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.InvalidSortplan,
                        Arguments = new string[] { sortingplanId.ToString() }
                    };
                    logger.ErrorMethod(String.Format("SortingplanCopyGet(): Invalid sorting plan with ID: {0}", sortingplanId));

                    return ret;
                    //throw new Exception();
                }
               
                List<SortPlan> sortplans = SortingPlan.GetSortingPlansBySorterId(originalSortplan.SorterId);

                //Check does the sum of sort plan count is larger than 20
                if (sortplans.Count >= 20)
                {
                    ret.Error = new ResponseError() 
                    { 
                        Message = WebServices.Constants.SortingplanPageIds.SortplanExceedSel,
                        Arguments = new string[] { "20" }
                    };
                    logger.ErrorMethod(String.Format("SortingplanCopyGet(): the sum of sort plan count is larger than 20, plan with ID: {0}", sortingplanId));
                    return ret;
                    //throw new Exception();
                }

                // Check does exist some sortplan for this sorter with same name
                if (sortplans.Any(s => s.Description.ToUpper().Equals(copyName.Trim().ToUpper())))
                {
                    ret.Error = new ResponseError() { Message = WebServices.Constants.SortingplanPageIds.SortplanWithSameName };
                    logger.ErrorMethod(String.Format("SortingplanCopyGet(): the copy plan name {0} has exists, plan with ID: {1}", copyName, sortingplanId));
                    return ret;
                    //throw new Exception();
                }

                #endregion

                #region Logic Process

                SortPlan sortPlanforSave = new SortPlan
                {
                    Id = 0, 
                    Description = copyName.Trim(), 
                    SorterId = originalSortplan.SorterId, 
                    UserId = userId
                };

                // Name of the transaction for statistics only
                theSession = SessionPool.GetStatelessSession("SortingplanCopyGet");

                //Get sorting plan rules
                List<SortingPlanRule> planRules = SortingPlan.GetSortingPlanRulesByPlanIdRuleType(sortingplanId);
                List<SortingPlanRuleChute> needCopyRuleChutes = new List<SortingPlanRuleChute>();

                bool retry;
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        retry = false;

                        // Insert (ID == 0) a new record or update existing records
                        SortingPlan.SaveSortingPlan(sortPlanforSave, theSession);
                        SortPlan copySortPlan = SortingPlan.GetSortPlanByPlanDescription(copyName, theSession);

                        //Insert copy sort plan rules and rule chutes
                        foreach (SortingPlanRule item in planRules)
                        {
                            SortingPlanRule tempRule = new SortingPlanRule()
                            {
                                Updated = DateTime.UtcNow,
                                Created = DateTime.UtcNow,
                                SortPlanId = copySortPlan.Id,
                                Priority = item.Priority,
                                RuleType = item.RuleType,
                                RuleCode = item.RuleCode,
                                ExtraRuleCode = item.ExtraRuleCode,
                                UserId = userId
                            };

                            List<SortingPlanRuleChute> tempRuleChutes = SortingPlan.GetSortingPlanRuleChutesByRuleId(item.Id);
                            if (null != tempRuleChutes && tempRuleChutes.Any())
                            {
                                needCopyRuleChutes.Clear();
                                needCopyRuleChutes.AddRange(tempRuleChutes);
                            }

                            SortingPlan.SaveSortingPlanRules(copySortPlan.Id, tempRule, tempRuleChutes, theSession);
                        }

                        //Add opeartion log
                        OperationsLog.Add(userId,
                            GetUsername(userId),
                            FedExConstants.OperatorActions.CopySortplan,
                            WebServices.Constants.OperatorActionsIds.CopySortplan,
                            new[] { oldName, copyName }, oldName, copyName, theSession);

                        theSession.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        logger.Error("SortingplanCopyGet the session process has exception", e);
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(e, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                }
                while (retry);

                #endregion
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SortingplanCopyGet exception occured. ", e);
                if (ret.Error == null)
                {
                    throw;
                }
            }
            finally
            {
                LockManager.Unlock(storyHandle);
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }
            return ret;
        }

        /// <summary>
        /// Delete sorting plan with all rules
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID which will be deleted</param>
        /// <returns></returns>
        public ResponseBase SortingplanDeleteGet(long userId, long sortingplanId)
        {
            IStatelessSession theSession = null;
            var ret = new ResponseBase();
            var storyHandle = LockManager.Lock("SortingPlanWrite");

            try
            {
                // Validate does sortingplanId exist
                SortPlan sortPlan = SortingPlan.GetSortPlanByPlanId(sortingplanId);
                if (sortPlan == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.InvalidSortplan,
                        Arguments = new string[] { sortingplanId.ToString() }
                    };
                    logger.ErrorMethod(String.Format("SortingplanDeleteGet(): Invalid sorting plan with ID: {0}", sortingplanId));
                    return ret;
                    //throw new Exception();
                }
                
                // Check is not requested soprting plan active
                bool flag = SortingPlan.IsSortingPlanActiveByPlanId(sortingplanId);
                if (flag)
                {
                    ret.Error = new ResponseError() { Message = WebServices.Constants.SortingplanPageIds.SortplanIsActive };
                    logger.ErrorMethod(String.Format("SortingplanDeleteGet(): Sorting plan with ID: {0} is active", sortingplanId));
                    return ret;
                   // throw new Exception();
                }

                // Name of the transaction for statistics only
                theSession = SessionPool.GetStatelessSession("SortingplanDeleteGet");
                bool retry;
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        retry = false;

                        bool success = SortingPlan.DeleteSortingPlan(sortingplanId, theSession);

                        if (success)
                        {
                            //Add opeartion log
                            OperationsLog.Add(userId,
                                GetUsername(userId),
                                FedExConstants.OperatorActions.DeleteSortplan,
                                WebServices.Constants.OperatorActionsIds.DeleteSortplan,
                                new[] { sortPlan.Description }, "", "", theSession);

                            theSession.Transaction.Commit();
                        }
                        else
                        {
                            theSession.Transaction.Rollback();
                            ret.Error = new ResponseError() { Message = WebServices.Constants.SortingplanPageIds.SortplanNotDeleted };
                        }

                    }
                    catch (Exception e)
                    {
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(e, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                }
                while (retry);
            }
            catch (Exception e)
            {
                if (ret.Error == null)
                {
                    logger.ErrorMethod("SortingplanDeleteGet(): Exception occured. ", e);
                    throw;
                }
            }
            finally
            {
                LockManager.Unlock(storyHandle);
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }
            return ret;
        }

        /// <summary>
        /// Deactivate sorting plan
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID which will be activated/decativated</param>
        /// <returns></returns>
        public ResponseBase SortingplanDeactivateGet(long userId, long sortingplanId)
        {
            IStatelessSession theSession = null;
            var ret = new ResponseBase();
            var storyHandle = LockManager.Lock("SortingPlanWrite");

            try
            {
                #region Validate

                // Validate sortingplanId
                SortPlan sortplan = SortingPlan.GetSortPlanByPlanId(sortingplanId);
                if (sortplan == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.InvalidSortplan,
                        Arguments = new string[] { sortingplanId.ToString() }
                    };
                    logger.ErrorMethod(String.Format("SortingplanDeactivateGet(): Invalid sorting plan with ID: {0}", sortingplanId));
                    return ret;
                }

                //Check whether the sortplan is deactive
                bool flag = SortingPlan.IsSortingPlanActiveByPlanId(sortplan.Id);
                if (!flag)
                {
                    ret.Error = new ResponseError() { Message = WebServices.Constants.SortingplanPageIds.SortplanNotActive };
                    logger.ErrorMethod(String.Format("SortingplanActivateGet(): Sorting plan with ID: {0} is deactive", sortingplanId));
                    return ret;
                }

                #endregion

                string userName = GetUsername(userId);
                bool retry = false;

                // Name of the transaction for statistics only
                theSession = SessionPool.GetStatelessSession("SortingplanDeactivateGet");
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        SortingPlan.SetSortingPlanDeActive(userId, sortplan.SorterId, sortingplanId, theSession);

                        // Add to OperationsLog
                        OperationsLog.Add(userId, userName, FedExConstants.OperatorActions.DeactivateSortplan,
                                            WebServices.Constants.OperatorActionsIds.DeactivateSortplan,
                                            new string[] { sortplan.Description },
                                            null,
                                            null,
                                            theSession
                                            );

                        theSession.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        logger.ErrorMethod("SortingplanDeactivateGet session process: Exception occured. ", e);
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(e, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                }
                while (retry);

            }
            catch (Exception e)
            {
                logger.ErrorMethod("SortingplanDeactivateGet(): Exception occured. ", e);
                throw;
            }
            finally
            {
                LockManager.Unlock(storyHandle);
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }

            return ret;
        }

        /// <summary>
        /// Activate sorting plan
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID which will be activated/decativated</param>
        /// <returns></returns>
        public ResponseBase SortingplanActivateGet(long userId, long sortingplanId)
        {
            IStatelessSession theSession = null;
            var ret = new ResponseBase();
            var storyHandle = LockManager.Lock("SortingPlanWrite");

            try
            {
                #region Validate 

                // Validate sortingplanId
                SortPlan sortplan = SortingPlan.GetSortPlanByPlanId(sortingplanId);
                if (sortplan == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.InvalidSortplan,
                        Arguments = new string[] { sortingplanId.ToString() }
                    };
                    logger.ErrorMethod(String.Format("SortingplanActivateGet(): Invalid sorting plan with ID: {0}", sortingplanId));
                    return ret;
                }

                //Validation when activate sort plan.
                ret.Error = ValidateWhenActivateSortPlan(sortplan);
                if (ret.Error != null)
                {
                    return ret;
                }

                #endregion

                string userName = GetUsername(userId);
                bool retry = false;

                // Name of the transaction for statistics only
                theSession = SessionPool.GetStatelessSession("SortingplanActivateGet");
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        SortingPlan.SetSortingPlanActive(userId, sortplan.SorterId, sortingplanId, theSession);

                        // Add to OperationsLog
                        OperationsLog.Add(userId, userName, FedExConstants.OperatorActions.ActivateSortplan,
                                            WebServices.Constants.OperatorActionsIds.ActivateSortplan,
                                            new string[] { sortplan.Description },
                                            null,
                                            null,
                                            theSession
                                            );

                        theSession.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        logger.ErrorMethod("SortingplanActivateGet session process: Exception occured. ", e);
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(e, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                }
                while (retry);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SortingplanActivateGet(): Exception occured. ", e);
                throw;
            }
            finally
            {
                LockManager.Unlock(storyHandle);
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }
            return ret;
        }

        /// <summary>
        /// Requests sorting plan rules for given sorting plan and rule type
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sortingplanId">Sorting plan ID</param>
        /// <param name="ruleType">Rule type string</param>
        /// <returns></returns>
        public SortingplanRulesGetResponse SortingplanRulesGet(long userId, long sortingplanId, string ruleType)
        {
            var ret = new SortingplanRulesGetResponse() { sortplanId = sortingplanId, ruleType = ruleType, rules = new List<SortingplanRuleData>()};
            //get the reading lock
            var lockhandle = LockManager.Lock("SortingPlanRuleGet");
            IStatelessSession theSession = SessionPool.GetStatelessSession("SortingplanRulesGet");
            try
            {
                // get sorting plan rule data by plan Id and rule type
                var spRuleData = SortingPlan.GetSortingPlanRulesByPlanIdRuleType(sortingplanId, ruleType, theSession);    
                // get the preload flag by the rule type
                bool isSpRulesMergeWithSelHandCode = IsSpRulesMergeWithSelHandCodeByType(ruleType);
                // if no data in sorting plan rule or sorting plan data is NULL
                // return default value
                if (spRuleData == null || !spRuleData.Any() && !isSpRulesMergeWithSelHandCode)
                {
                    return ret;
                }
                // get the sorting plan rule data by selection code and handling code
                var spRulesBySelHandCode = new List<SortingplanRuleData>();
                if (isSpRulesMergeWithSelHandCode)
                {
                    GetSortPlanRulesFromSelHandCodeByType(ref spRulesBySelHandCode, ruleType, theSession);
                }
                // get the merged sorting plan rule data
                ret.rules = MergeAndReturnSortingPlanRulesData(spRuleData, spRulesBySelHandCode, isSpRulesMergeWithSelHandCode, theSession);
            }
            catch (Exception e)
            {
                logger.ErrorMethod(string.Format("Failed to get sorting plan rule for Id({0}) and rule type({1})", sortingplanId, ruleType), e);
                throw;
            }
            finally
            {
                LockManager.Unlock(lockhandle);
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }
            return ret;
        }

        /// <summary>
        /// Add a new or modify existing sorting plan rule .
        /// </summary>
        /// <param name="request">sorting plan rule. Add new one if ID is 0, update existing one if it not 0.</param>
        /// <returns></returns>
        public ResponseBase SortingplanRulePut(SortingplanRulePutRequest request)
        {
            IStatelessSession theSession = null;
            ResponseBase ret = new ResponseBase();
            var lockHandle = LockManager.Lock("SortingPlanRulePut");

            try
            {
                //Check request
                ret = UIHelper.ValidatePlanRulePutRequest(request);
                if (ret.Error != null)
                {
                    return ret;
                }

                //Check whether sortPlanId is valid.
                SortPlan sortPlan = SortingPlan.GetSortPlanByPlanId(request.sortingplanId);
                if (sortPlan == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.InvalidSortplan,
                        Arguments = new string[] { request.sortingplanId.ToString() }
                    };
                    logger.ErrorMethod(string.Format("SortingplanRulePut(): sort plan is null by sortingplanId {0}", request.sortingplanId));
                    return ret;
                }

                ret = CheckSortingPlanRulePutLogic(request);
                if (ret.Error != null)
                {
                    return ret;
                }

                #region Logic Process

                //If requset rule id is 0, insert plan rule and rule chute
                SortingplanRuleData ruleData = request.rule;
                SortingPlanRule theRule = new SortingPlanRule()
                {
                    Id = ruleData.ruleId,
                    SortPlanId = request.sortingplanId,
                    ExtraRuleCode = ruleData.extraRuleCode == null ? ruleData.ruleCode.Trim() : ruleData.extraRuleCode.Trim(),
                    Priority = ruleData.priority,
                    RuleCode = ruleData.ruleCode.Trim(),
                    RuleType = request.ruleType,
                    UserId = request.userId,
                    Created = DateTime.UtcNow
                };

                List<SortingPlanRuleChute> theRuleChutes = new List<SortingPlanRuleChute>();
                if (null != ruleData.chutes)
                {
                    foreach (SorterChuteData item in ruleData.chutes)
                    {
                        SortingPlanRuleChute tempRuleChute = new SortingPlanRuleChute()
                        {
                            SortPlanRuleId = ruleData.ruleId,
                            ChuteId = item.chuteId,
                        };
                        theRuleChutes.Add(tempRuleChute);
                    }
                }
             
                //Add operationLogs
                string userName = GetUsername(request.userId);
                List<OperationsLogEntry> operationLogs = null;
                if (request.rule.ruleId > 0)
                {
                    operationLogs = UIHelper.ConstructOpeLogsForEditSortPlanRule(request, sortPlan, userName);
                }
                else
                {
                    operationLogs = UIHelper.ConstructOpeLogsForCreateSortPlanRule(request, sortPlan, userName);
                }

                // Name of the transaction for statistics only
                theSession = SessionPool.GetStatelessSession("SortingplanRulePut");
                bool retry = false;
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        SortingPlan.SaveSortingPlanRules(request.sortingplanId, theRule, theRuleChutes, theSession);

                        //Add operator logs
                        if (operationLogs != null && operationLogs.Count > 0)
                        {
                            OperationsLog.AddEntries(operationLogs, theSession);
                        }

                        theSession.Transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(ex, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                } while (retry);

                #endregion
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("SortingplanRulePut exception occured. ", ex);
                throw;
            }
            finally
            {
                LockManager.Unlock(lockHandle);
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }

            return ret;
        }

        /// <summary>
        /// Delete sorting plan rule
        /// </summary>
        /// <param name="request">sorting plan rule deletion request</param>
        /// <returns></returns>
        public ResponseBase SortingplanRuleDeletePut(SortingplanRuleDeletePutRequest request)
        {
            var ret = new ResponseBase();
            var lockHandler = LockManager.Lock("SortingPlanRuleDelete");
            var spRule2Delete = new SortingPlanRule();
            var spRuleChute2Delete = new List<SortingPlanRuleChute>();
            IStatelessSession theSession = SessionPool.GetStatelessSession("SortingplanRuleDeletePut");
            try
            {
                #region Validation

                if (request == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.ErrorMessages.RequestMissing,
                    };
                    logger.ErrorMethod("SortingplanRuleDeletePut(): request is missing.");
                    return ret;
                }

                if (request.ruleData == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.ErrorMessages.RequestInvalid,
                    };
                    logger.ErrorMethod("SortingplanRuleDeletePut(): request's rule data is null.");
                    return ret;
                }

                //Check whether sortPlanId is valid.
                SortPlan sortPlan = SortingPlan.GetSortPlanByPlanId(request.sortingPlanId);
                if (sortPlan == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.InvalidSortplan,
                        Arguments = new string[] { request.sortingPlanId.ToString() }
                    };
                    logger.ErrorMethod(string.Format("SortingplanRulePut(): sort plan is null by sortingplanId {0}", request.sortingPlanId));
                    return ret;
                }

                //Check sort plan whether it is active, if active, return false
                bool flag = SortingPlan.IsSortingPlanActiveByPlanId(request.sortingPlanId);
                if (flag)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.SortplanIsActive,
                    };
                    logger.ErrorMethod(string.Format("SortingplanRulePut(): sort plan is active by sortingplanId {0}", request.sortingPlanId));
                    return ret;
                }

                //Check sort plan rule, if rule has been deleted, should return true to keep idempotent
                spRule2Delete = SortingPlan.GetSortingPlanRuleByRuleId(request.ruleData.ruleId, theSession);
                if (spRule2Delete == null)
                {
                    //ret.Error = new ResponseError()
                    //{
                    //    Message = WebServices.Constants.SortingplanPageIds.InvalidSortplanRule,
                    //};
                    logger.InfoMethod(string.Format("SortingplanRulePut(): sort plan rule {0} has been deleted, return true.", request.ruleData.ruleId));
                    return ret;
                }

                #endregion
              
                spRuleChute2Delete = SortingPlan.GetSortingPlanRuleChutesByRuleId(request.ruleData.ruleId, theSession);
                //// check if sorting plan to be deleted is actived or not?
                //try
                //{
                //    // get the sorting plan rule data
                //    spRule2Delete = SortingPlan.GetSortingPlanRuleByRuleId(sortingplanRuleId, theSession);
                //    // validate the sorting plan rule data
                //    CheckSortingPlanRule2Delete(sortingplanRuleId, spRule2Delete);
                //    // get sorting plan rule chute data
                //    GetSortingPlanRuleChuteByPlanRule(sortingplanRuleId, spRule2Delete, ref spRuleChute2Delete, theSession);
                //}
                //catch (ArgumentException e)
                //{
                //    ret.Error = new ResponseError()
                //    {
                //        Message = e.Message,
                //        Arguments = new string[] { sortingplanRuleId.ToString()}
                //    };
                //    logger.ErrorMethod(string.Format("Failed to delete sorting plan rule(Id:{0}) due to INVALID arguments", sortingplanRuleId));
                //    return ret;
                //}
                
                bool retry = false;
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        if (!SortingPlan.DeleteSortingPlanRules(spRule2Delete.SortPlanId, spRule2Delete, spRuleChute2Delete, theSession))
                        {
                            theSession.Transaction.Rollback();
                            ret.Error = new ResponseError
                            {
                                Message = WebServices.Constants.SortingplanPageIds.DeleteSortplanRuleFailed,
                                Arguments = new string[] { request.ruleData.ruleId.ToString() }
                            };
                            return ret;
                        }
                        // Add to OperationsLog
                        OperationsLog.Add(request.userId,
                                          GetUsername(request.userId),
                                          FedExConstants.OperatorActions.DeleteSortplanRule,
                                          WebServices.Constants.OperatorActionsIds.DeleteSortplanRule,
                                          new string[] {    
                                              sortPlan.Description, 
                                              request.ruleData.ruleCode
                                          });
                        theSession.Transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(ex, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                } while (retry);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SortingplanRuleDeletePut has exception", e);
                throw;
            }
            finally
            {
                lockManager.Unlock(lockHandler);
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }
            return ret;
        }

        #region private method for sorting plan 

        /// <summary>
        /// Check if sorter has special parcel, D class and DPT must have assigned chute when activate sort plan
        /// </summary>
        /// <returns></returns>
        private ResponseError IsSpecialParcelTypeRuleFulfilled(SortPlan sortplan)
        {
            ResponseError ret = null;

            //Check if sorter has special parcel, D class and DPT must have assigned chute.
            SorterDetail detail = SorterInfo.GetSorterDetailBySorterIdAndKey(sortplan.SorterId, Entities.DBTables.Sorter.SorterDetail.SorterCapabilities.MAXSPECIALPARCELRULES);
            bool flag = long.Parse(detail.ValueAttr) > 0;
            if (flag)
            {
                List<SortingPlanRule> rules = SortingPlan.GetSortingPlanRulesByPlanIdRuleType(sortplan.Id, FedExConstants.SortingPlanRuleType.SpecialCode);
                SortingPlanRule ruleForDClass = rules.Where(r => r.RuleCode == FedExConstants.SpecialParcelType.DClass).FirstOrDefault();
                SortingPlanRule ruleForDiplomatic = rules.Where(r => r.RuleCode == FedExConstants.SpecialParcelType.DiplomaticCode).FirstOrDefault();
                ret = new ResponseError()
                {
                    Message = WebServices.Constants.SortingplanPageIds.SpecialParcaleRuleMissing,
                };
                if (ruleForDClass == null || ruleForDiplomatic == null)
                {
                    logger.ErrorMethod(String.Format("SortingplanActivateGet(): D or Diplomatic is miss, plan with ID: {0}", sortplan.Id));
                    return ret;
                }

                if (ruleForDClass != null)
                {
                    List<SortingPlanRuleChute> ruleChutes = SortingPlan.GetSortingPlanRuleChutesByRuleId(ruleForDClass.Id);
                    if (null == ruleChutes || !ruleChutes.Any())
                    {
                        logger.ErrorMethod(String.Format("SortingplanActivateGet(): D class rule chute is miss, plan with ID: {0}", sortplan.Id));
                        return ret;
                    }
                }

                if (ruleForDiplomatic != null)
                {
                    List<SortingPlanRuleChute> ruleChutes = SortingPlan.GetSortingPlanRuleChutesByRuleId(ruleForDClass.Id);
                    if (null == ruleChutes || !ruleChutes.Any())
                    {
                        logger.ErrorMethod(String.Format("SortingplanActivateGet(): diplomatic rule chute is miss, plan with ID: {0}", sortplan.Id));
                        return ret;
                    }
                }
                ret = null;
            }

            return ret;
        }

        /// <summary>
        ///  Check for sorter, exception chute and self pickup chute must exclude from rule assigned chutes when activate sort plan
        /// </summary>
        /// <returns></returns>
        private ResponseError ContainsExceptionAndSelfpickupChutesAsRegularChutes(SortPlan sortplan)
        {
            ResponseError ret = null;

            List<ExceptionChute> exceptionChutes = ExceptionChute.GetAllExceptionChutes(sortplan.SorterId);
            List<SortingPlanRuleChute> ruleChutes = SortingPlan.GetSortingPlanRuleChutesByPlanIdRuleType(sortplan.Id);
            List<long> ruleChuteIds = ruleChutes.Select(r => r.ChuteId).ToList();
            long chuteId = 0;

            if (UIHelper.CheckSelectChuteIsInExcepChute(ruleChuteIds, exceptionChutes, out chuteId))
            {
                Chute chute = ChuteInfo.GetChuteById(chuteId);

                ret = new ResponseError()
                {
                    Message = WebServices.Constants.SortingplanPageIds.ChuteAsException,
                    Arguments = new string[] { chute.OriginalDest }
                };
                logger.ErrorMethod(String.Format("SortingplanActivateGet(): chuteNo {0} as exception chute, plan with ID: {1}", chute.OriginalDest, sortplan.Id));
                return ret;
            }

            //If sorter has self pickup configuration, must filter self pickup chute.
            List<long> pickupChuteIds = GetSelfPickupChuteIdBySorterId(sortplan.SorterId);
            foreach (long pickupChuteId in pickupChuteIds)
            {
                if (ruleChuteIds.Contains(pickupChuteId))
                {
                    Chute chute = ChuteInfo.GetChuteById(pickupChuteId);
                    ret = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.ChuteAsSelfPickup,
                        Arguments = new string[] { chute.OriginalDest }
                    };
                    logger.ErrorMethod(String.Format("SortingplanActivateGet(): chuteNo {0} as self pickup chute, plan with ID: {1}", chute.OriginalDest, sortplan.Id));
                    return ret;
                }
            }

            return ret;
        }
        /// <summary>
        /// Check if the selection and handling code in sorting plan rule
        /// are missed in priority table
        /// </summary>
        /// <param name="sortplan">sorting plan</param>
        /// <returns></returns>
        private ResponseError CheckSelHandCodeInSpRuleMissedInPriorityTable(SortPlan sortplan)
        {
            ResponseError ret = null;

            // check the selection code and handling code
            var selCodesMissing = GetCodesInSpRulesMissedInPriorityTableByType(sortplan, FedExConstants.SortingPlanRuleType.SelectCode);
            var handCodesMissing = GetCodesInSpRulesMissedInPriorityTableByType(sortplan, FedExConstants.SortingPlanRuleType.HandCode);
            if (!string.IsNullOrEmpty(selCodesMissing) && !string.IsNullOrEmpty(handCodesMissing))
            {
                ret = new ResponseError()
                {
                    Message = WebServices.Constants.SortingplanPageIds.SelHandCodesMiss,
                    Arguments = new string[] { selCodesMissing, handCodesMissing }
                };
                logger.ErrorMethod(String.Format("SortingplanActivateGet(): Selection code ({0})/Handling code({1}) in sort plan(Id: {2}) are missed in the priority table", 
                                                 selCodesMissing, 
                                                 handCodesMissing, 
                                                 sortplan.Id));
            }
            else if (!string.IsNullOrEmpty(selCodesMissing))
            {
                ret = new ResponseError()
                {
                    Message = WebServices.Constants.SortingplanPageIds.SelCodesMiss,
                    Arguments = new string[] { selCodesMissing }
                };
                logger.ErrorMethod(String.Format("SortingplanActivateGet(): Selection code ({0}) in sort plan(Id: {1}) are missed in the priority table",
                                                 selCodesMissing,
                                                 sortplan.Id));
            }
            else if (!string.IsNullOrEmpty(handCodesMissing))
            {
                ret = new ResponseError()
                {
                    Message = WebServices.Constants.SortingplanPageIds.HandCodesMiss,
                    Arguments = new string[] { handCodesMissing }
                };
                logger.ErrorMethod(String.Format("SortingplanActivateGet(): Handling code ({0}) in sort plan(Id: {1}) are missed in the priority table",
                                                 handCodesMissing,
                                                 sortplan.Id));
            }

            return ret;
        }

        /// <summary>
        /// Get the selection and handling codes in sorting plan rule which are missed
        /// in selection and handling code priority table
        /// </summary>
        /// <param name="sortplan">sorting plan</param>
        /// <param name="codeType">code type, SELECTIONCODE and HANDLINGCODE</param>>
        /// <returns>codes missed in selection and handling code priority table</returns>
        private string GetCodesInSpRulesMissedInPriorityTableByType(SortPlan sortplan, string codeType)
        {
            string codesMissed = string.Empty;
            // check the code according to type
            var codes = GetSelectionHandlingCodeByType(codeType);
            if (codes.Any())
            {
                var spRuleCodes = SortingPlan.GetSortingPlanRulesByPlanIdRuleType(sortplan.Id, codeType);
                int i = 0;
                spRuleCodes.ForEach(r =>
                {
                    if (!codes.Where(t => t.Code == r.RuleCode).ToList().Any())
                    {
                        i++;
                        if (i == 1)
                        {
                            codesMissed += r.RuleCode;
                        }
                        else
                        {
                            codesMissed += "," + r.RuleCode;
                        }
                    }
                });
            }

            return codesMissed;
        }
        /// <summary>
        /// Get SorterChuteData info by chute Id
        /// </summary>
        /// <param name="chuteIds">lists of primary key chute Id</param>
        /// <returns>lists of chute data</returns>
        private List<SorterChuteData> GetSorterChuteDataByChuteIds(List<long> chuteIds)
        {
            var ret = new List<SorterChuteData>();
            try
            {
                chuteIds.ForEach(r =>
                {
                    var tmp = ChuteInfo.GetChuteById(r);
                    if (tmp != null)
                    {
                        ret.Add(new SorterChuteData()
                        {
                            chuteId = tmp.Id,
                            chuteValue = tmp.OriginalDest,
                            ChuteType = tmp.ChuteType,
                            Side = tmp.Side
                        });
                    }
                });
            }
            catch(Exception e)
            {
                logger.ErrorMethod("Failed to get sorter chute data by chute Ids", e);
            }
            return ret;
        }

        /// <summary>
        /// Get selection code and handling code by rule type
        /// </summary>
        /// <param name="ruleType">rule type, such as SELECTIONCODE</param>
        /// <param name="theSession">database session</param>
        /// <returns>lists of selection code and handling code</returns>
        private List<SelectionAndHandCode> GetSelectionHandlingCodeByType(string ruleType, IStatelessSession theSession = null)
        {
            var selHandCode = new List<SelectionAndHandCode>();
            // check selection code or handling code?
            if (ruleType.Contains(FedExConstants.SortingPlanRuleType.SelectCode))
            {
                selHandCode = SelecAndHandCode.GetSelectionCode(theSession);
            }
            else if (ruleType.Contains(FedExConstants.SortingPlanRuleType.HandCode))
            {
                // get the handling code without 'DPT' and 'D'
                selHandCode = SelecAndHandCode.GetHandlingCode(theSession)
                                              .Where(r => r.Code.Trim() != FedExConstants.SpecialParcelType.DiplomaticCode 
                                                       && r.Code.Trim() != FedExConstants.SpecialParcelType.DClass).ToList();
            }
            else if (ruleType == FedExConstants.SortingPlanRuleType.SpecialCode)
            {
                selHandCode.AddRange(new List<SelectionAndHandCode>
                {
                    new SelectionAndHandCode
                    {
                        Priority = 1,
                        Code = FedExConstants.SpecialParcelType.DClass
                    },
                    new SelectionAndHandCode
                    {
                        Priority = 2,
                        Code = FedExConstants.SpecialParcelType.DiplomaticCode
                    }
                });
            }
            return selHandCode;
        }

        /// <summary>
        /// Get sorting plan rule from selection code and handling code by rule type
        /// only get for selection code, handling cod and sepcial parcel
        /// </summary>
        /// <param name="spRules">sorting plan rule lists, return value</param>
        /// <param name="ruleType">rule type, such as SELECTIONCODE, HANDLINGCODE and son on. Refer to the RULETYPE column comment in the table fx_sortplanrule</param>
        /// <param name="session">database session, default is NULL</param>
        /// <returns>void</returns>
        private void GetSortPlanRulesFromSelHandCodeByType(ref List<SortingplanRuleData> spRules, string ruleType, IStatelessSession session = null)
        {
            try
            {
                // get selection and handling code bu rule type
                var selHandCode = GetSelectionHandlingCodeByType(ruleType, session);
                //loop all the selection code or handling code value
                if (selHandCode != null && selHandCode.Any())
                {
                    var tmp = new List<SortingplanRuleData>();
                    selHandCode.OrderBy(r => r.Priority).ForEach(r =>
                    {
                        tmp.Add(new SortingplanRuleData()
                        {
                            ruleId = 0, //default value, 0 means inserting to db
                            priority = r.Priority,
                            ruleCode = r.Code,
                            //extraRuleCode = string.Empty,
                            extraRuleCode = r.Code,
                            orphan = false,
                            chutes = new List<SorterChuteData>()
                        });
                    });
                    spRules.AddRange(tmp);
                }
            }
            catch (Exception e)
            {  
                logger.ErrorMethod(string.Format("Failed to Preload sorting plan rule by rule type {0}", ruleType), e);
                throw;
            }
        }

        /// <summary>
        /// Check if sorting plan rule should merge with selection and handling code by type
        /// </summary>
        /// <param name="ruleType">rule type, such as SELECTIONCODE, and son on</param>
        /// <returns>true means should merge with selection code and handling code, false means not</returns>
        private bool IsSpRulesMergeWithSelHandCodeByType(string ruleType)
        {
            bool ret;

            // check rule type
            if (ruleType.Contains(FedExConstants.SortingPlanRuleType.SelectCode)
             || ruleType.Contains(FedExConstants.SortingPlanRuleType.HandCode)
             || ruleType == FedExConstants.SortingPlanRuleType.SpecialCode)
            {
                ret = true;
            }
            else
            {
                ret = false;
            }
  
            return ret;
        }

        /// <summary>
        /// merge sorting plan rule data by sorting plan rule of selection code and handling code
        /// Check if sorting plan ID and sorting plan rule type
        /// </summary>
        /// <param name="sortingplanId">sorting plan Id</param>
        /// <param name="ruleType">sorting plan rule type</param>
        /// <param name="session">database session</param>
        /// <returns>void</returns>
        private void CheckSortingPlanIdAndRuleType(long sortingplanId, string ruleType, IStatelessSession session = null)
        {
            if (SortingPlan.GetSortPlanByPlanId(sortingplanId, session) == null)
            {
                logger.ErrorMethod(string.Format("INVALID sorting plan (Id:{0})", sortingplanId));
                throw new ArgumentException(WebServices.Constants.SortingplanPageIds.InvalidSortplan);
            }
            // if ruleType
            if (string.IsNullOrWhiteSpace(ruleType))
            {
                logger.ErrorMethod(string.Format("Argument ruleType({0}) is INVALID", ruleType));
                throw new ArgumentException(WebServices.Constants.SortingplanPageIds.RuleTypeMissing);
            }
        }

        /// <summary>
        /// and return the merged sorting plan data
        /// </summary>
        /// <param name="spRules">sorting plan rule data</param>
        /// <param name="spRulesBySelHandCode">sorting plan rule preload data</param>
        /// <param name="flagMerge">preload flag, true equal to preload, otherwise false</param>
        /// <param name="session">database session</param>
        /// <returns>lists of sorting plan rule data</returns>
        private List<SortingplanRuleData> MergeAndReturnSortingPlanRulesData(List<SortingPlanRule> spRules,
                                                                             List<SortingplanRuleData> spRulesBySelHandCode,
                                                                             bool flagMerge = false,
                                                                             IStatelessSession session = null)
        {
            var ret = new List<SortingplanRuleData>();

            try
            {
                // add the sorting plan rule by selection code and handling code to buffer
                if (spRulesBySelHandCode.Any() && flagMerge)
                {
                    ret.AddRange(spRulesBySelHandCode);
                }

                if (spRules != null && spRules.Any())
                {
                    spRules.ForEach(r =>
                    {
                        var chutesInfo = SortingPlan.GetSortingPlanRuleChutesByRuleId(r.Id, session).OrderBy(x=>x.Id);
                        var chuteDefault = new List<SorterChuteData>();
                        var sortingPlanRuleData = ret.Where(t => t.ruleCode == r.RuleCode && t.extraRuleCode == r.ExtraRuleCode)
                                                     .OrderByDescending(t=>t.ruleId)
                                                     .ToList();
                        // if merge with selection code and handling code, just merge sorting plan rule with selection and handling code
                        // or not merged, with unique destination key pair code
                        if (flagMerge || !sortingPlanRuleData.Any()) 
                        {
                            var tmpSpRuleData = new SortingplanRuleData();
                            if (sortingPlanRuleData.Any())
                            {
                                tmpSpRuleData = sortingPlanRuleData.First();
                            }
                            // fill value
                            {
                                tmpSpRuleData.ruleId = r.Id;
                                tmpSpRuleData.priority = r.Priority;
                                tmpSpRuleData.ruleCode = r.RuleCode;
                                tmpSpRuleData.extraRuleCode = r.ExtraRuleCode;
                                tmpSpRuleData.orphan = flagMerge & !sortingPlanRuleData.Any();
                                tmpSpRuleData.chutes = chutesInfo.Any() ? GetSorterChuteDataByChuteIds(chutesInfo.Select(s => s.ChuteId).ToList()) : chuteDefault;
                            }
                            // if sorting plan rule for selection and handling code without any data
                            // add the sorting plan rule data to the output lists
                            if (!sortingPlanRuleData.Any())
                            {
                                ret.Add(tmpSpRuleData);
                            }
                        }
                        // if not merge and sorting plan rule with data menas the same rules
                        // igonre the processing for the same sorting plan rule.
                        //else
                        //{
                        //}
                    });
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("Failed to merge and return sorting plan rule data", e);
                throw;
            }
            return ret;
        }

        /// <summary>
        /// Check sorting plan rule put reuqest logic
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private ResponseBase CheckSortingPlanRulePutLogic(SortingplanRulePutRequest request)
        {
            ResponseBase ret = new ResponseBase();

            //Check sort plan whether it is active, if active, return false
            bool flag = SortingPlan.IsSortingPlanActiveByPlanId(request.sortingplanId);
            if (flag)
            {
                ret.Error = new ResponseError()
                {
                    Message = WebServices.Constants.SortingplanPageIds.SortplanIsActive,
                };
                logger.ErrorMethod(string.Format("SortingplanRulePut(): sort plan is active by sortingplanId {0}", request.sortingplanId));
                return ret;
            }

            //For line sorter, to make sure direction of selected chute is the same.
            List<Sorter> allSorters = SorterInfo.GetAllSorters();
            Sorter currentSorter = allSorters.Where(s => s.Id == request.sorterId).FirstOrDefault();
            if (currentSorter.LogicalType == FedExConstants.SorterLogicalType.LineSorter && null != request.rule.chutes && request.rule.chutes.Any())
            {
                List<Chute> sorterChutes = ChuteInfo.GetChutesBySorterId(request.sorterId);
                List<long> selectChuteId = request.rule.chutes.Select(c => c.chuteId).ToList();
                List<Chute> selectChutes = sorterChutes.Where(c => selectChuteId.Contains(c.Id)).ToList();

                if (selectChutes.Any(c => c.Side == FedExConstants.ChuteSide.Left) && selectChutes.Any(c => c.Side == FedExConstants.ChuteSide.Right))
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.SorterChuteSideKeepSame,
                    };
                    logger.ErrorMethod("SortingplanRulePut(): Sorter chute side must keep same");
                    return ret;
                }
            }

            //Check destination flight/IRoads key pair is unique in current sorter if insert new rule p44  
            if (UIHelper.CheckIsDestinationRuleType(request.ruleType))
            {
                List<SortingPlanRule> currentSorterRules = currentSorterRules = SortingPlan.GetSortingPlanRuleBySorterId(currentSorter.Id);
                //If create new rule, just check destination flight/IRoads key pair
                if (request.rule.ruleId == 0)
                {
                    currentSorterRules = currentSorterRules.Where(r => r.RuleType.Contains("DEST")).ToList();
                }
                else//If edit one rule, exclude request ruleId
                {
                    currentSorterRules = currentSorterRules.Where(c => c.Id != request.rule.ruleId && c.RuleType.Contains("DEST")).ToList();
                }

                flag = currentSorterRules.Any(r => r.RuleCode.ToLower() == request.rule.ruleCode.Trim().ToLower() 
                    && r.ExtraRuleCode.ToLower() == request.rule.extraRuleCode.Trim().ToLower()
                    && r.RuleType == request.ruleType);
                if (flag)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.DestKeyPairNotUnique,
                        Arguments = new string[] { request.rule.ruleCode, request.rule.extraRuleCode, currentSorter.EqId }
                    };
                    logger.ErrorMethod(string.Format("SortingplanRulePut(): the destination key pair is not unique for sorter {0}", currentSorter.EqId));
                    return ret;
                }
            }

            //Check whether select chutes is in exception chutes. And for documenter to check chutes used in other custom clearance status table are also excluded.
            if (request.rule.chutes != null && request.rule.chutes.Any())
            {
                List<ExceptionChute> exceptionChute = ExceptionChute.GetAllExceptionChutes(request.sorterId);
                List<long> chuteIds = request.rule.chutes.Select(r => r.chuteId).ToList();
                long chuteId = 0;

                if (UIHelper.CheckSelectChuteIsInExcepChute(chuteIds, exceptionChute, out chuteId))
                {
                    Chute chute = ChuteInfo.GetChuteById(chuteId);
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.ChuteAsException,
                        Arguments = new string[] { chute.OriginalDest }
                    };
                    logger.ErrorMethod(String.Format("SortingplanRulePut(): chuteNo {0} as exception chute, plan with ID: {1}", chute.OriginalDest, request.sortingplanId));
                    return ret;
                }

                // for documenter to check chutes used in other custom clearance status table are also excluded.
                if (currentSorter.LogicalType == FedExConstants.SorterLogicalType.Document)
                {
                    List<SortingPlanRuleChute> customRuleChutes = null;
                    string otherCustomRuleType = UIHelper.GetOhterCustomClearType(request.ruleType);
                    if (otherCustomRuleType != "")
                    {
                        customRuleChutes = sortingPlan.GetSortingPlanRuleChutesByPlanIdRuleType(request.sortingplanId, otherCustomRuleType);
                        //Check chute has been used in other custom clearance status
                        ret = UIHelper.CheckChuteIsUsedInCustomeClearStatus(request.rule.chutes, customRuleChutes);
                        if (ret.Error != null)
                        {
                            return ret;
                        }
                    }
                }
            }

            //Same destination and flight pair should not be set on the sort plans of different final outbound sorters. p113 p126  ???
            #region For sorter SS00001 and SS0002 check destination key pair is unique, destination code is used at one sorter then it can’t be used on other
            //For sorter SS00001 and SS0002.  Think: if edit one rule and just flight code has change, this is right.
            //One destination key pair is unique for two conveyable final sorters SS01 and SS02. When destination code is used at one sorter then it can’t be used on other
            if (request.ruleType == FedExConstants.SortingPlanRuleType.DestFlightcode &&
                (currentSorter.EqId == FedExConstants.SorterEqId.ObFinA || currentSorter.EqId == FedExConstants.SorterEqId.ObFinB))
            {
                Sorter sorterS01 = allSorters.Where(s => s.EqId == FedExConstants.SorterEqId.ObFinA).FirstOrDefault();
                Sorter sorterS02 = allSorters.Where(s => s.EqId == FedExConstants.SorterEqId.ObFinB).FirstOrDefault();

                List<SortingPlanRule> ruleForS01 = SortingPlan.GetSortingPlanRuleBySorterId(sorterS01.Id);
                List<SortingPlanRule> ruleForS02 = SortingPlan.GetSortingPlanRuleBySorterId(sorterS02.Id);

                string otherSorterName = request.sorterId == sorterS01.Id ? sorterS02.EqId : sorterS01.EqId;

                flag = request.sorterId == sorterS01.Id
                       ? ruleForS02.Any(r => r.RuleCode.ToLower() == request.rule.ruleCode.Trim().ToLower() && r.ExtraRuleCode.ToLower() == request.rule.extraRuleCode.Trim().ToLower())
                       : ruleForS01.Any(r => r.RuleCode.ToLower() == request.rule.ruleCode.Trim().ToLower() && r.ExtraRuleCode.ToLower() == request.rule.extraRuleCode.Trim().ToLower());
                      ;
                if (flag)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.DestKeyPairNotUnique,
                        Arguments = new string[] { request.rule.ruleCode, request.rule.extraRuleCode, otherSorterName }
                    };
                    logger.ErrorMethod("SortingplanRulePut(): the destination key pair is not unique for SS00001 and SS00002");
                    return ret;
                }

                //Insert or edit one new rule, check when destination code is used at one sorter then it can’t be used on other
                flag = request.sorterId == sorterS01.Id
                         ? ruleForS02.Any(r => r.RuleCode.ToLower() == request.rule.ruleCode.Trim().ToLower())
                         : ruleForS01.Any(r => r.RuleCode.ToLower() == request.rule.ruleCode.Trim().ToLower());
                if (flag)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SortingplanPageIds.DestCodeHasExisting,
                        Arguments = new string[] { request.rule.ruleCode, otherSorterName }
                    };
                    logger.ErrorMethod("SortingplanRulePut(): the destination code is not unique");
                    return ret;
                }

                //if (request.rule.ruleId == 0)//Insert one new rule, check when destination code is used at one sorter then it can’t be used on other
                //{
                //    flag = request.sorterId == sorterS01.Id
                //        ? ruleForS02.Any(r => r.RuleCode.ToLower() == request.rule.ruleCode.Trim().ToLower())
                //        : ruleForS01.Any(r => r.RuleCode.ToLower() == request.rule.ruleCode.Trim().ToLower());
                //    if (flag)
                //    {
                //        ret.Error = new ResponseError()
                //        {
                //            Message = WebServices.Constants.SortingplanPageIds.DestCodeHasExisting,
                //        };
                //        logger.ErrorMethod("SortingplanRulePut(): the destination code is not unique");
                //        return ret;
                //    }
                //}
            }
            #endregion

            return ret;
        }
     
        ///// <summary>
        ///// Mock sorting plan rule test data
        ///// </summary>
        ///// <param name="spId">sorting plan Id</param>
        ///// <param name="rulType"></param>
        ///// <returns></returns>
        //private SortingplanRulesGetResponse MockSortingPlanRuleTestData(long spId, string rulType)
        //{
        //        if (rulType == FedExConstants.SortingPlanRuleType.SelectCode)
        //        {
        //            var ret = new SortingplanRulesGetResponse() { sortplanId = spId, ruleType = rulType };
        //            ret.rules = new List<SortingplanRuleData>();
        //            ret.rules.Add(new SortingplanRuleData() { ruleId = 0, priority = 1, ruleCode = "S20", extraRuleCode = "", chutes = null });
        //            ret.rules.Add(new SortingplanRuleData() { ruleId = 0, priority = 5, ruleCode = "S99", extraRuleCode = "", chutes = null });
        //            return ret;
        //        }

        //        if (rulType == FedExConstants.SortingPlanRuleType.HandCode) 
        //        {
        //            var ret = new SortingplanRulesGetResponse() { sortplanId = spId, ruleType = rulType };
        //            ret.rules = new List<SortingplanRuleData>();
        //            ret.rules.Add(new SortingplanRuleData() { ruleId = 0, priority = 1, ruleCode = "AAS", extraRuleCode = "", chutes = null });
        //            ret.rules.Add(new SortingplanRuleData() { ruleId = 0, priority = 4, ruleCode = "UIP", extraRuleCode = "", chutes = null });
        //            return ret;
        //        }

        //    return null;
        //}

        /// <summary>
        /// When activate sort plan, do some validation 
        /// </summary>
        /// <param name="sortplan"></param>
        /// <returns></returns>
        private ResponseError ValidateWhenActivateSortPlan(SortPlan sortplan)
        {
            ResponseError ret = null;

            //Check for sorter has special parcel, D class and DPT must have assigned chute.
            ret = IsSpecialParcelTypeRuleFulfilled(sortplan);
            if (ret != null)
            {
                return ret;
            }

            //Check for sorter, exception chute and self pickup chute must exclude from rule assigned chutes
            ret = ContainsExceptionAndSelfpickupChutesAsRegularChutes(sortplan);
            if (ret != null)
            {
                return ret;
            }

            //Check if selection and handling code for sorting plan rule are missing in the priority table
            //IRD page 28, "deactived sort plan can't be ..."
            ret = CheckSelHandCodeInSpRuleMissedInPriorityTable(sortplan);
            if (ret != null)
            {
                return ret;
            }

            //Check is there any active sorting plan for his sorter
            bool flag = SortingPlan.IsSorterHasPlanActiveBySorterId(sortplan.SorterId);
            if (flag)
            {
                var activeSortplan = SortingPlan.GetActiveSortPlanList().FirstOrDefault(s => s.SorterId == sortplan.SorterId);
                ret = new ResponseError()
                {
                    Message = WebServices.Constants.SortingplanPageIds.SortplanIsActive, 
                    Arguments = new string[] { activeSortplan == null ? "" : activeSortplan.Description }
                };
                logger.ErrorMethod(String.Format("SortingplanActivateGet(): Sorter {0} has active sorting plan '{1}'", 
                                                 sortplan.SorterId, 
                                                 activeSortplan == null ? "" : activeSortplan.Description));
                return ret;
            }

            //Check whether the sortplan is active
            flag = SortingPlan.IsSortingPlanActiveByPlanId(sortplan.Id);
            if (flag)
            {
                ret = new ResponseError() { Message = WebServices.Constants.SortingplanPageIds.SortplanIsActive };
                logger.ErrorMethod(String.Format("SortingplanActivateGet(): Sorting plan with ID: {0} is active", sortplan.Id));
                return ret;
            }

            return ret;
        }
        
        #endregion
        
        #endregion

        #region SystemLog related

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
        /// <returns>An empty list of no entries found.</returns>
        public SystemLogGetResponse SystemlogGet(long userId, DateTime startTime, DateTime endTime, long maxRecords, long startId)
        {
            //IStatelessSession theSession = null;
            var ret = new SystemLogGetResponse { Error = UIHelper.ValidateStartEndTimes(startTime, endTime) };

            if (ret.Error != null)
            {
                return ret;
            }
            
            try
            {
                ret.entries = SystemLog.GetEntries(userId, startTime, endTime, maxRecords, startId);
                ret.maxId = (ret.entries != null) && (ret.entries.Count > 0) ? ret.entries.Max(x => x.Id) : 0;
                //// Name of the transaction for statistics only
                //theSession = SessionPool.GetStatelessSession("SystemlogGet");
                //bool retry;
                //do
                //{
                //    theSession.BeginTransaction();
                //    try
                //    {
                //        retry = false;

                //        // Just an example. We would not need session context here.
                //        ret.entries = SystemLog.GetEntries(userId, startTime, endTime, maxRecords, startId, theSession);
                //        ret.maxId = (ret.entries != null) && (ret.entries.Count > 0) ? ret.entries.Max(x => x.Id) : 0;
                        
                //        theSession.Transaction.Commit();
                //    }
                //    catch (Exception e)
                //    {
                //        // HandleError will do a rollback and even a reconnect if necessary.
                //        retry = SessionPool.HandleError(e, ref theSession);
                //        if (!retry)
                //        {
                //            throw;
                //        }
                //    }
                //}
                //while (retry);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SystemlogGet Exception occured. ", e);
                throw;
            }
            //finally
            //{
            //    if (theSession != null)
            //    {
            //        SessionPool.ReleaseSession(theSession);
            //    }

            //}

            return ret;
        }

        #endregion

        #region OperationLog related

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
        public OperationLogGetResponse OperationlogGet(long userId, DateTime startTime, DateTime endTime, long maxRecords, long startId)
        {
            var ret = new OperationLogGetResponse { entries = new List<OperationsLogEntry>(), 
                                                    Error = UIHelper.ValidateStartEndTimes(startTime, endTime) };

            if (ret.Error != null)
            {
                return ret;
            }

            try
            {
                ret.entries = OperationsLog.GetEntries(userId, startTime, endTime, maxRecords, startId);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("OperationlogGet() exception occured. ", e);
                throw;
            }

            return ret;
        }

        #endregion

        #region Interception Parcel related functionality

        public InterceptionParcelReportGetResponse InterceptionParcelReportGet(long userId, DateTime startTime, DateTime endTime, long maxRecords, long startId)
        {
            var ret = new InterceptionParcelReportGetResponse { parcelReports = new List<InterceptionParcelReport>(),
                                                                Error = UIHelper.ValidateStartEndTimes(startTime, endTime)
            };

            if (ret.Error != null)
            {
                return ret;
            }

            try
            {
                var parcels = Interception.GetInterceptionReport(0, startTime, endTime, maxRecords, startId);

                //int seq = 0;

                foreach (var parcel in parcels.OrderBy(p => p.Id))
                {
                    ret.parcelReports.Add(new InterceptionParcelReport()
                    {
                        //sequenceNo = ++seq, 
                        id = parcel.Id, 
                        planChuteNo = parcel.ChuteName,
                        actualChuteNo = parcel.ActualChuteName,
                        arrivedTime = parcel.Created, 
                        barcodeAWB = parcel.Awb, 
                        barcodeMPS = parcel.Mps, 
                        sorterName = parcel.SortName 
                    });
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("InterceptionParcelReportGet() exception occured. ", e);
                throw;
            }

            return ret;
        }


        /// <summary>
        /// Requests current imported Interception list.
        /// Information is ordered by database ID ascending.
        /// </summary>
        /// <param name="userId">UserId asking for this information.</param>
        /// <returns></returns>
        public InterceptionImportGetResponse InterceptionImportGet(long userId)
        {
            InterceptionImportGetResponse ret = new InterceptionImportGetResponse();
            ret.interceptionList = new List<InterceptionData>();

            try
            {
                InterceptionSession activeIntercapSession = Interception.GetActiveInterceptionSession();
                if (activeIntercapSession == null)
                {
                    logger.WarnMethod("There is no active interception session.");
                    return ret;
                }

                ret = UIHelper.ConvertFromInterceptionParcels(Interception.GetInterceptionListBySession(activeIntercapSession.Id));
                //ret = UIHelper.ConvertFromInterceptionParcels(Interception.GetActiveInterceptionList());
            }
            catch (Exception e)
            {
                logger.ErrorMethod("InterceptionImportListGet() exception occured. ", e);
                throw;
            }

            return ret;
        }

        /// <summary>
        /// Import new Interception list.
        /// <param name="newInterceptionList">New interception list</param>
        /// </summary>
        /// <returns></returns>
        public InterceptionImportPutResponse InterceptionImportPut(InterceptionImportPutRequest newInterceptionList)
        {
            InterceptionImportPutResponse ret = new InterceptionImportPutResponse();

            try
            {
                bool import = true;
                List<InterceptionParcel> notSortedIntercepionParcels = Interception.GetNotSortedActivteInterceptionList();

                //If there is not enforce overwrite and not sorted interception parcel,
                //To check whether these barcode is in new imported file, if not existing, should return svs to notice user.
                if (newInterceptionList.enforceOverwrite == false && notSortedIntercepionParcels != null && notSortedIntercepionParcels.Any())
                {
                    logger.InfoMethod("To check whether these barcode is in new imported file, if not existing, should return svs to notice user.");
                    List<string> unSortedExistingBarcode = new List<string>();
                    foreach (InterceptionParcel item in notSortedIntercepionParcels)
                    {
                        // if not existing, should add to list
                        if (!newInterceptionList.csvLines.Any(c => c.Contains(item.Barcode)))
                        {
                            if (unSortedExistingBarcode.Count > 0 && (unSortedExistingBarcode.Count + 1) % 5 == 0)
                            {
                                unSortedExistingBarcode.Add(item.Barcode+ "<br/>");
                            }
                            else
                            {
                                unSortedExistingBarcode.Add(item.Barcode);
                            }
                        }
                    }

                    if (unSortedExistingBarcode.Count > 0)
                    {
                        ret.notSortedBarcodes = "<br/>" +  String.Join(",", unSortedExistingBarcode) + "<br/>";
                        import = false;
                    }
                }

                if (import == false)
                {
                    logger.InfoMethod("notice user to whether enforce overwriting.");
                    ret.needsUserItercation = true;
                    return ret;
                }

                var importedRecords = UIHelper.ImportBarcodesAndTimesFromCsv(newInterceptionList.csvLines, 
                                                                                FedExConstants.InterceptionImportFileHeader, 
                                                                                newInterceptionList.timeOffset, 
                                                                                MaxInterceptionImportRecord); 

                // Copy data for response
                ret.Error = importedRecords.error;
                ret.succesfullyImportedRowsNumber = importedRecords.succesfullyImportedRowsNumber;
                ret.duplicatedRowsNumber = importedRecords.duplicatedRowsNumber;
                ret.errorneousRowsNumber = importedRecords.errorneousRowsNumber;

                foreach (var importedRecord in importedRecords.records.Where(r => r.toReturn))
                {
                    ret.interceptionList.Add(new InterceptionImportData()
                    {
                        sequenceNo = importedRecord.line,
                        barcode = importedRecord.barcodeInput,
                        startTime = importedRecord.startTimeInput,
                        endTime = importedRecord.endTimeInput,
                        errors = importedRecord.errors
                    });
                }


                if (ret.Error == null)
                {
                    var interceptionList = new List<InterceptionParcel>();

                    // Extract only valid records for saving in database and convert them
                    foreach (var item in importedRecords.records.Where(r => r.errors.Count == 0))
                    {
                        interceptionList.Add(new InterceptionParcel()
                        {
                            UserID = newInterceptionList.userId,
                            Barcode = item.barcodeOutput,
                            StartDate = item.startTimeOutput,
                            EndDate = item.endTimeOutput,
                            Id = 0
                        });
                    }

                    Interception.SetInterceptionBarcodes(interceptionList);

                    OperationsLog.Add(newInterceptionList.userId,
                                        GetUsername(newInterceptionList.userId),
                                        FedExConstants.OperatorActions.InterceptionImport,
                                        WebServices.Constants.OperatorActionsIds.InterceptionImport,
                                        new[] { newInterceptionList.filename });

                    if (notSortedIntercepionParcels != null && notSortedIntercepionParcels.Any())
                    {
                        SystemLog.AddEntries(new List<SystemLogEntry>(){ new SystemLogEntry()
                        {
                            IdsLogAction = FedExConstants.SystemLogAction.InterceptListCanceled,
                            SorterOrProcess = FedExConstants.SystemLogProcess.UserEvent,
                            DeletedTag = FedExConstants.DeletedTag.NotDeleted
                        }});
                    }
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("InterceptionImportPut() exception occured. ", e);
                throw;
            }

            return ret;
        }

        /// <summary>
        /// Delete current active Interception list.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="interceptionSessionID">Session ID of interception list for deleting</param>
        /// <returns></returns>
        public ResponseBase InterceptionImportDeleteGet(long userId, long interceptionSessionID)
        {
            ResponseBase ret = new ResponseBase();
            // Name of the transaction for statistics only
            IStatelessSession theSession = SessionPool.GetStatelessSession("InterceptionImportDeleteGet");

            try
            {        
                bool retry;
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        retry = false;

                        if (!Interception.DeleteInterceptionBarcodes(interceptionSessionID, theSession))
                        {
                            theSession.Transaction.Rollback();
                            ret.Error = new ResponseError() { Message = WebServices.Constants.ImportPagesIds.DeleteError };
                            logger.ErrorMethod(String.Format("Unsuccessful deleting interception list with Session ID: {0} by User ID: {1}", interceptionSessionID, userId));
                            return ret;
                        }

                        theSession.Transaction.Commit();
                        logger.InfoMethod(String.Format("Interception list with Session ID: {0} deleted by User ID: {1}", interceptionSessionID, userId));
                    }
                    catch (Exception e)
                    {
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(e, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                }
                while (retry);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("InterceptionImportDeleteListGet() exception occured. " + e.ToString(), e);
                throw;
            }
            finally
            {
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }

            return ret;
        }

        #endregion

        #region Logout related functionality

        /// <summary>
        /// Requests information stored in the operation log. Information is ordered by database ID ascending.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="ipAddress">IP address of client machine</param>
        /// <returns></returns>
        public ResponseBase LogoutPut(string userName, string ipAddress)
        {
            var ret = new ResponseBase();

            try
            {
                OperationsLog.Add(GetUserId(userName), userName, FedExConstants.OperatorActions.Logout, WebServices.Constants.OperatorActionsIds.Logout, new string[] { ipAddress });
            }
            catch (Exception e)
            {
                logger.ErrorMethod("LogoutPut() exception occured. ", e);
                throw;
            }

            return ret;
        }

        #endregion

        #region Selection and Handling codes functionality

        /// <summary>
        /// Requests current imported selection and handling codes.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="codeType">Requested code type (Selection 'S' or Handling 'H' code type)</param>
        /// <returns></returns>
        public ImportSelHandlingGetResponse SelectionHandlingImportGet(long userId, string codeType)
        {
            var ret = new ImportSelHandlingGetResponse() {codes = new List<ImportSelHandlingData>()};
            Guid storyHandle = Guid.Empty;

            try
            {
                // After this point, please don't use return. Finaly block must be executed
                storyHandle = LockManager.Lock("SelectionHandlingImportGet");

                List<SelectionAndHandCode> codes = null;

                if (codeType.ToUpper().Equals(FedExConstants.HandlingCode))
                {
                    codes = SelecAndHandCode.GetHandlingCode();
                }
                else if (codeType.ToUpper().Equals(FedExConstants.SelectionCode))
                {
                    codes = SelecAndHandCode.GetSelectionCode();
                }

                if (codes != null)
                {
                    foreach (var item in codes)
                    {
                        ret.codes.Add(new ImportSelHandlingData()
                        {
                            Priority = Convert.ToString(item.Priority),
                            Code = item.Code,
                            CodeType = item.CodeType
                        });
                    }
                }
                else
                {
                    ret.Error = new ResponseError() { Message = WebServices.Constants.SelHandleImportPageIds.UnsupportedCodeType };
                }

            }
            catch (Exception e)
            {
                logger.ErrorMethod("SelectionHandlingImportGet() exception occured. ", e);
                throw;
            }
            finally
            {
                if (storyHandle != Guid.Empty)
                {
                    LockManager.Unlock(storyHandle);
                }
            }

            return ret;
        }

        /// <summary>
        /// Import new list with selection and handle codes.
        /// <param name="request">list of codes for import</param>
        /// </summary>
        /// <returns></returns>
        public ImportSelHandlingPutResponse SelectionHandlingImportPut(ImportSelHandlingPutRequest request)
        {
            ImportSelHandlingPutResponse ret;
            Guid storyHandle = Guid.Empty;

            try
            {
                ret = UIHelper.ConvertSelHandleCodesFromCSV(request.csvLines, MaxSelectionImportCodes, MaxHandlingImportCodes);
                if (ret.Success == false)
                {
                    ret.codes = ret.codes.Where(c => c.errors.Count > 0).ToList();
                    return ret;
                }

                // After this point, please don't use return. Finaly block must be executed
                storyHandle = LockManager.Lock("SelectionHandlingImportPut");

                // Check are missing some codes from active sortplans
                List<SortingPlanRuleChutePair> activePlanRules = SortingPlan.GetActiveSortPlanRuleChuteList();
                List<string> existingCodes = new List<string>();

                // Check are missing some selection code
                var selectionCodes = ret.codes.Where(c => c.CodeType.ToUpper().Equals(FedExConstants.SelectionCode)).ToList();
                foreach (var rule in activePlanRules.Where(r => r.RuleType.Equals(FedExConstants.SortingPlanRuleType.SelectCode)))
                {
                    if (!selectionCodes.Any(c => c.Code.Equals(rule.RuleCode)))
                    {
                        if (!existingCodes.Contains(rule.RuleCode))
                            existingCodes.Add(rule.RuleCode);
                    }
                }

                // Check are missing some handling  code
                var handlingCodes = ret.codes.Where(c => c.CodeType.ToUpper().Equals(FedExConstants.HandlingCode)).ToList();
                foreach (var rule in activePlanRules.Where(r => r.RuleType.Equals(FedExConstants.SortingPlanRuleType.HandCode)))
                {
                    if (!handlingCodes.Any(c => c.Code.Equals(rule.RuleCode)))
                    {
                        if (!existingCodes.Contains(rule.RuleCode))
                            existingCodes.Add(rule.RuleCode);
                    }
                }

                if (existingCodes.Count > 0)
                {
                    string joinCodes = String.Join(",", existingCodes);
                    ret.codes = ret.codes.Where(r => existingCodes.Contains(r.Code)).ToList();

                    logger.Warn(string.Format("Those codes: {0} are missing in active sort plans.", joinCodes));
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SelHandleImportPageIds.ExistCodeInActivePlan,
                        Arguments = new string[] { joinCodes }
                    };
                }

                if (ret.Error == null)
                {
                    DateTime now = DateTime.UtcNow;
                    // Convert to db entity list
                    List<SelectionAndHandCode> dbEntries = ret.codes.Select(item => new SelectionAndHandCode()
                    {
                        Priority = Int32.Parse(item.Priority),
                        Code = item.Code.ToUpper(),
                        CodeType = item.CodeType.ToUpper(),
                        UserId = request.userId,
                        Created = now,
                        Updated = now
                    }).ToList();

                    SelecAndHandCode.SaveSelectionAndHandCode(dbEntries);

                    OperationsLog.Add(request.userId,
                                        GetUsername(request.userId),
                                        FedExConstants.OperatorActions.SelHandlingImport,
                                        WebServices.Constants.OperatorActionsIds.SelHandlingImport,
                                        new[] { request.filename });

                    ret.SuccessImportedSelectionCodes = dbEntries.Count(c => c.CodeType.Equals(FedExConstants.SelectionCode));
                    ret.SuccessImportedHandlingCodes = dbEntries.Count(c => c.CodeType.Equals(FedExConstants.HandlingCode));
                    ret.codes.Clear();
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SelectionHandlingImportPut() exception occured. " + e.ToString(), e);
                throw;
            }
            finally
            {
                if (storyHandle != Guid.Empty)
                {
                    LockManager.Unlock(storyHandle);
                }
            }

            return ret;
        }

        #endregion

        #region Self-pickup related functionality

        /// <summary>
        /// return chutes assigned to self pickup list
        /// </summary>
        /// <param name="selfPickupListId"></param>
        /// <returns>list of string with chute names</returns>
        private List<string> GetAssignedSelfPickupChutes(long selfPickupListId)
        {
            var ret = new List<string>();

            var assignedChutes = SelfPickup.GetSelfPickupChutesByPickupListID(selfPickupListId);
            if (assignedChutes != null)
            {
                ret.AddRange(assignedChutes.Select(assignedChute => ChuteInfo.GetChuteById(assignedChute.ChuteID).OriginalDest));
            }

            return ret;
        }

        /// <summary>
        /// Create list of self-pickup chutes from strings with chute names.
        /// Also check are chute names correct. 
        /// For inbound pre-sorter ('PS00005') and inbound final sorter ('SS00005') only chute is allowed.
        /// For inbound document sorter ('AS00001') up to three chutes are allowed
        /// </summary>
        /// <param name="chuteDataPairs"></param>
        /// <param name="sorterId"></param>
        /// <param name="error">error if any</param>
        /// <returns></returns>
        private List<SelfPickupChute> ConvertSelfPickupChutes(List<ChuteDataPair> chuteDataPairs, long sorterId, out ResponseError error)
        {
            error = null;
            int maxAllowedChutes = 0;
            bool isValid = false;

            var sorterDetail = SorterInfo.GetAllSorterDetailInfo(sorterId);
            if (sorterDetail != null)
            {
                string value = sorterDetail.sorterDetails.First(d => d.KeyAttr.Equals(SorterDetail.SorterCapabilities.MAXPICKUPCHUTECOUNT)).ValueAttr;
                isValid = Int32.TryParse(value, out maxAllowedChutes);
                if (isValid && maxAllowedChutes < 1)
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                error = new ResponseError()
                {
                    Message = WebServices.Constants.ImportPagesIds.InvalidSorter,
                    Arguments = new string[] { sorterId.ToString() }
                };
                logger.WarnMethod(String.Format("ConvertSelfPickupChutes(): Invalid sorter with ID: {0}", sorterId));
                return null;
            }

            if (null != chuteDataPairs && chuteDataPairs.Count > maxAllowedChutes) 
            {
                error = new ResponseError()
                {
                    Message = WebServices.Constants.ImportPagesIds.ExceedChuteCount,
                    Arguments = new string[] { maxAllowedChutes.ToString() }
                };
                logger.WarnMethod(String.Format("ConvertSelfPickupChutes(): Too many selected chutes for sorter ID: {0}. Max allowed is: {1}", sorterId, maxAllowedChutes));
                return null;
            }

            var selfPickupChutes = new List<SelfPickupChute>();
            if (null == chuteDataPairs || chuteDataPairs.Count == 0)
            {
                return selfPickupChutes;
            }

            var chutes = ChuteInfo.GetChutesBySorterId(sorterId);           
            foreach (var chute in chuteDataPairs)
            {
                var foundChute = chutes.FirstOrDefault(c => c.Id == chute.chuteId);

                if (foundChute == null)
                {
                    error = new ResponseError()
                    {
                        Message = WebServices.Constants.ImportPagesIds.SelectedChutesNotFound,
                        Arguments = new string[] { chute.chuteValue }
                    };
                    logger.WarnMethod(String.Format("ConvertSelfPickupChutes(): Chute not foiund with ID: {0}", chute.chuteId));
                    return null;
                }

                selfPickupChutes.Add(new SelfPickupChute() { ChuteID = foundChute.Id });
            }

            return selfPickupChutes;
        }


        /// <summary>
        /// Get available chutes for self-pickup list for given sorter, exclude exception chute list
        /// </summary>
        /// <param name="sorterId"></param>
        /// <returns></returns>
        private List<ChuteDataPair> GetSelfPickupChutes(long sorterId)
        {
            var ret = new List<ChuteDataPair>();
            List<ExceptionChute> exceptionChute = ExceptionChute.GetAllExceptionChutes(sorterId);

            var sorterChutes = ChuteInfo.GetChutesBySorterId(sorterId);

            if (sorterChutes != null)
            {
                ret.AddRange(sorterChutes.Where(c => !c.ChuteType.Equals(FedExConstants.ChuteTypes.Hwer) 
                                                  && !c.ChuteType.Equals(FedExConstants.ChuteTypes.Xray)
                                                  && !c.ChuteType.Equals(FedExConstants.ChuteTypes.ByPassXray)
                                                  && !exceptionChute.Exists(r => r.ChuteId == c.Id))
                                         .OrderBy(c => c.OriginalDest)
                                         .Select(c => new ChuteDataPair(){chuteId = c.Id, chuteValue = c.OriginalDest})
                                         .ToList());
            }

            //If sorter has active sort plan , must filter active sort plan rule chute.
            List<long> activeRuleChuteIds = GetActivePlanRuleChuteBySorterId(sorterId);
            ret = ret.Where(r => !activeRuleChuteIds.Contains(r.chuteId)).ToList();

            return ret;
        }

        /// <summary>
        /// Requests self-pickup lists.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sorterId">Sorter ID</param>
        /// <returns></returns>
        public SelfPickupListGetResponse SelfPickupListsGet(long userId, long sorterId)
        {
            SelfPickupListGetResponse ret;
            Guid storyHandle = Guid.Empty;

            try
            {
                ret = new SelfPickupListGetResponse()
                {
                    sorterChutes = new List<ChuteDataPair>(), 
                    selfPickupLists = new List<SelfPickupListData>(), 
                };

                // Check does sorter exist and it is inbound
                Sorter sorter = SorterInfo.GetSorter(sorterId);

                if (sorter == null || !sorter.Direction.Equals(FedExConstants.SorterDirection.Ib))
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.ImportPagesIds.InvalidSorter,
                        //Arguments = new string[] { sorterId.ToString() }
                    };
                    logger.ErrorMethod(String.Format("SelfPickupListsGet(): invalid sorter ID: {0}", sorterId));
                    return ret;
                }

                // After this point, please don't use return. Finaly block must be executed
                storyHandle = LockManager.Lock("SelfPickupListsGet");

                //Get sorter detail and get key MAXPICKUPCHUTECOUNT;              
                SorterDetail detail = SorterInfo.GetSorterDetailBySorterIdAndKey(sorterId, Entities.DBTables.Sorter.SorterDetail.SorterCapabilities.MAXPICKUPCHUTECOUNT);
                ret.maxPickupChuteCount = detail != null ? Int32.Parse(detail.ValueAttr) : 0;
 
                ret.sorterChutes = GetSelfPickupChutes(sorterId);

                var selfPickUpLists = SelfPickup.GetSelfPickUpListBySorter(sorterId);

                if (selfPickUpLists != null)
                {
                    foreach (var selfPickUpList in selfPickUpLists)
                    {
                        ret.selfPickupLists.Add(new SelfPickupListData()
                        {
                            id = selfPickUpList.Id,
                            name = selfPickUpList.Name,
                            description = selfPickUpList.Description,
                            assignedChutes = GetAssignedSelfPickupChutes(selfPickUpList.Id)
                        });
                    }
                }
                else
                {
                    logger.WarnMethod(String.Format("SelfPickupListsGet(): no self-pickup list for sorter ID: {0}", sorterId));
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SelfPickupListsGet() exception occured. ", e);
                throw;
            }
            finally
            {
                if (storyHandle != Guid.Empty)
                {
                    LockManager.Unlock(storyHandle);
                }
            }

            return ret;
        }

        /// <summary>
        /// Update self-pickup list (description and assigned chutes).
        /// </summary>
        /// <param name="request">Request for changing.</param>
        /// <returns></returns>
        public ResponseBase SelfPickupListUpdatePut(SelfPickupListUpdatePutRequest request)
        {
            ResponseBase ret;
            IStatelessSession theSession = null;
            Guid storyHandle = Guid.Empty;

            try
            {
                ret = new ResponseBase();

                if (request == null)
                {
                    ret.Error = new ResponseError() { Message = WebServices.Constants.ErrorMessages.RequestMissing };
                    logger.WarnMethod("SelfPickupListUpdatePut(): request is null");
                    return ret;
                }

                if (request.selfPickupLists == null || !request.selfPickupLists.Any())
                {
                    logger.WarnMethod("SelfPickupListUpdatePut(): SelfPickupLists is empty. Nothing is updated.");
                    return ret; // Nothing to save
                }

                if (request.selfPickupLists.Any(s => string.IsNullOrWhiteSpace(s.description)))
                {
                    ret.Error = new ResponseError() { Message = WebServices.Constants.SelfPickupPageIds.DescriptionMissing };
                    logger.WarnMethod("SelfPickupListUpdatePut(): request description is null or empty");
                    return ret;
                }

                #region Validate selecte chuete in exception chute or in active sort plan rule chutes

                //Check whether select chutes is in exception chutes.
                List<ExceptionChute> exceptionChutes = ExceptionChute.GetAllExceptionChutes(request.sorterId);
                List<long> chuteIds = new List<long>();
                long chuteId = 0;
                foreach (var item in request.selfPickupLists)
                {
                    if (item.assignedChutes == null || item.assignedChutes.Count == 0)
                    {
                        continue;
                    }
                    chuteIds.AddRange(item.assignedChutes.Select(c => c.chuteId));
                }

                if (UIHelper.CheckSelectChuteIsInExcepChute(chuteIds, exceptionChutes, out chuteId))
                {
                    Chute chute = ChuteInfo.GetChuteById(chuteId);
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.SelfPickupPageIds.ChuteAsException,
                        Arguments = new string[] { chute.OriginalDest }
                    };
                    logger.WarnMethod(String.Format("SelfPickupListUpdatePut(): chuteNo {0} as exception chute, sorter with ID: {1}", chute.OriginalDest, request.sorterId));
                    return ret;
                }

                //Check if sorter has active sort plan, the chute in request must not exist in active plan rule chute.
                List<SortPlan> activeSortPlans = SortingPlan.GetActiveSortPlanList();
                SortPlan activeSortPlan = activeSortPlans.Where(s => s.SorterId == request.sorterId).FirstOrDefault();
                if (activeSortPlan != null && chuteIds.Count > 0)
                {
                    List<SortingPlanRuleChutePair> ruleChutes = SortingPlan.GetActiveSortPlanRuleChuteList().Where(s => s.SortPlanId == activeSortPlan.Id).ToList();
                    List<long> ruleChuteIds = ruleChutes.Select(r => r.ChuteId).ToList();
                    foreach (var id in chuteIds)
                    {
                        if (ruleChuteIds.Contains(id))
                        {
                            Chute chute = ChuteInfo.GetChuteById(id);
                            ret.Error = new ResponseError()
                            {
                                Message = WebServices.Constants.SelfPickupPageIds.ChuteAsRuleChute,
                                Arguments = new string[] { chute.OriginalDest }
                            };
                            logger.ErrorMethod(String.Format("SelfPickupListUpdatePut(): chuteNo {0} as active sort plan rule chute", chute.OriginalDest));
                            return ret;
                        }
                    }
                }

                #endregion

                // After this point, please don't use return. Finaly block must be executed
                storyHandle = LockManager.Lock("SelfPickupListsGet");

                if (ret.Error == null)
                {
                    List<OperationsLogEntry> operationLogs = new List<OperationsLogEntry>();
                    Sorter sorter = SorterInfo.GetSorter(request.sorterId);
                    string oldValue = null;
                    string newValue = null;
                    // Name of the transaction for statistics only
                    theSession = SessionPool.GetStatelessSession("SelfPickupListUpdatePut");
                    bool retry;

                    do
                    {
                        theSession.BeginTransaction();
                        try
                        {
                            retry = false;

                            // Update list of selfpickup lists
                            foreach (var item in request.selfPickupLists)
                            {
                                ResponseError error;

                                var assignedChutes = ConvertSelfPickupChutes(item.assignedChutes, request.sorterId, out error);
                                if (error != null)
                                {
                                    ret.Error = error;
                                    theSession.Transaction.Rollback(); // Discard changes

                                    break; // Stop with operation
                                }

                                var selfPickupList = SelfPickup.GetSelfPickUpListBySelfPickUpListId(item.selfPickupListId, theSession);
                                if (selfPickupList == null)
                                {
                                    ret.Error = new ResponseError()
                                    {
                                        Message = WebServices.Constants.ImportPagesIds.InvalidSelfPickupList,
                                        //Arguments = new string[] { Convert.ToString(item.selfPickupListId) }
                                    };
                                    logger.WarnMethod(String.Format("SelfPickupListUpdatePut(): Invalid self-pickup list with ID: {0}", item.selfPickupListId));
                                    theSession.Transaction.Rollback(); // Discard changes

                                    break; // Stop with operation
                                }

                                selfPickupList.Description = item.description;
                                SelfPickup.SaveSelfPickupSettingsChute(selfPickupList, assignedChutes, theSession);

                                #region Construct operation log

                                if (item.description != item.oldData.description)
                                {
                                    OperationsLogEntry logEntry = new OperationsLogEntry()
                                    {
                                        UserId = request.userId,
                                        UserName = GetUsername(request.userId),
                                        Created = DateTime.UtcNow,
                                        OperatorAction = FedExConstants.OperatorActions.ChangeSelfPickupDescription,
                                        IdsAction = WebServices.Constants.OperatorActionsIds.ChangeSelfPickupDescription,
                                        NewValue = item.description,
                                        OldValue = item.oldData.description,
                                        P1 = sorter.EqId,
                                        P2 = item.pickupName,
                                        P3 = item.oldData.description,
                                        P4 = item.description
                                    };
                                    operationLogs.Add(logEntry);
                                }

                                if (UIHelper.CheckAssignChuteHasChange(item.oldData.assignedChutes, item.assignedChutes, out oldValue, out newValue))
                                {
                                    OperationsLogEntry logEntry = new OperationsLogEntry()
                                    {
                                        UserId = request.userId,
                                        UserName = GetUsername(request.userId),
                                        Created = DateTime.UtcNow,
                                        OperatorAction = FedExConstants.OperatorActions.ChangeSelfPickupChute,
                                        IdsAction = WebServices.Constants.OperatorActionsIds.ChangeSelfPickupChute,
                                        NewValue = newValue,
                                        OldValue = oldValue,
                                        P1 = sorter.EqId,
                                        P2 = item.pickupName,
                                        P3 = oldValue,
                                        P4 = newValue
                                    };
                                    operationLogs.Add(logEntry);
                                }

                                #endregion
                            }

                            if (ret.Error == null)
                            {
                                OperationsLog.AddEntries(operationLogs, theSession);

                                theSession.Transaction.Commit();
                            }
                        }
                        catch (Exception e)
                        {
                            // HandleError will do a rollback and even a reconnect if necessary.
                            retry = SessionPool.HandleError(e, ref theSession);
                            if (!retry)
                            {
                                throw;
                            }
                        }
                    }
                    while (retry);
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SelfPickupListUpdatePut() exception occured. ", e);
                throw;
            }
            finally
            {
                if (storyHandle != Guid.Empty)
                {
                    LockManager.Unlock(storyHandle);
                }
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }

            return ret;
        }


        /// <summary>
        /// Requests selected self-pickup list.
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="selfPickupListId">Requested self pickup List ID</param>
        /// <returns></returns>
        public SelfPickupGetResponse SelfPickupGet(long userId, long selfPickupListId)
        {
            SelfPickupGetResponse ret;

            try
            {
                ret = new SelfPickupGetResponse();

                // Check does exist requested self-pickup list
                if (SelfPickup.GetSelfPickUpListBySelfPickUpListId(selfPickupListId) == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.ImportPagesIds.InvalidSelfPickupList,
                        //Arguments = new string[] { Convert.ToString(selfPickupListId) }
                    };
                    logger.WarnMethod(String.Format("SelfPickupGet(): Invalid self-pickup list with ID: {0}", selfPickupListId));
                }

                var selfPickupParcels = SelfPickup.GetSelfPickupParcelByPickUpList(selfPickupListId);

                if (selfPickupParcels != null)
                {
                    long seq = 0;
                    foreach (var item in selfPickupParcels)
                    {
                        ret.selfPickupParcels.Add(new SelfPickupData()
                        {
                            sequenceNo = ++seq,
                            barcode = item.Barcode,
                            startTime = item.StartTime,
                            endTime = item.EndTime
                        });
                    }
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SelfPickupGet() exception occured. ", e);
                throw;
            }

            return ret;
        }

        /// <summary>
        /// Import self-pickup list
        /// <param name="request">Requested data for saving</param>
        /// </summary>
        /// <returns></returns>
        public SelfPickupImportPutResponse SelfPickupImportPut(SelfPickupImportPutRequest request)
        {
            IStatelessSession theSession = null;
            SelfPickupImportPutResponse ret = new SelfPickupImportPutResponse();

            try
            {
                bool import = true;

                // Check does exist requested self-pickup list
                var selfPickupList = SelfPickup.GetSelfPickUpListBySelfPickUpListId(request.selfPickupListId);
                if (selfPickupList == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.ImportPagesIds.InvalidSelfPickupList,
                    };
                    logger.WarnMethod(String.Format("SelfPickupImportPut(): Invalid self-pickup list with ID: {0}", request.selfPickupListId));
                    return ret;
                }

                var activeList = SelfPickup.GetSelfPickupParcelByPickUpList(request.selfPickupListId);           
                // Check is it needed to force overwrite active list
                if (!request.enforceOverwrite && activeList != null)
                {
                    //If all barcode are expired, it  doesn't belong to force import. If partial barcode are expired, it beong to force import
                    int allCount = activeList.Count;
                    //Filter expired time
                    activeList = activeList.Where(a => a.EndTime < DateTime.UtcNow).ToList();
                    if ( activeList.Count < allCount)
                    {
                        import = false;
                    }
                }

                //If import is false, just return directly to tell user to force notication.
                if (import == false)
                {
                    ret = new SelfPickupImportPutResponse() { needsUserIteraction = true };
                    return ret;
                }

                var importedRecords = UIHelper.ImportBarcodesAndTimesFromCsv(request.csvLines,
                                                                                FedExConstants.SelfPickupImportFileHeader,
                                                                                request.timeOffset,
                                                                                MaxSelfPickupImportRecord); 

                // Copy data for response
                ret.Error = importedRecords.error;
                ret.succesfullyImportedRowsNumber = importedRecords.succesfullyImportedRowsNumber;
                ret.duplicatedRowsNumber = importedRecords.duplicatedRowsNumber;
                ret.errorneousRowsNumber = importedRecords.errorneousRowsNumber;
                foreach (var importedRecord in importedRecords.records.Where(r => r.toReturn))
                {
                    ret.list.Add(new SelfPickupImportData()
                    {
                        line = importedRecord.line,
                        barcode = importedRecord.barcodeInput,
                        startTime = importedRecord.startTimeInput,
                        endTime = importedRecord.endTimeInput,
                        errors = importedRecord.errors
                    });
                }

                //If importedRecord has errors, just return directly.
                if (ret.Error != null)
                {
                    return ret;
                }

                List<SelfPickupParcel> selfPickupParcels = new List<SelfPickupParcel>();
                // Extract only valid records for saving in database and convert them
                foreach (var item in importedRecords.records.Where(r => r.errors.Count == 0))
                {
                    selfPickupParcels.Add(new SelfPickupParcel()
                    {
                        SelfPickupListID = request.selfPickupListId,
                        Barcode = item.barcodeOutput,
                        StartTime = item.startTimeOutput,
                        EndTime = item.endTimeOutput,
                    });
                }
                selfPickupList.Description = request.csvLines[0];

                string userName = GetUsername(request.userId);

                List<SystemLogEntry> systemLogs = null;
                if (activeList != null && activeList.Any())
                {
                    systemLogs = new List<SystemLogEntry>();
                    systemLogs.Add(new SystemLogEntry()
                    {
                        IdsLogAction = FedExConstants.SystemLogAction.InterceptListCanceled,
                        SorterOrProcess = FedExConstants.SystemLogProcess.UserEvent,
                        DeletedTag = FedExConstants.DeletedTag.NotDeleted
                    });
                }

                // Name of the transaction for statistics only
                theSession = SessionPool.GetStatelessSession("SelfPickupImportPut");
                bool retry = false;
                bool flag = false;
                do
                {
                    theSession.BeginTransaction();
                    try
                    {
                        //Save selfpickup setting
                        flag = SelfPickup.SaveSelfPickupSettingsParcel(selfPickupList, selfPickupParcels, theSession);
                        if (flag == false)
                        {
                            theSession.Transaction.Rollback();
                            ret.Error = new ResponseError()
                            {
                                Message = WebServices.Constants.ErrorMessages.RequestInvalid,
                            };
                            return ret;
                        }

                        //Add operation log
                        OperationsLog.Add(request.userId,
                                            userName,
                                            FedExConstants.OperatorActions.SelfPickImport,
                                            WebServices.Constants.OperatorActionsIds.SelfPickImport,
                                            new[] { request.fileName }, null, null, theSession);

                        //Add system log
                        if (systemLogs != null)
                        {
                            SystemLog.AddEntries(systemLogs, theSession);
                        }
 
                        theSession.Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(e, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                }
                while (retry);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SelfPickupImportPut() exception occured. ", e);
                throw;
            }
            finally
            {
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }

            return ret;
        }

        /// <summary>
        /// Delete requested pickup-list
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="selfPickupListId">Requested self pickup List ID</param>
        /// <returns></returns>
        public ResponseBase SelfPickupDeleteGet(long userId, long selfPickupListId)
        {
            ResponseBase ret = new ResponseBase();

            if (SelfPickup.GetSelfPickUpListBySelfPickUpListId(selfPickupListId) == null)
            {
                ret.Error = new ResponseError()
                {
                    Message = WebServices.Constants.ImportPagesIds.InvalidSelfPickupList,
                    //Arguments = new string[] { Convert.ToString(selfPickupListId) }
                };
                logger.WarnMethod(String.Format("SelfPickupDeleteGet(): Invalid self-pickup list with ID: {0}", selfPickupListId));
            }
            else
            {
                SelfPickup.DeleteSelfPickupByListID(selfPickupListId);
            }

            return ret;
        }


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
        public ParcelReportGetResponse SelfPickupReportGet(long userId,
                                                           long sorterId,
                                                           long selfPickupListId,
                                                           DateTime startTime,
                                                           DateTime endTime,
                                                           long maxRecords,
                                                           long startId)
        {
            var ret = new ParcelReportGetResponse()
            {
                parcels = new List<ParcelData>(),
                Error = UIHelper.ValidateStartEndTimes(startTime, endTime)
            };

            if (ret.Error != null)
            {
                return ret;
            }

            try
            {
                var tmpParcels = ParcelInfo.GetSelfPickupReport(sorterId, selfPickupListId, startTime, endTime, maxRecords, startId);


                int seq = 0;

                foreach (var parcel in tmpParcels.OrderBy(p => p.Id))
                {
                    ret.parcels.Add(new ParcelData()
                    {
                        SequenceNo = ++seq,
                        Pic = parcel.Pic,
                        ILoc = parcel.ILoc,
                        EqId = parcel.EqId,
                        SortName = parcel.SortName,
                        SorterId = parcel.SorterId,
                        HostPic = parcel.HostPic,
                        SortingPlanSessionId = parcel.SortingPlanSessionId,
                        Barcode_1 = parcel.Barcode_1,
                        Barcode_2 = parcel.Barcode_2,
                        Barcode_3 = parcel.Barcode_3,
                        Barcode_1_Sym = parcel.Barcode_1_Sym,
                        Barcode_2_Sym = parcel.Barcode_2_Sym,
                        Barcode_3_Sym = parcel.Barcode_3_Sym,
                        LeadingBarcode = parcel.LeadingBarcode,
                        Awb = parcel.Awb,
                        Mps = parcel.Mps,
                        SelectionCode = parcel.SelectionCode,
                        HandlingCode = parcel.HandlingCode,
                        CustomsStatus = parcel.CustomsStatus,
                        ClrSchema = parcel.ClrSchema,
                        Diplomatic = parcel.Diplomatic,
                        Ursa = parcel.Ursa,
                        IRoadNumber = parcel.IRoadNumber,
                        FlightInfo = parcel.FlightInfo,
                        Length = parcel.Length,
                        Width = parcel.Width,
                        Height = parcel.Height,
                        LwhUnit = parcel.LwhUnit,
                        Weight = parcel.Weight,
                        WeightUnit = parcel.WeightUnit,
                        CarrierNo = parcel.CarrierNo,
                        OperIntercept = parcel.OperIntercept,
                        OpinError = parcel.OpinError,
                        Dest = parcel.Dest,
                        Alt_Dest_1 = parcel.Alt_Dest_1,
                        Alt_Dest_2 = parcel.Alt_Dest_2,
                        Alt_Dest_3 = parcel.Alt_Dest_3,
                        Alt_Dest_4 = parcel.Alt_Dest_4,
                        Recirculation = parcel.Recirculation,
                        DivDest = parcel.DivDest,
                        DivC = parcel.DivC,
                        DivC_Alt1 = parcel.DivC_Alt1,
                        DivC_Alt2 = parcel.DivC_Alt2,
                        DivC_Alt3 = parcel.DivC_Alt3,
                        DivC_Alt4 = parcel.DivC_Alt4,
                        FinallySorted = parcel.FinallySorted,
                        FinalChuteId = parcel.FinalChuteId,
                        SortReason = parcel.SortReason,
                        SortReasonId = parcel.SortReasonId,
                        Reported = parcel.Reported,
                        Deleted = parcel.Deleted
                    });
                }

            }
            catch (Exception e)
            {
                logger.ErrorMethod("SelfPickupReportGet() exception occured. ", e);
                throw;
            }

            return ret;
        }

        #endregion

        #region Exception chutes related functionality

        /// <summary>
        /// Returns all available chutes which can be exception chutes for given sorter
        /// </summary>
        /// <param name="sorterId"></param>
        /// <returns></returns>
        private List<ChuteDataPair> GetAvailableExceptionChutes(long sorterId)
        {
            var ret = new List<ChuteDataPair>();

            var sorterChutes = ChuteInfo.GetChutesBySorterId(sorterId);

            if (sorterChutes != null)
            {
                ret.AddRange(sorterChutes.Where(c => c.ChuteType!=FedExConstants.ChuteTypes.Hwer && c.Exceptional==1)
                                         .OrderBy(c => c.OriginalDest)
                                         .Select(c => new ChuteDataPair() { chuteId = c.Id, chuteValue = c.OriginalDest })
                                         .ToList());
            }

            //If sorter has active sort plan , must filter active sort plan rule chute.
            List<long> activeRuleChuteIds = GetActivePlanRuleChuteBySorterId(sorterId);
            ret = ret.Where(r => !activeRuleChuteIds.Contains(r.chuteId)).ToList();

            //List<SortPlan> activeSortPlans = SortingPlan.GetActiveSortPlanList();
            //SortPlan activeSortPlan = activeSortPlans.Where(s => s.SorterId == sorterId).FirstOrDefault();
            //if (activeSortPlan != null)
            //{
            //    List<SortingPlanRuleChutePair> ruleChutes = SortingPlan.GetActiveSortPlanRuleChuteList().Where(s => s.SortPlanId == activeSortPlan.Id).ToList();
            //    List<long> ruleChuteIds = ruleChutes.Select(r => r.ChuteId).ToList();

            //    ret = ret.Where(r => !ruleChuteIds.Contains(r.chuteId)).ToList();
            //}

            //If sorter has self pickup configuration, must filter self pickup chute.
            List<long> selfPickupChuteIds = GetSelfPickupChuteIdBySorterId(sorterId);
            ret = ret.Where(r => !selfPickupChuteIds.Contains(r.chuteId)).ToList();

            return ret;
        }

        /// <summary>
        /// Returns list of exception chutes configurations for given sorter
        /// </summary>
        /// <param name="sorterId"></param>
        /// <returns></returns>
        private List<ExceptionChuteData> GetExceptionChuteConfigs(long sorterId)
        {
            List<ExceptionChuteData> ret = new List<ExceptionChuteData>();

            var sorterDetailInfo = SorterInfo.GetAllSorterDetailInfo(sorterId);

            List<SorterDetail> sorterDetails = sorterDetailInfo.sorterDetails.OrderBy(d => d.Id).Where(d => d.KeyAttr.ToUpper().StartsWith("CHUTE_TYPE_")).ToList();

            long count = 0;

            foreach (var sorterDetail in sorterDetails)
            {
                string exceptionType = sorterDetail.KeyAttr.ToUpper().Substring("CHUTE_TYPE_".Length);
                string ids;

                if (WebServices.Constants.ExceptionTypesIds.TryGetValue(exceptionType, out ids))
                {
                    ExceptionChute excChute = null;
                    if (sorterDetail.ValueAttr == "1")
                        excChute = ExceptionChute.GetExceptionChute(sorterDetailInfo.Id, exceptionType).FirstOrDefault();

                    var excetpionChuteData = new ExceptionChuteData()
                    {
                        exceptionType = exceptionType,
                        chuteId = excChute != null ? excChute.ChuteId : 0,
                        chuteName = excChute != null ? ChuteInfo.GetChuteById(excChute.ChuteId).OriginalDest : "",
                        exceptionIds = ids,
                        isApplicableSettings = sorterDetail.ValueAttr.Trim().Equals("1"),
                        seqNo = ++count
                    };

                    ret.Add(excetpionChuteData);
                }
                else
                {
                    logger.ErrorMethod(String.Format("GetExceptionChuteConfigs(): Exception chute type '{0}' not found", exceptionType));
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns all exception chutes for requested sorter
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sorterId">ID of sorter name defined in FX_SORTER. </param>
        /// <returns></returns>
        public ExceptionChuteGetResponse ExceptionChutesGet(long userId, long sorterId)
        {
            ExceptionChuteGetResponse ret = new ExceptionChuteGetResponse();
            Guid storyHandle = Guid.Empty;

            try
            {
                if (SorterInfo.GetSorter(sorterId) == null)
                {
                    ret.Error = new ResponseError() { Message = WebServices.Constants.ImportPagesIds.InvalidSorter };
                    logger.WarnMethod(String.Format("ExceptionChutesGet(): Invalid sorter ID: {0}", sorterId));
                    return ret;
                }

                // After this point, please don't use return. Finaly block must be executed
                storyHandle = LockManager.Lock("ExceptionChutesGet");

                ret.availableChutes = GetAvailableExceptionChutes(sorterId);
                ret.exceptionChutes = GetExceptionChuteConfigs(sorterId);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("ExceptionChutesGet() exception occured. ", e);
                throw;
            }
            finally
            {
                if (storyHandle != Guid.Empty)
                {
                    LockManager.Unlock(storyHandle);
                }
            }

            return ret;
        }

        /// <summary>
        /// Update exception chutes for one sorter. Only chnaged records are sent from SVS.
        /// </summary>
        /// <param name="request">Requested changes for updating.</param>
        /// <returns></returns>
        public ResponseBase ExceptionChutesPut(ExceptionChutePutRequest request)
        {
            ResponseBase ret;
            IStatelessSession theSession = null;
            Guid storyHandle = Guid.Empty;

            try
            {
                ret = new ResponseBase();

                #region Validate

                if (request == null)
                {
                    ret.Error = new ResponseError() { Message = WebServices.Constants.ErrorMessages.RequestMissing };
                    logger.ErrorMethod("ExceptionChutesPut(): request is null");
                    return ret;
                }

                var sorter = SorterInfo.GetSorter(request.sorterId);
                if (sorter == null)
                {
                    ret.Error = new ResponseError()
                    {
                        Message = WebServices.Constants.ImportPagesIds.InvalidSorter,
                        //Arguments = new string[] { request.sorterId.ToString() }
                    };
                    logger.ErrorMethod(String.Format("ExceptionChutesPut(): Invalid sorter ID: {0}", request.sorterId));
                    return ret;
                }

                //Check if sorter has active sort plan, the chute in request must not exist in active plan rule chute.
                List<SortPlan> activeSortPlans = SortingPlan.GetActiveSortPlanList();
                SortPlan activeSortPlan = activeSortPlans.Where(s => s.SorterId == request.sorterId).FirstOrDefault();
                if (activeSortPlan != null)
                {
                    List<SortingPlanRuleChutePair> ruleChutes = SortingPlan.GetActiveSortPlanRuleChuteList().Where(s => s.SortPlanId == activeSortPlan.Id).ToList();
                    List<long> ruleChuteIds = ruleChutes.Select(r => r.ChuteId).ToList();
                    foreach (var item in request.exceptionChutes)
                    {
                        if (ruleChuteIds.Contains(item.newChuteId))
                        {
                            ret.Error = new ResponseError()
                            {
                                Message = WebServices.Constants.ExceptionChutesIds.ChuteAsRuleChute,
                                Arguments = new string[] { item.newChuteValue }
                            };
                            logger.ErrorMethod(String.Format("ExceptionChutesPut(): chuteNo {0} as active sort plan rule chute", item.newChuteValue));
                            return ret;
                        }
                    }
                }

                //Check If sorter has self-pickup, the chute in request must not exist in self-pickup chutes.
                List<long> pickupChuteIds = GetSelfPickupChuteIdBySorterId(request.sorterId);
                foreach (long pickupChuteId in pickupChuteIds)
                {
                    ExceptionChuteUpdateData tempChuteData = request.exceptionChutes.Where(ex => ex.newChuteId == pickupChuteId).FirstOrDefault();
                    if (tempChuteData != null)
                    {
                        ret.Error = new ResponseError()
                        {
                            Message = WebServices.Constants.ExceptionChutesIds.ChuteAsSelfPickup,
                            Arguments = new string[] { tempChuteData.newChuteValue }
                        };
                        logger.ErrorMethod(String.Format("ExceptionChutesPut(): chuteNo {0} as self pickup chute", tempChuteData.newChuteValue));
                        return ret;
                    }
                }

                #endregion

                string sorterDirection = ExceptionChuteDirection.InOutbound;
                if (sorter.Direction.Equals(FedExConstants.SorterDirection.Ib))
                {
                    sorterDirection = ExceptionChuteDirection.Inbound;
                }
                else if (sorter.Direction.Equals(FedExConstants.SorterDirection.Ob))
                {
                    sorterDirection = ExceptionChuteDirection.Outbound;
                }

                List<ExceptionChuteData> applicableExceptions = GetExceptionChuteConfigs(request.sorterId).Where(c => c.isApplicableSettings).ToList();

                // After this point, please don't use return. Finaly block must be executed
                storyHandle = LockManager.Lock("ExceptionChutesPut");

                // Name of the transaction for statistics only
                theSession = SessionPool.GetStatelessSession("ExceptionChutesPut");

                bool retry;

                do
                {
                    theSession.BeginTransaction();

                    try
                    {
                        retry = false;

                        var existingExceptionChutes = ExceptionChute.GetAllExceptionChutes(request.sorterId, theSession);
                        //var availableChutes = GetAvailableExceptionChutes(request.sorterId);

                        var excChutesForDeleting = new List<ExceptionChute>();
                        var excChutesForInserting = new List<ExceptionChute>();

                        // Prepare exception chutes for deleting and inserting 
                        foreach (var exceptionChuteUpdateData in request.exceptionChutes)
                        {
                            if (!applicableExceptions.Any(e => e.exceptionType.Equals(exceptionChuteUpdateData.exceptionType)))
                            {
                                ret.Error = new ResponseError()
                                {
                                    Message = WebServices.Constants.ExceptionChutesIds.InvalidExceptionType,
                                    Arguments = new string[] { exceptionChuteUpdateData.exceptionType }
                                };
                                logger.ErrorMethod(String.Format("ExceptionChutesPut(): Invalid exception type: '{0}'", exceptionChuteUpdateData.exceptionType));
                                theSession.Transaction.Rollback();
                                break;
                            }

                            if (existingExceptionChutes != null)
                            {
                                // add to list for deleting
                                excChutesForDeleting.AddRange(existingExceptionChutes.Where(e => e.ExceptionType.Equals(exceptionChuteUpdateData.exceptionType)).
                                                                                      ToList());
                            }

                            if (exceptionChuteUpdateData.newChuteId > 0)
                            {
                                //if (availableChutes.Any(c => c.chuteId == exceptionChuteUpdateData.newChuteId))
                                //{
                                excChutesForInserting.Add(new ExceptionChute()
                                {
                                    ChuteId = exceptionChuteUpdateData.newChuteId,
                                    SorterId = request.sorterId,
                                    UserId = request.userId,
                                    ExceptionType = exceptionChuteUpdateData.exceptionType,
                                    Direction = sorterDirection,
                                }); // add to list for inserting
                                //}
                                //else
                                //{
                                //    ret.Error = new ResponseError()
                                //    {
                                //        Message = WebServices.Constants.ExceptionChutesIds.InvalidChute,
                                //        Arguments = new string[] { exceptionChuteUpdateData.newChuteValue.ToString() }
                                //    };
                                //    logger.ErrorMethod(String.Format("ExceptionChutesPut(): Invalid chute with ID: '{0}'", exceptionChuteUpdateData.newChuteId));
                                //    theSession.Transaction.Rollback();
                                //    break;
                                //}
                            }

                            if (ret.Error == null)
                            {
                                // Add to OperationsLog
                                OperationsLog.Add(request.userId,
                                                  GetUsername(request.userId),
                                                  FedExConstants.OperatorActions.ChangeExceptionChute,
                                                  WebServices.Constants.OperatorActionsIds.ChangeExceptionChute,
                                                  new[]
                                                  {
                                                      sorter.IdSorterName,
                                                      exceptionChuteUpdateData.exceptionType
                                                  },
                                                  exceptionChuteUpdateData.oldChuteValue,
                                                  exceptionChuteUpdateData.newChuteValue,
                                                  theSession);
                            }
                        }

                        if (ret.Error == null)
                        {
                            if (excChutesForDeleting.Any())
                            {
                                ExceptionChute.DeleteExceptionChute(excChutesForDeleting, theSession);
                            }

                            if (excChutesForInserting.Any())
                            {
                                ExceptionChute.SetExceptionChute(excChutesForInserting, theSession);
                            }

                            theSession.Transaction.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        // HandleError will do a rollback and even a reconnect if necessary.
                        retry = SessionPool.HandleError(e, ref theSession);
                        if (!retry)
                        {
                            throw;
                        }
                    }
                }
                while (retry);
            }
            catch (Exception e)
            {
                logger.ErrorMethod("ExceptionChutesPut() exception occured. ", e);
                throw;
            }
            finally
            {
                if (storyHandle != Guid.Empty)
                {
                    LockManager.Unlock(storyHandle);
                }
                if (theSession != null)
                {
                    SessionPool.ReleaseSession(theSession);
                }
            }


            return ret;
        }

        #endregion

        #region Sorter info related functionality

        /// <summary>
        /// Returns info about sorters
        /// </summary>
        /// <param name="userId">ID of user requesting this service.</param>
        /// <param name="sorterType">Sorter type defined in FedExConstants.SorterDirection. if null or empty sttring return all sorters</param>
        /// <returns></returns>
        public SorterInfoGetResponse SorterInfoGet(long userId, string sorterType)
        {
            SorterInfoGetResponse ret = new SorterInfoGetResponse();

            try
            {
                List<Sorter>sorters = SorterInfo.GetAllSorters();

                if (!String.IsNullOrWhiteSpace(sorterType))
                {
                    sorters = sorters.Where(s => s.Direction.Equals(sorterType)).ToList();
                }

                foreach (var sorter in sorters)
                {
                    var sorterDetails = SorterInfo.GetAllSorterDetailInfo(sorter.Id);

                    ret.sortersInfo.Add(new SorterData()
                    {
                        sorterId = sorter.Id,
                        idsSortname = sorter.IdSorterName,
                        idsDescription = sorter.IdDescription,
                        logicalType = sorter.LogicalType,
                        role = sorter.Role,
                        direction = sorter.Direction,
                        details = sorterDetails.sorterDetails.ToDictionary(sorterDetail => sorterDetail.KeyAttr, sorterDetail => sorterDetail.ValueAttr),
                    });
                }
            }
            catch (Exception e)
            {
                logger.ErrorMethod("SorterInfoGet() exception occured. ", e);
                throw;
            }

            return ret;
        }

        /// <summary>
        /// Returns sorter chute info
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sorterId"></param>
        /// <param name="ruleType"></param>
        /// <param name="planId"></param>
        /// <returns></returns>
        public SorterChuteInfoGetResponse SorterChuteInfoGet(long userId, long sorterId, string ruleType, long planId)
        {
            SorterChuteInfoGetResponse ret = new SorterChuteInfoGetResponse();
            try
            {
                #region Validate

                Sorter sorter = SorterInfo.GetSorter(sorterId);
                if (sorter == null)
                {
                    ret.Error = new ResponseError() {
                        Message = WebServices.Constants.SortingplanPageIds.InvalidSorterId,
                        Arguments = new string[] { sorterId.ToString() }
                    };
                    logger.ErrorMethod(string.Format("SorterChuteInfoGet(): sorter is null by sorterId {0}", sorterId));
                    return ret;
                }

                #endregion

                ret.sorterChuteData = new List<SorterChuteData>();
                List<Chute> targetChutes = ChuteInfo.GetChutesBySorterId(sorterId);
                
                List<ExceptionChute> exceptionChute = ExceptionChute.GetAllExceptionChutes(sorterId);
                List<SortingPlanRuleChute> customRuleChutes = new List<SortingPlanRuleChute>();

                #region Filter chutes by sorter and rule type
                //Filter exception chutes and over flow chute 
                targetChutes = targetChutes.Where(c => !exceptionChute.Exists(r => r.ChuteId == c.Id) && c.ChuteType != FedExConstants.ChuteTypes.Hwer).ToList();

                switch (ruleType)
                {
                    case FedExConstants.SortingPlanRuleType.SelectCode://selection code rule
                        {
                            //On the inbound pre-sorter and outbound pre-sorters,
                            //packages with valid selection codes are not allowed be sorted to X-ray or by pass X-ray side chutes
                            if (sorter.Role == FedExConstants.SorterRole.PreSorter)
                            {
                                targetChutes = targetChutes.Where(c => c.ChuteType != FedExConstants.ChuteTypes.Xray && c.OriginalDest != "12518").ToList();
                            }
                            break;
                        }
                    case FedExConstants.SortingPlanRuleType.SpecialCode://special parcel code rule
                        {
                            //Filter X-ray chute
                            targetChutes = targetChutes.Where(c => c.ChuteType != FedExConstants.ChuteTypes.Xray).ToList();
                            break;
                        }
                    case FedExConstants.SortingPlanRuleType.HandCode://handling code rule
                        {
                            //this is wrong: Parcels found on outbound pre–sorter with selection and handling codes can’t be directed to X-ray chutes. p112 
                            //the right is: Parcels found on  pre–sorter with selection and handling codes can’t be directed to X-ray chutes or by pass X-ray side chutes FEDSH-334                          
                            //if (sorter.Direction == FedExConstants.SorterDirection.Ob && sorter.Role == FedExConstants.SorterRole.PreSorter)
                            if (sorter.Role == FedExConstants.SorterRole.PreSorter)
                            {
                                //Filter X-ray chute
                                targetChutes = targetChutes.Where(c => c.ChuteType != FedExConstants.ChuteTypes.Xray && c.OriginalDest != "12518").ToList();
                            }
                            break;
                        }
                    case FedExConstants.SortingPlanRuleType.DestFlightcode:
                    case FedExConstants.SortingPlanRuleType.DestIRoads:
                        {
                            //"X-Ray chutes are not listed at destination sort rule on outbound pre-sorter              
                            if (sorter.Direction == FedExConstants.SorterDirection.Ob && sorter.Role == FedExConstants.SorterRole.PreSorter)
                            {
                                targetChutes = targetChutes.Where(c => c.ChuteType != FedExConstants.ChuteTypes.Xray).ToList();
                            }

                            //For IB presorter PS0005, "X-Ray" chutes SS005 and bypass chute 12518 are not listed 
                            if (sorter.EqId == FedExConstants.SorterEqId.IbPre)
                            {
                                targetChutes = targetChutes.Where(c => c.ChuteType != FedExConstants.ChuteTypes.Xray && c.OriginalDest != "12518").ToList();
                            }
                            break;
                        }
                    default:
                        break;
                }

                //For custom clearance status rule type process
                string customRuleType = UIHelper.GetOhterCustomClearType(ruleType);
                if (customRuleType != string.Empty)
                {
                    customRuleChutes = sortingPlan.GetSortingPlanRuleChutesByPlanIdRuleType(planId, customRuleType);
                    targetChutes = targetChutes.Where(c => !customRuleChutes.Exists(r => r.ChuteId == c.Id)).ToList();
                }

                //If sorter has self pickup configuration, must filter self pickup chute.
                List<long> selfPickupChuteIds = GetSelfPickupChuteIdBySorterId(sorterId);
                targetChutes = targetChutes.Where(r => !selfPickupChuteIds.Contains(r.Id)).ToList();

                #endregion

                foreach (Chute item in targetChutes)
                {
                    SorterChuteData tempData = new SorterChuteData()
                    {
                        chuteId = item.Id,
                        ChuteType = item.ChuteType,
                        chuteValue = item.OriginalDest,
                        Side = item.Side
                    };

                    ret.sorterChuteData.Add(tempData);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorMethod("SorterChuteInfoGet() exception occured. ", ex);
                throw;
            }

            return ret;
        }


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
        public SortReportArchiveGetResponse SortReportArchiveGet(long userId,
                                                           string barcode,
                                                           DateTime startTime,
                                                           DateTime endTime,
                                                           long maxRecords,
                                                           long startId)
        {
            var ret = new SortReportArchiveGetResponse()
            {
                entries = new List<SortReport>(),
                Error = UIHelper.ValidateStartEndTimes(startTime, endTime)
            };

            if (ret.Error != null)
            {
                return ret;
            }

            try
            {
                var tmpParcels = ParcelArchive.GetEntries(userId, barcode, startTime, endTime, maxRecords, startId);


                foreach (var parcel in tmpParcels.OrderBy(p => p.Id))
                {
                    ret.entries.Add(new SortReport()
                    {
						Created = parcel.Created,
						Updated = parcel.Updated,
                        Pic = parcel.Pic,
                        ILoc = parcel.ILoc,
                        EqId = parcel.EqId,
                        SortName = parcel.SortName,
                        SorterId = parcel.SorterId,
                        HostPic = parcel.HostPic,
                        SortingPlanSessionId = parcel.SortingPlanSessionId,
                        Barcode_1 = parcel.Barcode_1,
                        Barcode_2 = parcel.Barcode_2,
                        Barcode_3 = parcel.Barcode_3,
                        Barcode_1_Sym = parcel.Barcode_1_Sym,
                        Barcode_2_Sym = parcel.Barcode_2_Sym,
                        Barcode_3_Sym = parcel.Barcode_3_Sym,
                        LeadingBarcode = parcel.LeadingBarcode,
                        AWB = parcel.AWB,
                        MPS = parcel.MPS,
                        SelectionCode = parcel.SelectionCode,
                        HandlingCode = parcel.HandlingCode,
                        CustomsStatus = parcel.CustomsStatus,
                        ClrSchema = parcel.ClrSchema,
                        Diplomatic = parcel.Diplomatic,
                        Ursa = parcel.Ursa,
                        IRoadNumber = parcel.IRoadNumber,
                        FlightInfo = parcel.FlightInfo,
                        Length = parcel.Length,
                        Width = parcel.Width,
                        Height = parcel.Height,
                        LwhUnit = parcel.LwhUnit,
                        CarrierNo = parcel.CarrierNo,
                        OperIntercept = parcel.OperIntercept,
                        OpinError = parcel.OpinError,
                        Dest = parcel.Dest,
                        Alt_Dest_1 = parcel.Alt_Dest_1,
                        Alt_Dest_2 = parcel.Alt_Dest_2,
                        Alt_Dest_3 = parcel.Alt_Dest_3,
                        Alt_Dest_4 = parcel.Alt_Dest_4,
                        Recirculation = parcel.Recirculation,
                        DivDest = parcel.DivDest,
                        DivC = parcel.DivC,
                        DivC_Alt1 = parcel.DivC_Alt1,
                        DivC_Alt2 = parcel.DivC_Alt2,
                        DivC_Alt3 = parcel.DivC_Alt3,
                        DivC_Alt4 = parcel.DivC_Alt4,
                        FinallySorted = parcel.FinallySorted,
                        FinalChuteId = parcel.FinalChuteId,
                        SortReason = UIHelper.GetLogicExcepReasonIds(parcel.SortReason),
                        SortReasonId = parcel.SortReasonId,
                        Deleted = parcel.Deleted
                    });
                }

            }
            catch (Exception e)
            {
                logger.ErrorMethod("SortReportArchiveGet() exception occured. ", e);
                throw;
            }

            return ret;
        }
        #endregion SortReportArchive related functionality
    }
}
