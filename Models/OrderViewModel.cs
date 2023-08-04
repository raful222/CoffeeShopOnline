using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CoffeeShopOnline.Models
{
    public partial class Order
    {
        public Order()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
        }
        [Key]
        public int OrderId { get; set; }
        public System.DateTime OrderDate { get; set; }
        public string OrderNumber { get; set; }
        public DateTime TableSitTimeEnd { get; set; }

        public int TableNumber { get; set; }
        public int NumberOfDiners { get; set; }

        public bool IsAprroved { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
    public partial class OrderDetail
    {
        [Key]
        public int OrderDetaild { get; set; }
        public int OrderId { get; set; }
        public string ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UintPrice { get; set; }
        public decimal Total { get; set; }

        public virtual Order Order { get; set; }
    }
}