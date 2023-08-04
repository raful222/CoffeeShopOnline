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
    public class RoomTablesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();



        // GET: RoomTables
        [CustomAuthorizeArtribute(Roles = "Admin")]
        public async Task<ActionResult> Index()
        {
            return View(await db.RoomTables.ToListAsync());
        }

        // GET: RoomTables/Details/5
        [CustomAuthorizeArtribute(Roles = "Admin")]
        public async Task<ActionResult>Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RoomTable roomTable = await db.RoomTables.FindAsync(id);
            if (roomTable == null)
            {
                return HttpNotFound();
            }
            return View(roomTable);
        }

        // GET: RoomTables/Create
        [CustomAuthorizeArtribute(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuthorizeArtribute(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,TableNumber,TableSits,Available,InOrOut,roomType,SitArea_Id")] RoomTable roomTable)
        {
            var InNumber = 100;
            var OutNumber = 200;
            var NumberRoom = db.RoomTables.ToList();
            for (int i = 0; i < NumberRoom.Count(); i++)
            {
                if (NumberRoom[i].InOrOut == true)
                {
                    if (NumberRoom[i].TableNumber >= InNumber)
                    {
                        InNumber = NumberRoom[i].TableNumber + 1;
                    }
                }
                else
                {
                    if (NumberRoom[i].TableNumber >= OutNumber)
                    {
                        OutNumber = NumberRoom[i].TableNumber + 1;
                    }
                }
            }
            if (roomTable.InOrOut == true)
            {
                roomTable.TableNumber = InNumber;
                roomTable.RoomType = "InSide";
            }

            else
            {
                roomTable.TableNumber = OutNumber;
                roomTable.RoomType = "OutSide";
            }
            roomTable.Available = false;

            if (ModelState.IsValid)
            {
                db.RoomTables.Add(roomTable);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(roomTable);
        }

        // GET: RoomTables/Edit/5
        [CustomAuthorizeArtribute(Roles = "Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RoomTable roomTable = await db.RoomTables.FindAsync(id);
            if (roomTable == null)
            {
                return HttpNotFound();
            }
            return View(roomTable);
        }

        // POST: RoomTables/Edit/5
        [CustomAuthorizeArtribute(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,TableSits,Available,InOrOut,roomType,SitArea_Id")] RoomTable roomTable)
        {
           
            if (ModelState.IsValid)
            {
                db.Entry(roomTable).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(roomTable);
        }

        // GET: RoomTables/Delete/5
        [CustomAuthorizeArtribute(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RoomTable roomTable = await db.RoomTables.FindAsync(id);
            if (roomTable == null)
            {
                return HttpNotFound();
            }
            return View(roomTable);
        }

        // POST: RoomTables/Delete/5
        [CustomAuthorizeArtribute(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RoomTable roomTable = await db.RoomTables.FindAsync(id);
            db.RoomTables.Remove(roomTable);
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
