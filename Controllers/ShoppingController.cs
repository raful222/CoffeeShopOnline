using CoffeeShopOnline.Models;
using CoffeeShopOnline.Services;
using System;
using System.Collections.Generic;
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
            int tableId;
            if (Session["table"] == null || !int.TryParse(Session["table"].ToString(), out tableId))
            {
                TempData["Message"] = "בחרו שולחן לפני המעבר לתפריט.";
                return RedirectToAction("OnlineOrder", "Home");
            }

            var roomTable = db.RoomTables.SingleOrDefault(model => model.Id == tableId);
            if (roomTable == null)
            {
                Session.Remove("table");
                return RedirectToAction("OnlineOrder", "Home");
            }

            var closeParty = Session["Party"] != null && Convert.ToBoolean(Session["Party"]);
            var menu = (from item in db.Items
                        join category in db.Categories on item.CatogoryId equals category.CategoryId
                        select new ShoppingViewModel
                        {
                            ImagePath = item.ImagePath,
                            ItemName = item.ItemName,
                            ItemId = item.ItemId,
                            ItemPrice = item.ItemPrice,
                            Description = item.Decription,
                            ItemCode = item.ItemCode,
                            Category = category.CetegoryName,
                            Popular = item.popular,
                            Quantity = item.Quantity,
                            Promo = item.Promo,
                            PromoPrice = item.PromoPrice,
                            PruductOfDay = item.PruductOfDay,
                            InOrOutTable = roomTable.InOrOut,
                            ClosedParty = closeParty,
                            DishOfDay = false
                        }).ToList();

            return View(menu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Index(string itemId)
        {
            Guid parsedItemId;
            if (!Guid.TryParse(itemId, out parsedItemId))
            {
                return Json(new { Success = false, Message = "The selected item is invalid." });
            }

            var product = db.Items.SingleOrDefault(item => item.ItemId == parsedItemId);
            if (product == null || product.Quantity <= 0)
            {
                return Json(new { Success = false, Message = "This item is no longer available." });
            }

            var cart = GetCart();
            var cartItem = cart.SingleOrDefault(item => item.ItemId == itemId);
            if (cartItem != null)
            {
                if (cartItem.Quantity >= product.Quantity)
                {
                    return Json(new { Success = false, Message = "There is no more stock available for " + product.ItemName + ".", Counter = CartQuantity(cart) });
                }

                cartItem.Quantity++;
                cartItem.Total = cartItem.Quantity * cartItem.UnitPrice;
                cartItem.TotalQuantity = product.Quantity - (int)cartItem.Quantity;
            }
            else
            {
                if (product.CatogoryId == 3)
                {
                    var currentUser = User.Identity.IsAuthenticated
                        ? db.Users.SingleOrDefault(user => user.UserName == User.Identity.Name)
                        : null;
                    if (currentUser == null || currentUser.Age < 18)
                    {
                        return Json(new { Success = false, Message = "You must be signed in and at least 18 to order alcohol.", Counter = CartQuantity(cart) });
                    }
                }

                var unitPrice = IsPromotionActive(product) ? product.PromoPrice : product.ItemPrice;
                cart.Add(new ShoppingCartModel
                {
                    ItemId = itemId,
                    ImagePath = product.ImagePath,
                    ItemName = product.ItemName,
                    Quantity = 1,
                    UnitPrice = unitPrice,
                    Total = unitPrice,
                    Category = product.CatogoryId,
                    TotalQuantity = product.Quantity - 1
                });
            }

            SaveCart(cart);
            return Json(new { Success = true, Counter = CartQuantity(cart) });
        }

        public ActionResult Business_Lunch()
        {
            var menu = (from item in db.Items
                        join category in db.Categories on item.CatogoryId equals category.CategoryId
                        select new ShoppingViewModel
                        {
                            ImagePath = item.ImagePath,
                            ItemName = item.ItemName,
                            ItemId = item.ItemId,
                            ItemPrice = item.ItemPrice,
                            Description = item.Decription,
                            ItemCode = item.ItemCode,
                            Category = category.CetegoryName,
                            Popular = item.popular,
                            Quantity = item.Quantity,
                            Promo = item.Promo,
                            PromoPrice = item.PromoPrice
                        }).ToList();

            return View("business_lunch", menu);
        }

        public ActionResult ShoppingCart()
        {
            var cart = GetCart();
            foreach (var item in cart)
            {
                item.Total = item.Quantity * item.UnitPrice;
            }

            var user = User.Identity.IsAuthenticated
                ? db.Users.SingleOrDefault(candidate => candidate.UserName == User.Identity.Name)
                : null;
            if (user != null && user.stars >= 10)
            {
                var coffee = cart.FirstOrDefault(item => item.Category == 1);
                if (coffee != null)
                {
                    coffee.Total -= coffee.UnitPrice;
                    TempData["Message"] = "Your loyalty reward has been applied: one coffee is free.";
                }
            }

            SaveCart(cart);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddOrder()
        {
            var cart = GetCart();
            int tableId;
            int dinerCount;
            if (cart.Count == 0)
            {
                TempData["NoItem"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }
            if (Session["table"] == null || !int.TryParse(Session["table"].ToString(), out tableId) ||
                Session["count"] == null || !int.TryParse(Session["count"].ToString(), out dinerCount))
            {
                return RedirectToAction("OnlineOrder", "Home");
            }

            var placement = new OrderPlacementService(db).PlaceOrder(new OrderPlacementRequest
            {
                Cart = cart,
                TableId = tableId,
                DinerCount = dinerCount,
                UserName = User.Identity.IsAuthenticated ? User.Identity.Name : null,
                IsApproved = false
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

            Session.Remove("CartItem");
            Session.Remove("CartCounter");
            Session.Remove("TotalAmount");
            return RedirectToAction("OnlineOrder", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(string itemId)
        {
            var cart = GetCart();
            cart.RemoveAll(item => item.ItemId == itemId);
            SaveCart(cart);
            return RedirectToAction("ShoppingCart");
        }

        [Authorize(Roles = "Admin,Baristas")]
        public async Task<ActionResult> AproveOrder()
        {
            return View(await db.Orders.ToListAsync());
        }

        private List<ShoppingCartModel> GetCart()
        {
            return Session["CartItem"] as List<ShoppingCartModel> ?? new List<ShoppingCartModel>();
        }

        private void SaveCart(List<ShoppingCartModel> cart)
        {
            Session["CartItem"] = cart;
            Session["CartCounter"] = CartQuantity(cart);
            Session["TotalAmount"] = cart.Sum(item => item.Total);
        }

        private static int CartQuantity(IEnumerable<ShoppingCartModel> cart)
        {
            return (int)cart.Sum(item => item.Quantity);
        }

        private static bool IsPromotionActive(Item item)
        {
            return item.Promo && DateTime.Now.Hour > 12 && DateTime.Now.Hour < 17;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
