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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDBContext _db;

        public OrderDetailRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetail orderDetail)
        {
            _db.OrderDetail.Update(orderDetail);
        }
    }
}
