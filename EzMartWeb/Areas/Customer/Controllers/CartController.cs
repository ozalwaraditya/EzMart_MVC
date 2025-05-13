using EzMart.Models;
using EzMart.Models.ViewModels;
using EzMart.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EzMartWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartViewModel = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product")
            };


            foreach(var cart in ShoppingCartViewModel.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartViewModel.OrderTotal += (cart.Price * cart.Count);
            }            
            return View(ShoppingCartViewModel);
        }

        public IActionResult Plus(int cartId)
        {
            var cartInDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartInDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartInDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cartId)
        {
            var cartInDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if(cartInDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartInDb);
            }
            else
            {
                cartInDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartInDb);
            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Summary()
        {
            return View();
        }

        public IActionResult Remove(int cartId)
        {
            var cartInDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cartInDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }else if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}
