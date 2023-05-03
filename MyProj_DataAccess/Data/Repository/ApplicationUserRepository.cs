using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProj_DataAccess.Data.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDBContext _db;

        public ApplicationUserRepository(ApplicationDBContext db): base(db)
        {
            _db = db;
        }
    }
}
