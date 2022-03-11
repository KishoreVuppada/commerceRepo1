﻿namespace Microsoft.Dynamics.Commerce.CustomerMigrationUtility.Core
{
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>The AX user manager class.</summary>
    public class AXUserManager
    {
        /// <summary>The provider id.</summary>
        private static readonly string ProviderIdValue = ConfigurationManager.AppSettings["ProviderId"];

        /// <summary>The data area id.</summary>
        private static readonly string DataAreaIdValue = ConfigurationManager.AppSettings["DataAreaId"];

        /// <summary>The AX Odata Url.</summary>
        private static string AXOdataUrl = ConfigurationManager.AppSettings["AX-AOS-Url"];

        /// <summary>The authentication result.</summary>
        private static AuthenticationResult AuthenticationResult = null;

        /// <summary>The ExternalIdToCustomerMaps uri.</summary>
        private const string ExternalIdToCustomerMaps = "/data/ExternalIdToCustomerMaps";

        /// <summary>The client credential.</summary>
        private readonly ClientCredential credential;

        /// <summary>The authentication context.</summary>
        private readonly AuthenticationContext authenticationContext;

        /// <summary>The logger.</summary>
        private Logger logger = new Logger();

        /// <summary>Initializes a new instance of the <see cref="AXUserManager" /> class.</summary>       
        public AXUserManager()
        {
            var tenant = ConfigurationManager.AppSettings["AX-AAD-Tenant"];
            var clientId = ConfigurationManager.AppSettings["AX-AAD-ClientId"];
            var clientPassword = ConfigurationManager.AppSettings["AX-AAD-ClientPassword"];

            if(string.IsNullOrEmpty(tenant) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientPassword))
            {
                return;
            }

            this.authenticationContext = new AuthenticationContext("https://login.microsoftonline.com/" + tenant);
            this.credential = new ClientCredential(clientId, clientPassword);
            this.logger = new Logger();
        }

        /// <summary>Creating AX account through Retail Server Api.</summary>
        /// <param name="requestPayload">The request payload.</param> 
        /// <param name="userToken">The user token</param>     
        /// <param name="retailServerEndpoint">The retail server endpoint</param>
        /// <param name="operatingUnitNumber">The operating unit number</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public async Task<Response> CreateAccount(JObject requestPayload, string userToken, string retailServerEndpoint, string operatingUnitNumber)
        {
            HttpResponseMessage httpResponse = null;
            var httpClient = new HttpClient();

            try
            {
                // Creating authentication header
                var authenticationHeaderValue = new AuthenticationHeaderValue("id_token", userToken);

                var headers = new List<KeyValuePair<string, string>>()
                       {
                         new KeyValuePair<string, string>("OUN", operatingUnitNumber),
                         new KeyValuePair<string, string>("Accept-Charset", "UTF-8"),
                         new KeyValuePair<string, string>("User-Agent", "Microsoft Dynamics commerce")
                       };

                if (!retailServerEndpoint.EndsWith("/"))
                {
                    retailServerEndpoint = retailServerEndpoint + "/";
                }

                string url = retailServerEndpoint + "Customers('')?api-version=7.3";

                this.logger.Trace("Checking the customer account in AX");

                httpResponse = await httpClient.GetAsync(url, authenticationHeaderValue, headers);

                if (httpResponse.IsSuccessStatusCode && requestPayload != null)
                {
                    this.logger.Trace("Customer account is already exist in AX.");
                    var response = await httpClient.GetContentAsync(httpResponse);
                    var jsonObject = JObject.Parse(response);
                    var accountNumber = jsonObject["AccountNumber"].ToString();
                    requestPayload["AccountNumber"] = accountNumber;

                    url = retailServerEndpoint + "Customers('" + accountNumber + "')?api-version=7.3";

                    this.logger.Trace("Updating the exiting customer in AX.");
                    httpResponse = await httpClient.PatchAsync(requestPayload, url, authenticationHeaderValue, headers);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        this.logger.Trace("Updating the exiting customer in AX is failed.");
                        return new Response(Status.Failed);
                    }

                    return new Response(Status.AlreadyExist, accountNumber);
                }
                else if (httpResponse.ReasonPhrase.Equals("Forbidden") && requestPayload != null)
                {
                    url = retailServerEndpoint + "Customers?api-version=7.3";

                    this.logger.Trace("Creating new customer in AX.");
                    httpResponse = await httpClient.PostAsync(requestPayload, url, authenticationHeaderValue, headers);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        this.logger.Trace("Creating new customer in AX is failed.");
                        return new Response(Status.Failed);
                    }

                    var response = await httpClient.GetContentAsync(httpResponse);

                    var jsonObject = JObject.Parse(response);
                    return new Response(Status.Success, jsonObject["AccountNumber"].ToString());
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Exception : " + ex.Message.ToString() + "\n";

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message.ToString()))
                {
                    errorMessage += "InnerException : " + ex.InnerException.Message.ToString() + "\n";
                }

                if (httpResponse != null)
                {
                    var response = await httpClient.GetContentAsync(httpResponse);
                    if (!string.IsNullOrEmpty(response))
                    {
                        errorMessage += "Service Failure Detail : " + response + "\n";
                    }
                }

                this.logger.Error(errorMessage);
            }

            return new Response(Status.Failed);
        }

        /// <summary>Creating AX account through Retail Server Api.</summary>
        /// <param name="customerAccountNumber">The customer account number.</param> 
        /// <param name="b2cUserId">The b2c user id(Oid)</param>  
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public async Task<Response> LinkAccount(string customerAccountNumber, string b2cUserId)
        {
            HttpResponseMessage httpResponse = null;
            var httpClient = new HttpClient();

            if (AXOdataUrl.EndsWith("/"))
            {
                AXOdataUrl = AXOdataUrl.Replace("/", string.Empty);
            }

            if (AuthenticationResult == null || AuthenticationResult.ExpiresOn < DateTimeOffset.UtcNow.AddMinutes(5))
            {
                this.logger.Trace("Acquiring the access token...");
                AuthenticationResult = await this.authenticationContext.AcquireTokenAsync(AXOdataUrl, this.credential);
                this.logger.Trace("Acquiring the access token is done.");
            }

            // Creating authentication header
            var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", AuthenticationResult.AccessToken);
            
            var externalIdToCustomerMap = new ExternalIdToCustomerMap()
            {
                CustomerAccountNumber = customerAccountNumber,
                DataAreaId = DataAreaIdValue,
                ExternalIdentityId = b2cUserId,
                ProviderId = Convert.ToInt64(ProviderIdValue)
            };

            httpResponse = await httpClient.PostAsync(externalIdToCustomerMap,
                AXOdataUrl + ExternalIdToCustomerMaps,
                authenticationHeaderValue);

            if (!httpResponse.IsSuccessStatusCode)
            {
                this.logger.Trace($"Error occured in creating account mapping in AX");
                return new Response(Status.Failed);
            }

            return new Response(Status.Success, b2cUserId);

        }
    }
}