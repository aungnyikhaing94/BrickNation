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
    public class InquiryDetailRepository : Repository<InquiryDetail>, IInquiryDetailRepository
    {
        private readonly ApplicationDBContext _db;

        public InquiryDetailRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public void Update(InquiryDetail inquiryDetail)
        {
            _db.InquiryDetail.Update(inquiryDetail);
        }
    }
}
