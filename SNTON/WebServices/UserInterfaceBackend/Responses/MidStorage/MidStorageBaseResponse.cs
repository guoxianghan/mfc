
using SNTON.WebServices.UserInterfaceBackend.Models.MidStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.MidStorage
{
    public class MidStorageBaseResponse : ResponseBase
    {
        public List<MidStorageBaseDataUI> data { get; set; } = new List<MidStorageBaseDataUI>();
    }

    public class MidStorageDetailResponse : ResponseBase
    {
        public List<MidStorageDetailDataUI> data { get; set; } = new List<MidStorageDetailDataUI>();
        //public List<MidStorageCountDataUI> count { get; set; } = new List<MidStorageCountDataUI>();
    }
    public class MidStorageCountResponse : ResponseBase
    {
        public List<MidStorageCountDataUI> data { get; set; } = new List<MidStorageCountDataUI>();
        public int allCount
        {
            get
            {
                int i = 0;
                if (data != null && data.Count != 0)
                {
                    data.ForEach(x => i = i + x.Count);
                }
                return i;
            }
            set { }
        }
    }
    public class MidStorageInfoResponse : ResponseBase
    {
        public List<MidStorageInfoDataUI> data { get; set; } = new List<MidStorageInfoDataUI>();
    }
}
