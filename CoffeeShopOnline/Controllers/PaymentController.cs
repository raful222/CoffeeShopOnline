using CoffeeShopOnline.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Item = PayPal.Api.Item;

namespace CoffeeShopOnline.Controllers
{
    public class PaymentController : Controller
    {

        // GET: Payment
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //getting the apiContext
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Payment/PaymentWithPaypal?";


                    var guid = Convert.ToString((new Random()).Next(100000));
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
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }

                }

            }

            catch (Exception ex)
            {
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

            if (Session["CartItem"] != "")
            {
                List<ShoppingCartModel> cart = Session["CartItem"] as List<ShoppingCartModel>;
                foreach (var item in cart)
                {
                    //Adding Item Details like name, currency, price etc
                    itemList.items.Add(new Item()
                    {
                        name = item.ItemName,
                        currency = "USD",
                        price = item.UnitPrice.ToString(),
                        quantity = item.Quantity.ToString(),
                        sku = "sku"
                    });
                }
            }

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            // Adding Tax, shipping and Subtotal details
            var details = new Details()
            {
                tax = "1",
                shipping = "1",
                subtotal = Session["TotalAmount"].ToString(),
                shipping_discount = "-1"
            };

            //Final amount with details
            var amount = new Amount()
            {
                currency = "USD",
                total = (Convert.ToDecimal(Session["TotalAmount"]) + 1).ToString(), // Total must be equal to sum of tax, shipping and subtotal.
                details = details
            };

            var transactionList = new List<Transaction>();
            // Adding description about the transaction
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
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

        [HttpPost]
        public ActionResult SuccessView() {
            return View();
        }
    }
}