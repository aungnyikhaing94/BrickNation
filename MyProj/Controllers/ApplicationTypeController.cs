using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProj_DataAccess;
using MyProj_DataAccess.Data.Repository.IRepository;
using MyProj_Models;
using MyProj_Utility;

namespace MyProj.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ApplicationTypeController : Controller
    {        
        private readonly IApplicationTypeRepository _applicationTypeRepository;

        public ApplicationTypeController(IApplicationTypeRepository applicationTypeRepository)
        {
            _applicationTypeRepository = applicationTypeRepository;
        }

        public IActionResult Index()
        {
            IEnumerable<ApplicationType> objList = _applicationTypeRepository.GetAll();
            return View(objList);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationType obj)
        {
            if (ModelState.IsValid)
            {
                _applicationTypeRepository.Add(obj);
                _applicationTypeRepository.Save();
                TempData[WC.Success] = "Application type created successfully.";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET - EDIT
        public IActionResult Edit(int? Id)
        {
            if(Id == null || Id == 0)
            {
                return NotFound(); 
            }
            var obj = _applicationTypeRepository.Find(Id.GetValueOrDefault());
            if(obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ApplicationType obj)
        {
            if (ModelState.IsValid)
            {
                _applicationTypeRepository.Update(obj);
                _applicationTypeRepository.Save();
                TempData[WC.Success] = "Application type updated successfully.";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET - DELETE
        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var obj = _applicationTypeRepository.Find(Id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? Id)
        {
            var obj = _applicationTypeRepository.Find(Id.GetValueOrDefault());
            if(obj == null)
            {
                return NotFound();
            }
            _applicationTypeRepository.Remove(obj);
            _applicationTypeRepository.Save();
            TempData[WC.Success] = "Application type deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
