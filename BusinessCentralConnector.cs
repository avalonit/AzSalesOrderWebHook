using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace com.businesscentral
{
    public class BusinessCentraConnector
    {
        private ConnectorConfig config;
        private string ApiBaseEndPoint = string.Empty;
        private string AuthInfo = string.Empty;
        public BusinessCentraConnector(ConnectorConfig config)
        {
            this.config = config;
            this.ApiBaseEndPoint = String.Format("https://api.businesscentral.dynamics.com/{0}/{1}/", config.apiVersion1, config.tenant);
            this.AuthInfo = Convert.ToBase64String(Encoding.Default.GetBytes(config.authInfo));
        }
        public async Task<SalesOrder> GetOrderByWebhook(WebHookEvent ev)
        {
            SalesOrder orders = null;

            if (ev == null || ev.Value == null || ev.Value.Count == 0)
                return null;

            if (!ev.Value[0].Resource.Contains("salesOrders"))
                return null;

            var apiEndPoint = this.ApiBaseEndPoint + ev.Value[0].Resource;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", this.AuthInfo);

                var responseMessage = await httpClient.GetAsync(apiEndPoint);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var jsonContent = await responseMessage.Content.ReadAsStringAsync();
                    orders = JsonConvert.DeserializeObject<SalesOrder>(jsonContent);
                }
            }
            return orders;
        }

        public async Task<Employees> GetSaleagentByOrder(SalesOrder order)
        {
            Employees employees = null;

            if (order == null || String.IsNullOrEmpty(order.Salesperson))
                return null;

            var query = String.Format("employees?$filter=number eq '{0}'", order.Salesperson);

            var apiEndPoint = this.ApiBaseEndPoint + query;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", this.AuthInfo);

                var responseMessage = await httpClient.GetAsync(apiEndPoint);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var jsonContent = await responseMessage.Content.ReadAsStringAsync();
                    employees = JsonConvert.DeserializeObject<Employees>(jsonContent);
                }
            }
            return employees;
        }
    }

}
