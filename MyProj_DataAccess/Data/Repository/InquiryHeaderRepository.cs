using Microsoft.AspNetCore.Mvc.Rendering;
using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_Models;
using MyProj_Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProj_DataAccess.Data.Repository
{
    public class InquiryHeaderRepository : Repository<InquiryHeader>, IInquiryHeaderRepository
    {
        private readonly ApplicationDBContext _db;

        public InquiryHeaderRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public void Update(InquiryHeader inquiryHeader)
        {
            _db.InquiryHeader.Update(inquiryHeader);
        }
    }
}
