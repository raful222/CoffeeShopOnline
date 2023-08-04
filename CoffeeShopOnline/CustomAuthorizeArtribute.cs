using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CoffeeShopOnline
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomAuthorizeArtribute : AuthorizeAttribute
    {
        public string ViewName { get; set; }

        public CustomAuthorizeArtribute()
        {
            ViewName = "AuthorizeFailed";
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            IsUserAuthorized(filterContext);
        }


        void IsUserAuthorized(AuthorizationContext filterContext)
        {
            if (filterContext.Result == null)
                return;

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                ViewDataDictionary dic = new ViewDataDictionary();
                dic.Add("Message", "You dont have sufficiecnt privileges for this page");
                var result = new ViewResult() { ViewName = this.ViewName, ViewData = dic };
                filterContext.Result = result;
            }

        }
    }
}