using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CoffeeShopOnline.Models
{
    public sealed class OrderDashboardViewModel
    {
        public OrderDashboardViewModel()
        {
            Orders = new List<OrderDashboardItemViewModel>();
        }

        public IList<OrderDashboardItemViewModel> Orders { get; set; }
        public int PendingCount { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public sealed class OrderDashboardItemViewModel
    {
        public OrderDashboardItemViewModel()
        {
            Lines = new List<OrderDashboardLineViewModel>();
        }

        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime TableSitTimeEnd { get; set; }
        public int TableNumber { get; set; }
        public int NumberOfDiners { get; set; }
        public bool IsApproved { get; set; }
        public bool IsCompleted { get; set; }
        public decimal Total { get; set; }
        public decimal ItemCount { get; set; }
        public IList<OrderDashboardLineViewModel> Lines { get; set; }

        public string StatusKey
        {
            get { return IsCompleted ? "completed" : IsApproved ? "active" : "pending"; }
        }
    }

    public sealed class OrderDashboardLineViewModel
    {
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
    }

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
