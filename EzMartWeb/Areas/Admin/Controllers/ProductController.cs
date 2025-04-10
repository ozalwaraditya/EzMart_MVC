using EzMart.Models;
using EzMart.Models.ViewModels;
using EzMart.Repository;
using EzMart.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace EzMartWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(products);
        }

        //Update + Insert
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(i =>
                new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }
            );
            ProductViewModel ProductVM = new ProductViewModel
            {
                CategoryList = CategoryList,
                Product = new Product()
            };

            if(id == null || id == 0)
            {
                return View(ProductVM);
            }
            else
            {
                ProductVM.Product = _unitOfWork.Product.Get(u=>u.Id == id);

                return View(ProductVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel model, IFormFile? file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images/product");

                if (!Directory.Exists(productPath))
                {
                    Directory.CreateDirectory(productPath);
                }

                if (!string.IsNullOrEmpty(model.Product.ImgUrl))
                {
                    // Delete old image if it was uploaded previously
                    var oldImagePath = Path.Combine(wwwRootPath, model.Product.ImgUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                model.Product.ImgUrl = @"\images\product\" + fileName;
            }

            // No file uploaded, check if the user provided a URL directly
            // We'll accept the ImgUrl as-is (already bound to model.Product.ImgUrl)

            if (model.Product.Id == 0)
            {
                _unitOfWork.Product.Add(model.Product);
                TempData["success"] = "Product added successfully!";
            }
            else
            {
                _unitOfWork.Product.Update(model.Product);
                TempData["success"] = "Product updated successfully!";
            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }



       #region API CALLS

        [HttpGet]
        public IActionResult GetALl()
        {
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = products });
        }

        public IActionResult Delete(int id)
        {
            var productInDb = _unitOfWork.Product.Get(p => p.Id == id);

            if (productInDb == null)
            {
                return Json(new { success = false, message = "Error while deleting product." });
            }

            if (!string.IsNullOrEmpty(productInDb.ImgUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, productInDb.ImgUrl.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _unitOfWork.Product.Remove(productInDb);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleted successfully." });
        }



        #endregion
    }
}
