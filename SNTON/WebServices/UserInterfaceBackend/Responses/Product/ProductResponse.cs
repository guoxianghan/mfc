using SNTON.WebServices.UserInterfaceBackend.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.WebServices.UserInterfaceBackend.Responses.Product
{
    public class ProductResponse : ResponseBase
    {
        public List<ProductDataUI> data { get; set; } = new List<ProductDataUI>();
    }
}
