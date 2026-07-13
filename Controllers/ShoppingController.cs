using CoffeeShopOnline.Models;
using CoffeeShopOnline.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CoffeeShopOnline.Controllers
{
    public class ShoppingController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var reservation = CartService().GetReservation();
            if (!reservation.TableId.HasValue || !reservation.DinerCount.HasValue)
            {
                TempData["Message"] = "בחרו שולחן לפני המעבר לתפריט.";
                return RedirectToAction("OnlineOrder", "Home");
            }

            var roomTable = db.RoomTables.SingleOrDefault(model => model.Id == reservation.TableId.Value);
            if (roomTable == null)
            {
                return RedirectToAction("OnlineOrder", "Home");
            }

            var menu = (from item in db.Items
                        join category in db.Categories on item.CatogoryId equals category.CategoryId
                        select new ShoppingViewModel
                        {
                            ImagePath = item.ImagePath, ItemName = item.ItemName, ItemId = item.ItemId,
                            ItemPrice = item.ItemPrice, Description = item.Decription, ItemCode = item.ItemCode,
                            Category = category.CetegoryName, Popular = item.popular, Quantity = item.Quantity,
                            Promo = item.Promo, PromoPrice = item.PromoPrice, PruductOfDay = item.PruductOfDay,
                            InOrOutTable = roomTable.InOrOut, ClosedParty = reservation.ClosedParty, DishOfDay = false
                        }).ToList();
            return View(menu);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Index(string itemId)
        {
            Guid parsedItemId;
            if (!Guid.TryParse(itemId, out parsedItemId))
                return Json(new { Success = false, Message = "הפריט שנבחר אינו תקין." });

            var product = db.Items.SingleOrDefault(item => item.ItemId == parsedItemId);
            if (product == null || product.Quantity <= 0)
                return Json(new { Success = false, Message = "הפריט אינו זמין כרגע.", Counter = CartService().GetCount() });

            if (product.CatogoryId == 3)
            {
                var currentUser = User.Identity.IsAuthenticated
                    ? db.Users.SingleOrDefault(user => user.UserName == User.Identity.Name) : null;
                if (currentUser == null || currentUser.Age < 18)
                    return Json(new { Success = false, Message = "כדי להזמין אלכוהול יש להתחבר ולהיות מעל גיל 18.", Counter = CartService().GetCount() });
            }

            var result = CartService().AddItem(parsedItemId);
            return Json(new { result.Success, result.Message, Counter = result.Count });
        }

        public ActionResult Business_Lunch()
        {
            var menu = (from item in db.Items
                        join category in db.Categories on item.CatogoryId equals category.CategoryId
                        select new ShoppingViewModel
                        {
                            ImagePath = item.ImagePath, ItemName = item.ItemName, ItemId = item.ItemId,
                            ItemPrice = item.ItemPrice, Description = item.Decription, ItemCode = item.ItemCode,
                            Category = category.CetegoryName, Popular = item.popular, Quantity = item.Quantity,
                            Promo = item.Promo, PromoPrice = item.PromoPrice
                        }).ToList();
            return View("business_lunch", menu);
        }

        public ActionResult ShoppingCart()
        {
            var cart = CartService().GetCart();
            var user = User.Identity.IsAuthenticated
                ? db.Users.SingleOrDefault(candidate => candidate.UserName == User.Identity.Name) : null;
            if (user != null && user.stars >= 10)
            {
                var coffee = cart.FirstOrDefault(item => item.Category == 1);
                if (coffee != null)
                {
                    coffee.Total -= coffee.UnitPrice;
                    TempData["Message"] = "הטבת הנאמנות הופעלה: קפה אחד ללא עלות.";
                }
            }
            return View(cart);
        }

        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            return PartialView("_CartPartial", CartService().GetCount());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddOrder()
        {
            var cartService = CartService();
            var cart = cartService.GetCart();
            var reservation = cartService.GetReservation();
            if (cart.Count == 0)
            {
                TempData["NoItem"] = "הסל ריק. הוסיפו פריטים לפני שליחת ההזמנה.";
                return RedirectToAction("Index");
            }
            if (!reservation.TableId.HasValue || !reservation.DinerCount.HasValue)
                return RedirectToAction("OnlineOrder", "Home");

            var placement = new OrderPlacementService(db).PlaceOrder(new OrderPlacementRequest
            {
                Cart = cart, TableId = reservation.TableId.Value, DinerCount = reservation.DinerCount.Value,
                UserName = User.Identity.IsAuthenticated ? User.Identity.Name : null, IsApproved = false
            });
            if (!placement.Success)
            {
                if (placement.RequiresNewTable)
                {
                    TempData["SitIstaken"] = placement.Message;
                    return RedirectToAction("OnlineOrder", "Home");
                }
                TempData["NoItem"] = placement.Message;
                return RedirectToAction("ShoppingCart");
            }

            cartService.Clear();
            return RedirectToAction("OnlineOrder", "Home");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(string itemId)
        {
            Guid parsedItemId;
            if (Guid.TryParse(itemId, out parsedItemId)) CartService().RemoveItem(parsedItemId);
            return RedirectToAction("ShoppingCart");
        }

        [Authorize(Roles = "Admin,Baristas")]
        public async Task<ActionResult> AproveOrder()
        {
            return View(await db.Orders.ToListAsync());
        }

        private PersistentCartService CartService()
        {
            return new PersistentCartService(db, HttpContext, User.Identity.GetUserId());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
