using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProj_DataAccess.Data.Repository
{
    public class ApplicationTypeRepository : Repository<ApplicationType>, IApplicationTypeRepository
    {
        private readonly ApplicationDBContext _db;

        public ApplicationTypeRepository(ApplicationDBContext db): base(db)
        {
            _db = db;
        }

        public void Update(ApplicationType applicationType)
        {
            var objFromDB = base.FirstOrDefault(u => u.Id == applicationType.Id);
            if(objFromDB != null)
            {
                objFromDB.Name = applicationType.Name;
            }
        }
    }
}
