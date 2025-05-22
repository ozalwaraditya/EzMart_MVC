using EzMart.Repository.IRepository;
using EzMart.Utilities;
using EzMart.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.CodeAnalysis;

namespace EzMartWeb.Areas.Customer.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        private OrderViewModel orderViewModel { get; set; } 

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            orderViewModel = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

                return View(orderViewModel);
        }

        [HttpPost]
        [Authorize (Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrdersDetail(OrderViewModel orderViewModel)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderViewModel.OrderHeader.Id);

            orderHeaderFromDb.Name = orderViewModel.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderViewModel.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderViewModel.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderViewModel.OrderHeader.City;
            orderHeaderFromDb.State = orderViewModel.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderViewModel.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderViewModel.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderViewModel.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderViewModel.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Details Update Successfully";

            return RedirectToAction("Details",new {orderId = orderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(OrderViewModel orderViewModel)
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderViewModel.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Update Successfully";

            return View("Details", new { orderId = orderViewModel.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder(OrderViewModel orderViewModel)
        {

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderViewModel.OrderHeader.Id);
            orderHeader.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderViewModel.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShipppingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.OrderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(OrderViewModel orderViewModel)
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderViewModel.OrderHeader.Id, SD.StatusCancelled);
            _unitOfWork.Save();
            TempData["success"] = "Order Cancel Successfully";

            return RedirectToAction("Details", new { orderId = orderViewModel.OrderHeader.Id });
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            var objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");


            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = _unitOfWork.OrderHeader
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = objOrderHeaders });
        }

        #endregion
    }
}
