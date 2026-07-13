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
        [ValidateAntiForgeryToken]
        public  JsonResult Create(ItemViewModel obItemViewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { Success = false, Message = "בדקו את פרטי המנה ונסו שוב." });
            }
            if (obItemViewModel.ImagePath == null || obItemViewModel.ImagePath.ContentLength <= 0)
            {
                return Json(new { Success = false, Message = "יש לבחור תמונה למנה." });
            }
            if (obItemViewModel.ImagePath.ContentLength > 5 * 1024 * 1024)
            {
                return Json(new { Success = false, Message = "גודל התמונה יכול להיות עד 5MB." });
            }

            var extension = Path.GetExtension(obItemViewModel.ImagePath.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(extension) || !HasValidImageSignature(obItemViewModel.ImagePath))
            {
                return Json(new { Success = false, Message = "ניתן להעלות תמונת JPG, PNG או WebP תקינה בלבד." });
            }

            string NewImage = Guid.NewGuid() + extension;
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

        private static bool HasValidImageSignature(HttpPostedFileBase file)
        {
            var header = new byte[12];
            var stream = file.InputStream;
            var originalPosition = stream.CanSeek ? stream.Position : 0;
            var bytesRead = stream.Read(header, 0, header.Length);
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }

            var isJpeg = bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            var isPng = bytesRead >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                        header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
            var isWebP = bytesRead >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                         header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
            return isJpeg || isPng || isWebP;
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
