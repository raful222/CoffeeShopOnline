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



        public ActionResult TableInfo(People p)
        {
            Session["count"] = p.NumberOfClient;
            Session["Party"] = p.ClosedParty;
            IEnumerable<RoomTable> ListRommTable = db.RoomTables.ToList() as IEnumerable<RoomTable>;
            List<Order> order = db.Orders.OrderBy(x => x.TableNumber).ToList();
            List<RoomTable> roomTable = db.RoomTables.ToList();
            int orderCount = order.Count;
            int roomTableCount = roomTable.Count;
            for (int i = 0; i < roomTableCount; i++)
            {
                for (int j = 1; j < orderCount; j++)
                {
                    if (roomTable[i].TableNumber == order[j-1].TableNumber &&
                        roomTable[i].TableNumber != order[j].TableNumber
                        )
                    {
                        if (DateTime.Now > order[j].TableSitTimeEnd)
                        roomTable[i].Available = false;
                    }
                }
            }
            return View(ListRommTable);
        }
        public ActionResult CheckTable(string mine)
        {
            RoomTable objItem = db.RoomTables.Single(model => model.Id.ToString() == mine);
            int number = (int)Session["count"];
            if (number <= objItem.TableSits)
            {
                Session["table"] = mine;
                return RedirectToAction("Index", "Shopping");
            }
            TempData["Message"] = "You Cant Sit at table that too small,Please Choose table again";

            return RedirectToAction("TableInfo");

        }
    }
}