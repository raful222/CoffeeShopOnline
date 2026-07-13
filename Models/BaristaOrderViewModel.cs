using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CoffeeShopOnline.Models
{
    public class BaristaOrderViewModel
    {
        public class WelcomeBaristaViewModel
        {
            [Required]
            [Display(Name = "מספר אורחים")]
            [Range(1, 12, ErrorMessage = "יש להזין מספר אורחים בין 1 ל-12.")]
            public int NumberOfClient { get; set; }

            [Display(Name = "אירוע פרטי")]
            public bool ClosedParty { get; set; }

            [Display(Name = "שם הלקוח או שם משתמש")]
            [StringLength(100, ErrorMessage = "שם הלקוח יכול להכיל עד 100 תווים.")]
            public string UserName { get; set; }

        }
    }
}
