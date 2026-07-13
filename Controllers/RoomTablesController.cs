using CoffeeShopOnline.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CoffeeShopOnline.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoomTablesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            var now = DateTime.Now;
            var tables = await db.RoomTables.OrderBy(table => table.TableNumber).ToListAsync();
            var activeOrders = (await db.Orders.Where(order => order.TableSitTimeEnd > now).ToListAsync())
                .GroupBy(order => order.TableNumber)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(order => order.TableSitTimeEnd).First());
            var usageCounts = await db.Orders.GroupBy(order => order.TableNumber)
                .Select(group => new { TableNumber = group.Key, Count = group.Count() })
                .ToDictionaryAsync(row => row.TableNumber, row => row.Count);

            var dashboardTables = tables.Select(table =>
            {
                Order activeOrder;
                activeOrders.TryGetValue(table.TableNumber, out activeOrder);
                return new RoomTableDashboardItemViewModel
                {
                    Id = table.Id,
                    TableNumber = table.TableNumber,
                    TableSeats = table.TableSits,
                    IsInside = table.InOrOut,
                    IsOccupied = activeOrder != null,
                    RequiresRelease = table.Available && activeOrder == null,
                    TimesUsed = usageCounts.ContainsKey(table.TableNumber) ? usageCounts[table.TableNumber] : 0,
                    ActiveDiners = activeOrder == null ? (int?)null : activeOrder.NumberOfDiners,
                    ActiveUntil = activeOrder == null ? (DateTime?)null : activeOrder.TableSitTimeEnd,
                    ActiveOrderNumber = activeOrder == null ? null : activeOrder.OrderNumber
                };
            }).ToList();

            return View(new RoomTableDashboardViewModel
            {
                Tables = dashboardTables,
                FreeCount = dashboardTables.Count(table => !table.IsOccupied && !table.RequiresRelease),
                OccupiedCount = dashboardTables.Count(table => table.IsOccupied),
                RequiresReleaseCount = dashboardTables.Count(table => table.RequiresRelease),
                InsideCount = dashboardTables.Count(table => table.IsInside),
                OutsideCount = dashboardTables.Count(table => !table.IsInside),
                TotalSeats = dashboardTables.Sum(table => table.TableSeats)
            });
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var table = await db.RoomTables.FindAsync(id);
            return table == null ? (ActionResult)HttpNotFound() : View(table);
        }

        public ActionResult Create()
        {
            return View(new RoomTableCreateViewModel { TableSeats = 2, IsInside = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RoomTableCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var baseNumber = model.IsInside ? 100 : 200;
                var lastNumber = await db.RoomTables
                    .Where(table => table.InOrOut == model.IsInside)
                    .OrderByDescending(table => table.TableNumber)
                    .Select(table => (int?)table.TableNumber)
                    .FirstOrDefaultAsync();
                var nextNumber = lastNumber.HasValue && lastNumber.Value >= baseNumber
                    ? lastNumber.Value + 1
                    : baseNumber;

                db.RoomTables.Add(new RoomTable
                {
                    TableNumber = nextNumber,
                    TableSits = model.TableSeats,
                    InOrOut = model.IsInside,
                    RoomType = model.IsInside ? "InSide" : "OutSide",
                    Available = false,
                    NumberOfTaken = 0
                });
                await db.SaveChangesAsync();
                transaction.Commit();
            }

            TempData["TableMessage"] = "השולחן נוסף למפת הישיבה בהצלחה.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var table = await db.RoomTables.FindAsync(id);
            if (table == null)
                return HttpNotFound();

            var now = DateTime.Now;
            var activeOrder = await db.Orders
                .Where(order => order.TableNumber == table.TableNumber && order.TableSitTimeEnd > now)
                .OrderByDescending(order => order.TableSitTimeEnd)
                .FirstOrDefaultAsync();
            return View(BuildEditModel(table, activeOrder));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RoomTableEditViewModel model)
        {
            RoomTable table;
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                table = await db.RoomTables.SingleOrDefaultAsync(candidate => candidate.Id == model.Id);
                if (table == null)
                    return HttpNotFound();

                var now = DateTime.Now;
                var activeOrder = await db.Orders
                    .Where(order => order.TableNumber == table.TableNumber && order.TableSitTimeEnd > now)
                    .OrderByDescending(order => order.TableSitTimeEnd)
                    .FirstOrDefaultAsync();
                if (activeOrder != null && model.TableSeats < activeOrder.NumberOfDiners)
                    ModelState.AddModelError("TableSeats", "לא ניתן להקטין את השולחן מתחת למספר האורחים בהזמנה הפעילה.");

                if (!ModelState.IsValid)
                {
                    var invalidModel = BuildEditModel(table, activeOrder);
                    invalidModel.TableSeats = model.TableSeats;
                    return View(invalidModel);
                }

                table.TableSits = model.TableSeats;
                await db.SaveChangesAsync();
                transaction.Commit();
            }

            TempData["TableMessage"] = "קיבולת שולחן " + table.TableNumber + " עודכנה.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Release(int id)
        {
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var table = await db.RoomTables.SingleOrDefaultAsync(candidate => candidate.Id == id);
                if (table == null)
                    return HttpNotFound();

                var now = DateTime.Now;
                var hasActiveOrder = await db.Orders.AnyAsync(order =>
                    order.TableNumber == table.TableNumber && order.TableSitTimeEnd > now);
                if (hasActiveOrder)
                {
                    TempData["TableWarning"] = "לא ניתן לשחרר שולחן עם הזמנה פעילה. סיימו את השירות מלוח ההזמנות.";
                    return RedirectToAction("Index");
                }

                table.Available = false;
                await db.SaveChangesAsync();
                transaction.Commit();
            }

            TempData["TableMessage"] = "השולחן שוחרר וזמין שוב להזמנה.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var table = await db.RoomTables.FindAsync(id);
            return table == null ? (ActionResult)HttpNotFound() : View(table);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var table = await db.RoomTables.FindAsync(id);
                if (table == null)
                    return HttpNotFound();

                if (await db.Orders.AnyAsync(order => order.TableNumber == table.TableNumber))
                {
                    TempData["TableWarning"] = "לא ניתן למחוק שולחן שמופיע בהיסטוריית הזמנות. אפשר להשאיר אותו במערכת ולעדכן את הקיבולת בלבד.";
                    return RedirectToAction("Index");
                }

                db.RoomTables.Remove(table);
                await db.SaveChangesAsync();
                transaction.Commit();
            }

            TempData["TableMessage"] = "השולחן הוסר ממפת הישיבה.";
            return RedirectToAction("Index");
        }

        private static RoomTableEditViewModel BuildEditModel(RoomTable table, Order activeOrder)
        {
            return new RoomTableEditViewModel
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                TableSeats = table.TableSits,
                IsInside = table.InOrOut,
                IsOccupied = activeOrder != null,
                ActiveDiners = activeOrder == null ? (int?)null : activeOrder.NumberOfDiners
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
