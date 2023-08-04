using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CoffeeShopOnline.Models
{
    public class ShoppingCartModel
    {
            public string ItemId { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total { get; set; }
            public string ImagePath { get; set; }
            public string ItemName { get; set; }
            public DbSet<Item> Items { get; set; }
        public bool Promo { get; set; }

        public decimal PromoPrice { get; set; }
        public int TotalQuantity { get; set; }
        public int Category { get; set; }
        public List<ApplicationUser> users { get; set; }

        public int popular { get; set; }
        public Category categorie { get; internal set; }


    }
    public class ShoppingViewModel
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal ItemPrice { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public string ItemCode { get; set; }
        public int Popular { get; set; }
        public int Quantity { get; set; }
        public bool Promo { get; set; }
        public bool InOrOutTable { get; set; }
        public bool ClosedParty { get; set; }
        public bool PruductOfDay { get; set; }

        public decimal PromoPrice { get; set; }
        public string Category { get; set; }

        public List<Item> Items { get; set; }

        public bool DishOfDay { get; set; }
        public IEnumerable<SelectListItem> CategorySelectListItems { get; set; }
        public string UserName { get; set; }

    }
}