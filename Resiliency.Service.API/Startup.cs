using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Resiliency.Service.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Resiliency.Service.API", Version = "v1" });
            });

            services.AddHttpClient<ProductService>(opt =>
            {
                opt.BaseAddress = new Uri("https://localhost:5003/api/products/");
            }).AddPolicyHandler(GetAdvanceCircuitBreakerPolicy());
        }

        private IAsyncPolicy<HttpResponseMessage> GetCurcuitBreakerPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(3, TimeSpan.FromSeconds(10), onBreak: (arg1, arg2) =>
            {
                Debug.WriteLine($"Curcuit Breaker => On Break");
            }, onReset: () =>
            {
                Debug.WriteLine($"Curcuit Breaker => On Reset");
            }, onHalfOpen: () =>
            {
                Debug.WriteLine($"Curcuit Breaker => On Half Open");
            });
        }

        private IAsyncPolicy<HttpResponseMessage> GetAdvanceCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError().AdvancedCircuitBreakerAsync(0.1, TimeSpan.FromSeconds(15), 3, TimeSpan.FromSeconds(30), onBreak: (arg1, arg2) =>
            {
                Debug.WriteLine($"Curcuit Breaker => On Break -----------------------------------------------------------------------------------------------");
            }, onReset: () =>
            {
                Debug.WriteLine($"Curcuit Breaker => On Reset -----------------------------------------------------------------------------------------------");
            }, onHalfOpen: () =>
            {
                Debug.WriteLine($"Curcuit Breaker => On Half Open -----------------------------------------------------------------------------------------------");
            });
        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() 
        {
            return HttpPolicyExtensions.HandleTransientHttpError().OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound).WaitAndRetryAsync(5, r =>
            {
                Debug.WriteLine($"Count : {r}");
                return TimeSpan.FromSeconds(2);
            }, onRetryAsync: OnRetryAsync);
        }
        private Task OnRetryAsync(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan)
        {
            Debug.WriteLine($"Request is made again : {timeSpan}");
            return Task.CompletedTask;
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resiliency.Service.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
