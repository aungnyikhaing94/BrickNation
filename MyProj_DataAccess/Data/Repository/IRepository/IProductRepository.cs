using Microsoft.AspNetCore.Mvc.Rendering;
using MyProj_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProj_DataAccess.Data.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);

        IEnumerable<SelectListItem> GetAllDropDownList(string obj);
    }
}
