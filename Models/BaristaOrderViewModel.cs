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
            [Range(1, 12, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
            public int NumberOfClient { get; set; }

            [DisplayName("You want to closed a Party menu or Regular?\n Sgin in if yes")]
            [Required]
            public bool ClosedParty { get; set; }

            public string UserName { get; set; }

        }
    }
}