using CoffeeShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CoffeeShopOnline.Controllers
{
    public class ShoppingController : Controller
    {

        private ApplicationDbContext db;
        private List<ShoppingCartModel> listOfShoppingCartModel;
        private decimal TotalAmount = 0.00M;

        public ShoppingController()
        {
            db = new ApplicationDbContext();
            listOfShoppingCartModel = new List<ShoppingCartModel>();
        }
        // GET: Shopping
        public ActionResult Index()
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
                                                                           ItemCode = objItem.ItemCode,
                                                                           Category = objCate.CetegoryName,
                                                                           Popular=objItem.popular,
                                                                           Quantity=objItem.Quantity,
                                                                           Promo=objItem.Promo,
                                                                           PromoPrice=objItem.PromoPrice,
                                                                           PruductOfDay=objItem.PruductOfDay, 
                                                                           InOrOutTable=roomTable.InOrOut,
                                                                           ClosedParty = closeParty,
                                                                           DishOfDay=false
                                                                       }
                                                                      ).ToList();

            
                return View(listOfShoppingViewModels);
        }

        [HttpPost]
        public JsonResult Index(string ItemId)
        {
            if(Session["TotalAmount"]!=null)
                TotalAmount = (decimal)Session["TotalAmount"];
            ShoppingCartModel ob = new ShoppingCartModel();
            Item objItem = db.Items.Single(model => model.ItemId.ToString() == ItemId);
            if (Session["CartCounter"] != null)
            {
                listOfShoppingCartModel = Session["CartItem"] as List<ShoppingCartModel>;
            }
            if (listOfShoppingCartModel.Any(model => model.ItemId == ItemId))
            {

                ob = listOfShoppingCartModel.Single(model => model.ItemId == ItemId);
                if (ob.TotalQuantity > 0)
                {
                    ob.Quantity = ob.Quantity + 1;
                    ob.Total = ob.Quantity * ob.UnitPrice;
                    ob.TotalQuantity = (int)(objItem.Quantity - ob.Quantity);
                    TotalAmount+= ob.Total;
                }
                else
                {

                    return Json(new { Success = false, Message = "No more " + objItem.ItemName + " item to add to cart,out of stack!", Counter = listOfShoppingCartModel.Count }, JsonRequestBehavior.AllowGet);

                }

            }
            else
            {
                if (objItem.CatogoryId == 3)
                {
                    var nv = User.Identity.Name;
                    if (User.Identity.IsAuthenticated)
                    {
                        var update = (from c in db.Users
                                      where c.UserName == nv
                                      select c).SingleOrDefault();
                        if (update.Age >= 18)
                        {
                            ob.ItemId = ItemId;
                            ob.ImagePath = objItem.ImagePath;
                            ob.ItemName = objItem.ItemName;
                            ob.Quantity = 1;
                            ob.Total = (decimal)objItem.ItemPrice;
                            ob.UnitPrice = (decimal)objItem.ItemPrice;
                            ob.Category = objItem.CatogoryId;
                            ob.TotalQuantity = objItem.Quantity - 1;
                            TotalAmount += ob.UnitPrice;
                            listOfShoppingCartModel.Add(ob);
                        }
                        else
                        {
                            return Json(new { Success = false, Message = "cant buy אלכוהול when you not 18+", Counter = listOfShoppingCartModel.Count }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    else
                    {
                        return Json(new { Success = false, Message = "cant buy אלכוהול when you not login", Counter = listOfShoppingCartModel.Count }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ob.ItemId = ItemId;
                    ob.ImagePath = objItem.ImagePath;
                    ob.ItemName = objItem.ItemName;
                    ob.Quantity = 1;
                    if(objItem.Promo && (@DateTime.Now.Hour < 17 && DateTime.Now.Hour > 12))
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
            }

            Session["TotalAmount"] = TotalAmount;
            Session["CartCounter"] = listOfShoppingCartModel.Count;
            Session["CartItem"] = listOfShoppingCartModel;

            return Json(new { Success = true, Counter = listOfShoppingCartModel.Count }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult business_lunch()
        {
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
                                                                           ItemCode = objItem.ItemCode,
                                                                           Category = objCate.CetegoryName,
                                                                           Popular = objItem.popular,
                                                                           Quantity = objItem.Quantity,
                                                                           Promo = objItem.Promo,
                                                                           PromoPrice = objItem.PromoPrice

                                                                       }
                                                                                  ).ToList();

           
            return View(listOfShoppingViewModels);
        }

        public ActionResult ShoppingCart()
        {
            listOfShoppingCartModel = Session["CartItem"] as List<ShoppingCartModel>;
            var Username = Session["Username"];
            if (Username != null)
            {
                if (db.Users.Any(model => model.UserName == Username.ToString()))
                {
                    var update = (from c in db.Users
                                  where c.UserName == Username.ToString()
                                  select c).FirstOrDefault();
                    if (update.stars == 10)
                    {
                        foreach (var model in listOfShoppingCartModel)
                        {
                            if (model.Category == 1)
                            {
                                model.Total = model.Total - model.UnitPrice;
                                TempData["Message"] = "you have one coffee for free " + model.UnitPrice + "Off from the bill";
                                break;

                            }
                        }
                    }
                }
                else
                    return View(listOfShoppingCartModel);


            }
            else
            {
                var nva = User.Identity.Name;
                if (User.Identity.IsAuthenticated)
                {
                    var update = (from c in db.Users
                                  where c.UserName == nva
                                  select c).FirstOrDefault();

                    if (update.stars == 10)
                    {
                        foreach (var model in listOfShoppingCartModel)
                        {
                            if (model.Category == 1)
                            {
                                model.Total = model.Total - model.UnitPrice;
                                TempData["Message"] = "you have one coffee for free " + model.UnitPrice + "Off from the bill";
                                break;

                            }
                        }
                    }
                }

            }
            return View(listOfShoppingCartModel);
        }


        [HttpPost]
        public ActionResult AddOrder()
        {
            int OrderId = 0;
            listOfShoppingCartModel = Session["CartItem"] as List<ShoppingCartModel>;
            if (listOfShoppingCartModel.Count == 0)
            {
                TempData["NoItem"] = "you dont have any item at the cart ";
                return RedirectToAction("Index");


            }
            var talbenumber = Convert.ToDecimal(Session["table"]);
            int numOfDiners = (int)Session["count"];
            RoomTable roomTable = db.RoomTables.Single(model => model.Id == talbenumber);
            roomTable.Available = true;
            roomTable.NumberOfTaken++;
            Order orderObj = new Order()
            {
                NumberOfDiners = numOfDiners,
                TableNumber = roomTable.TableNumber,
                TableSitTimeEnd = DateTime.Now.AddHours(2),
                OrderDate = DateTime.Now,
                OrderNumber = String.Format("{0:ddmmyyyyHHmmsss}", DateTime.Now),
            };
            db.Orders.Add(orderObj);
            db.SaveChanges();
            OrderId = orderObj.OrderId;
           
            foreach (var item in listOfShoppingCartModel)
            {
                Item objItem = db.Items.Single(model => model.ItemId.ToString() == item.ItemId);

                    var nv = User.Identity.Name;
                    if (User.Identity.IsAuthenticated)
                    {
                        var update = (from c in db.Users
                                      where c.UserName == nv
                                      select c).SingleOrDefault();
                        if (item.Category== 1)
                        {
                            if (update.stars == 10)
                            {
                                item.UnitPrice = 0;
                                update.stars-=10;
                                TempData["Message"] = "you have one coffee for free ";
                            }
                        update.stars++;

                    }
                };

                    Guid customerProfileGuid = new Guid(item.ItemId);

                    var upd = (from c in db.Items
                               where c.ItemId == customerProfileGuid
                               select c).SingleOrDefault();
                    var q = item.Quantity;
                    upd.popular = (int)(upd.popular + q);
                    db.Entry(upd).State = EntityState.Modified;

                    objItem.Quantity -= (int)item.Quantity;
                    db.Entry(objItem).State = EntityState.Modified;
                    db.SaveChanges();

                    OrderDetail objOrderDetail = new OrderDetail();
                    objOrderDetail.Total = item.Total;
                    objOrderDetail.ItemId = item.ItemId;
                    objOrderDetail.OrderId = OrderId;
                    objOrderDetail.Quantity = item.Quantity;
                    objOrderDetail.UintPrice = item.UnitPrice;
                    db.OrderDetails.Add(objOrderDetail);
                    db.SaveChanges();

                
            }
            Session["CartItem"] = null;
            Session["CartCounter"] = null;

            return RedirectToAction("OnlineOrder","Home");

        }

        public ActionResult RemoveFromCart(string ItemId)
        {
            listOfShoppingCartModel = Session["CartItem"] as List<ShoppingCartModel>;
            foreach (var item in listOfShoppingCartModel.ToArray())
            {
                if (item.ItemId == ItemId)
                {
                    listOfShoppingCartModel.Remove(item);
                }
                Session["CartItem"] = listOfShoppingCartModel;

            }
            Session["CartItem"] = listOfShoppingCartModel;

            return Redirect("ShoppingCart");
        }


        public async Task<ActionResult> AproveOrder()
        {
            return View(await db.Orders.ToListAsync());

        }

        
        
    }
}