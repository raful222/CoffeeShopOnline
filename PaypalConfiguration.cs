using PayPal.Api;
using System;
using System.Collections.Generic;

namespace CoffeeShopOnline
{
    public static class PaypalConfiguration
    {
        private const string ClientIdVariable = "COFFEESHOP_PAYPAL_CLIENT_ID";
        private const string ClientSecretVariable = "COFFEESHOP_PAYPAL_CLIENT_SECRET";
        // getting properties from the web.config
        public static Dictionary<string, string> GetConfig()
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
        }
        private static string GetAccessToken()
        {
            var clientId = Environment.GetEnvironmentVariable(ClientIdVariable);
            var clientSecret = Environment.GetEnvironmentVariable(ClientSecretVariable);
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException(
                    "PayPal is not configured. Set " + ClientIdVariable + " and " + ClientSecretVariable + ".");
            }

            string accessToken = new OAuthTokenCredential(clientId, clientSecret, GetConfig()).GetAccessToken();
            return accessToken;
        }
        public static APIContext GetAPIContext(string accessToken = "")
        {
            // return apicontext object by invoking it with the accesstoken
            var apiContext = new APIContext(string.IsNullOrEmpty(accessToken) ? GetAccessToken() : accessToken);
            apiContext.Config = GetConfig();
            return apiContext;
        }
    }
}
