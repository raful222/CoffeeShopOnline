using IdentityModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CoffeeShopOnline.Models
{
    public partial class Item
    {

        [Key]
        public System.Guid ItemId { get; set; }
        public int CatogoryId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Decription { get; set; }
        public string ImagePath { get; set; }
        public decimal ItemPrice { get; set; }
        public int popular { get; set; }
        public int Quantity { get; set; }
        public bool Promo { get; set; }
        public decimal PromoPrice { get; set; }

        public bool PruductOfDay { get; set; }

        public Category Category { get; set; }

        
    }

    public class ItemViewModel
    {
        public Guid Itemid { get; set; }
        public int CategoryId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public decimal ItemPrice { get; set; }
        public HttpPostedFileBase ImagePath { get; set; }
        public int Quantity { get; set; }
        public bool Promo { get; set; }
        public decimal PromoPrice { get; set; }

        public IEnumerable<SelectListItem> CategorySelectListItems { get; set; }
    }



    public partial class Category
    {
        
        [Key]
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CetegoryName { get; set; }

    }
    
    public partial class RoomTable
    {
       
        [Key]
        public int Id { get; set;}
        public int TableNumber { get; set; }
        [Required]
        [Range(1, 12, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int TableSits { get; set; }
        public bool Available { get; set; }

        [DisplayName("Table is InSide or OutSide ,Checkbox off is Outside and On is Inside")]
        [Required]
        public bool InOrOut { get; set; }
        public string RoomType { get; set; }
        public int NumberOfTaken { get; set; }
    }

    public class People
    {
        [Required]
        [Range(1, 12, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int NumberOfClient { get; set; }

        [DisplayName("You want to closed a Party menu or Regular?\n Sgin in if yes")]
        [Required]
        public bool ClosedParty { get; set; }
        public string UserName { get; set; }


    }





}