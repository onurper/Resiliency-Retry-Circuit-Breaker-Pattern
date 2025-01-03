using Microsoft.Extensions.Logging;
using Resiliency.Service.API.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Resiliency.Service.API
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> logger;
        public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
        {
            _httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<Product> GetProduct(int id)
        {
            var product = await _httpClient.GetFromJsonAsync<Product>($"{id}");
            logger.LogInformation($"{product.Id}");
            return product;
        }
    }
}