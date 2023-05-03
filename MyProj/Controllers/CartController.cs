using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MyProj_DataAccess;
using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_Models;
using MyProj_Models.ViewModels;
using MyProj_Utility;
using MyProj_Utility.BrainTree;
using System.Security.Claims;
using System.Text;

namespace MyProj.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IInquiryHeaderRepository _inquiryHeaderRepository;
        private readonly IInquiryDetailRepository _inquiryDetailRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IConfiguration _configuration;

        public BrainTreeSettings _brainTreeSettings { get; set; }
        //private readonly IBrainTreeGate _brainTreeGate;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(IProductRepository productRepository, IApplicationUserRepository applicationUserRepository,
            IInquiryHeaderRepository inquiryHeaderRepository, IInquiryDetailRepository inquiryDetailRepository,
            IOrderHeaderRepository orderHeaderRepository, IOrderDetailRepository orderDetailRepository,
            IWebHostEnvironment webHostEnvironment, IEmailSender emailSender, IConfiguration configuration)
        {
            _productRepository = productRepository;
            _applicationUserRepository = applicationUserRepository;
            _inquiryHeaderRepository = inquiryHeaderRepository;
            _inquiryDetailRepository = inquiryDetailRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _configuration = configuration;
            //_brainTreeGate = brainTreeGate;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodIdsInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _productRepository.GetAll(u => prodIdsInCart.Contains(u.Id));
            IList<Product> prodList = new List<Product>();

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(x => x.Id == cartObj.ProductId);
                prodTemp.TempSqft = cartObj.Sqft;
                prodList.Add(prodTemp);
            }

            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> productList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product product in productList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = product.Id, Sqft = product.TempSqft });
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            _brainTreeSettings = _configuration.GetSection("BrainTree").Get<BrainTreeSettings>();

            ApplicationUser applicationUser;

            if (User.IsInRole(WC.AdminRole))
            {
                if(HttpContext.Session.Get<int>(WC.SessionInquiryId) != 0)
                {
                    //cart has been loaded using an inquiry
                    InquiryHeader inquiryHeader = _inquiryHeaderRepository.FirstOrDefault(x => x.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));
                    applicationUser = new ApplicationUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.FullName,
                        PhoneNumber = inquiryHeader.PhoneNumber,
                    };
                }
                else
                {
                    applicationUser = new ApplicationUser();
                }
                //var gateway = _brainTreeGate.GetGateway();
                
                var gateway = new BraintreeGateway(_brainTreeSettings.Environment, _brainTreeSettings.MerchantId, _brainTreeSettings.PublicKey, _brainTreeSettings.PrivateKey);
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                applicationUser = _applicationUserRepository.FirstOrDefault(x => x.Id == claim.Value);
            }

            //var userId = User.FindFirstValue(ClaimTypes.Name);

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodIdsInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _productRepository.GetAll(u => prodIdsInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = applicationUser
            };

            foreach(var cartObj in shoppingCartList)
            {
                Product prodTemp = _productRepository.FirstOrDefault(x => x.Id == cartObj.ProductId);
                prodTemp.TempSqft = cartObj.Sqft;
                ProductUserVM.ProductList.Add(prodTemp);
            }

            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(IFormCollection collection)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(WC.AdminRole))
            {
                //we need to create an order
                //var orderTotal = 0.0;
                //foreach(Product prod in ProductUserVM.ProductList)
                //{
                //    orderTotal += prod.Price * prod.TempSqft;
                //}
                OrderHeader orderHeader = new OrderHeader()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = ProductUserVM.ProductList.Sum(x => x.Price * x.TempSqft),
                    City = ProductUserVM.ApplicationUser.City,
                    StreetAddress = ProductUserVM.ApplicationUser.StreetAddress,
                    State = ProductUserVM.ApplicationUser.State,
                    PostalCode = ProductUserVM.ApplicationUser.PostalCode,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    OrderDate = DateTime.Now,
                    OrderStatus = WC.StatusPending
                };

                _orderHeaderRepository.Add(orderHeader);
                _orderHeaderRepository.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        ProductId = prod.Id,
                        PricePerSqFt = prod.Price,
                        Sqft = prod.TempSqft
                    };
                    _orderDetailRepository.Add(orderDetail);
                }

                _orderDetailRepository.Save();

                string nonceFromTheClient = collection["payment_method_nonce"];

                var request = new TransactionRequest
                {
                    Amount = Convert.ToDecimal(orderHeader.FinalOrderTotal),
                    PaymentMethodNonce = nonceFromTheClient,
                    OrderId = orderHeader.Id.ToString(),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true,
                    }
                };

                _brainTreeSettings = _configuration.GetSection("BrainTree").Get<BrainTreeSettings>();
                var gateway = new BraintreeGateway(_brainTreeSettings.Environment, _brainTreeSettings.MerchantId, _brainTreeSettings.PublicKey, _brainTreeSettings.PrivateKey);
                Result<Transaction> result = gateway.Transaction.Sale(request);

                if(result.Target.ProcessorResponseText == "Approved")
                {
                    orderHeader.TransactionId = result.Target.Id;
                    orderHeader.OrderStatus = WC.StatusApproved;
                }
                else
                {
                    orderHeader.OrderStatus = WC.StatusCancelled;
                }

                _orderHeaderRepository.Save();

                return RedirectToAction(nameof(InquiryConfirmation), new {id=orderHeader.Id});
            }
            else
            {
                //we need to create an inquiry
                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "templates" + Path.DirectorySeparatorChar.ToString()
                + "Inquiry.html";

                var subject = "New Inquiry";
                var HtmlBody = "";

                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }

                StringBuilder productListSB = new StringBuilder();
                foreach (var prod in ProductUserVM.ProductList)
                {
                    productListSB.Append($" - Name: { prod.Name } <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
                }

                string messageBody = string.Format(HtmlBody,
                    ProductUserVM.ApplicationUser.FullName,
                    ProductUserVM.ApplicationUser.Email,
                    ProductUserVM.ApplicationUser.PhoneNumber,
                    productListSB.ToString());

                await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

                InquiryHeader inquiryHeader = new InquiryHeader()
                {
                    ApplicationUserId = claim.Value,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    InquiryDate = DateTime.Now,
                };

                _inquiryHeaderRepository.Add(inquiryHeader);
                _inquiryHeaderRepository.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    InquiryDetail inquiryDetail = new InquiryDetail()
                    {
                        InquiryHeaderId = inquiryHeader.Id,
                        ProductId = prod.Id,
                    };
                    _inquiryDetailRepository.Add(inquiryDetail);
                    _inquiryDetailRepository.Save();
                }
            }

            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation(int id=0)
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(x => x.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> productList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product product in productList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = product.Id, Sqft = product.TempSqft });
            }
            
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}
