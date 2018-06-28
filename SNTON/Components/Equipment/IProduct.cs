using NHibernate;
using SNTON.Entities.DBTables.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Components.Equipment
{
    public interface IProduct
    {
        ProductEntity GetProductEntityByID(long id, IStatelessSession session = null);
        List<ProductEntity> GetAllProductEntity(IStatelessSession session = null);

        int UpdateEntity(IStatelessSession session, params ProductEntity[] products);
        bool AddProduct(ProductEntity entity, IStatelessSession session = null);
        List<ProductEntity> PruductCache { get; set; }

    }
}

