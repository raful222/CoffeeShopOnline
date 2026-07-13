using CoffeeShopOnline.Models;
using CoffeeShopOnline.Services;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static CoffeeShopOnline.Models.BaristaOrderViewModel;

namespace CoffeeShopOnline.Controllers
{
    [Authorize(Roles = "Admin,Baristas")]
    public class BaristaController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private decimal TotalAmount = 0.00M;
        private List<ShoppingCartModel> listOfShoppingCartModel =new List<ShoppingCartModel>();

        // GET: Barista
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult BaristaOrder()
        {
            return View();

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Table(WelcomeBaristaViewModel p)
        {
            if (!ModelState.IsValid)
            {
                return View("BaristaOrder", p);
            }
            Session["UserName"] = p.UserName;
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
                    if (roomTable[i].TableNumber == order[j - 1].TableNumber &&
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
                return RedirectToAction("Shopping", "Barista");
            }
            TempData["Message"] = "You Cant Sit at table that too small,Please Choose table again";

            return RedirectToAction("Table");
        }

            public ActionResult Shopping()
            {
                var closeParty = Convert.ToBoolean(Session["Party"]);
                var talbenumber = Convert.ToDecimal(Session["table"]);
                
                RoomTable roomTable = db.RoomTables.Single(model => model.Id == talbenumber);
                IEnumerable<ShoppingViewModel> listOfShoppingViewModels = (from objItem in db.Items
                                                                           join
                                                                           objCate in db.Categories
                                                                           on objItem.CatogoryId equals objCate.CategoryId
                                                                           select new ShoppingViewModel()
                                                                           {
                                                                               ImagePath = objItem.ImagePath,
                                                                               ItemName = objItem.ItemName,
                                                                               ItemId = objItem.ItemId,
                                                                               ItemPrice = objItem.ItemPrice,
                                                                               Description = objItem.Decription,
                                                                               Category = objCate.CetegoryName,
                                                                               Popular = objItem.popular,
                                                                               Quantity = objItem.Quantity,
                                                                               Promo = objItem.Promo,
                                                                               PromoPrice = objItem.PromoPrice,
                                                                               InOrOutTable = roomTable.InOrOut,
                                                                               ClosedParty = closeParty,
                                                                           }
                                                                          ).ToList();


                return View(listOfShoppingViewModels);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Shopping(string ItemId)
        {

            if (Session["TotalAmount"] != null)
                TotalAmount = (decimal)Session["TotalAmount"];
            ShoppingCartModel ob = new ShoppingCartModel();
            Item objItem = db.Items.FirstOrDefault(model => model.ItemId.ToString() == ItemId);
            if (Session["CartCounter"] != null)
            {
                listOfShoppingCartModel = Session["CartItem"] as List<ShoppingCartModel>;
            }
            if (listOfShoppingCartModel.Any(model => model.ItemId == ItemId))
            {

                ob = listOfShoppingCartModel.FirstOrDefault(model => model.ItemId == ItemId);
                if (ob.TotalQuantity > 0)
                {
                    ob.Quantity = ob.Quantity + 1;
                    ob.Total = ob.Quantity * ob.UnitPrice;
                    ob.TotalQuantity = (int)(objItem.Quantity - ob.Quantity);
                    TotalAmount += ob.Total;
                }
                else
                {
                    return Json(new { Success = false, Message = "No more " + objItem.ItemName + " item to add to cart,out of stack!", Counter = listOfShoppingCartModel.Count }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                    ob.ItemId = ItemId;
                    ob.ImagePath = objItem.ImagePath;
                    ob.ItemName = objItem.ItemName;
                    ob.Quantity = 1;
                    if (objItem.Promo && (@DateTime.Now.Hour < 17 && DateTime.Now.Hour > 12))
                    {
                        ob.UnitPrice = objItem.PromoPrice;
                        ob.Total = objItem.PromoPrice;
                        TotalAmount += ob.UnitPrice;
                    }
                    else
                    {
                        ob.Total = (decimal)objItem.ItemPrice;
                        ob.UnitPrice = (decimal)objItem.ItemPrice;
                        TotalAmount += ob.UnitPrice;

                    }
                    ob.Category = objItem.CatogoryId;
                    ob.TotalQuantity = objItem.Quantity - 1;

                    listOfShoppingCartModel.Add(ob);
                }

            Session["TotalAmount"] = TotalAmount;
            Session["CartCounter"] = listOfShoppingCartModel.Count;
            Session["CartItem"] = listOfShoppingCartModel;

            return Json(new { Success = true, Counter = listOfShoppingCartModel.Count }, JsonRequestBehavior.AllowGet);

        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddOrderBarista()
        {
            listOfShoppingCartModel = Session["CartItem"] as List<ShoppingCartModel>;
            if (listOfShoppingCartModel == null || listOfShoppingCartModel.Count == 0)
            {
                TempData["NoItem"] = "הסל ריק. הוסיפו פריטים לפני שליחת ההזמנה.";
                return RedirectToAction("Shopping");
            }
            int tableId;
            int dinerCount;
            if (Session["table"] == null || !int.TryParse(Session["table"].ToString(), out tableId) ||
                Session["count"] == null || !int.TryParse(Session["count"].ToString(), out dinerCount))
            {
                return RedirectToAction("BaristaOrder");
            }

            var placement = new OrderPlacementService(db).PlaceOrder(new OrderPlacementRequest
            {
                Cart = listOfShoppingCartModel,
                TableId = tableId,
                DinerCount = dinerCount,
                UserName = Session["Username"] == null ? null : Session["Username"].ToString(),
                IsApproved = true
            });
            if (!placement.Success)
            {
                if (placement.RequiresNewTable)
                {
                    TempData["SitIstaken"] = placement.Message;
                    return RedirectToAction("BaristaOrder");
                }
                TempData["NoItem"] = placement.Message;
                return RedirectToAction("Shopping");
            }

            Session.Remove("CartItem");
            Session.Remove("CartCounter");
            Session.Remove("TotalAmount");

            return RedirectToAction("BaristaOrder", "Barista");

        }
    }
}
