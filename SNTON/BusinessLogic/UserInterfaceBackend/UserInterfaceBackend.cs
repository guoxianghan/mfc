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
using System.Linq;
using SNTON.BusinessLogic.UserInterfaceBackend;
using SNTON.Constants;
using SNTON.WebServices;
//using SNTON.WebServices.UserInterfaceBackend.Models;
using SNTON.WebServices.UserInterfaceBackend.Requests;
using SNTON.WebServices.UserInterfaceBackend.Responses;
using NHibernate;
using NHibernate.Util;
using VI.MFC.Logging;
using SNTON.Entities.DBTables.Message;
using System.Linq.Expressions;
using System.Text;
using SNTON.Entities.DBTables.Spools;
using SNTON.Components.Equipment;
using SNTON.WebServices.UserInterfaceBackend.Models.Equip;
using SNTON.WebServices.UserInterfaceBackend.Models;
using SNTON.WebServices.UserInterfaceBackend.Responses.Equip;
using SNTON.WebServices.UserInterfaceBackend.Responses.AGV;
using SNTON.WebServices.UserInterfaceBackend.Models.AGV;
using SNTON.Entities.DBTables.AGV;
using SNTON.WebServices.UserInterfaceBackend.Requests.AGV;
using SNTON.Entities.DBTables.Equipments;
using SNTON.WebServices.UserInterfaceBackend.Requests.SystemParameters;
using SNTON.WebServices.UserInterfaceBackend.Responses.MidStorage;
using SNTON.Entities.DBTables.RobotArmTask;
using SNTON.Entities.DBTables.MidStorage;
using static SNTON.Constants.SNTONConstants;
using SNTON.WebServices.UserInterfaceBackend.Responses.RobotArmTask;
using System.IO;
using SNTON.WebServices.UserInterfaceBackend.Responses.EquipWatch;
using SNTON.WebServices.UserInterfaceBackend.Responses.Product;

namespace SNTON.BusinessLogic
{

    /// <summary>
    /// Business logic interface for the user interface backend goes here.
    /// Basically, these methods will be called primarily by the RESTful service layer.
    /// </summary>
    public partial class BusinessLogic : IUserInterfaceBackend
    {
        #region Properites

        #endregion
        #region Helper methods
        #endregion
        public MessageResponse NewMessageGet(long msgLevel)
        {
            MessageResponse mess = new MessageResponse();
            var r = this.MessageInfoProvider.GetNewMessageByLevel((int)msgLevel, null);
            if (r != null)
                mess.data.Add(new WebServices.UserInterfaceBackend.Models.MessageDataUI() { Created = r.Created, Id = r.Id, Message = r.MsgContent, MsgLevel = r.MsgLevel, Source = r.Source, SeqNo = 1 });
            return mess;
        }

        public ResponseDataBase MessageSource()
        {
            ResponseDataBase res = new ResponseDataBase();
            res.data = this.MessageInfoProvider.GetMessageSource(null);
            return res;
        }
        public SystemParametersResponse SystemParameterQuery()
        {
            SystemParametersResponse spr = new SystemParametersResponse();
            var list = this.SystemParametersProvider.GetSystemParamrters(null);
            if (list != null)
                foreach (var item in list)
                {
                    var tmp = new WebServices.UserInterfaceBackend.Models.SystemParametersUI()
                    { Description = item.Description.Trim(), Id = item.Id, ParamName = item.ParameterName.Trim(), Value = item.ParameterValue.Trim(), ValueType = item.DisplayFormat.ToString().Trim() };

                    try
                    {
                        var dic = this.SystemParametersConfigurationProvider.GetAllSystemParametersConfigurationEntity(null);
                        //tmp.SelectValue = new Dictionary<string, string>();
                        StringBuilder sb = new StringBuilder();
                        if (dic != null)
                            dic.FindAll(x => x.SysParamId == item.Id)?.ForEach(x =>
                                {
                                    //tmp.SelectValue.Add(x.Value, x.DisplayValue);
                                    sb.Append(x.Value + "|" + x.DisplayValue + ";");
                                });
                        tmp.SelectValue = sb.ToString().Trim(';');
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                    spr.data.Add(tmp);
                }
            return spr;
        }

        public MessageResponse MessageSearch(MessageSearchRequest searchRequest)
        {
            MessageResponse mess = new MessageResponse();
            //Expression<Func<MessageEntity, object>> e = c => searchRequest.endTime.HasValue?:e.;

            var list = this.MessageInfoProvider.GetMessagesBySearch(searchRequest, null);
            if (list != null)
            {
                mess.CountNumber = list.Count;
                mess.pageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(list.Count / searchRequest.pageSize)));
                if (list.Count % searchRequest.pageSize > 0)
                    mess.pageCount = mess.pageCount + 1;
                list = list.Skip(searchRequest.pageSize * (searchRequest.pageNumber - 1)).Take(searchRequest.pageSize).ToList();
                foreach (var item in list)
                {
                    var m = new WebServices.UserInterfaceBackend.Models.MessageDataUI() { Created = item.Created, Id = item.Id, Message = item.MsgContent, MsgLevel = item.MsgLevel, Source = item.Source };
                    m.SeqNo = 0;
                    mess.data.Add(m);
                }
            }

            return mess;
        }

        public ResponseBase SystemParameterSave(SystemParametersRequest request)
        {
            ResponseBase res = new ResponseBase();
            this.SystemParametersProvider.SaveSystemParameters(request, null);
            return res;
        }

        public SpoolsResponse GetSpoolsByBarcode(string barcode)
        {
            SpoolsResponse obj = new SpoolsResponse();
            var en = this.SpoolsProvider.GetSpoolByBarcode(barcode, null);
            if (en != null)
            {
                var sp = new WebServices.UserInterfaceBackend.Models.SpoolDataUI()
                { Barcode = en.FdTagNo, EquipIdFrom = en.EquipIdFrom, EquipIdTo = en.EquipIdTo, ProductType = en.ProductType, SpoolId = en.Id };
                sp.EquipNameFrom = this.EquipConfigProvider.GetEquipConfigById(en.EquipIdFrom, null).EquipName;
                sp.EquipNameTo = this.EquipConfigProvider.GetEquipConfigById(en.EquipIdTo, null).EquipName;
                sp.MidStorageLocation = this.MidStorageProvider.GetMidStorageById(en.MidStorageId, null).Description;
                obj.data.Add(sp);
            }
            return obj;
        }

        public SpoolsResponse GetSpoolsByMidStorageId(long midStorageId)
        {
            SpoolsResponse obj = new SpoolsResponse();
            var en = this.SpoolsProvider.GetSpoolByMidStorageId((int)midStorageId, null);
            if (en != null)
            {
                var sp = new WebServices.UserInterfaceBackend.Models.SpoolDataUI()
                { Barcode = en.FdTagNo, EquipIdFrom = en.EquipIdFrom, EquipIdTo = en.EquipIdTo, ProductType = en.ProductType, SpoolId = en.Id };

                sp.EquipNameFrom = this.EquipConfigProvider.GetEquipConfigById(en.EquipIdFrom, null).EquipName;
                sp.EquipNameTo = this.EquipConfigProvider.GetEquipConfigById(en.EquipIdTo, null).EquipName;
                sp.MidStorageLocation = this.MidStorageProvider.GetMidStorageById(en.MidStorageId, null).Description;
                obj.data.Add(sp);
            }
            return obj;
        }

        public SpoolsResponse GetSpoolsByProudctType(string proudctType)
        {
            SpoolsResponse obj = new SpoolsResponse();
            List<SpoolsEntity> en = this.SpoolsProvider.GetSpoolsByProudctType(proudctType, null);
            if (en != null && en.Count != 0)
            {
                foreach (var item in en)
                {
                    var sp = new WebServices.UserInterfaceBackend.Models.SpoolDataUI()
                    { Barcode = item.FdTagNo, EquipIdFrom = item.EquipIdFrom, EquipIdTo = item.EquipIdTo, ProductType = item.ProductType, SpoolId = item.Id };
                    try
                    {
                        sp.EquipNameFrom = ((EquipConfig)this.EquipConfigProvider)._AllEquipConfigDic[item.EquipIdFrom].EquipName;
                        sp.EquipNameTo = ((EquipConfig)this.EquipConfigProvider)._AllEquipConfigDic[item.EquipIdTo].EquipName;
                        sp.MidStorageLocation = this.MidStorageProvider.GetMidStorageById(item.MidStorageId, null).Description;
                    }
                    catch (Exception e)
                    {
                        logger.ErrorMethod("Failed to get GetSpoolsByProudctType", e);
                    }
                    obj.data.Add(sp);
                }
            }
            return obj;
        }

        public EquipConfigResponse EquipConfigs(short plantno)
        {
            EquipConfigResponse obj = new EquipConfigResponse();
            var statulist = this.EquipStatusProvider.GetAllEquipStatusEntity(null);
            Dictionary<long, byte> dic = new Dictionary<long, byte>();
            foreach (var item in statulist)
                dic.Add(item.EquipId, item.Status);

            var list = this.EquipConfigProvider.GetEquipConfigByPlantNo((short)plantno, null);
            EquipProductionEntity ent = new EquipProductionEntity();
            var productlist = this.EquipProductionProvider.GetAllEquipProductionEntity();
            if (list != null)
                foreach (var item in list)
                {
                    var ui = new EquipConfigDataUI() { Description = item.Description, EquipName = item.EquipName, X = item.X, Id = item.Id, Y = item.Y };
                    if (dic.ContainsKey(item.Id))
                        ui.Status = dic[ui.Id];
                    ui.GroupID = item.GroupID;
                    ui.GroupName = productlist.FirstOrDefault(X => X.GroupId == item.GroupID)?.ProductType;
                    obj.data.Add(ui);
                }
            return obj;
        }

        public EquipConfigStatusResponse EquipConfigStatus(short plantno)
        {
            EquipConfigStatusResponse obj = new EquipConfigStatusResponse();
            var statulist = this.EquipStatusProvider.GetAllEquipStatusEntity(null);
            Dictionary<short, byte> dic = new Dictionary<short, byte>();
            foreach (var item in statulist)
                dic.Add(item.EquipId, item.Status);
            var list = this.EquipConfigProvider.GetEquipConfigByPlantNo(plantno, null);
            if (list != null)
                foreach (var item in list)
                {
                    var tmp = new EquipStatusDataUI() { EquipId = (short)item.Id };
                    if (dic.Keys.Contains(tmp.EquipId))
                        tmp.Status = dic[tmp.EquipId];
                    obj.data.Add(tmp);
                }
            return obj;
        }

        public EquipConfigInfoResponse EquipConfigInfo(long id)
        {
            EquipConfigInfoResponse obj = new EquipConfigInfoResponse();
            var entity = this.EquipConfigProvider.GetEquipConfigById(id, null);
            if (entity != null)
            {
                var ui = new EquipInfoDataUI() { ControllerName = entity.ControllerName, Description = entity.Description, EquipControllerId = entity.EquipControllerId, EquipName = entity.EquipName, X = entity.X, Id = entity.Id, PlantNo = entity.PlantNo, Y = entity.Y };
                var eq = this.EquipStatusProvider.GetEquipStatusEntityByEquipID(entity.Id, null);
                if (eq != null)
                    ui.Status = eq.Status;// "尚未获取状态";
                obj.data.Add(ui);
            }
            return obj;
        }

        public EquipProductionResponse EquipProductionsByGroupID(long groupid)
        {
            EquipProductionResponse obj = new EquipProductionResponse();
            var tmp = this.EquipProductionProvider.GetEquipProductionsByGroupID(groupid, null);
            if (tmp != null)
                foreach (var item in tmp)
                {
                    obj.data.Add(new EquipProductionDataUI() { Description = item.Description, EquipId = item.EquipId, GroupId = item.GroupId, Id = item.Id, Operator = item.Operator, ProductType = item.ProductType });
                }
            return obj;
        }

        public EquipProductionResponse EquipProductionsByProductType(string producttype)
        {
            EquipProductionResponse obj = new EquipProductionResponse();
            var tmp = this.EquipProductionProvider.GetEquipProductionsByProductType(producttype, null);
            if (tmp != null)
                foreach (var item in tmp)
                {
                    obj.data.Add(new EquipProductionDataUI() { Description = item.Description, EquipId = item.EquipId, GroupId = item.GroupId, Id = item.Id, Operator = item.Operator, ProductType = item.ProductType });
                }
            return obj;
        }

        public EquipProductionResponse EquipProductionsSearch(EquipProductionSearchRequest search)
        {
            EquipProductionResponse obj = new EquipProductionResponse();
            var tmp = this.EquipProductionProvider.GetEquipProductionsSearch(search, null);
            if (tmp != null)
                foreach (var item in tmp)
                {
                    obj.data.Add(new EquipProductionDataUI() { Description = item.Description, EquipId = item.EquipId, GroupId = item.GroupId, Id = item.Id, Operator = item.Operator, ProductType = item.ProductType });
                }
            return obj;
        }

        public EquipControllerConfigResponse GetAllEquipControllerConfig()
        {
            EquipControllerConfigResponse obj = new EquipControllerConfigResponse();
            var tmp = this.EquipControllerConfigProvider.GetAllEquipControllerConfig(null);
            if (tmp != null)
                foreach (var item in tmp)
                {
                    obj.data.Add(new EquipControllerConfigDataUI() { Description = item.Description, EquipControllerName = item.EquipControllerName, PlantNo = item.PlantNo, Id = item.Id });
                }
            return obj;
        }

        public EquipControllerConfigResponse GetEquipControllerConfigByPlantNo(string plantno)
        {
            EquipControllerConfigResponse obj = new EquipControllerConfigResponse();
            var tmp = this.EquipControllerConfigProvider.GetEquipControllerConfigByPlantNo(plantno, null);
            if (tmp != null)
                foreach (var item in tmp)
                {
                    obj.data.Add(new EquipControllerConfigDataUI() { Description = item.Description, EquipControllerName = item.EquipControllerName, PlantNo = item.PlantNo, Id = item.Id });
                }
            return obj;
        }

        public EquipControllerConfigResponse GetEquipControllerConfigByCtlName(string controllername)
        {
            EquipControllerConfigResponse obj = new EquipControllerConfigResponse();
            var tmp = this.EquipControllerConfigProvider.GetEquipControllerConfigByCtlName(controllername, null);
            if (tmp != null)
                foreach (var item in tmp)
                {
                    obj.data.Add(new EquipControllerConfigDataUI() { Description = item.Description, EquipControllerName = item.EquipControllerName, PlantNo = item.PlantNo, Id = item.Id });
                }
            return obj;
        }

        public EquipControllerConfigResponse GetEquipControllerConfigById(long id)
        {
            EquipControllerConfigResponse obj = new EquipControllerConfigResponse();
            var item = this.EquipControllerConfigProvider.GetEquipControllerConfigById(id, null);
            if (item != null)
                obj.data.Add(new EquipControllerConfigDataUI() { Description = item.Description, EquipControllerName = item.EquipControllerName, PlantNo = item.PlantNo, Id = item.Id });
            return obj;
        }

        public ResponseBase SaveEquipProductionList(List<EquipProductionDataUI> list, string oper)
        {
            ResponseBase obj = new ResponseBase();
            this.EquipProductionProvider.SaveEquipProductionList(list, oper, null);
            return obj;
        }

        public ResponseBase UpdateEquipProduction(EquipProductionDataUI product, string oper)
        {
            ResponseBase obj = new ResponseBase();
            this.EquipProductionProvider.UpdateEquipProduction(product, oper, null);
            return obj;
        }

        public AGVConfigResponse GetAGVConfig(string plantno)
        {
            AGVConfigResponse obj = new AGVConfigResponse();
            foreach (var item in this.AGVConfigProvider._DicAGVConfig)
            {
                var o = new AGVConfigUI() { Id = item.Key, Name = item.Value.AGVName, PlantNo = item.Value.PlantNo.ToString() };
                obj.data.Add(o);
            }
            return obj;
        }

        public AGVDetailResponse GetAGVDetail(long id)
        {
            AGVDetailResponse obj = new AGVDetailResponse();
            var ent = this.AGVConfigProvider.GetAGVById(id, null);
            obj.data = new AGVDetailUI() { Id = ent.Id, Description = ent.Description, Name = ent.AGVName, PlantNo = ent.PlantNo.ToString() };
            obj.data.BarCodes = new List<string>();
            AGVTasksEntity task = this.AGVTasksProvider.GetAGVTaskEntityById((int)id, null);
            //var spools = this.SpoolsProvider.GetSpoolsByTaskNo(task.TaskNo.ToString(), null);
            //spools.ForEach(x => obj.data.BarCodes.Add(x.FdTagNo));
            return obj;
        }

        public AGVConfigInfoResponse GetAGVConfigInfo(string plantno)
        {
            AGVConfigInfoResponse obj = new AGVConfigInfoResponse();
            var allagv = this.AGVConfigProvider.GetAllAGVConfig(null);
            allagv = allagv.FindAll(x => x.PlantNo.ToString() == plantno);
            var allstatus = this.AGVStatusProvider.GetAllAGVStatus(null);
            var rute = this.AGVRouteProvider.GetAllAGVRute(null);
            var task = this.AGVTasksProvider.GetAllAGVTaskEntity(null);
            foreach (var item in allagv)
            {
                var i = new AGVConfigInfoUI() { Id = item.Id };
                i.Status = allstatus.FirstOrDefault(x => x.AGVId.ToString() == item.Id.ToString())?.Status.ToString();
                var t = task.FirstOrDefault(x => x.AGVId.ToString() == item.Id.ToString());
                i.IsHasSpools = t.IsHasSpools;
                var r = rute.FirstOrDefault(x => x.AGVId.ToString() == item.Id.ToString());
                if (r != null)
                {
                    i.X = r.X;
                    i.Y = r.Y;
                }
                obj.data.Add(i);
            }

            return obj;
        }

        public AGVRouteArchiveResponse HistoryAGVRoute(AGVRuteSearchRequest search)
        {
            AGVRouteArchiveResponse obj = new AGVRouteArchiveResponse();
            var list = this.AGVRouteArchiveProvider.GetHistoryAGVRoute(search);
            obj.pagecount = list.Item2;
            obj.pagesize = list.Item3;
            int inx = 0;
            foreach (var item in list.Item1)
            {
                obj.data.Add(new AGVRouteArchiveUI() { agvid = search.agvid, x = item.X, y = item.Y, index = inx++ });
            }
            return obj;
        }
        /// <summary>
        /// 首次加载暂存库加载整体详细信息
        /// </summary>
        /// <param name="storagearea"></param>
        /// <returns></returns>
        public MidStorageBaseResponse GetMidStorageBase(int storagearea)
        {
            //Console.WriteLine("storagearea:" + storagearea);
            MidStorageBaseResponse obj = new MidStorageBaseResponse();
            List<MidStorageSpoolsEntity> list = new List<MidStorageSpoolsEntity>();
            list = this.MidStorageSpoolsProvider.GetMidStorageByArea((short)storagearea, null);
            if (list != null)
                foreach (var item in list)
                {
                    var mid = new WebServices.UserInterfaceBackend.Models.MidStorage.MidStorageBaseDataUI() { Id = item.Id, StorageArea = item.StorageArea, X = item.HCoordinate, Y = item.VCoordinate };
                    mid.Id = ConvertLocation(Convert.ToInt32(item.SeqNo));
                    mid.OriginalId = item.SeqNo;
                    if (item.Spool != null)
                        mid.Barcodes = item.Spool.FdTagNo.Trim();
                    int Status = 6;
                    int IsOccupied = item.IsOccupied;
                    if (item.IsVisible == -1)
                        Status = 6;

                    Status = MidStorageStatusToShow(Status, IsOccupied);
                    mid.Status = Status;
                    obj.data.Add(mid);
                }
            obj.data = obj.data.OrderBy(x => x.Id).ToList();
            return obj;
        }

        public MidStorageDetailResponse GetMidStorageDetail(int storagearea)
        {
            MidStorageDetailResponse obj = new MidStorageDetailResponse();
            int hours = this.SystemParametersProvider.GetSystemParametersSpoolTimeOut(null);
            var list = this.MidStorageSpoolsProvider.GetMidStorageByArea((short)storagearea, null);
            list = list.OrderBy(x => x.SeqNo).ToList();
            foreach (var item in list)
            {
                var mid = new WebServices.UserInterfaceBackend.Models.MidStorage.MidStorageDetailDataUI() { Id = item.Id, StorageArea = item.StorageArea, Description = item.Description, Barcodes = item.FdTagNo?.Trim(), BobbinNo = item.BobbinNo, Cname = item.CName, Length = item.Length, StructBarCode = item.StructBarCode?.Trim() };
                //mid.Barcodes = item.IdsList;
                mid.Id = ConvertLocation(Convert.ToInt32(item.SeqNo));
                mid.OriginalId = item.SeqNo;
                //mid.IsEnable = item.IsEnable;
                //mid.IsOccupied = item.IsOccupied;
                int Status = 6;
                int IsOccupied = item.IsOccupied;
                if (item.IsVisible == -1)
                    Status = 6;
                mid.InStoreageTime = item.Updated;
                if (IsOccupied == 1)
                    if (item.Updated.Value.AddHours(hours) > DateTime.Now)
                        mid.IsTimeOut = false;
                    else
                        mid.IsTimeOut = true;

                Status = MidStorageStatusToShow(Status, IsOccupied);
                mid.Status = Status;
                obj.data.Add(mid);
            }
            obj.data = obj.data.OrderBy(x => x.OriginalId).ToList();
            //var li = this.MidStorageProvider.GetMidStorageAccount(null);
            //foreach (var item in li)
            //{
            //    obj.count.Add(new WebServices.UserInterfaceBackend.Models.MidStorage.MidStorageCountDataUI() { Cname = item.CName, Count = item.Count, Length = item.Length, StorageArea = item.StorageArea, StructBarCode = item.StructBarCode });
            //}
            return obj;
        }

        private static int MidStorageStatusToShow(int Status, int IsOccupied)
        {
            switch (IsOccupied)
            {
                case -1:
                    Status = -1;
                    break;
                case 0:
                    Status = 1;
                    break;
                case 1:
                    Status = 3;
                    break;
                case 2:
                    //已预约 
                    break;
                case 3:
                    break;
                case 4:
                    Status = 4;
                    break;
                case 5:
                    Status = 5;
                    break;
                default:
                    break;
            }

            return Status;
        }

        public MidStorageCountResponse MidStorageDescription(int areaid)
        {
            MidStorageCountResponse obj = new MidStorageCountResponse();
            var li = this.MidStorageProvider.GetMidStorageAccount(null);
            //li = li.FindAll(x => x != null);
            var d = li.GroupBy(x => x.StorageArea, (y, z) => z.GroupBy(x => x.Length));

            var p = from i in li
                    group new MidStorageSpoolsCountEntity { StorageArea = i.StorageArea, StructBarCode = i.StructBarCode, Length = i.Length, BobbinNo = i.BobbinNo, Count = i.Count, CName = i.CName.Trim(), Const = i.Const?.Trim() }
                    by new { i.StorageArea, i.Length, i.Const } into t
                    select t;
            foreach (var items in p)
            {
                try
                {
                    string Const = items.FirstOrDefault()?.Const;
                    var l = items.ToList().FindAll(x => !string.IsNullOrEmpty(x.BobbinNo) && x.BobbinNo.Trim() == "L");
                    var r = items.ToList().FindAll(x => !string.IsNullOrEmpty(x.BobbinNo) && x.BobbinNo.Trim() == "R");
                    var o = items.ToList().FindAll(x => string.IsNullOrEmpty(x.BobbinNo) || (x.BobbinNo.Trim() != "R" && x.BobbinNo.Trim() != "L"));
                    int lc = 0, rc = 0, oc = 0;
                    if (l.Count != 0)
                    {
                        l.ForEach(x => lc += x.Count);
                    }
                    if (r.Count != 0)
                    {
                        r.ForEach(x => rc += x.Count);
                    }
                    if (o.Count != 0)
                    {
                        o.ForEach(x => oc += x.Count);
                    }
                    if (areaid == 0)
                    {
                        obj.data.Add(new WebServices.UserInterfaceBackend.Models.MidStorage.MidStorageCountDataUI() { L = lc, R = rc, other = oc, Count = lc + rc + oc, Cname = items.FirstOrDefault().CName, Length = items.FirstOrDefault().Length, StorageArea = items.FirstOrDefault().StorageArea, StructBarCode = items.FirstOrDefault().StructBarCode, Description = $"L:{lc}   R:{rc}  其他:{oc}", Const = Const });
                    }
                    else if (items.Key.StorageArea == areaid)
                    {
                        obj.data.Add(new WebServices.UserInterfaceBackend.Models.MidStorage.MidStorageCountDataUI() { L = lc, R = rc, other = oc, Count = lc + rc + oc, Cname = items.FirstOrDefault().CName, Length = items.FirstOrDefault().Length, StorageArea = items.FirstOrDefault().StorageArea, StructBarCode = items.FirstOrDefault().StructBarCode, Description = $"L:{lc}   R:{rc}  其他:{oc}", Const = Const });
                    }
                }
                catch (Exception)
                {

                }

            }
            obj.allCount = obj.allCount;
            return obj;
        }
        public MidStorageInfoResponse GetMidStorageInfo(int storagearea)
        {//-1不可见区域,0禁用,1没有轮子(可放置),3有轮子(被占用)
            //int hours = this.SystemParametersProvider.GetSystemParametersSpoolTimeOut(null);
            MidStorageInfoResponse obj = new MidStorageInfoResponse();
            //var list = this.MidStorageProvider.GetMidStorageByArea((short)storagearea, null);
            var list = this.MidStorageSpoolsProvider.GetMidStorageByArea((short)storagearea, null);
            foreach (var item in list)
            {
                var mid = new WebServices.UserInterfaceBackend.Models.MidStorage.MidStorageInfoDataUI() { Id = item.Id };
                mid.Id = ConvertLocation(Convert.ToInt32(item.SeqNo));
                mid.Barcodes = item.FdTagNo;

                int Status = 6;
                int IsOccupied = item.IsOccupied;
                if (item.IsVisible == -1)
                    Status = 6;

                //mid.InStoreageTime = item.Updated;
                //if (item.Updated.Value.AddHours(hours) > DateTime.Now)
                //    mid.IsTimeOut = false;
                //else
                //    mid.IsTimeOut = true;
                Status = MidStorageStatusToShow(Status, IsOccupied);
                mid.Status = Status;
                obj.data.Add(mid);
            }
            obj.data = obj.data.OrderBy(x => x.Id).ToList();
            return obj;
        }
        /// <summary>
        /// 将前端的暂存库坐标转换为数据库里的坐标
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static int ConvertStoreage(int i)
        {
            int row = i % 35;//取余 第二行
            int rows = i / 35;//第几列    
            int v = 0;
            if (row != 0)
            {
                v = (row - 1) * 11 + rows + 1;
            }
            else
            {
                v = 34 * 11 + rows;
            }
            return v;
        }
        /// <summary>
        /// 将暂存库库位坐标编号转换为前端库位编号
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static int ConvertLocation(int i)
        {
            int row = i % 11;//取余 第二行
            int rows = i / 11;//第几列    
            int v = 0;
            if (row != 0)
            {
                v = (row - 1) * 35 + rows + 1;
            }
            else
            {
                v = 10 * 35 + rows;
            }
            return v;
        }

        public MidStorageDetailResponse GetMidStorageDetailById(int storageid, int OriginalId)
        {
            MidStorageDetailResponse obj = new MidStorageDetailResponse();
            int hours = this.SystemParametersProvider.GetSystemParametersSpoolTimeOut(null);
            var item = this.MidStorageSpoolsProvider.GetMidStorageById(storageid, OriginalId, null);
            var mid = new WebServices.UserInterfaceBackend.Models.MidStorage.MidStorageDetailDataUI() { InStoreageTime = item.Updated, Id = item.Id, StorageArea = item.StorageArea, Description = item.Description.Trim(), Barcodes = item.FdTagNo.Trim(), BobbinNo = item.BobbinNo, Cname = item.CName.Trim(), Length = item.Length, StructBarCode = item.StructBarCode.Trim() };
            //mid.Barcodes = item.IdsList;
            //mid.IsEnable = item.IsEnable;
            //mid.Id = ConvertLocation(Convert.ToInt32(item.SeqNo));
            mid.Id = item.SeqNo;
            mid.Barcodes = item.FdTagNo;
            int Status = 6;
            int IsOccupied = item.IsOccupied;
            if (item.IsVisible == -1)
                Status = 6;
            //mid.InStoreageTime = item.Updated;
            if (IsOccupied == 1)
                if (item.Updated.Value.AddHours(hours) > DateTime.Now)
                    mid.IsTimeOut = false;
                else
                    mid.IsTimeOut = true;
            Status = MidStorageStatusToShow(Status, IsOccupied);
            mid.Status = Status;
            obj.data.Add(mid);
            return obj;
        }


        public ResponseBase TTT()
        {
            //this.systemParametersConfigurationProvider
            return null;
        }
        /// <summary>
        /// 手动异常口出库
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="OriginalIds"></param>
        /// <returns></returns>
        public ResponseDataBase ExcepitonLineOutStore(byte PlantNo, int storage, params string[] OriginalIds)
        {
            ResponseDataBase obj = new ResponseDataBase();
            Guid guid = Guid.NewGuid();
            DateTime dt = DateTime.Now;
            try
            {
                if (OriginalIds.Length == 0)
                {
                    obj = new ResponseDataBase();
                    obj.Error = new ResponseError() { Message = "id是空的" };
                    return obj;
                }
                StringBuilder sb = new StringBuilder();
                sb.Append("StorageArea=" + storage);
                sb.Append(" AND SeqNo IN (");
                foreach (var item in OriginalIds)
                {
                    sb.Append($"'{item.Trim()}',");
                }
                sb = new StringBuilder(sb.ToString().Trim(','));
                sb.Append(")");
                var storages = this.MidStorageSpoolsProvider.GetMidStorages(sb.ToString(), null);
                if (storages == null || storages.Count == 0)
                {
                    obj = new ResponseDataBase();
                    obj.Error = new ResponseError() { Message = "错误的ID" };
                    return obj;
                }
                var occupied = storages.FindAll(x => x.IsOccupied == 4 || x.IsOccupied == 5);
                var taskspool = storages.FindAll(x => x.IsOccupied == 1);
                List<RobotArmTaskEntity> armtsks = new List<RobotArmTaskEntity>();
                List<MidStorageSpoolsEntity> mids = new List<MidStorageSpoolsEntity>();
                StringBuilder sbbarcodes = new StringBuilder();
                foreach (var item in occupied)
                {
                    sbbarcodes.Append(item.FdTagNo.Trim() + ",");
                }
                sbbarcodes = new StringBuilder(sbbarcodes.ToString().Trim(','));
                int i = 0;
                StringBuilder sbout = new StringBuilder();
                foreach (var item in taskspool)
                {
                    ++i;
                    sbout.Append("SeqNo:" + item.SeqNo + ",FdTagNo:" + item.FdTagNo.Trim() + ",");
                    item.IsOccupied = 4;
                    var tskconfig = TaskConfig.GetEnoughAGVEquipCount(item.CName.Trim());//8 / 12
                    armtsks.Add(new RobotArmTaskEntity() { Created = dt, FromWhere = item.SeqNo, StorageArea = storage, TaskGroupGUID = guid, TaskLevel = 8, PlantNo = PlantNo, ProductType = tskconfig.Item3.ToString(), TaskType = 4, ToWhere = MidStoreLine.ExceptionLine, WhoolBarCode = item.FdTagNo.Trim(), SeqNo = i, TaskStatus = 0, SpoolStatus = 0, AGVSeqNo = 0, IsDeleted = 0, CName = item.CName, RobotArmID = storage.ToString() });
                }
                if (armtsks.Count != 0)
                {
                    this.RobotArmTaskProvider.InsertArmTask(armtsks, null);
                    logger.InfoMethod("手动创建异常口出库任务成功,guid:" + guid.ToString());
                    this.MidStorageSpoolsProvider.UpdateMidStore(null, taskspool.ToArray());
                    logger.InfoMethod("更新暂存库异常口出库任务状态成功");
                }
                obj.Error = new ResponseError();
                obj.Error.Arguments = new string[2];


                if (occupied.Count != 0 && taskspool.Count != 0)
                {
                    obj.Error.Message = "部分单丝无法抓取到异常口,原因:已分配给其他设备叫料任务";
                    obj.Error.Arguments[0] = "成功:" + sbout.ToString().Trim(',');//成功的
                    obj.Error.Arguments[1] = "失败:" + sbbarcodes.ToString().Trim(',');//失败的
                }
                else if (occupied.Count == 0 && taskspool.Count != 0)
                {
                    obj.Error = new ResponseError();
                    obj.Error.Message = "创建成功,稍后进行抓取.....";
                }
                else if (occupied.Count != 0 && taskspool.Count == 0)
                {
                    obj.Error.Message = "所选单丝无法抓取到异常口,原因:已分配给其他设备叫料任务";
                    obj.Error.Arguments[0] = "失败:" + sbout.ToString().Trim(',');//成功的
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
            /*
            校验库位状态
            创建龙门任务
            更新库位状态
            */
            return obj;
        }

        public ResponseBase CreateEquipTask(int ctrlid, int TaskType, int PlantNo)
        {
            ResponseBase obj = new ResponseBase();
            this.EquipTaskProvider.CreateEquipTask(ctrlid, TaskType, PlantNo, null);
            return obj;
        }

        /// <summary>
        /// 叫料按钮状态 地面滚筒状态
        /// </summary>
        /// <param name="plantno"></param>
        /// <returns></returns>
        public EquipConfigStatusResponse EquipCtrlStatus(short plantno)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();//叫料按钮id,界面ID
            //dic = getdic(); 

            EquipConfigStatusResponse obj = new EquipConfigStatusResponse();
            var equip = this.EquipConfigProvider.GetEquipConfigByPlantNo(3, null);
            var equiptsks = this.EquipTaskViewProvider.GetEquipTaskViewEntities($"[PlantNo]={plantno} AND [STATUS] IN (0,1,2,3,4,5,6,10) AND AGVStatus<=17", null);
            if (equip != null)
                foreach (var item in equip)
                {
                    var da = new EquipStatusDataUI() { EquipId = Convert.ToInt16(item.EquipControllerId), ShowID = item.ShowID };
                    obj.data.Add(da);//item.Status
                    da.Status = 1;
                    if (equiptsks == null)
                        continue;
                    var equptsk = equiptsks.FirstOrDefault(x => x.EquipContollerId == item.EquipControllerId);
                    if (equptsk != null)
                    {
                        if (equptsk.TaskType == 1)
                        {
                            da.Status = 5;
                        }
                        else
                        {
                            da.Status = 2;
                        }
                        switch (equptsk.Status)
                        {//0初始化EquipTask,1创建AGVTask和龙门Task,2正在抓取,3,抓取完毕,4等待调度AGV,5已调度AGV,6AGV运行中,7任务完成(拉空论或满轮),8任务失败,9已通知地面滚筒创建好任务,10库里单丝不够
                            #region MyRegion
                            case 0:
                                break;
                            case 1:
                                break;
                            case 2:
                                da.Status = 6;
                                break;
                            case 3:
                            case 4:
                                break;
                            case 5:
                            case 6:
                                da.Status = 4;
                                break;
                            case 7:
                                break;
                            case 8:
                                break;
                            case 9:
                                break;
                            case 10:
                                da.Status = 3;
                                break;
                            default:
                                break;
                                #endregion
                        }
                    }
                }
            return obj;
        }



        /// <summary>
        /// 1 绿色 初始状态   2红色 叫有料轮   3橙色  （叫有料轮）库里没有轮子 4 蓝色 呼叫小车成功   5黄色  呼叫拉空轮子,6 正在抓取
        /// </summary>
        /// <param name="PlantNo"></param>
        /// <param name="storageareaid"></param>
        /// <returns></returns>
        public EquipTaskStatusResponse EquipTaskStatus(byte PlantNo, byte storageareaid)
        {
            //0初始化EquipTask,1创建AGVTask和龙门Task,2正在抓取,3,抓取完毕,4等待调度AGV,5已调度AGV,6AGV运行中,7任务完成(拉空论或满轮),8任务失败,9已通知地面滚筒创建好任务,10库里单丝不够
            EquipTaskStatusResponse obj = new EquipTaskStatusResponse();
            var equip = this.EquipConfigProvider.GetEquipConfigByPlantNo(3, null);
            string stor = "";
            if (storageareaid == 3)
                stor = "'3'";
            else if (storageareaid == 1 || storageareaid == 2)
                stor = "'12'";
            else stor = "'3','12'";
            var equiptsks = this.EquipTaskViewProvider.GetEquipTaskViewEntities("[PlantNo]=3 AND [StorageArea]IN (" + stor + ") AND [STATUS] IN (0,1,2,3,4,5,6,10)", null);
            foreach (var item in equiptsks)
            {
                var d = new EquipTaskStatusDataUI() { Length = item.Length, AGVRoute = item.AGVRoute, Created = item.Created, EquipControllerId = item.EquipContollerId, EquipTaskID = item.Id, PlantNo = item.PlantNo, ProductType = item.ProductType, Status = item.Status, StorageArea = item.StorageArea, Supply1 = item.Supply1, TaskGuid = item.TaskGuid, TaskLevel = item.TaskLevel, TaskType = item.TaskType, AGVID = item.AGVId.ToString() };
                var config = equip.FindAll(x => x.EquipControllerId == item.EquipContollerId);

                foreach (var c in config)
                {
                    d.EquipName = d.EquipName + "," + c.EquipName.Trim();
                }
                d.EquipName = d.EquipName.Trim(',');
                obj.data.Add(d);
            }
            obj.data = obj.data.OrderByDescending(x => x.EquipTaskID).ToList();
            return obj;
        }

        public ResponseBase DeleteEquipTask(long taskid)
        {
            ResponseBase obj = new ResponseBase();
            var equiptsk = this.EquipTaskProvider.GetEquipTaskEntityByID(taskid, null);
            if (equiptsk.Status == 0)
            {
                equiptsk.IsDeleted = 1;
                bool r = this.EquipTaskProvider.UpdateEntity(equiptsk, null);
                if (r)
                    obj.Error = new ResponseError() { Message = "删除成功" };
                else obj.Error = new ResponseError() { Message = "删除失败" };
            }
            else
                obj.Error = new ResponseError() { Message = "任务已执行,无法删除" };
            return obj;
        }

        public EquipCallInfoResponse EquipCallInfor(int plantno, int EquipId)
        {
            EquipCallInfoResponse obj = new EquipCallInfoResponse();
            var tsk = this.EquipTaskView2Provider.GetEquipTaskView2("PlantNo=" + plantno + " AND EquipControllerId=" + EquipId, null);
            if (tsk == null || tsk.Count == 0)
                return obj;
            tsk = tsk.OrderBy(x => x.EquipTaskID).Take(2).ToList();
            string name = "";
            tsk.ForEach(x => name = name + x.EquipName.Trim() + ",");
            obj.data.Add(new EquipCallDataUI() { EquipId = EquipId, EquipName = name.Trim(','), EquipStatus = tsk.First().Status, plantno = plantno });
            if (tsk.Exists(x => x.Status == 9))
            {
                obj.data[0].EquipStatus = 1;
            }
            else
            {
                obj.data[0].EquipStatus = tsk.FirstOrDefault(x => x.Status != 9).Status;
            }
            return obj;
        }
        public RobotArmTaskResponse ExceptionRobotArmTask()
        {
            RobotArmTaskResponse obj = new RobotArmTaskResponse();
            var armtsks = this.RobotArmTaskSpoolProvider.GetRobotArmTaskSpools("SpoolStatus=1", null);
            if (armtsks == null)
                return obj;
            StringBuilder sb = new StringBuilder();
            List<EquipTaskView2Entity> equip = new List<EquipTaskView2Entity>();
            foreach (var item in armtsks)
            {
                sb.Append("'" + item.TaskGroupGUID.ToString() + "',");
            }
            equip = this.EquipTaskView2Provider.GetEquipTaskView2("[TaskGuid] in (" + sb.ToString().Trim(',') + ")", null);
            var armtskunit = this.RobotArmTaskSpoolProvider.GetRobotArmTaskSpools("[TaskGroupGUID] in (" + sb.ToString().Trim(',') + ")", null);
            var spools = this.SpoolsProvider.GetSpoolByBarcodes(null, (from i in armtsks select i.WhoolBarCode.Trim()).ToArray());
            foreach (var item in armtsks)
            {
                var tmpspools = spools.FirstOrDefault(x => x.FdTagNo.Trim() == item.WhoolBarCode.Trim());
                var t = new WebServices.UserInterfaceBackend.Models.RobotArmTask.RobotArmTaskDataUI() { id = item.Id, CName = item.CName.Trim(), FromWhere = item.FromWhere, PlantNo = item.PlantNo, ProductType = item.ProductType.Trim(), RobotArmID = item.RobotArmID, SeqNo = item.SeqNo, SpoolStatus = item.SpoolStatus, StorageArea = item.StorageArea, TaskGroupGUID = item.TaskGroupGUID, TaskStatus = item.TaskStatus, TaskType = item.TaskType, ToWhere = item.ToWhere, WhoolBarCode = item.WhoolBarCode.Trim(), EquipControllerId = item.EquipControllerId, SpoolSeqNo = item.SpoolSeqNo, LR = tmpspools.BobbinNo.ToString(), Length = tmpspools.Length };
                obj.data.Add(t);
                #region  equip
                if (equip != null)
                {
                    var ee = equip.FindAll(x => x.TaskGuid == item.TaskGroupGUID);
                    if (ee != null && ee.Count != 0)
                    {
                        foreach (var e in ee)
                        {
                            if (!string.IsNullOrEmpty(e.EquipName))
                                t.EquipName += e.EquipName.Trim() + ",";
                        }
                        t.EquipName = t.EquipName.Trim(',');
                    }
                }
                #endregion

                var tmp = armtskunit?.FindAll(x => x.TaskGroupGUID == item.TaskGroupGUID);
                if (tmp != null)
                {

                    tmp = tmp.OrderBy(x => x.SeqNo).ToList();
                    foreach (var arm in tmp)
                    {
                        var tmpspool = spools.FirstOrDefault(x => x.FdTagNo.Trim() == arm.WhoolBarCode.Trim());
                        t.Spool = (new WebServices.UserInterfaceBackend.Models.RobotArmTask.RobotArmSpoolDataUI() { SpoolSeqNo = arm.SpoolSeqNo, SpoolStatus = arm.SpoolStatus, WhoolBarCode = arm.WhoolBarCode, Length = tmpspool.Length, LR = tmpspool.BobbinNo.ToString() });
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status">0重发;1校正;2完成</param>
        /// <returns></returns>
        public ResponseDataBase ExceptionRobotArmTask(long id, int status)
        {
            ResponseDataBase obj = new ResponseDataBase();
            obj.data = new List<string>(2);
            List<RobotArmTaskEntity> armtsks = new List<RobotArmTaskEntity>();
            List<MidStorageEntity> mids = new List<MidStorageEntity>();
            var armtsk = this.RobotArmTaskProvider.GetRobotArmTaskEntityByID(id, null);
            if (armtsk == null)
            {
                obj.data.Add("错误的ID");
                return obj;
            }
            armtsks.Add(armtsk);
            if (status == 0 || status == 2)
            {
                armtsk.SpoolStatus = status;
                this.RobotArmTaskProvider.UpdateArmTask(armtsk, null);
                obj.data.Add("发送成功,等待抓取");
                if (status == 2)
                {
                    /*
                    完成后应更新该库位信息
                    */
                    var mid = this.MidStorageProvider.GetMidStorage("[SeqNo] = " + armtsk.FromWhere + " AND [StorageArea]=" + armtsk.StorageArea, null);
                    if (mid != null)
                    {
                        #region mid
                        if (armtsk.TaskType == 0 || armtsk.TaskType == 1 || armtsk.TaskType == 3)
                        {//出库
                            mid.IdsList = "";
                            mid.IsOccupied = 0;
                        }
                        else if (armtsk.TaskType == 2)
                        {//入库 
                            mid.IsOccupied = 1;
                        }
                        #endregion
                        mids.Add(mid);
                    }
                    bool r = this.SqlCommandProvider.ExceptionRobotArmTask(armtsks, mids, null);
                    if (r)
                        this.RobotArmTaskProvider.SetArmTasksUnitStatus(armtsk.TaskGroupGUID);
                    if (r)
                    {
                        var equiptsks = this.EquipTaskProvider.GetEquipTaskEntitySqlWhere($"TaskGuid='{armtsk.TaskGroupGUID}'", null);
                        if (equiptsks != null && equiptsks.Count != 0)
                        {
                            equiptsks.ForEach(x => x.Status = 3);
                            this.EquipTaskProvider.UpdateEntity(equiptsks, null);
                        }
                    }
                    obj.data.Add("发送成功");
                }
            }
            else if (status == 1)
            {//矫正
                //var misspools = this.MidStorageSpoolsProvider.GetMidStorages($"[Length]={armtsk.Length} AND [IsOccupied]=1 AND [StorageArea]={armtsk.StorageArea}", null);
                //if (misspools == null)
                //    misspools = this.MidStorageSpoolsProvider.GetMidStorages($"[IsOccupied]=1 AND [StorageArea]={armtsk.StorageArea}", null);
                //var misspool = misspools.FirstOrDefault();
                //misspool.IsOccupied = 4;
                //armtsk.SpoolStatus = 0;
                //armtsk.WhoolBarCode = misspool.FdTagNo.Trim();
                //armtsk.FromWhere = misspool.SeqNo;
                //this.RobotArmTaskSpoolProvider.UpdateArmTask(armtsk, null);
                obj.data.Add("错误的龙门任务状态指令");
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
            /*
            清空缓存的轮子
            清空已创建的入库龙门任务
            重置暂存库状态
            */
            ResponseDataBase o = new ResponseDataBase();
            o.data = new List<string>(1);

            //File.WriteAllText($"./StorageArea{storeageareaid}QrCode.json", "[]");
            var armtsks = this.RobotArmTaskSpoolProvider.GetRobotArmTaskSpools("StorageArea=" + storeageareaid + " AND TaskType=2 AND [PlantNo]=" + plantno + " AND [SpoolStatus] IN(0,1) AND [RobotArmID]=" + storeageareaid, null);
            if (armtsks == null)
                armtsks = new List<RobotArmTaskSpoolEntity>();
            armtsks.ForEach(x => x.IsDeleted = 1);
            armtsks.ForEach(x => x.TaskStatus = 8);

            var mids = this.MidStorageProvider.GetMidStorages($"IsOccupied=5 AND [StorageArea]={storeageareaid}", null);
            if (mids == null)
                mids = new List<MidStorageEntity>();
            mids.ForEach(x => x.IsOccupied = 0);
            mids.ForEach(x => x.IdsList = "");

            bool r = this.SqlCommandProvider.ClearInStoreageLine(armtsks, mids, null);
            if (!r)
                o.data.Add($"清除{storeageareaid}号暂存库直通线失败");
            else
            {
                File.WriteAllText($".{SNTONConstants.FileTmpPath}/StorageArea{storeageareaid}QrCode.json", "[]");
                File.WriteAllText($".{SNTONConstants.FileTmpPath}/StorageArea{storeageareaid}LRQrCode.json", "[]");
                o.data.Add($"清除{storeageareaid}号暂存库直通线成功");
            }
            return o;
        }

        public ResponseDataBase StoreageStatus()
        {
            ResponseDataBase obj = new ResponseDataBase();
            obj.data = new List<string>(1);
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= 3; i++)
            {
                if (this.GetMidStoreLineLogic(i) != null)
                {
                    if (this.GetMidStoreLineLogic(i).IsScanEnough)
                        sb.Append(i + "号暂存库扫码口满; ");
                    if (this.GetMidStoreLineLogic(i).IsWarning)
                        sb.Append(i + "号暂存库线体报警; ");
                    if (this.GetMidStoreLineLogic(i).IsStoreageEnough)
                        sb.Append(i + "号暂存库已满; ");
                }
                if (GetMidStoreRobotArmLogic(i) != null)
                {
                    if (GetMidStoreRobotArmLogic(i).IsWarning)
                        sb.Append(i + "号龙门退出自动或故障; ");
                    if (!GetMidStoreRobotArmLogic(i).IsCanSend)
                        sb.Append(i + "号龙门库不允许下发指令; ");
                }
            }
            if (sb.Length == 0)
                obj.data.Add("龙门库运行正常");
            else
                obj.data.Add(sb.ToString());
            return obj;
        }

        public AGVRouteResponse RealTimeAGVRoute()
        {
            AGVRouteResponse obj = new AGVRouteResponse();
            var tmp = this.AGVRouteProvider.GetAllAGVRute();
            if (tmp != null)
                foreach (var item in tmp)
                {
                    obj.data.Add(new AGVRouteDataUI() { agvid = item.AGVId, id = item.Id, x = item.X.Trim(), y = item.Y.Trim(), Status = item.Status });
                }
            return obj;
        }

        public InStoreageLineResponse InStoreageLineStatus()
        {
            InStoreageLineResponse obj = new InStoreageLineResponse();
            for (int i = 1; i <= 3; i++)
            {
                if (this.GetMidStoreLineLogic(i) != null)
                {
                    //var qrcodes = this.GetMidStoreLineLogic(i).GetbarcodeQueue();
                }
                var qrcodes = GetbarcodeQueue(i);
                obj.data.Add(new WebServices.UserInterfaceBackend.Models.EquipWatch.InStoreageLineDataUI() { QrCodes = qrcodes.ToList(), StoreageNo = i });
            }
            return obj;
        }
        public Queue<string> GetbarcodeQueue(int StorageArea)
        {
            if (!File.Exists($".{SNTONConstants.FileTmpPath}/StorageArea{StorageArea}QrCode.json"))
                return new Queue<string>();
            Queue<string> obj = new Queue<string>();
            try
            {
                string json = File.ReadAllText($".{SNTONConstants.FileTmpPath}/StorageArea{StorageArea}QrCode.json");
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Queue<string>>(json);
            }
            catch (Exception ex)
            {
                logger.WarnMethod("反序列化barcodeQueue失败", ex);
            }
            return obj;
        }
        public ResponseDataBase SendToExceptionFlow(int plantno, int storeageareaid, int type)
        {
            ResponseDataBase obj = new ResponseDataBase();
            this.GetMidStoreLineLogic(storeageareaid).SendToExceptionFlow(type);
            obj.data = new List<string>(1);
            obj.data.Add("指令发送成功");
            return obj;
        }

        public AGVTaskResponse RunningAGVTask()
        {
            AGVTaskResponse obj = new AGVTaskResponse();

            var s1l1 = this.AGVTasksProvider.GetAGVTasks(5, "StorageArea=1 AND StorageLineNo=1", null);
            var s1l2 = this.AGVTasksProvider.GetAGVTasks(5, "StorageArea=1 AND StorageLineNo=2", null);
            var s2l1 = this.AGVTasksProvider.GetAGVTasks(5, "StorageArea=2 AND StorageLineNo=1", null);
            var s2l2 = this.AGVTasksProvider.GetAGVTasks(5, "StorageArea=2 AND StorageLineNo=2", null);
            var s3l1 = this.AGVTasksProvider.GetAGVTasks(5, "StorageArea=3 AND StorageLineNo=1", null);
            var s3l2 = this.AGVTasksProvider.GetAGVTasks(5, "StorageArea=3 AND StorageLineNo=2", null);
            var kong = this.AGVTasksProvider.GetAGVTasks(5, "StorageLineNo=0", null);
            List<AGVTasksEntity> d = new List<AGVTasksEntity>();
            if (s1l1 != null)
                d.AddRange(s1l1);
            if (s1l2 != null)
                d.AddRange(s1l2);
            if (s2l1 != null)
                d.AddRange(s2l1);
            if (s2l2 != null)
                d.AddRange(s2l2);
            if (s3l1 != null)
                d.AddRange(s3l1);
            if (s3l2 != null)
                d.AddRange(s3l2);
            if (kong != null)
                d.AddRange(kong);
            foreach (var item in d)
            {
                var agvtsk = new AGVTaskDataUI() { AGVId = item.AGVId, Created = item.Created, id = item.Id, LineNo = item.StorageLineNo, SeqNo = item.SeqNo, Status = item.Status, Storeageid = item.StorageArea, TaskType = item.TaskType };
                item._EquipTasks2.ForEach(x => agvtsk.EquipNames.Add(x.EquipName.Trim()));
                obj.data.Add(agvtsk);
            }

            return obj;
        }
        public ResponseDataBase SetAGVTaskStatus(long id, int status)
        {
            ResponseDataBase obj = new ResponseDataBase();
            bool r = this.AGVTasksProvider.UpdateStatus(id, status);
            if (r)
                obj.data.Add("发送指令成功");
            else obj.data.Add("发送指令失败");
            return obj;
        }

        public ProductResponse GetProduct()
        {
            ProductResponse obj = new ProductResponse();
            var list = this.ProductProvider.GetAllProductEntity();
            if (list != null)
                foreach (var item in list)
                {
                    obj.data.Add(new WebServices.UserInterfaceBackend.Models.Product.ProductDataUI() { CName = item.CName, Const = item.Const, Id = item.Id, Length = item.Length, LRRatio = item.LRRatio, PlatingType = item.PlatingType, ProductNo = item.ProductNo, ProductType = item.ProductType, SeqNo = item.SeqNo });
                }
            return obj;
        }

        public ResponseDataBase SaveProductLRRadio(int id, string lrratio)
        {
            ResponseDataBase obj = new ResponseDataBase();
            var o = this.ProductProvider.GetProductEntityByID(id);
            o.LRRatio = lrratio;
            int i = this.ProductProvider.UpdateEntity(null, o);
            if (i == 1)
                obj.data.Add("保存成功");
            else obj.data.Add("保存失败");
            return obj;
        }
    }

}

