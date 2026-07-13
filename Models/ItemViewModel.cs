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

    public class ItemViewModel : IValidatableObject
    {
        public Guid ItemId { get; set; }

        [Display(Name = "קטגוריה")]
        [Range(1, int.MaxValue, ErrorMessage = "יש לבחור קטגוריה.")]
        public int CategoryId { get; set; }

        [Display(Name = "קוד מוצר")]
        [StringLength(30, ErrorMessage = "קוד המוצר יכול להכיל עד 30 תווים.")]
        public string ItemCode { get; set; }

        [Required(ErrorMessage = "יש להזין שם מוצר.")]
        [StringLength(100, ErrorMessage = "שם המוצר יכול להכיל עד 100 תווים.")]
        [Display(Name = "שם המוצר")]
        public string ItemName { get; set; }

        [StringLength(500, ErrorMessage = "התיאור יכול להכיל עד 500 תווים.")]
        [Display(Name = "תיאור")]
        public string Description { get; set; }

        [Range(typeof(decimal), "0.01", "10000", ErrorMessage = "יש להזין מחיר בין 0.01 ל־10,000.")]
        [Display(Name = "מחיר רגיל")]
        public decimal ItemPrice { get; set; }

        [Display(Name = "תמונת מוצר")]
        public HttpPostedFileBase ImagePath { get; set; }

        [Range(0, 100000, ErrorMessage = "כמות המלאי חייבת להיות בין 0 ל־100,000.")]
        [Display(Name = "כמות במלאי")]
        public int Quantity { get; set; }

        [Display(Name = "מבצע פעיל")]
        public bool Promo { get; set; }

        [Range(typeof(decimal), "0", "10000", ErrorMessage = "מחיר המבצע אינו תקין.")]
        [Display(Name = "מחיר מבצע")]
        public decimal PromoPrice { get; set; }

        [Display(Name = "מנת היום")]
        public bool ProductOfDay { get; set; }

        public string ExistingImagePath { get; set; }

        public IEnumerable<SelectListItem> CategorySelectListItems { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Promo && PromoPrice <= 0)
                yield return new ValidationResult("יש להזין מחיר מבצע גדול מאפס.", new[] { "PromoPrice" });

            if (Promo && PromoPrice >= ItemPrice)
                yield return new ValidationResult("מחיר המבצע חייב להיות נמוך מהמחיר הרגיל.", new[] { "PromoPrice" });
        }
    }

    public sealed class InventoryDashboardViewModel
    {
        public InventoryDashboardViewModel()
        {
            Items = new List<InventoryItemViewModel>();
            Categories = new List<string>();
        }

        public IList<InventoryItemViewModel> Items { get; set; }
        public IList<string> Categories { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int PromotionCount { get; set; }
        public int TotalUnits { get; set; }
    }

    public sealed class InventoryItemViewModel
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string CategoryName { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal PromoPrice { get; set; }
        public int Quantity { get; set; }
        public int Popularity { get; set; }
        public bool Promo { get; set; }
        public bool ProductOfDay { get; set; }
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
