using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyProj_DataAccess;
using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_Models;
using MyProj_Models.ViewModels;
using MyProj_Utility;

namespace MyProj.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository productRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        { 
            IEnumerable<Product> objList = _productRepository.GetAll(includeProperties: "Category,ApplicationType");

            //foreach (var obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
            //};

            return View(objList);
        }

        //GET - UPSERT
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _productRepository.GetAllDropDownList(WC.CategoryName),
                ApplicationTypeSelectList = _productRepository.GetAllDropDownList(WC.ApplicationTypeName)
            };

            if (id == null)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _productRepository.Find(id.GetValueOrDefault());
                if(productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        //POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            //foreach (var key in ModelState.Keys)
            //{
            //    var state = ModelState[key];
            //    if (state.Errors.Count > 0)
            //    {
            //        var errorMessage = state.Errors.First().ErrorMessage;
            //        Console.WriteLine(errorMessage);
            //    }
            //}

            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if(productVM.Product.Id == 0)
                {
                    //creating
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using(var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;

                    _productRepository.Add(productVM.Product);
                }
                else
                {
                    //editing
                    var objFromDb = _productRepository.FirstOrDefault(u => u.Id == productVM.Product.Id, isTracking: false);

                    if(files.Count > 0)
                    {
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, objFromDb.Image);

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        productVM.Product.Image = fileName + extension;

                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }

                    _productRepository.Update(productVM.Product);
                }

                _productRepository.Save();
                TempData[WC.Success] = "Action completed successfully.";
                return RedirectToAction("Index");
            }

            productVM.CategorySelectList = _productRepository.GetAllDropDownList(WC.CategoryName);
            productVM.ApplicationTypeSelectList = _productRepository.GetAllDropDownList(WC.ApplicationTypeName);

            return View(productVM);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            
            Product product = _productRepository.FirstOrDefault(u => u.Id == id, "Category,ApplicationType");
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //POST - DELETE
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _productRepository.Find(id.GetValueOrDefault());

            if (obj == null)
            {
                return NotFound();
            }

            string webRootPath = _webHostEnvironment.WebRootPath;
            string upload = webRootPath + WC.ImagePath;     
            var oldFile = Path.Combine(upload, obj.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            _productRepository.Remove(obj);
            _productRepository.Save();
            TempData[WC.Success] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }

    }

}
