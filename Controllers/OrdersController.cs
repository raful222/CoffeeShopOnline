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

namespace CoffeeShopOnline.Controllers
{
    [Authorize(Roles = "Admin,Baristas")]
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Orders
        public async Task<ActionResult> Index()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var historyStart = today.AddDays(-1);
            var orders = await db.Orders
                .Include(order => order.OrderDetails)
                .Where(order => order.OrderDate >= historyStart || order.TableSitTimeEnd > now)
                .OrderBy(order => order.IsAprroved)
                .ThenBy(order => order.TableSitTimeEnd)
                .ToListAsync();

            var itemNames = (await db.Items.Select(item => new { item.ItemId, item.ItemName }).ToListAsync())
                .ToDictionary(item => item.ItemId.ToString(), item => item.ItemName, StringComparer.OrdinalIgnoreCase);

            var dashboardOrders = orders.Select(order => new OrderDashboardItemViewModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TableSitTimeEnd = order.TableSitTimeEnd,
                TableNumber = order.TableNumber,
                NumberOfDiners = order.NumberOfDiners,
                IsApproved = order.IsAprroved,
                IsCompleted = order.TableSitTimeEnd <= now,
                Total = order.OrderDetails.Sum(line => line.Total),
                ItemCount = order.OrderDetails.Sum(line => line.Quantity),
                Lines = order.OrderDetails.Select(line => new OrderDashboardLineViewModel
                {
                    ItemName = itemNames.ContainsKey(line.ItemId) ? itemNames[line.ItemId] : "פריט שאינו במלאי",
                    Quantity = line.Quantity,
                    Total = line.Total
                }).ToList()
            }).ToList();

            var model = new OrderDashboardViewModel
            {
                Orders = dashboardOrders,
                PendingCount = dashboardOrders.Count(order => order.StatusKey == "pending"),
                ActiveCount = dashboardOrders.Count(order => order.StatusKey == "active"),
                CompletedCount = dashboardOrders.Count(order => order.StatusKey == "completed"),
                TodayRevenue = dashboardOrders.Where(order => order.OrderDate >= today).Sum(order => order.Total),
                UpdatedAt = now
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(int id)
        {
            var order = await db.Orders.SingleOrDefaultAsync(candidate => candidate.OrderId == id);
            if (order == null)
                return HttpNotFound();

            if (order.TableSitTimeEnd <= DateTime.Now)
            {
                TempData["OrderMessage"] = "לא ניתן לאשר הזמנה שזמן הישיבה שלה הסתיים.";
                TempData["OrderMessageType"] = "warning";
                return RedirectToAction("Index");
            }

            order.IsAprroved = true;
            await db.SaveChangesAsync();
            TempData["OrderMessage"] = "ההזמנה אושרה ונכנסה לטיפול.";
            TempData["OrderMessageType"] = "success";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Complete(int id)
        {
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var order = await db.Orders.SingleOrDefaultAsync(candidate => candidate.OrderId == id);
                if (order == null)
                    return HttpNotFound();

                var now = DateTime.Now;
                order.TableSitTimeEnd = now;

                var hasAnotherActiveOrder = await db.Orders.AnyAsync(candidate =>
                    candidate.OrderId != order.OrderId &&
                    candidate.TableNumber == order.TableNumber &&
                    candidate.TableSitTimeEnd > now);
                if (!hasAnotherActiveOrder)
                {
                    var table = await db.RoomTables.SingleOrDefaultAsync(candidate => candidate.TableNumber == order.TableNumber);
                    if (table != null)
                        table.Available = false;
                }

                await db.SaveChangesAsync();
                transaction.Commit();
            }

            TempData["OrderMessage"] = "השירות הסתיים והשולחן שוחרר להזמנה הבאה.";
            TempData["OrderMessageType"] = "success";
            return RedirectToAction("Index");
        }

        // GET: Orders/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([Bind(Include = "OrderId,OrderDate,OrderNumber,TableSitTimeEnd,TableNumber,NumberOfDiners,IsAprroved")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // GET: Orders/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "OrderId,OrderDate,OrderNumber,TableSitTimeEnd,TableNumber,NumberOfDiners,IsAprroved")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Order order = await db.Orders.FindAsync(id);
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult BaristaOrder()
        {
            return View();
        }
    }
}
