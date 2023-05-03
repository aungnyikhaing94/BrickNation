using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProj_DataAccess.Data.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDBContext _db;

        public CategoryRepository(ApplicationDBContext db): base(db)
        {
            _db = db;
        }
        public void Update(Category category)
        {
            var objFromDb = base.FirstOrDefault(u => u.Id == category.Id);
            if(objFromDb != null)
            {
                objFromDb.Name = category.Name;
                objFromDb.DisplayOrder = category.DisplayOrder;
            }
        }
    }
}
