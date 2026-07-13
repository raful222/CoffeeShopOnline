using CoffeeShopOnline.Models;
using CoffeeShopOnline.Services;
using Microsoft.AspNet.Identity;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Item = PayPal.Api.Item;

namespace CoffeeShopOnline.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Payment
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            if (string.Equals(Cancel, "true", StringComparison.OrdinalIgnoreCase))
            {
                return View("FailureView");
            }

            try
            {
                var apiContext = PaypalConfiguration.GetAPIContext();
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Payment/PaymentWithPaypal?";


                    var guid = Guid.NewGuid().ToString("N");
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid
                    Session.Add(guid, createdPayment.id);
                    if (string.IsNullOrWhiteSpace(paypalRedirectUrl))
                    {
                        return View("FailureView");
                    }
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment
                    var guid = Request.Params["guid"];
                    var paymentId = Session[guid] as string;
                    if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(paymentId))
                    {
                        return View("FailureView");
                    }
                    var executedPayment = ExecutePayment(apiContext, payerId, paymentId);
                    Session.Remove(guid);
                    //If executed payment failed then we will show payment failure message to user
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }

                }

            }

            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("PayPal payment failed: {0}", ex);
                return View("FailureView");
            }
            //on successful payment, show success page to user.
            return View("SuccessView");
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private PayPal.Api.Payment payment;

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
           
            var itemList = new ItemList() { items = new List<Item>() };

            var cart = new PersistentCartService(db, HttpContext, User.Identity.GetUserId()).GetCart();
            if (cart == null || cart.Count == 0)
            {
                throw new InvalidOperationException("The shopping cart is empty.");
            }
            foreach (var item in cart)
            {
                itemList.items.Add(new Item
                {
                    name = item.ItemName,
                    currency = "USD",
                    price = item.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture),
                    quantity = item.Quantity.ToString("0", CultureInfo.InvariantCulture),
                    sku = item.ItemId
                });
            }

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            // Adding Tax, shipping and Subtotal details
            var total = cart.Sum(item => item.UnitPrice * item.Quantity);
            var details = new Details()
            {
                tax = "0.00",
                shipping = "0.00",
                subtotal = total.ToString("0.00", CultureInfo.InvariantCulture)
            };

            //Final amount with details
            var amount = new Amount()
            {
                currency = "USD",
                total = total.ToString("0.00", CultureInfo.InvariantCulture),
                details = details
            };

            var transactionList = new List<Transaction>();
            // Adding description about the transaction
            transactionList.Add(new Transaction()
            {
                description = "GTR Coffee order",
                invoice_number = Guid.NewGuid().ToString("N"),
                amount = amount,
                item_list = itemList
            });


            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            return this.payment.Create(apiContext);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
