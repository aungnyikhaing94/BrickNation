using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_Models;
using MyProj_Models.ViewModels;
using MyProj_Utility;

namespace MyProj.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepository _inquiryHeaderRepository;
        private readonly IInquiryDetailRepository _inquiryDetailRepository;

        [BindProperty]
        public InquiryVM InquiryVM { get; set; }

        public InquiryController(IInquiryHeaderRepository inquiryHeaderRepository, IInquiryDetailRepository inquiryDetailRepository)
        {
            _inquiryDetailRepository = inquiryDetailRepository;
            _inquiryHeaderRepository = inquiryHeaderRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            InquiryVM = new InquiryVM()
            {
                InquiryHeader = _inquiryHeaderRepository.FirstOrDefault(x => x.Id == id),
                InquiryDetail = _inquiryDetailRepository.GetAll(x => x.InquiryHeaderId == id, includeProperties: "Product")
            };

            return View(InquiryVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            InquiryVM.InquiryDetail = _inquiryDetailRepository.GetAll(x => x.InquiryHeaderId == InquiryVM.InquiryHeader.Id);

            foreach(var detail in InquiryVM.InquiryDetail)
            {
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    ProductId = detail.ProductId,
                };

                shoppingCartList.Add(shoppingCart);
            }

            HttpContext.Session.Clear();
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            HttpContext.Session.Set(WC.SessionInquiryId, InquiryVM.InquiryHeader.Id);            

            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult Delete()
        {
            InquiryHeader inquiryHeader = _inquiryHeaderRepository.FirstOrDefault(x => x.Id == InquiryVM.InquiryHeader.Id);
            IEnumerable<InquiryDetail> inquiryDetail = _inquiryDetailRepository.GetAll(x => x.InquiryHeaderId == InquiryVM.InquiryHeader.Id);

            _inquiryDetailRepository.RemoveRange(inquiryDetail);
            _inquiryHeaderRepository.Remove(inquiryHeader);
            _inquiryHeaderRepository.Save();
            TempData[WC.Success] = "Action completed successfully.";

            return RedirectToAction(nameof(Index));
        }

        #region API CALL

        public IActionResult GetInquiryList()
        {
            return Json(new { data = _inquiryHeaderRepository.GetAll() });
        }

        #endregion
    }
}
