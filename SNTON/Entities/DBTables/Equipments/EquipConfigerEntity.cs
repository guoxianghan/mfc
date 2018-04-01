using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using SNTON.Entities.DBTables.MES;

namespace SNTON.Entities.DBTables.Equipments
{
    public class EquipConfigerEntity
    {

        [DataMember]
        public virtual int ControlID { get; set; }
        /// <summary>
        /// W00地址
        /// </summary>
        [DataMember]
        public virtual string EquipFlag { get; set; }
        /// <summary>
        /// 滚筒>WCS请求调度AGV(读)
        /// </summary>
        [DataMember]
        public virtual string LWCS { get; set; }
        /// <summary>
        /// 滚筒状态地址
        /// </summary>
        [DataMember]
        public virtual string LineStatus { get; set; }
        /// <summary>
        /// 滚筒状态
        /// </summary>
        [DataMember]
        public virtual string LStatus { get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        [DataMember]
        public virtual string Equip1Status { get; set; }
        /// <summary>
        /// 设备状态地址
        /// </summary>
        [DataMember]
        public virtual string EStatus1 { get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        [DataMember]
        public virtual string Equip2Status { get; set; }
        /// <summary>
        /// 设备状态地址
        /// </summary>
        [DataMember]
        public virtual string EStatus2 { get; set; }
        /// <summary>
        /// WCS已调度AGV(写)地址
        /// </summary>
        [DataMember]
        public virtual string TaskFlagDispatch { get; set; }
        /// <summary>
        /// WCS已调度AGV(写)W400
        /// </summary>
        [DataMember]
        public virtual string WAStatus { get; set; }
        [DataMember]
        public virtual string DispatchStatus { get; set; }
        [DataMember]
        public virtual string AGVDisStatus { get; set; }
        [DataMember]
        public virtual int PLCNo { get; set; }
        [DataMember]
        public virtual DateTime? Created { get; set; }
        [DataMember]
        public virtual DateTime? Updated { get; set; }
        [DataMember]
        public virtual DateTime? Deleted { get; set; }
        [DataMember]
        public virtual int IsDeleted { get; set; }
        [DataMember]
        public virtual int IsEnable { get; set; }
        [DataMember]
        public virtual string Location { get; set; }
        [DataMember]
        public virtual string StorageArea { get; set; }
        [DataMember]
        public virtual string AGVRoute { get; set; }
        [DataMember]
        public virtual int PlantNo { get; set; }

        /// <summary>
        ///  
        /// </summary>
        [DataMember]
        public virtual int AStation { get; set; }
        /// <summary>
        ///  
        /// </summary>
        [DataMember]
        public virtual int BStation { get; set; }
        public virtual List<EquipConfigEntity> EquipList { get; set; }
        public virtual string MachCode
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (EquipList != null && EquipList.Count > 0)
                {
                    return EquipList[0].EquipName.Replace("-", "").Substring(2);
                }
                else
                    return "";
            }
        }
        public virtual string EquipName
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (EquipList != null && EquipList.Count > 0)
                {
                    foreach (var item in EquipList)
                    {
                        sb.Append(item.EquipName.Trim() + ",");
                    }
                    return sb.ToString().Trim(',');
                }
                else
                    return "";
            }
        }
        /// <summary>
        /// 作业标准书以及子作业标准书
        /// </summary>
        public virtual tblProdCodeStructMachEntity MachStructCode { get; set; }
        /// <summary>
        /// 任务需要的产品规格
        /// </summary>
        public virtual string ProductType { get; set; }
        /// <summary>
        /// 1拉空轮,2送满轮
        /// </summary>
        public virtual int Request { get; set; }
        /// <summary>
        /// 是否已经给光电,0无,W400
        /// </summary>
        public virtual int Light { get; set; }
        /// <summary>
        /// 是否已经创建任务
        /// </summary>
        public virtual int IsEquipTask { get; set; }
    }
}
