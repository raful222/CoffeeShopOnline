using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CoffeeShopOnline.Models;
using System.IO;

namespace CoffeeShopOnline.Controllers
{
    public class ItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Items
        [CustomAuthorizeArtribute(Roles = "Admin,Baristas")]
        public async Task<ActionResult> Index()
        {
            return View(await db.Items.ToListAsync());
        }

        // GET: Items/Details/5
        [CustomAuthorizeArtribute(Roles = "Admin")]
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = await db.Items.FindAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: Items/Create
        [CustomAuthorizeArtribute(Roles = "Admin")]
        public ActionResult Create()
        {
            ItemViewModel objItemViewModel = new ItemViewModel();
            objItemViewModel.CategorySelectListItems = (from objCat in db.Categories
                                                        select new SelectListItem()
                                                        {
                                                            Text = objCat.CetegoryName,
                                                            Value = objCat.CategoryId.ToString(),
                                                            Selected = true
                                                        });
            return View(objItemViewModel);
        }

        [CustomAuthorizeArtribute(Roles = "Admin")]
        [HttpPost]
        public  JsonResult Create(ItemViewModel obItemViewModel)
        {
            string NewImage = Guid.NewGuid() + Path.GetExtension(obItemViewModel.ImagePath.FileName);
            obItemViewModel.ImagePath.SaveAs(filename: Server.MapPath("~/Images/" + NewImage));

            Item objItem = new Item();
            objItem.ImagePath = "~/Images/" + NewImage;
            objItem.CatogoryId = obItemViewModel.CategoryId;
            objItem.Decription = obItemViewModel.Description;
            objItem.ItemCode = obItemViewModel.ItemCode;
            objItem.ItemName = obItemViewModel.ItemName;
            objItem.ItemPrice = obItemViewModel.ItemPrice;
            objItem.ItemId = Guid.NewGuid();
            objItem.Quantity = obItemViewModel.Quantity;
            objItem.Promo = obItemViewModel.Promo;
            objItem.PromoPrice = obItemViewModel.PromoPrice;
            objItem.popular =0;
            db.Items.Add(objItem);
            db.SaveChanges();
            return Json(new { Success = true, Message = "Item is added successfully." }, JsonRequestBehavior.AllowGet);
        }

        // GET: Items/Edit/5
        [CustomAuthorizeArtribute(Roles = "Admin")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = await db.Items.FindAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        [CustomAuthorizeArtribute(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ItemId,CatogoryId,ItemCode,ItemName,Decription,Quantity,ImagePath,ItemPrice,Promo,PromoPrice,PruductOfDay,popular")] Item item)
        {
            if (ModelState.IsValid)
            {
                db.Entry(item).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        // GET: Items/Delete/5
        [CustomAuthorizeArtribute(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = await db.Items.FindAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Items/Delete/5
        [CustomAuthorizeArtribute(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Item item = await db.Items.FindAsync(id);
            db.Items.Remove(item);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [CustomAuthorizeArtribute(Roles = "Admin")]
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
