using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CoffeeShopOnline.Startup))]
namespace CoffeeShopOnline
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
