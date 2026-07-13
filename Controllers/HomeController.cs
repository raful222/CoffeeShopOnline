using CoffeeShopOnline.Models;
using CoffeeShopOnline.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CoffeeShopOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index() { return View(); }
        [HttpGet] public ActionResult OnlineOrder() { return View(); }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult TableInfo(People people)
        {
            if (!ModelState.IsValid) return View("OnlineOrder", people);
            CartService().SetReservation(people.NumberOfClient, people.ClosedParty);
            return RedirectToAction("TableInfo");
        }

        [HttpGet]
        public ActionResult TableInfo()
        {
            var reservation = CartService().GetReservation();
            if (!reservation.DinerCount.HasValue) return RedirectToAction("OnlineOrder");
            ViewBag.GuestCount = reservation.DinerCount.Value;

            var tables = db.RoomTables.ToList();
            var latestOrders = db.Orders.GroupBy(order => order.TableNumber)
                .Select(group => new { TableNumber = group.Key, EndTime = group.Max(order => order.TableSitTimeEnd) }).ToList();
            var availabilityChanged = false;
            foreach (var table in tables.Where(table => table.Available))
            {
                var latestOrder = latestOrders.SingleOrDefault(order => order.TableNumber == table.TableNumber);
                if (latestOrder != null && DateTime.Now > latestOrder.EndTime)
                {
                    table.Available = false;
                    availabilityChanged = true;
                }
            }
            if (availabilityChanged) db.SaveChanges();
            return View(tables);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CheckTable(string mine)
        {
            int tableId;
            var cartService = CartService();
            var reservation = cartService.GetReservation();
            if (!int.TryParse(mine, out tableId) || !reservation.DinerCount.HasValue)
                return RedirectToAction("OnlineOrder");

            var roomTable = db.RoomTables.SingleOrDefault(model => model.Id == tableId);
            if (roomTable == null || roomTable.Available)
            {
                TempData["Message"] = "השולחן שבחרתם כבר אינו זמין. בחרו שולחן אחר ונמשיך מיד.";
                return RedirectToAction("TableInfo");
            }
            if (reservation.DinerCount.Value <= roomTable.TableSits)
            {
                cartService.SelectTable(tableId);
                return RedirectToAction("Index", "Shopping");
            }

            TempData["Message"] = "השולחן שבחרתם קטן מדי למספר האורחים. סימנו עבורכם את השולחנות המתאימים.";
            return RedirectToAction("TableInfo");
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
