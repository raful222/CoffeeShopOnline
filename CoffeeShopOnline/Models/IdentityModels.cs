using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CoffeeShopOnline.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Full Name"), Required]
        public string FullName { get; set; }
        public int stars { get; set; }

        [Display(Name = "Age"), Required]
        public int Age { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

       
    }
    public class ApplicationRole : IdentityRole
    {

        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
        /*        public System.Data.Entity.DbSet<WebAPli.ViewModel.RoleViewModel> RoleViewModels { get; set; }
        */
    }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<CoffeeShopOnline.Models.Order> Orders { get; set; }

        public System.Data.Entity.DbSet<CoffeeShopOnline.Models.OrderDetail> OrderDetails { get; set; }

        public System.Data.Entity.DbSet<CoffeeShopOnline.Models.Category> Categories { get; set; }

        public System.Data.Entity.DbSet<CoffeeShopOnline.Models.Item> Items { get; set; }


        public System.Data.Entity.DbSet<CoffeeShopOnline.Models.RoomTable> RoomTables { get; set; }


    }
}