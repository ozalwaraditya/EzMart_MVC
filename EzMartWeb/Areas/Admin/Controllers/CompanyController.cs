using EzMart.Models;
using EzMart.Models.ViewModels;
using EzMart.Repository;
using EzMart.Repository.IRepository;
using EzMart.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace EzMartWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> companies = _unitOfWork.Company.GetAll().ToList();
            return View(companies);
        }

        //Update + Insert
        public IActionResult Upsert(int? id)
        {
            if(id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company companyObj = _unitOfWork.Company.Get(u=>u.Id == id);

                return View(companyObj);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (companyObj.Id == 0)
            {
                _unitOfWork.Company.Add(companyObj);
                TempData["success"] = "Company added successfully!";
            }
            else
            {
                _unitOfWork.Company.Update(companyObj);
                TempData["success"] = "Company updated successfully!";
            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }



       #region API CALLS

        [HttpGet]
        public IActionResult GetALl()
        {
            var Companys = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = Companys });
        }

        public IActionResult Delete(int id)
        {
            var CompanyInDb = _unitOfWork.Company.Get(p => p.Id == id);

            if (CompanyInDb == null)
            {
                return Json(new { success = false, message = "Error while deleting Company." });
            }

            _unitOfWork.Company.Remove(CompanyInDb);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleted successfully." });
        }



        #endregion
    }
}
