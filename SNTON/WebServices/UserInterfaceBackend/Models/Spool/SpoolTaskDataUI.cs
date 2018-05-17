using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Models.Spool
{
    public class SpoolTaskDataUI
    {

        public long Id
        {
            get;
            set;
        }

        /// <summary>
        /// 入库时间
        /// </summary>        
        public DateTime Created
        {
            get;
            set;
        }

        /// <summary>
        /// 出库时间
        /// </summary>
        public DateTime? Updated
        {
            get;
            set;
        }

        /// <summary>
        /// When has this entity been updated?
        /// </summary>

        public DateTime? Deleted
        {
            get;
            set;
        }
        /// <summary>
        /// Version identifier in case non .NET CLR apps are using this entity
        /// </summary>
        //
        //public  long Version
        //{
        //get;
        //set;
        //}

        /// <summary>
        /// delete tag , 0 = Not deleted, 1 = Should be deleted
        /// </summary>

        public short IsDeleted
        {
            get;
            set;
        }
        /// <summary>
        /// FdTagNo
        /// </summary>

        public string FdTagNo { get; set; }

        /// <summary>
        /// ProductType
        /// </summary>

        public string ProductType { get; set; }

        /// <summary>
        /// CName
        /// </summary>

        public string CName { get; set; }

        /// <summary>
        /// Const
        /// </summary>

        public string Const { get; set; }

        /// <summary>
        /// Length
        /// </summary>

        public int Length { get; set; }
        /// <summary>
        /// Length
        /// </summary>

        public int StorageArea { get; set; }

        /// <summary>
        /// BobbinNo
        /// </summary>

        public string BobbinNo { get; set; }
        public string EquipName { get; set; }
        /// <summary>
        /// TaskGroupGUID
        /// </summary>

        public Guid TaskGroupGUID { get; set; }
    }
}
