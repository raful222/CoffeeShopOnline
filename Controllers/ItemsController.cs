using CoffeeShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CoffeeShopOnline.Controllers
{
    [Authorize(Roles = "Admin,Baristas")]
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            var items = await db.Items.OrderBy(item => item.ItemName).ToListAsync();
            var categoryNames = await db.Categories.ToDictionaryAsync(category => category.CategoryId, category => category.CetegoryName);
            var dashboardItems = items.Select(item => new InventoryItemViewModel
            {
                ItemId = item.ItemId,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                Description = item.Decription,
                ImagePath = item.ImagePath,
                CategoryName = categoryNames.ContainsKey(item.CatogoryId) && !string.IsNullOrWhiteSpace(categoryNames[item.CatogoryId])
                    ? categoryNames[item.CatogoryId]
                    : "ללא קטגוריה",
                ItemPrice = item.ItemPrice,
                PromoPrice = item.PromoPrice,
                Quantity = item.Quantity,
                Popularity = item.popular,
                Promo = item.Promo,
                ProductOfDay = item.PruductOfDay
            }).ToList();

            return View(new InventoryDashboardViewModel
            {
                Items = dashboardItems,
                Categories = dashboardItems.Select(item => item.CategoryName).Distinct().OrderBy(name => name).ToList(),
                LowStockCount = dashboardItems.Count(item => item.Quantity > 0 && item.Quantity <= 5),
                OutOfStockCount = dashboardItems.Count(item => item.Quantity == 0),
                PromotionCount = dashboardItems.Count(item => item.Promo && item.PromoPrice > 0 && item.PromoPrice < item.ItemPrice),
                TotalUnits = dashboardItems.Sum(item => item.Quantity)
            });
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = await db.Items.FindAsync(id);
            return item == null ? (ActionResult)HttpNotFound() : View(item);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create()
        {
            var model = new ItemViewModel();
            await PopulateCategories(model);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ItemViewModel model)
        {
            await ValidateEditor(model, true);
            if (!ModelState.IsValid)
            {
                await PopulateCategories(model);
                return View(model);
            }

            string imagePath = null;
            try
            {
                imagePath = SaveImage(model.ImagePath);
                db.Items.Add(new Item
                {
                    ItemId = Guid.NewGuid(),
                    CatogoryId = model.CategoryId,
                    ItemCode = Clean(model.ItemCode),
                    ItemName = Clean(model.ItemName),
                    Decription = Clean(model.Description),
                    ImagePath = imagePath,
                    ItemPrice = model.ItemPrice,
                    Quantity = model.Quantity,
                    Promo = model.Promo,
                    PromoPrice = model.Promo ? model.PromoPrice : 0,
                    PruductOfDay = model.ProductOfDay,
                    popular = 0
                });
                await db.SaveChangesAsync();
            }
            catch
            {
                TryDeleteImage(imagePath);
                throw;
            }

            TempData["InventoryMessage"] = "המוצר נוסף לתפריט בהצלחה.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = await db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();

            var model = new ItemViewModel
            {
                ItemId = item.ItemId,
                CategoryId = item.CatogoryId,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                Description = item.Decription,
                ItemPrice = item.ItemPrice,
                Quantity = item.Quantity,
                Promo = item.Promo,
                PromoPrice = item.PromoPrice,
                ProductOfDay = item.PruductOfDay,
                ExistingImagePath = item.ImagePath
            };
            await PopulateCategories(model);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ItemViewModel model)
        {
            var item = await db.Items.SingleOrDefaultAsync(candidate => candidate.ItemId == model.ItemId);
            if (item == null)
                return HttpNotFound();

            model.ExistingImagePath = item.ImagePath;
            await ValidateEditor(model, false);
            if (!ModelState.IsValid)
            {
                await PopulateCategories(model);
                return View(model);
            }

            string newImagePath = null;
            var oldImagePath = item.ImagePath;
            try
            {
                if (model.ImagePath != null && model.ImagePath.ContentLength > 0)
                {
                    newImagePath = SaveImage(model.ImagePath);
                    item.ImagePath = newImagePath;
                }

                item.CatogoryId = model.CategoryId;
                item.ItemCode = Clean(model.ItemCode);
                item.ItemName = Clean(model.ItemName);
                item.Decription = Clean(model.Description);
                item.ItemPrice = model.ItemPrice;
                item.Quantity = model.Quantity;
                item.Promo = model.Promo;
                item.PromoPrice = model.Promo ? model.PromoPrice : 0;
                item.PruductOfDay = model.ProductOfDay;
                await db.SaveChangesAsync();
            }
            catch
            {
                TryDeleteImage(newImagePath);
                throw;
            }

            if (newImagePath != null)
                TryDeleteImage(oldImagePath);

            TempData["InventoryMessage"] = "פרטי המוצר והמלאי עודכנו.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStock(Guid id, int quantity)
        {
            if (quantity < 0 || quantity > 100000)
            {
                TempData["InventoryWarning"] = "כמות המלאי אינה תקינה.";
                return RedirectToAction("Index");
            }

            var item = await db.Items.SingleOrDefaultAsync(candidate => candidate.ItemId == id);
            if (item == null)
                return HttpNotFound();

            item.Quantity = quantity;
            await db.SaveChangesAsync();
            TempData["InventoryMessage"] = "המלאי של " + item.ItemName + " עודכן.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = await db.Items.FindAsync(id);
            return item == null ? (ActionResult)HttpNotFound() : View(item);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            var item = await db.Items.FindAsync(id);
            if (item == null)
                return HttpNotFound();

            var itemId = item.ItemId.ToString();
            if (await db.OrderDetails.AnyAsync(line => line.ItemId == itemId))
            {
                TempData["InventoryWarning"] = "לא ניתן למחוק מוצר שמופיע בהיסטוריית הזמנות. אפשר להגדיר את המלאי שלו ל־0.";
                return RedirectToAction("Index");
            }

            var imagePath = item.ImagePath;
            db.Items.Remove(item);
            await db.SaveChangesAsync();
            TryDeleteImage(imagePath);
            TempData["InventoryMessage"] = "המוצר הוסר מהתפריט.";
            return RedirectToAction("Index");
        }

        private async Task ValidateEditor(ItemViewModel model, bool imageRequired)
        {
            if (!await db.Categories.AnyAsync(category => category.CategoryId == model.CategoryId))
                ModelState.AddModelError("CategoryId", "הקטגוריה שנבחרה אינה קיימת.");

            var cleanCode = Clean(model.ItemCode);
            if (!string.IsNullOrEmpty(cleanCode) && await db.Items.AnyAsync(item => item.ItemCode == cleanCode && item.ItemId != model.ItemId))
                ModelState.AddModelError("ItemCode", "קוד המוצר כבר נמצא בשימוש.");

            if (imageRequired && (model.ImagePath == null || model.ImagePath.ContentLength <= 0))
                ModelState.AddModelError("ImagePath", "יש לבחור תמונה למוצר.");

            if (model.ImagePath != null && model.ImagePath.ContentLength > 0)
            {
                if (model.ImagePath.ContentLength > 5 * 1024 * 1024)
                    ModelState.AddModelError("ImagePath", "גודל התמונה יכול להיות עד 5MB.");

                var extension = Path.GetExtension(model.ImagePath.FileName).ToLowerInvariant();
                if (!HasValidImageSignature(model.ImagePath, extension))
                    ModelState.AddModelError("ImagePath", "ניתן להעלות תמונת JPG, PNG או WebP תקינה בלבד.");
            }
        }

        private async Task PopulateCategories(ItemViewModel model)
        {
            model.CategorySelectListItems = await db.Categories
                .OrderBy(category => category.CetegoryName)
                .Select(category => new SelectListItem
                {
                    Text = category.CetegoryName,
                    Value = category.CategoryId.ToString()
                }).ToListAsync();
        }

        private string SaveImage(HttpPostedFileBase file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = Guid.NewGuid().ToString("N") + extension;
            var imageDirectory = Server.MapPath("~/Images");
            Directory.CreateDirectory(imageDirectory);
            file.SaveAs(Path.Combine(imageDirectory, fileName));
            return "~/Images/" + fileName;
        }

        private void TryDeleteImage(string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath) || !virtualPath.StartsWith("~/Images/", StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                var fullPath = Server.MapPath(virtualPath);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            catch (IOException)
            {
                // Image cleanup is best-effort and must not fail a completed database update.
            }
            catch (UnauthorizedAccessException)
            {
                // A deployment may expose read-only image storage; the product update still succeeds.
            }
        }

        private static bool HasValidImageSignature(HttpPostedFileBase file, string extension)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(extension))
                return false;

            var header = new byte[12];
            var stream = file.InputStream;
            var originalPosition = stream.CanSeek ? stream.Position : 0;
            var bytesRead = stream.Read(header, 0, header.Length);
            if (stream.CanSeek)
                stream.Position = originalPosition;

            var isJpeg = bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            var isPng = bytesRead >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                        header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
            var isWebP = bytesRead >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                         header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;

            return (extension == ".jpg" || extension == ".jpeg") ? isJpeg : extension == ".png" ? isPng : isWebP;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
