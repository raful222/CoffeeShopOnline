using CoffeeShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace CoffeeShopOnline.Controllers
{ 
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        public ActionResult Index()
        {
            return View();

        }
        [HttpGet]
        public ActionResult OnlineOrder()
        {
            return View();

        }
       

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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TableInfo(People p)
        {
            if (!ModelState.IsValid)
            {
                return View("OnlineOrder", p);
            }

            Session["count"] = p.NumberOfClient;
            Session["Party"] = p.ClosedParty;
            return RedirectToAction("TableInfo");
        }

        [HttpGet]
        public ActionResult TableInfo()
        {
            if (Session["count"] == null)
            {
                return RedirectToAction("OnlineOrder");
            }

            var tables = db.RoomTables.ToList();
            var latestOrders = db.Orders
                .GroupBy(order => order.TableNumber)
                .Select(group => new { TableNumber = group.Key, EndTime = group.Max(order => order.TableSitTimeEnd) })
                .ToList();
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
            if (availabilityChanged)
            {
                db.SaveChanges();
            }

            return View(tables);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckTable(string mine)
        {
            int tableId;
            if (!int.TryParse(mine, out tableId) || Session["count"] == null)
            {
                return RedirectToAction("OnlineOrder");
            }

            RoomTable objItem = db.RoomTables.SingleOrDefault(model => model.Id == tableId);
            if (objItem == null || objItem.Available)
            {
                TempData["Message"] = "That table is no longer available. Please choose another one.";
                return RedirectToAction("TableInfo");
            }

            int number = (int)Session["count"];
            if (number <= objItem.TableSits)
            {
                Session["table"] = mine;
                return RedirectToAction("Index", "Shopping");
            }
            TempData["Message"] = "You Cant Sit at table that too small,Please Choose table again";

            return RedirectToAction("TableInfo");

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
