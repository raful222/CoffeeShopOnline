using CoffeeShopOnline.Models;
using CoffeeShopOnline.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static CoffeeShopOnline.Models.BaristaOrderViewModel;

namespace CoffeeShopOnline.Controllers
{
    [Authorize(Roles = "Admin,Baristas")]
    public class BaristaController : Controller
    {
        private const string CartKey = "Barista.Cart";
        private const string DinerCountKey = "Barista.DinerCount";
        private const string ClosedPartyKey = "Barista.ClosedParty";
        private const string CustomerNameKey = "Barista.CustomerName";
        private const string TableKey = "Barista.TableId";
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index() { return RedirectToAction("BaristaOrder"); }

        public ActionResult BaristaOrder()
        {
            return View(new WelcomeBaristaViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Table(WelcomeBaristaViewModel model)
        {
            if (!ModelState.IsValid) return View("BaristaOrder", model);

            Session[CustomerNameKey] = string.IsNullOrWhiteSpace(model.UserName) ? "לקוח מזדמן" : model.UserName.Trim();
            Session[DinerCountKey] = model.NumberOfClient;
            Session[ClosedPartyKey] = model.ClosedParty;
            Session.Remove(TableKey);
            ClearCart();
            ViewBag.GuestCount = model.NumberOfClient;
            return View(GetTablesWithCurrentAvailability());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CheckTable(string mine)
        {
            int tableId;
            int dinerCount;
            if (!int.TryParse(mine, out tableId) || !TryGetDinerCount(out dinerCount))
                return RedirectToAction("BaristaOrder");

            var roomTable = db.RoomTables.SingleOrDefault(table => table.Id == tableId);
            if (roomTable == null || roomTable.Available)
            {
                TempData["Message"] = "השולחן כבר אינו זמין. בחרו שולחן אחר.";
                ViewBag.GuestCount = dinerCount;
                return View("Table", GetTablesWithCurrentAvailability());
            }
            if (dinerCount > roomTable.TableSits)
            {
                TempData["Message"] = "השולחן קטן מדי למספר האורחים.";
                ViewBag.GuestCount = dinerCount;
                return View("Table", GetTablesWithCurrentAvailability());
            }

            Session[TableKey] = tableId;
            return RedirectToAction("Shopping");
        }

        public ActionResult Shopping()
        {
            int tableId;
            int dinerCount;
            if (!TryGetTableId(out tableId) || !TryGetDinerCount(out dinerCount))
                return RedirectToAction("BaristaOrder");

            var roomTable = db.RoomTables.SingleOrDefault(table => table.Id == tableId);
            if (roomTable == null || roomTable.Available)
            {
                TempData["Message"] = "יש לבחור שולחן זמין לפני פתיחת התפריט.";
                return RedirectToAction("BaristaOrder");
            }

            ViewBag.TableNumber = roomTable.TableNumber;
            ViewBag.DinerCount = dinerCount;
            ViewBag.CustomerName = Session[CustomerNameKey] as string;
            ViewBag.CartCount = CartQuantity(GetCart());
            var closedParty = Session[ClosedPartyKey] != null && Convert.ToBoolean(Session[ClosedPartyKey]);
            var menu = (from item in db.Items
                        join category in db.Categories on item.CatogoryId equals category.CategoryId
                        select new ShoppingViewModel
                        {
                            ImagePath = item.ImagePath, ItemName = item.ItemName, ItemId = item.ItemId,
                            ItemPrice = item.ItemPrice, Description = item.Decription, Category = category.CetegoryName,
                            Popular = item.popular, Quantity = item.Quantity, Promo = item.Promo,
                            PromoPrice = item.PromoPrice, PruductOfDay = item.PruductOfDay,
                            InOrOutTable = roomTable.InOrOut, ClosedParty = closedParty
                        }).ToList();
            return View(menu);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Shopping(string itemId)
        {
            Guid parsedItemId;
            if (!Guid.TryParse(itemId, out parsedItemId))
                return Json(new { Success = false, Message = "הפריט שנבחר אינו תקין.", Counter = CartQuantity(GetCart()) });

            var product = db.Items.SingleOrDefault(item => item.ItemId == parsedItemId);
            if (product == null || product.Quantity <= 0)
                return Json(new { Success = false, Message = "הפריט אינו זמין כרגע.", Counter = CartQuantity(GetCart()) });

            var cart = GetCart();
            var cartItem = cart.SingleOrDefault(item => item.ItemId == itemId);
            if (cartItem != null)
            {
                if (cartItem.Quantity >= product.Quantity)
                    return Json(new { Success = false, Message = "אין כמות נוספת במלאי עבור " + product.ItemName + ".", Counter = CartQuantity(cart) });
                cartItem.Quantity++;
                cartItem.Total = cartItem.Quantity * cartItem.UnitPrice;
                cartItem.TotalQuantity = product.Quantity - (int)cartItem.Quantity;
            }
            else
            {
                var unitPrice = IsPromotionActive(product) ? product.PromoPrice : product.ItemPrice;
                cart.Add(new ShoppingCartModel
                {
                    ItemId = itemId, ImagePath = product.ImagePath, ItemName = product.ItemName,
                    Quantity = 1, UnitPrice = unitPrice, Total = unitPrice,
                    Category = product.CatogoryId, TotalQuantity = product.Quantity - 1
                });
            }
            SaveCart(cart);
            return Json(new { Success = true, Counter = CartQuantity(cart) });
        }

        public ActionResult BaristaCart()
        {
            ViewBag.TableNumber = GetSelectedTableNumber();
            ViewBag.CustomerName = Session[CustomerNameKey] as string;
            return View(GetCart());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RemoveFromBaristaCart(string itemId)
        {
            var cart = GetCart();
            cart.RemoveAll(item => item.ItemId == itemId);
            SaveCart(cart);
            return RedirectToAction("BaristaCart");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddOrderBarista()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                TempData["NoItem"] = "הסל ריק. הוסיפו פריטים לפני שליחת ההזמנה.";
                return RedirectToAction("Shopping");
            }

            int tableId;
            int dinerCount;
            if (!TryGetTableId(out tableId) || !TryGetDinerCount(out dinerCount))
                return RedirectToAction("BaristaOrder");

            var placement = new OrderPlacementService(db).PlaceOrder(new OrderPlacementRequest
            {
                Cart = cart, TableId = tableId, DinerCount = dinerCount,
                UserName = Session[CustomerNameKey] as string, IsApproved = true
            });
            if (!placement.Success)
            {
                TempData[placement.RequiresNewTable ? "SitIstaken" : "NoItem"] = placement.Message;
                return RedirectToAction(placement.RequiresNewTable ? "BaristaOrder" : "BaristaCart");
            }

            ClearOrderContext();
            TempData["Message"] = "ההזמנה נשלחה למטבח ואושרה בהצלחה.";
            return RedirectToAction("BaristaOrder");
        }

        private List<RoomTable> GetTablesWithCurrentAvailability()
        {
            var tables = db.RoomTables.ToList();
            var latestOrders = db.Orders.GroupBy(order => order.TableNumber)
                .Select(group => new { TableNumber = group.Key, EndTime = group.Max(order => order.TableSitTimeEnd) }).ToList();
            var changed = false;
            foreach (var table in tables.Where(table => table.Available))
            {
                var latestOrder = latestOrders.SingleOrDefault(order => order.TableNumber == table.TableNumber);
                if (latestOrder != null && DateTime.Now > latestOrder.EndTime)
                {
                    table.Available = false;
                    changed = true;
                }
            }
            if (changed) db.SaveChanges();
            return tables;
        }

        private List<ShoppingCartModel> GetCart()
        {
            return Session[CartKey] as List<ShoppingCartModel> ?? new List<ShoppingCartModel>();
        }

        private void SaveCart(List<ShoppingCartModel> cart) { Session[CartKey] = cart; }
        private void ClearCart() { Session.Remove(CartKey); }

        private void ClearOrderContext()
        {
            ClearCart();
            Session.Remove(DinerCountKey);
            Session.Remove(ClosedPartyKey);
            Session.Remove(CustomerNameKey);
            Session.Remove(TableKey);
        }

        private bool TryGetDinerCount(out int value)
        {
            value = 0;
            return Session[DinerCountKey] != null && int.TryParse(Session[DinerCountKey].ToString(), out value);
        }

        private bool TryGetTableId(out int value)
        {
            value = 0;
            return Session[TableKey] != null && int.TryParse(Session[TableKey].ToString(), out value);
        }

        private int? GetSelectedTableNumber()
        {
            int tableId;
            if (!TryGetTableId(out tableId)) return null;
            return db.RoomTables.Where(table => table.Id == tableId).Select(table => (int?)table.TableNumber).SingleOrDefault();
        }

        private static int CartQuantity(IEnumerable<ShoppingCartModel> cart) { return (int)cart.Sum(item => item.Quantity); }
        private static bool IsPromotionActive(Item item) { return item.Promo && DateTime.Now.Hour > 12 && DateTime.Now.Hour < 17; }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
