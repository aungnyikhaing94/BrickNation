using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_Models;
using MyProj_Models.ViewModels;
using MyProj_Utility;
using MyProj_Utility.BrainTree;

namespace MyProj.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IConfiguration _configuration;

        public BrainTreeSettings _brainTreeSettings { get; set; }

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IOrderHeaderRepository orderHeaderRepository, IOrderDetailRepository orderDetailRepository,
            IConfiguration configuration)
        {
            _orderDetailRepository = orderDetailRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _configuration = configuration;
        }
        public IActionResult Index(string searchName=null, string searchEmail=null, string searchPhone=null, string Status=null)
        {
            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHeaderList = _orderHeaderRepository.GetAll(),
                StatusList = WC.listStatus.ToList().Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

            if (!string.IsNullOrEmpty(searchName))
            {
                orderListVM.OrderHeaderList = orderListVM.OrderHeaderList.Where(u => u.FullName.ToLower().Contains(searchName.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(searchEmail))
            {
                orderListVM.OrderHeaderList = orderListVM.OrderHeaderList.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(searchPhone))
            {
                orderListVM.OrderHeaderList = orderListVM.OrderHeaderList.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(Status) && Status!= "--Order Status--")
            {
                orderListVM.OrderHeaderList = orderListVM.OrderHeaderList.Where(u => u.OrderStatus.ToLower().Contains(Status.ToLower())).ToList();
            }

            return View(orderListVM);
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == id),
                OrderDetail = _orderDetailRepository.GetAll(o => o.OrderHeaderId == id, includeProperties: "Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        public IActionResult StartProcessing()
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusInProcess;
            _orderHeaderRepository.Save();
            TempData[WC.Success] = "Order is In Process";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            _orderHeaderRepository.Save();
            TempData[WC.Success] = "Order Shipped Successfully"; 

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            _brainTreeSettings = _configuration.GetSection("BrainTree").Get<BrainTreeSettings>();
            var gateway = new BraintreeGateway(_brainTreeSettings.Environment, _brainTreeSettings.MerchantId, _brainTreeSettings.PublicKey, _brainTreeSettings.PrivateKey);
            Transaction transaction = gateway.Transaction.Find(orderHeader.TransactionId);
            if(transaction.Status == TransactionStatus.AUTHORIZED || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                //no refund
                Result<Transaction> resultNoRefund = gateway.Transaction.Void(orderHeader.TransactionId);
            }
            else
            {
                //refund
                Result<Transaction> resultRefund = gateway.Transaction.Refund(orderHeader.TransactionId);
            }
            

            orderHeader.OrderStatus = WC.StatusRefunded;
            _orderHeaderRepository.Save();
            TempData[WC.Success] = "Order Cancelled Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateOrderDetails()
        {
            OrderHeader orderHeaderFromDb = _orderHeaderRepository.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.FullName = OrderVM.OrderHeader.FullName;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.Email = OrderVM.OrderHeader.Email;

            _orderHeaderRepository.Save();
            TempData[WC.Success] = "Order Details Updated Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
